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
        _alreadyUpdatedTiles.Clear();
        _alreadySetTiles.Clear();
        // _revisedTiles.Clear();
    }

    private void Start()
    {
        
        GenerateRandomMap();
        
    }

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

    void GenerateRandomMap()
    {
        ClearMap();
        _alreadySetTiles.Clear();
        // _revisedTiles.Clear();
        int x = Random.Range(0, mapSize - 1);
        int y = Random.Range(0, mapSize - 1);
        GenerateCell(new Vector3Int(x,y));
    }

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
        if (position.x < mapSize-1)
        {
            foreach (var tile in gameTiles)
            {
                if (!_map[position.x+1, position.y].WestPossibilities.Contains(tile))
                {
                    possibleTiles.Remove(tile);
                }
            }
        }
        if (position.x > 0)
        {
            foreach (var tile in gameTiles)
            {
                
                if (!_map[position.x-1, position.y].EastPossibilities.Contains(tile))
                {
                    possibleTiles.Remove(tile);
                }
            }
        }

        if (position.y < mapSize-1)
        {
            
            foreach (var tile in gameTiles)
            {
                if (!_map[position.x, position.y + 1].SouthPossibilities.Contains(tile))
                {
                    possibleTiles.Remove(tile);
                }
            }
        }

        if (position.y > 0)
        {
            foreach (var tile in gameTiles)
            {
                if (!_map[position.x, position.y - 1].NorthPossibilities.Contains(tile))
                {
                    possibleTiles.Remove(tile);
                }
            }
        }

        if (possibleTiles.Count == 1)
        {
            _alreadySetTiles.Add(_map[position.x, position.y]);
            _alreadyUpdatedTiles.Add(_map[position.x, position.y]);
        }
        return possibleTiles;
    }
    
    List<TileSO> GetPossibleTilesFromModifiedTiles(Vector3Int position)
    {
        List<TileSO> possibleTiles = new List<TileSO>();
        possibleTiles.AddRange(gameTiles);
        
        if (position.x < mapSize-1)
        {
            if (_alreadyUpdatedTiles.Contains(_map[position.x + 1, position.y]))
            {
                foreach (var tile in gameTiles)
                { 
                    if (!_map[position.x+1, position.y].WestPossibilities.Contains(tile))
                    {
                        
                        possibleTiles.Remove(tile);
                    }
                }
            }
           
        }
        if (position.x > 0)
        {
            if (_alreadyUpdatedTiles.Contains(_map[position.x - 1, position.y]))
            {
                
                foreach (var tile in gameTiles)
                {
                    if (!_map[position.x-1, position.y].EastPossibilities.Contains(tile))
                    {
                        possibleTiles.Remove(tile);
                    }
                }
            }
            
        }

        if (position.y < mapSize-1)
        {
            if (_alreadyUpdatedTiles.Contains(_map[position.x, position.y+1]))
            {
                foreach (var tile in gameTiles)
                {
                    if (!_map[position.x, position.y + 1].SouthPossibilities.Contains(tile))
                    {
                        possibleTiles.Remove(tile);
                    }
                }
            }
            
        }

        if (position.y > 0)
        {
            if (_alreadyUpdatedTiles.Contains(_map[position.x, position.y-1]))
            {
                foreach (var tile in gameTiles)
                {
                    if (!_map[position.x, position.y - 1].NorthPossibilities.Contains(tile))
                    {
                        possibleTiles.Remove(tile);
                    }
                }
            }
        }

        if (possibleTiles.Count == 1)
        {
            _alreadySetTiles.Add(_map[position.x, position.y]);
        }
        return possibleTiles;
    }

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
    }

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
    private void UpdateNeighborPossibilities(Vector3Int position)
    {
        if (!_alreadyUpdatedTiles.Contains(_map[position.x, position.y]))
        {
            _alreadyUpdatedTiles.Add(_map[position.x, position.y]);  
        }
        
        if (position.x > 0)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x - 1, position.y]))
            {
                List<TileSO> possibilities = GetPossibleTiles(new Vector3Int(position.x -1, position.y));
                if (!CheckCompatibility(new Vector3Int(position.x -1, position.y), possibilities))
                {
                    possibilities = GetPossibleTiles(new Vector3Int(position.x -1, position.y));
                }
                _map[position.x -1,position.y].UpdatePossibility(possibilities);
                UpdateNeighborPossibilities(new Vector3Int(position.x -1, position.y));
            }
            
        }
        if (position.x < mapSize -1)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x + 1, position.y]))
            {
                List<TileSO> possibilities = GetPossibleTiles(new Vector3Int(position.x +1, position.y));
                if (!CheckCompatibility(new Vector3Int(position.x +1, position.y), possibilities))
                {
                    possibilities = GetPossibleTiles(new Vector3Int(position.x +1, position.y));
                }
                _map[position.x +1,position.y].UpdatePossibility(possibilities);
                UpdateNeighborPossibilities(new Vector3Int(position.x +1, position.y));
            }
        }
        if (position.y > 0)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x,position.y - 1]))
            { 
                List<TileSO> possibilities = GetPossibleTiles(new Vector3Int(position.x, position.y - 1));
                if (!CheckCompatibility(new Vector3Int(position.x, position.y - 1), possibilities))
                {
                    possibilities = GetPossibleTiles(new Vector3Int(position.x, position.y -1));
                }
                _map[position.x,position.y-1].UpdatePossibility(possibilities);
                UpdateNeighborPossibilities(new Vector3Int(position.x, position.y -1));
            }
        }
        if (position.y < mapSize -1)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x,position.y+1]))
            {
                List<TileSO> possibilities = GetPossibleTiles(new Vector3Int(position.x , position.y+1));
                if (!CheckCompatibility(new Vector3Int(position.x, position.y+1), possibilities))
                {
                    possibilities = GetPossibleTiles(new Vector3Int(position.x, position.y+1));
                }
                _map[position.x,position.y+1].UpdatePossibility(possibilities);
                UpdateNeighborPossibilities(new Vector3Int(position.x, position.y+1));
            }
        }
    }

    void ResetNeighbor(Vector3Int position)
    {
        _map[position.x, position.y].ResetPossibilities();
    }

    void ResetAllNeighbor(Vector3Int position)
    {
        if (position.x > 0)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x -1, position.y]))
            {
                ResetNeighbor(new Vector3Int(position.x -1, position.y));
            }
            
        }
        if (position.x < mapSize -1)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x+1, position.y]))
            {
                ResetNeighbor(new Vector3Int(position.x +1, position.y));
            }
            
        }
        
        if (position.y > 0)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x, position.y - 1]))
            {
                ResetNeighbor(new Vector3Int(position.x, position.y -1));
            }
            
        }
        if (position.y < mapSize  -1)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x, position.y + 1]))
            {
                ResetNeighbor(new Vector3Int(position.x, position.y + 1));
            }
        }
    }

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