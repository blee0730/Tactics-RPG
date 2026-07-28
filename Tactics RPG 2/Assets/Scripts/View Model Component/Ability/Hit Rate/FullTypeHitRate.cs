using UnityEngine;
using System.Collections;

public class FullTypeHitRate : HitRate 
{
	public override bool IsAngleBased { get { return false; }}

	public override int Calculate (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Unit defender = target.content.GetComponent<Unit>();
		if (defender == null)
			return 0;

		if (AutomaticMiss(defender))
			return 0;

		return 100;
	}
}
