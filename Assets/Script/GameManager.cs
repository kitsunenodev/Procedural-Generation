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
    }

    private void Start()
    {
        
        GenerateRandomMapFromCell();
        
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

    void GenerateRandomMapFromCell()
    {
        ClearMap();
        int x = Random.Range(0, mapSize - 1);
        int y = Random.Range(0, mapSize - 1);
        GenerateCell(new Vector3Int(x,y));
    }

    void GenerateCell(Vector3Int position)
    {
        _alreadyUpdatedTiles.Clear();
        _alreadyUpdatedTiles.AddRange(_alreadySetTiles);
        GenerateTile(position);
        UpdatesAllNeighborTiles(position);
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
        int possibilitiesCount = _map[position.x, position.y].GetEntropy();
        List<TileSO> possibleTiles = new List<TileSO>();
        possibleTiles.Clear();
        possibleTiles.AddRange(gameTiles);
        if (possibilitiesCount == 1)
        {
            _alreadySetTiles.Add(_map[position.x, position.y]);
            _alreadyUpdatedTiles.Add(_map[position.x, position.y]);
            return _map[position.x, position.y]._possibilities;
        }
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

    public void UpdatesAllNeighborTiles(Vector3Int position)
    {
        _alreadyUpdatedTiles.Add(_map[position.x, position.y]);
        if (position.x > 0)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x - 1, position.y]))
            {
                _map[position.x -1,position.y].UpdatePossibility(GetPossibleTiles(new Vector3Int(position.x -1, position.y)));
                UpdatesAllNeighborTiles(new Vector3Int(position.x - 1, position.y));
            }
            
        }
        if (position.x < mapSize -1)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x +1,position.y]))
            {
                _map[position.x + 1,position.y].UpdatePossibility(GetPossibleTiles(new Vector3Int(position.x +1, position.y)));
                UpdatesAllNeighborTiles(new Vector3Int(position.x + 1, position.y));
                
            }
        }
        if (position.y > 0)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x,position.y - 1]))
            {
                _map[position.x,position.y-1].UpdatePossibility(GetPossibleTiles(new Vector3Int(position.x, position.y -1)));
                UpdatesAllNeighborTiles(new Vector3Int(position.x , position.y - 1));
                
            }
        }
        if (position.y < mapSize -1)
        {
            if (!_alreadyUpdatedTiles.Contains(_map[position.x,position.y+1]))
            {
                _map[position.x,position.y +1].UpdatePossibility(GetPossibleTiles(new Vector3Int(position.x, position.y+1)));
                UpdatesAllNeighborTiles(new Vector3Int(position.x , position.y+1));
            }
        }
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
                if (_map[x,y].IsSet   && displayMap.GetTile(new Vector3Int(position.x, position.y, 0)) == null)
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
            Vector3 position = Input.mousePosition;
            Vector3Int cellPosition = grid.WorldToCell(position);
            _map[cellPosition.x, cellPosition.y].UpdatePossibility(gameTiles[0]);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 position = Input.mousePosition;
            Vector3Int cellPosition = grid.WorldToCell(position);
            _map[cellPosition.x, cellPosition.y].UpdatePossibility(gameTiles[1]);
        }
    }

    public void SetCell(Vector3Int position, TileSO tile)
    {
        
    }
}