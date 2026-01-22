using UnityEngine;
using System.Collections;

public class TileTypeHitRate : HitRate 
{
	public override bool IsAngleBased { get { return false; }}

	public override int Calculate (Tile target)
	{
		return 100;
	}
}