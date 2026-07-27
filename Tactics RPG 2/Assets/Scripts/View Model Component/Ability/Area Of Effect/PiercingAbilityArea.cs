using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PiercingAbilityArea : AbilityArea 
{
	public override List<Tile> GetTilesInArea(Board board, Point pos)
	{
		return GetTilesInArea(board, board.GetTile(pos));
	}

	public override List<Tile> GetTilesInArea(Board board, Tile tile)
	{
		Unit attacker = GetComponentInParent<Unit>();
		List<Tile> retValue = new List<Tile>();
		if (attacker == null || attacker.tile == null || tile == null)
			return retValue;

		Tile start = attacker.tile;
		Point diff = start.pos - tile.pos;
		retValue.Add(tile);

		if (diff.x - diff.y == 0 && diff.y < 0)
			AddClosest(retValue, board, tile.pos + new Point(1, 1), tile.height);
		else if (diff.x - diff.y == 0 && diff.y > 0)
			AddClosest(retValue, board, tile.pos + new Point(-1, -1), tile.height);
		else if (diff.x - diff.y == diff.x * 2 && diff.y > 0)
			AddClosest(retValue, board, tile.pos + new Point(1, -1), tile.height);
		else if (diff.x - diff.y == diff.x * 2 && diff.y < 0)
			AddClosest(retValue, board, tile.pos + new Point(-1, 1), tile.height);
		else if (diff.x + diff.y < 0 && diff.x - diff.y > 0 && diff.y < 0)
			AddClosest(retValue, board, tile.pos + new Point(0, 1), tile.height);
		else if (diff.x + diff.y > 0 && diff.x - diff.y < 0 && diff.y > 0)
			AddClosest(retValue, board, tile.pos + new Point(0, -1), tile.height);
		else if (diff.x + diff.y > 0 && diff.x - diff.y > 0 && diff.x > 0)
			AddClosest(retValue, board, tile.pos + new Point(-1, 0), tile.height);
		else if (diff.x + diff.y < 0 && diff.x - diff.y < 0 && diff.x < 0)
			AddClosest(retValue, board, tile.pos + new Point(1, 0), tile.height);

		return retValue;
	}

	void AddClosest (List<Tile> tiles, Board board, Point pos, float height)
	{
		Tile tile = board.GetClosestSelectableTile(pos, height);
		if (tile != null)
			tiles.Add(tile);
	}
}
