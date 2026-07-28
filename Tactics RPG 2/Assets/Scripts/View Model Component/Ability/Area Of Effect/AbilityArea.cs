using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AbilityArea : MonoBehaviour
{
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
