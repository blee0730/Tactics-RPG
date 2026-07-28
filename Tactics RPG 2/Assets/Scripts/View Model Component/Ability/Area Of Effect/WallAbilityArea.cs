using UnityEngine;
using System.Collections.Generic;

public class WallAbilityArea : AbilityArea
{
    public int length = 3;
    public bool perpendicularToUserFacing = true;

    public override List<Tile> GetTilesInArea(Board board, Point pos)
    {
        List<Tile> retValue = new List<Tile>();
        Unit user = GetComponentInParent<Unit>();
        Directions facing = user != null ? user.dir : Directions.North;

        Point axis;
        if (perpendicularToUserFacing)
        {
            axis = (facing == Directions.North || facing == Directions.South) ? new Point(1, 0) : new Point(0, 1);
        }
        else
        {
            axis = facing.GetNormal();
        }

        int half = length / 2;
        for (int i = -half; i <= half; ++i)
        {
            Tile tile = board.GetTile(pos + new Point(axis.x * i, axis.y * i));
            if (tile != null)
                retValue.Add(tile);
        }
        return retValue;
    }
}
