using UnityEngine;
using System.Collections.Generic;

public class WallAbilityArea : AbilityArea
{
<<<<<<< Updated upstream
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
=======
	public int length = 3;
	public bool perpendicularToUserFacing = true;

	public override List<Tile> GetTilesInArea (Board board, Point pos)
	{
		return GetTilesInArea(board, board.GetTile(pos));
	}

	public override List<Tile> GetTilesInArea (Board board, Tile center)
	{
		List<Tile> result = new List<Tile>();
		Unit user = GetComponentInParent<Unit>();
		if (center == null)
			return result;

		result.Add(center);
		Directions facing = user != null ? user.dir : Directions.North;
		Point axis;
		if (perpendicularToUserFacing)
			axis = (facing == Directions.North || facing == Directions.South) ? new Point(1, 0) : new Point(0, 1);
		else
			axis = facing.GetNormal();

		int radius = Mathf.Max(0, length / 2);
		for (int i = 1; i <= radius; ++i)
		{
			Tile a = board.GetClosestSelectableTile(center.pos + new Point(axis.x * i, axis.y * i), center.height);
			Tile b = board.GetClosestSelectableTile(center.pos + new Point(-axis.x * i, -axis.y * i), center.height);
			if (a != null)
				result.Add(a);
			if (b != null)
				result.Add(b);
		}
		return result;
	}
>>>>>>> Stashed changes
}
