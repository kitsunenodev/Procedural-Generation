using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    //List of all the possible tiles
    public List<TileSO> gameTiles;

    public static GameManager Instance;

    //Size of the map
    [Range(1,50)]
    public int mapSize = 1;

    private bool _isExpandingMap = false;

    private bool _isModifyingMap = false;

    //Dictionary of the umber of possibility for each cell
    public Dictionary<Vector3Int, int> Entropies = new Dictionary<Vector3Int, int>();
    
    //Tile that have already been Updated
    private readonly List<SuperpositionTile> _alreadyUpdatedTiles = new List<SuperpositionTile>();

    private readonly List<SuperpositionTile> _TilesToCheck = new List<SuperpositionTile>();
    private readonly List<SuperpositionTile> _TilesToCheckNext= new List<SuperpositionTile>();
    
    
    //Tile that are already that to one possibility
    private readonly List<SuperpositionTile> _alreadySetTiles = new List<SuperpositionTile>();
    
    //List of the cell with the least possibilities
    public List<Vector3Int> lowestEntropyTilePositions = new List<Vector3Int>();

    //2 dimensional array to stock all the cells 
    private SuperpositionTile[,] _map;
    
    public bool allTileSet;
    
    public Grid grid;
    public Tilemap displayMap;


    private void Awake()
    {
        if (Instance != null)
        {
            return;
        }

        Instance = this;
        _map = new SuperpositionTile[mapSize, mapSize];
        ClearMap();
        _alreadyUpdatedTiles.Clear();
        _alreadySetTiles.Clear();
    }

    private void Start()
    {
        ClearMap();
        _alreadySetTiles.Clear();
        // GenerateCell(new Vector3Int(Random.Range(0, mapSize - 1),Random.Range(0, mapSize - 1)));
        StartCoroutine(ExpandGeneration());
    }
    
    private void Update()
    {
        //Place a grass tile on the map and update the map from it
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = grid.WorldToCell(position);
            if (cellPosition.x >= 0 && cellPosition.x < mapSize && cellPosition.y >= 0 && cellPosition.y < mapSize)
            {
                SetCell(cellPosition,gameTiles[0]);
            }
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = grid.WorldToCell(position);
            if (cellPosition.x >= 0 && cellPosition.x < mapSize && cellPosition.y >= 0 && cellPosition.y < mapSize)
            {
                SetCell(cellPosition,gameTiles[1]);
            }
        }
        //
        // //Place a water tile on the map and update the map from it
        // if (Input.GetMouseButtonDown(1))
        // {
        //     Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     Vector3Int cellPosition = grid.WorldToCell(position);
        //     if (cellPosition.x >= 0 && cellPosition.x < mapSize && cellPosition.y >= 0 && cellPosition.y < mapSize)
        //     {
        //         SetCell(cellPosition,gameTiles[1]);
        //     }
        // }
    }

    //File all the tiles of the map with new superposition cell that contain all the possibilitiies
    private void ClearMap()
    {
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                _map[x, y] = new SuperpositionTile();
            }
        }
    }
    
    // Generate a tile at the given position and update the map
    void GenerateCell(Vector3Int position)
    {
        if (_alreadySetTiles.Contains(_map[position.x, position.y]))
        {
            CheckIfMapSet();
            if (!allTileSet)
            {
                ListEntropy();
                GetLowestEntropyCells();
                int random = Random.Range(0, lowestEntropyTilePositions.Count -1);
                GenerateCell(lowestEntropyTilePositions[random]);
                return;
            }
            return;
        }
        _alreadyUpdatedTiles.Clear();
        _alreadyUpdatedTiles.AddRange(_alreadySetTiles);
        //generate a tile
        GenerateTile(position);
    }
    
    //Select a random tileSO from the possibilities of the cell at the given position and Set the cell to be this tile
    void GenerateTile(Vector3Int position)
    {
        _map[position.x, position.y].SelectRandomPossibleCell();
        displayMap.SetTile(new Vector3Int(position.x, position.y,0), _map[position.x, position.y].GetTile().tile);
        if (!_alreadySetTiles.Contains(_map[position.x, position.y]))
        {
            _alreadySetTiles.Add( _map[position.x, position.y]);
        }
        
        _alreadyUpdatedTiles.Add(_map[position.x, position.y]);
        UpdateNeighborsPossibilities(position);
        if(!_isExpandingMap)StartCoroutine(ExpandGeneration());
    }
    
    private void AddNeighborToCheck(Vector3Int position)
    {
        if (position.x > 0)
        {
            if (!_TilesToCheckNext.Contains(_map[position.x - 1, position.y]) 
                && !_alreadyUpdatedTiles.Contains(_map[position.x - 1, position.y]))
            {
                _TilesToCheckNext.Add(_map[position.x - 1, position.y]); 
            }
        }

        if (position.x < mapSize - 1)
        {
            if (!_TilesToCheckNext.Contains(_map[position.x + 1, position.y])
                && !_alreadyUpdatedTiles.Contains(_map[position.x + 1, position.y]))
            {
                _TilesToCheckNext.Add(_map[position.x + 1, position.y]);
            }
        }

        if (position.y > 0)
        {
            if (!_TilesToCheckNext.Contains(_map[position.x, position.y - 1])
                && !_alreadyUpdatedTiles.Contains(_map[position.x, position.y - 1]))
            {
                _TilesToCheckNext.Add(_map[position.x, position.y - 1]);
            }
        }
        if (position.y >= mapSize - 1) return;
        if (!_TilesToCheckNext.Contains(_map[position.x, position.y + 1])
            && !_alreadyUpdatedTiles.Contains(_map[position.x, position.y + 1]))
        {
            _TilesToCheckNext.Add(_map[position.x, position.y + 1]);
        }
    }

    //Return the list off all the tile that can assemble with the tiles of the neighbors;
    List<TileSO> GetPossibleTiles(Vector3Int position)
    {
        List<TileSO> possibleTiles = new List<TileSO>();
        possibleTiles.Clear();
        possibleTiles.AddRange(gameTiles);
        
        //Check if tile have a cell on the east
        if (position.x < mapSize-1)
        {
            foreach (var tile in gameTiles)
            {
                //remove all the tile that can't be on the west of the cell on the east of the given position from the possibilities
                if (!_map[position.x+1, position.y].WestPossibilities.Contains(tile))
                {
                    possibleTiles.Remove(tile);
                }
            }
        }
        
        //Check if tile have a cell on the west
        if (position.x > 0)
        {
            foreach (var tile in gameTiles)
            {
                //remove all the tile that can't be on the east of the cell on the west of the given position from the possibilities
                if (!_map[position.x-1, position.y].EastPossibilities.Contains(tile))
                {
                    possibleTiles.Remove(tile);
                }
            }
        }

        //Check if tile have a cell on the North
        if (position.y < mapSize-1)
        {
            
            foreach (var tile in gameTiles)
            {
                //remove all the tile that can't be on the South of the cell on the North of the given position from the possibilities
                if (!_map[position.x, position.y + 1].SouthPossibilities.Contains(tile))
                {
                    possibleTiles.Remove(tile);
                }
            }
        }

        //Check if tile have a cell on the South
        if (position.y > 0)
        {
            foreach (var tile in gameTiles)
            {
                //remove all the tile that can't be on the North of the cell on the South of the given position from the possibilities
                if (!_map[position.x, position.y - 1].NorthPossibilities.Contains(tile))
                {
                    possibleTiles.Remove(tile);
                }
            }
        }

        //if there can be only one possibility, the tile is Set To be that possibility
        // if (possibleTiles.Count == 1)
        // {
        //     _alreadySetTiles.Add(_map[position.x, position.y]);
        //     _alreadyUpdatedTiles.Add(_map[position.x, position.y]);
        //     displayMap.SetTile(new Vector3Int(position.x, position.y,0), _map[position.x, position.y].GetTile().tile);
        //     
        // }
        return possibleTiles;
    }
    
    //Check for all tile of the tileMap the number of possible tile it can be
    public void ListEntropy()
    {
        Vector3Int position = new Vector3Int(0, 0);
        for (int y = 0; y < mapSize; y++)
        {
            position.y = y;
            for (int x = 0; x < mapSize; x++)
            {
                position.x = x;
                Entropies[position] = _map[x, y].GetEntropy();
            }
        }
    }

    //Check for all the cell of the map if the tile is set, if it is, display it on the tile map
    public void CheckTiles()
    {
        Vector3Int position = new Vector3Int(0, 0);
        for (int y = 0; y < mapSize; y++)
        {
            position.y = y;
            for (int x = 0; x < mapSize; x++)
            {
                position.x = x;
                if (_map[x,y].IsSet)
                {
                    displayMap.SetTile(new Vector3Int(position.x, position.y, 0), _map[x,y].GetTile().tile);
                    if (!_alreadySetTiles.Contains(_map[x,y]))
                    {
                        _alreadySetTiles.Add(_map[x,y]);
                    }
                }
            }
        }
    }
    
    //Return a list of the cell that are not set with the list possibility
    public void GetLowestEntropyCells()
    {
        int minEntropy = Int32.MaxValue;
        lowestEntropyTilePositions.Clear();
        foreach (var keyValuePair in Entropies)
        {
            if (keyValuePair.Value < minEntropy && keyValuePair.Value > 1)
            {
                minEntropy = keyValuePair.Value;
                lowestEntropyTilePositions.Clear();
                lowestEntropyTilePositions.Add(keyValuePair.Key);
            }
            if (keyValuePair.Value == minEntropy)
            {
                lowestEntropyTilePositions.Add(keyValuePair.Key);
            }
        }
    }

    //Return true if all the tile of the map are Set
    private void CheckIfMapSet()
    {
        List<SuperpositionTile> tilesToRemove = new List<SuperpositionTile>();
        foreach (var Tile in _alreadySetTiles)
        {
            if (!Tile.GetTile())
            {
                tilesToRemove.Add(Tile);
                continue;
            }

            if (GetPossibleTiles(GetTilePosition(Tile)).Count == 0)
            {
                tilesToRemove.Add(Tile);
            }
            displayMap.SetTile(GetTilePosition(Tile),Tile.GetTile().tile);
        }
        foreach (var tile in tilesToRemove)
        {
            _alreadySetTiles.Remove(tile);
        }
        
        allTileSet = _alreadySetTiles.Count == mapSize * mapSize;
    }

    //Set the cell clicked on to the tile given in parameters and update the neighbors
    private void SetCell(Vector3Int position, TileSO tile)
    {
        if (_isModifyingMap) return;
        _isModifyingMap = true;
        // ResetMap();
        _alreadyUpdatedTiles.Clear();
        // _alreadySetTiles.Clear();
        allTileSet = false;
        _map[position.x, position.y].UpdatePossibility(tile);
        displayMap.SetTile(new Vector3Int(position.x, position.y,0), _map[position.x, position.y].GetTile().tile);
        _alreadyUpdatedTiles.Add(_map[position.x, position.y]);
        if(!_alreadySetTiles.Contains(_map[position.x, position.y])) _alreadySetTiles.Add(_map[position.x, position.y]);
        UpdateNeighborsPossibilities(position);
        if (!_isExpandingMap) StartCoroutine(ExpandGeneration());
    }
    
    //Cycle through all the neighbor and update their possibilities
    private void UpdateNeighborsPossibilities(Vector3Int position)
    {
        //Add the current tile to the already updated tile to avoid infinite recursion
        if (!_alreadyUpdatedTiles.Contains(_map[position.x, position.y])) _alreadyUpdatedTiles.Add(_map[position.x, position.y]);

        if (position.x > 0)
        {
            UpdateNeighborPossibilities(new Vector3Int(position.x -1, position.y, 0));
        }
        //if the current cell has a tile on the East
        if (position.x < mapSize -1)
        {
            UpdateNeighborPossibilities(new Vector3Int(position.x +1, position.y, 0 ));
        }
        
        //if the current cell has a tile on the North
        if (position.y > 0)
        {
            UpdateNeighborPossibilities(new Vector3Int(position.x, position.y - 1, 0 ));
        }
        
        //if the current cell has a tile on the South
        if (position.y < mapSize -1)
        {
            UpdateNeighborPossibilities(new Vector3Int(position.x, position.y +1, 0));
        }
    }

    private void UpdateNeighborPossibilities(Vector3Int position)
    {
         int entropyBeforeUpdate;
         
        //If the tile on the west has not already been updated
        if (!_alreadyUpdatedTiles.Contains(_map[position.x, position.y]))
        {
            //Get all the tile the cell can be
            List<TileSO> possibilities = GetPossibleTiles(new Vector3Int(position.x, position.y));
            
            //Check if there is at least one possible tile the cell can be, if not reset the neighbors
            if (!CheckCompatibility(new Vector3Int(position.x, position.y), possibilities))
            {
                //Get all the tile the cell can be after changing the neighbors
                possibilities = GetPossibleTiles(new Vector3Int(position.x, position.y));
            }

            entropyBeforeUpdate = _map[position.x, position.y].GetEntropy();
            
            //if we are modifying a map that is already set, reset the possibilities of this tile if the current tile is Invalid
            if (entropyBeforeUpdate < possibilities.Count && _isModifyingMap)
            {
                //Check if the tile previously set is still a valid option
                if (!_map[position.x, position.y].IsCurrentPossibilityValid(possibilities))
                {
                    _map[position.x, position.y].ResetPossibilities();
                    if (_alreadySetTiles.Contains(_map[position.x, position.y]))
                    {
                        _alreadySetTiles.Remove(_map[position.x, position.y]);
                    }
                    entropyBeforeUpdate = _map[position.x, position.y].GetEntropy();
                }
            }
            
            //Update the possibilities
            _map[position.x,position.y].UpdatePossibilities(possibilities);
            
            if (_map[position.x,position.y].IsSet)
            {
                if (!_alreadySetTiles.Contains(_map[position.x, position.y]))
                {
                    _alreadySetTiles.Add(_map[position.x,position.y]);
                }
                displayMap.SetTile(new Vector3Int(position.x,position.y,0), _map[position.x, position.y].GetTile().tile);
            }
            else
            {
                if (_alreadySetTiles.Contains(_map[position.x, position.y]))
                    _alreadySetTiles.Remove(_map[position.x, position.y]);
            }

            if (entropyBeforeUpdate != _map[position.x, position.y].GetEntropy())
            {
                AddNeighborToCheck(new Vector3Int(position.x, position.y));
            }
            // _alreadyUpdatedTiles.Add(_map[position.x -1,position.y]);
            //Update the neighbors of the current cell
            
        }
    }
    
    //Reset the possible tiles the cell can be at the given position
    void ResetNeighbor(Vector3Int position)
    {
        _map[position.x, position.y].ResetPossibilities();
        if (_alreadySetTiles.Contains(_map[position.x, position.y]))
        {
            _alreadySetTiles.Remove(_map[position.x, position.y]);
        }
    }
    
    //Reset all the neighbor of the tile at the given position
    void ResetAllNeighbor(Vector3Int position)
    {
        //If the cell has a cell on the West reset the cell on the West
        if (position.x > 0)
        {
            //We do not reset UpdatedTiles
            if (!_alreadyUpdatedTiles.Contains(_map[position.x -1, position.y]))
            {
                ResetNeighbor(new Vector3Int(position.x -1, position.y));
            }
        }
        
        //If the cell has a cell on the East reset the cell on the East
        if (position.x < mapSize -1)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x+1, position.y]))
            {
                ResetNeighbor(new Vector3Int(position.x +1, position.y));
            }
            
        }
        
        //If the cell has a cell on the South reset the cell on the South
        if (position.y > 0)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x, position.y - 1]))
            {
                ResetNeighbor(new Vector3Int(position.x, position.y -1));
            }
            
        }
        
        //If the cell has a cell on the North reset the cell on the North
        if (position.y < mapSize  -1)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x, position.y + 1]))
            {
                ResetNeighbor(new Vector3Int(position.x, position.y + 1));
            }
        }
    }

    //Check if the neighbors of the cell at the given position allow at least one possibility
    public bool CheckCompatibility(Vector3Int position,List<TileSO> possibilities)
    {
        if (possibilities.Count == 0)
        {
            ResetAllNeighbor(position);
            return false;
        }
        return true;
    }

    private IEnumerator ExpandGeneration()
    {
        if (_isExpandingMap) yield break;
        _isExpandingMap = true;
        while (!allTileSet)
        {
            if (_TilesToCheckNext.Count == 0)
            {
                CheckIfMapSet();
                if (allTileSet)
                {
                    _isExpandingMap = false;
                    _isModifyingMap = false;
                    yield break;
                    // StopCoroutine(ExpandGeneration());
                }
                ListEntropy();
                GetLowestEntropyCells();
                if (lowestEntropyTilePositions.Count <= 0)
                {
                    _isExpandingMap = false;
                    _isModifyingMap = false;
                    yield break;
                }
                    
                int random = Random.Range(0, lowestEntropyTilePositions.Count -1);
                GenerateCell(lowestEntropyTilePositions[random]);
                
            }
            else
            {
                _TilesToCheck.AddRange(_TilesToCheckNext);
                _TilesToCheckNext.Clear();
                 
                 foreach (var tile in _TilesToCheck)
                 {
                     UpdateNeighborsPossibilities(GetTilePosition(tile));
                 }
                 CheckTiles();
                 _TilesToCheck.Clear();
            }
        }
    }

    private Vector3Int GetTilePosition(SuperpositionTile tile)
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                if (_map[j, i] == tile)
                {
                    return new Vector3Int(j, i, 0);
                }
            }
        }
        return new Vector3Int(-1,-1,-1);
    }
    
}