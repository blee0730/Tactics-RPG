using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpinAbilityRange : AbilityRange 
{
	public override List<Tile> GetTilesInRange (Board board)
	{
		Point centerPos = unit.tile.pos;
		List<Tile> retValue = new List<Tile>
        {
            board.GetTile(centerPos + new Point(0, 1)),
            board.GetTile(centerPos + new Point(0, -1)),
            board.GetTile(centerPos + new Point(-1, 0)),
            board.GetTile(centerPos + new Point(1, 0)),
            board.GetTile(centerPos + new Point(1, 1)),
            board.GetTile(centerPos + new Point(1, -1)),
            board.GetTile(centerPos + new Point(-1, 1)),
            board.GetTile(centerPos + new Point(-1, -1))
        };

        return retValue;
	}
}