using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

public class PiercingAbilityArea : AbilityArea 
{
	public override List<Tile> GetTilesInArea(Board board, Point pos)
	{
		Unit attacker = GetComponentInParent<Unit>();
		Tile start = attacker.tile;
		List<Tile> retValue = new List<Tile>();
		Tile tile = board.GetTile(pos);
		Point diff = start.pos - tile.pos;
		if (tile != null)
			retValue.Add(tile);
		if (diff.x - diff.y == 0 && diff.y < 0 && board.GetTile(tile.pos + new Point(1, 1)) != null)
			retValue.Add(board.GetTile(tile.pos + new Point(1, 1)));
		else if (diff.x - diff.y == 0 && diff.y > 0 && board.GetTile(tile.pos + new Point(-1, -1)) != null)
			retValue.Add(board.GetTile(tile.pos + new Point(-1, -1)));
		else if (diff.x - diff.y == diff.x * 2 && diff.y > 0 && board.GetTile(tile.pos + new Point(1, -1)) != null)
			retValue.Add(board.GetTile(tile.pos + new Point(1, -1)));
		else if (diff.x - diff.y == diff.x * 2 && diff.y < 0 && board.GetTile(tile.pos + new Point(-1, 1)) != null)
			retValue.Add(board.GetTile(tile.pos + new Point(-1, 1)));
		else if (diff.x + diff.y < 0 && diff.x - diff.y > 0 && diff.y < 0 && board.GetTile(tile.pos + new Point(0, 1)) != null)
			retValue.Add(board.GetTile(tile.pos + new Point(0, 1)));
		else if (diff.x + diff.y > 0 && diff.x - diff.y < 0 && diff.y > 0 && board.GetTile(tile.pos + new Point(0, -1)) != null)
			retValue.Add(board.GetTile(tile.pos + new Point(0, -1)));
		else if (diff.x + diff.y > 0 && diff.x - diff.y > 0 && diff.x > 0 && board.GetTile(tile.pos + new Point(-1, 0)) != null)
			retValue.Add(board.GetTile(tile.pos + new Point(-1, 0)));
		else if (diff.x + diff.y < 0 &&diff.x - diff.y < 0 && diff.x < 0 && board.GetTile(tile.pos + new Point(1, 0)) != null)
			retValue.Add(board.GetTile(tile.pos + new Point(1, 0)));
		return retValue;
	}
}