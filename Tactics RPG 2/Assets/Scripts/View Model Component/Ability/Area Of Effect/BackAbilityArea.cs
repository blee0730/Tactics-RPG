using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

public class BackAbilityArea : AbilityArea 
{
	public override List<Tile> GetTilesInArea(Board board, Point pos)
	{
		Unit attacker = GetComponentInParent<Unit>();
		List<Tile> retValue = new List<Tile>();
		Tile tile = board.GetTile(pos);
		if (attacker.dir == Directions.North && tile != null)
			retValue.Add(board.GetTile(pos + new Point(0, -2)));
		if (attacker.dir == Directions.East && tile != null)
			retValue.Add(board.GetTile(pos + new Point(-2, 0)));
		if (attacker.dir == Directions.South && tile != null)
			retValue.Add(board.GetTile(pos + new Point(0, 2)));
		if (attacker.dir == Directions.West && tile != null)
			retValue.Add(board.GetTile(pos + new Point(2, 0)));
		if (tile != null)
			retValue.Add(tile);
		return retValue;
	}
}