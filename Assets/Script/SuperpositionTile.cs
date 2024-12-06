using System.Collections.Generic;
using UnityEngine;

public class SuperpositionTile
{
    private readonly List<TileSO> _possibilities;
    public readonly List<TileSO> NorthPossibilities = new List<TileSO>();
    public readonly List<TileSO> SouthPossibilities = new List<TileSO>();
    public readonly List<TileSO> EastPossibilities = new List<TileSO>();
    public readonly List<TileSO> WestPossibilities = new List<TileSO>();
    public bool IsSet;

    
    public SuperpositionTile()
    {
        _possibilities = new List<TileSO>(GameManager.Instance.gameTiles);
        UpdateAdjacentPossibilities();
    }

    private void UpdateAdjacentPossibilities()
    {
        ClearAdjacentPossibilities();
        foreach (var tile in _possibilities)
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

    private void ClearAdjacentPossibilities()
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
                _possibilities.Remove(tile);
            }
        }
        UpdateAdjacentPossibilities();
        IsSet = _possibilities.Count == 1;
        
    }
    
    public void UpdatePossibility(TileSO possibleTile)
    {
        _possibilities.Clear();
        _possibilities.Add(possibleTile);
        UpdateAdjacentPossibilities();
        IsSet = true;
    }

    public int GetEntropy()
    {
        return _possibilities.Count;
    }

    public void SelectRandomPossibleCell()
    {
        int random = Random.Range(0, _possibilities.Count - 1);
        UpdatePossibility(_possibilities[random]);
    }

    public void ResetPossibilities()
    {
        _possibilities.Clear();
        _possibilities.AddRange(GameManager.Instance.gameTiles);
        UpdateAdjacentPossibilities();
        IsSet = false;
    }
    public TileSO GetTile()
    {
        return IsSet ? _possibilities[0] : null;
    }
    
}
