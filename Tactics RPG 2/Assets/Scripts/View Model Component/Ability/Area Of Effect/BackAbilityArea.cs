using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackAbilityArea : AbilityArea 
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

		if (attacker.dir == Directions.North)
			AddClosest(retValue, board, tile.pos + new Point(0, -2), tile.height);
		if (attacker.dir == Directions.East)
			AddClosest(retValue, board, tile.pos + new Point(-2, 0), tile.height);
		if (attacker.dir == Directions.South)
			AddClosest(retValue, board, tile.pos + new Point(0, 2), tile.height);
		if (attacker.dir == Directions.West)
			AddClosest(retValue, board, tile.pos + new Point(2, 0), tile.height);

		retValue.Add(tile);
		return retValue;
	}

	void AddClosest (List<Tile> tiles, Board board, Point pos, float height)
	{
		Tile tile = board.GetClosestSelectableTile(pos, height);
		if (tile != null)
			tiles.Add(tile);
	}
}
