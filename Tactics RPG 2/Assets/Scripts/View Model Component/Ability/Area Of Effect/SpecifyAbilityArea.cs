using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpecifyAbilityArea : AbilityArea 
{
	public int horizontal;
	public int vertical;
	Tile tile;

	public override List<Tile> GetTilesInArea (Board board, Point pos)
	{
		return GetTilesInArea(board, board.GetTile(pos));
	}

	public override List<Tile> GetTilesInArea (Board board, Tile selectedTile)
	{
		tile = selectedTile;
		return board.Search(tile, ExpandSearch);
	}

	bool ExpandSearch (Tile from, Tile to)
	{
		return (from.distance + 1) <= horizontal && Mathf.Abs(to.height - tile.height) <= vertical;
	}
}
