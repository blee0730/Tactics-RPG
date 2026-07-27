using UnityEngine;
using System.Collections;

public class SwapPositionsAbilityEffect : BaseAbilityEffect
{
	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		Unit user = GetComponentInParent<Unit>();
		Unit other = target != null && target.content != null ? target.content.GetComponent<Unit>() : null;
		if (user == null || other == null || user.tile == null || other.tile == null)
			return 0;

		Tile a = user.tile;
		Tile b = other.tile;
		user.Place(b);
		other.Place(a);
		user.Match();
		other.Match();
		return 0;
	}
}
