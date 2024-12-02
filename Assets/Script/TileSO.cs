using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileSO", menuName = "Scriptable Objects/TileSO")]
public class TileSO : ScriptableObject
{
    public Tile tile;
    public List<TileSO> possibleTilesNorth;
    public List<TileSO> possibleTilesSouth;
    public List<TileSO> possibleTilesEast;
    public List<TileSO> possibleTilesWest;
}
