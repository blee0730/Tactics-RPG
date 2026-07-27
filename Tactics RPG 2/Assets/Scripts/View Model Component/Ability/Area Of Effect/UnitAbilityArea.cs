using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitAbilityArea : AbilityArea 
{
	public override List<Tile> GetTilesInArea (Board board, Point pos)
	{
		return GetTilesInArea(board, board.GetTile(pos));
	}

	public override List<Tile> GetTilesInArea (Board board, Tile tile)
	{
		List<Tile> retValue = new List<Tile>();
		if (tile != null)
			retValue.Add(tile);
		return retValue;
	}
}
