using UnityEngine;
using System.Collections;

public class MoveToTargetAbilityEffect : BaseAbilityEffect
{
	public bool requireEmptyTile = true;

	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		Unit mover = GetComponentInParent<Unit>();
		if (mover == null || target == null)
			return 0;

		if (requireEmptyTile && target.content != null)
			return 0;

		mover.Place(target);
		mover.Match();
		return 0;
	}
}
