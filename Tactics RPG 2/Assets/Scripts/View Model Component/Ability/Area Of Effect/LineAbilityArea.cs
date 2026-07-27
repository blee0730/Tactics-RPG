using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineAbilityArea : AbilityArea 
{
	public override List<Tile> GetTilesInArea(Board board, Point pos)
	{
		return GetTilesInArea(board, board.GetTile(pos));
	}

	public override List<Tile> GetTilesInArea(Board board, Tile tile)
	{
		Unit attacker = GetComponentInParent<Unit>();
		List<Tile> retValue = new List<Tile>();
		if (attacker == null || tile == null)
			return retValue;

		// The selected position is the center/anchor tile of the slash. Add the
		// two side tiles perpendicular to the attacker's facing, then the center
		// target tile. Null side tiles are skipped so edge-of-map attacks stay safe.
		if (attacker.dir == Directions.North || attacker.dir == Directions.South)
		{
			AddClosest(retValue, board, tile.pos + new Point(1, 0), tile.height);
			AddClosest(retValue, board, tile.pos + new Point(-1, 0), tile.height);
		}
		else
		{
			AddClosest(retValue, board, tile.pos + new Point(0, 1), tile.height);
			AddClosest(retValue, board, tile.pos + new Point(0, -1), tile.height);
		}

		retValue.Add(tile);
		return retValue;
	}

	void AddClosest(List<Tile> tiles, Board board, Point pos, float height)
	{
		Tile tile = board.GetClosestSelectableTile(pos, height);
		if (tile != null)
			tiles.Add(tile);
	}
}
