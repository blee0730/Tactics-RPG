using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelData : ScriptableObject 
{
	public List<Vector3> tiles;
	public List<Tile.TileType> tileTypes;
	public List<bool> topTiles;
	public List<bool> splitTops;
}
