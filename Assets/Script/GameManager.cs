using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public List<TileSO> gameTiles;

    public TileSO[,] Map;

    public Tilemap displayMap;

    private void Start()
    {
        displayMap.SetTile(new Vector3Int(0,0,0), gameTiles[0].tile );
    }

    void GenerateTile(Vector3Int position, List<TileSO> possibleTiles)
    {
        TileSO tile;
        int randomTileNumber;
        if (possibleTiles.Count > 0)
        {
            randomTileNumber = Random.Range(0, possibleTiles.Count - 1);
            tile = possibleTiles[randomTileNumber];
        }
        else
        {
            randomTileNumber = Random.Range(0, gameTiles.Count - 1);
            tile = gameTiles[randomTileNumber];
        }
        Map[position.x, position.y] = tile;
        displayMap.SetTile(position, tile.tile);
    }

    void GetPossibleTiles(Vector3Int position, List<TileSO> possibleTiles)
    {
        if (Map[position.x+1,position.y]!= null)
        {
            possibleTiles.AddRange(Map[position.x+1, position.y].possibleTilesWest);
        }
        if (Map[position.x-1,position.y]!= null)
        {
            possibleTiles.AddRange(Map[position.x-1, position.y].possibleTilesEast);
        }
        if (Map[position.x,position.y+1]!= null)
        {
            possibleTiles.AddRange(Map[position.x, position.y+1].possibleTilesNorth);
        }
        if (Map[position.x,position.y-1]!= null)
        {
            possibleTiles.AddRange(Map[position.x, position.y-1].possibleTilesSouth);
        }
        
    }
}