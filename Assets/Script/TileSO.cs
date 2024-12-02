using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileSO", menuName = "Scriptable Objects/TileSO")]
public class TileSO : ScriptableObject
{
    public Tile tile;
    public List<Tile> possibleTilesNorth;
    public List<Tile> possibleTilesSouth;
    public List<Tile> possibleTilesEast;
    public List<Tile> possibleTilesWest;
}
