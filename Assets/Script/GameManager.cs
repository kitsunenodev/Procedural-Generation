using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    
    public List<TileSO> gameTiles;

    public static GameManager Instance;

    public int mapSize = 1;

    public Dictionary<Vector3Int, int> Entropies = new Dictionary<Vector3Int, int>();
    private readonly List<SuperpositionTile> _alreadyUpdatedTiles = new List<SuperpositionTile>();
    private readonly List<SuperpositionTile> _alreadySetTiles = new List<SuperpositionTile>();
    // private readonly List<SuperpositionTile> _revisedTiles = new List<SuperpositionTile>();
    
    public List<Vector3Int> lowestEntropyTilePositions = new List<Vector3Int>();

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
        
        GenerateRandomMap();
        
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

    
    //Select a random place on the tile map, assign it a random tile and complete the map accordingly
    void GenerateRandomMap()
    {
        ClearMap();
        _alreadySetTiles.Clear();
        int x = Random.Range(0, mapSize - 1);
        int y = Random.Range(0, mapSize - 1);
        GenerateCell(new Vector3Int(x,y));
    }

    
    // Generate a tile at the given position
    void GenerateCell(Vector3Int position)
    {
        _alreadyUpdatedTiles.Clear();
        _alreadyUpdatedTiles.AddRange(_alreadySetTiles);
        GenerateTile(position);
        UpdateNeighborPossibilities(position);
        CheckTiles();
        ListEntropy();
        GetLowestEntropyCells();
        CheckIfMapSet();
        if (!allTileSet)
        {
            int random = Random.Range(0, lowestEntropyTilePositions.Count -1);
            GenerateCell(lowestEntropyTilePositions[random]);
        }
    }
    
    //Select a random tileSO from the possibilities of the cell at the given position and Set the cell to be this tile
    void GenerateTile(Vector3Int position)
    {
        _map[position.x, position.y].SelectRandomPossibleCell();
        displayMap.SetTile(new Vector3Int(position.x, position.y,0), _map[position.x, position.y].GetTile().tile);
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

        //if there cna be only one possibility, the tile is Set To be that possibility
        if (possibleTiles.Count == 1)
        {
            _alreadySetTiles.Add(_map[position.x, position.y]);
            _alreadyUpdatedTiles.Add(_map[position.x, position.y]);
        }
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
    public void CheckIfMapSet()
    {
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                if (!_map[x,y].IsSet)
                {
                    allTileSet = false;
                    return ;
                }
            }
        }

        allTileSet = true;
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

        //Place a water tile on the map and update the map from it
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = grid.WorldToCell(position);
            if (cellPosition.x >= 0 && cellPosition.x < mapSize && cellPosition.y >= 0 && cellPosition.y < mapSize)
            {
                SetCell(cellPosition,gameTiles[1]);
            }
        }
    }

    //Set the cell clicked on to the tile given in parameters and update the neighbors
    public void SetCell(Vector3Int position, TileSO tile)
    {
        ResetMap();
        _map[position.x, position.y].UpdatePossibility(tile);
        UpdateNeighborPossibilities(position);
        CheckTiles();
        ListEntropy();
        GetLowestEntropyCells();
        CheckIfMapSet();
        if (!allTileSet)
        {
            int random = Random.Range(0, lowestEntropyTilePositions.Count -1);
            GenerateCell(lowestEntropyTilePositions[random]);
        }
    }

    //Give all the tiles on the map all the possibilities
    public void ResetMap()
    {
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                _map[x,y].ResetPossibilities();
            }
        }
        _alreadySetTiles.Clear();
        _alreadyUpdatedTiles.Clear();
    }
    
    //Cycle through all the neighbor and update their possibilities
    private void UpdateNeighborPossibilities(Vector3Int position)
    {
        //Add the current tile to the already updated tile to avoid infinite recursion
        if (!_alreadyUpdatedTiles.Contains(_map[position.x, position.y]))
        {
            _alreadyUpdatedTiles.Add(_map[position.x, position.y]);  
        }
        
        //if the current cell has a tile on the west
        if (position.x > 0)
        {
            //If the tile on the west has not already been updated
            if (!_alreadyUpdatedTiles.Contains(_map[position.x - 1, position.y]))
            {
                //Get all the tile the cell can be
                List<TileSO> possibilities = GetPossibleTiles(new Vector3Int(position.x -1, position.y));
                
                //Check if there is at least one possible tile the cell can be, if not reset the neighbors
                if (!CheckCompatibility(new Vector3Int(position.x -1, position.y), possibilities))
                {
                    //Get all the tile the cell can be after changing the neighbors
                    possibilities = GetPossibleTiles(new Vector3Int(position.x -1, position.y));
                }
                
                //Update the possibilities
                _map[position.x -1,position.y].UpdatePossibility(possibilities);
                
                //Update the neighbors of the current cell
                UpdateNeighborPossibilities(new Vector3Int(position.x -1, position.y));
            }
            
        }
        
        //if the current cell has a tile on the East
        if (position.x < mapSize -1)
        {
            //If the tile on the East has not already been updated
            if (!_alreadyUpdatedTiles.Contains(_map[position.x + 1, position.y]))
            {
                //Get all the tile the cell can be
                List<TileSO> possibilities = GetPossibleTiles(new Vector3Int(position.x +1, position.y));
                
                //Check if there is at least one possible tile the cell can be, if not reset the neighbors
                if (!CheckCompatibility(new Vector3Int(position.x +1, position.y), possibilities))
                {
                    //Get all the tile the cell can be after changing the neighbors
                    possibilities = GetPossibleTiles(new Vector3Int(position.x +1, position.y));
                }
                
                //Update the possibilities
                _map[position.x +1,position.y].UpdatePossibility(possibilities);
                
                //Update the neighbors of the current cell
                UpdateNeighborPossibilities(new Vector3Int(position.x +1, position.y));
            }
        }
        
        //if the current cell has a tile on the North
        if (position.y > 0)
        {
            //If the tile on the North has not already been updated
            if (!_alreadyUpdatedTiles.Contains(_map[position.x,position.y - 1]))
            { 
                //Get all the tile the cell can be
                List<TileSO> possibilities = GetPossibleTiles(new Vector3Int(position.x, position.y - 1));
                
                //Check if there is at least one possible tile the cell can be, if not reset the neighbors
                if (!CheckCompatibility(new Vector3Int(position.x, position.y - 1), possibilities))
                {
                    //Get all the tile the cell can be after changing the neighbors
                    possibilities = GetPossibleTiles(new Vector3Int(position.x, position.y -1));
                }
                
                //Update the possibilities
                _map[position.x,position.y-1].UpdatePossibility(possibilities);
                
                //Update the neighbors of the current cell
                UpdateNeighborPossibilities(new Vector3Int(position.x, position.y -1));
            }
        }
        
        //if the current cell has a tile on the South
        if (position.y < mapSize -1)
        {
            //If the tile on the South has not already been updated
            if (!_alreadyUpdatedTiles.Contains(_map[position.x,position.y+1]))
            {
                //Get all the tile the cell can be
                List<TileSO> possibilities = GetPossibleTiles(new Vector3Int(position.x , position.y+1));
                
                //Check if there is at least one possible tile the cell can be, if not reset the neighbors
                if (!CheckCompatibility(new Vector3Int(position.x, position.y+1), possibilities))
                {
                    //Get all the tile the cell can be after changing the neighbors
                    possibilities = GetPossibleTiles(new Vector3Int(position.x, position.y+1));
                }
                
                //Update the possibilities
                _map[position.x,position.y+1].UpdatePossibility(possibilities);
                
                //Update the neighbors of the current cell
                UpdateNeighborPossibilities(new Vector3Int(position.x, position.y+1));
            }
        }
    }

    
    //Reset the possible tiles the cell can be at the given position
    void ResetNeighbor(Vector3Int position)
    {
        _map[position.x, position.y].ResetPossibilities();
    }

    
    //Reset all the neighbor of the tile at the given position
    void ResetAllNeighbor(Vector3Int position)
    {
        //If the cell has a cell on the West reset the cell on the West
        if (position.x > 0)
        {
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
    
    
}