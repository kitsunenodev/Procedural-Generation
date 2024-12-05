using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SuperpositionTile
{
    public readonly List<TileSO> Possibilities;
    public readonly List<TileSO> NorthPossibilities = new List<TileSO>();
    public readonly List<TileSO> SouthPossibilities = new List<TileSO>();
    public readonly List<TileSO> EastPossibilities = new List<TileSO>();
    public readonly List<TileSO> WestPossibilities = new List<TileSO>();
    public bool IsSet;

    public SuperpositionTile()
    {
        Possibilities = new List<TileSO>(GameManager.Instance.gameTiles);
        UpdateAdjacentPossibilities();
    }

    public void UpdateAdjacentPossibilities()
    {
        ClearAdjacentPossibilities();
        foreach (var tile in Possibilities)
        {
            foreach (var tilePossible in tile.possibleTilesNorth)
            {
                if (!NorthPossibilities.Contains(tilePossible))
                {
                    NorthPossibilities.Add(tilePossible);
                }
            }
            foreach (var tilePossible in tile.possibleTilesSouth)
            {
                if (!SouthPossibilities.Contains(tilePossible))
                {
                    SouthPossibilities.Add(tilePossible);
                }
            }
            foreach (var tilePossible in tile.possibleTilesEast)
            {
                if (!EastPossibilities.Contains(tilePossible))
                {
                    EastPossibilities.Add(tilePossible);
                }
            }
            foreach (var tilePossible in tile.possibleTilesWest)
            {
                if (!WestPossibilities.Contains(tilePossible))
                {
                    WestPossibilities.Add(tilePossible);
                }
            }
        }
    }

    public void ClearAdjacentPossibilities()
    {
        NorthPossibilities.Clear();
        SouthPossibilities.Clear();
        EastPossibilities.Clear();
        WestPossibilities.Clear();
    }

    public void UpdatePossibility(List<TileSO> possibleTiles)
    {
        foreach (var tile in GameManager.Instance.gameTiles)
        {
            if (!possibleTiles.Contains(tile))
            {
                Possibilities.Remove(tile);
            }
        }
        UpdateAdjacentPossibilities();
        IsSet = Possibilities.Count == 1;
        
    }
    
    public void UpdatePossibility(TileSO possibleTile)
    {
        Possibilities.Clear();
        Possibilities.Add(possibleTile);
        UpdateAdjacentPossibilities();
        IsSet = true;
    }

    public int GetEntropy()
    {
        return Possibilities.Count;
    }

    public void SelectRandomPossibleCell()
    {
        int random = Random.Range(0, Possibilities.Count - 1);
        UpdatePossibility(Possibilities[random]);
    }

    public void ResetPossibilities()
    {
        Possibilities.Clear();
        Possibilities.AddRange(GameManager.Instance.gameTiles);
        UpdateAdjacentPossibilities();
        IsSet = false;
    }
    public TileSO GetTile()
    {
        return IsSet ? Possibilities[0] : null;
    }
    
}
