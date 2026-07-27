using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreasingHeightAbilityRange : AbilityRange
{
    public override List<Tile> GetTilesInRange(Board board)
    {
        return board.Search(unit.tile, ExpandSearch);
    }

    bool ExpandSearch(Tile from, Tile to)
    {
        return (from.distance + 1) <= horizontal + unit.tile.height && Mathf.Abs(to.height - unit.tile.height) <= vertical;
    }
}
