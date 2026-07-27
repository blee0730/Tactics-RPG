using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AbilityArea : MonoBehaviour
{
<<<<<<< Updated upstream
<<<<<<< Updated upstream
	public abstract List<Tile> GetTilesInArea (Board board, Point pos);
}
=======
=======
>>>>>>> Stashed changes
	public int count = 1;
	public int counter = 1;
	public List<Tile> tiles;
	public abstract List<Tile> GetTilesInArea(Board board, Point pos);

	public virtual List<Tile> GetTilesInArea(Board board, Tile tile)
	{
		if (tile == null)
			return new List<Tile>();
		return GetTilesInArea(board, tile.pos);
	}
}
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
