using UnityEngine;
using System.Collections;

public class ApplyDoTAbilityEffect : BaseAbilityEffect
{
	public int duration = 3;
	public float percentOfMaxHP = 0.1f;
	public int minimumDamage = 1;
	public bool canKnockOut = true;

	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Status status = target.content.GetComponentInChildren<Status>();
		if (status == null)
			return 0;

		DurationStatusCondition condition = status.Add<DoTStatusEffect, DurationStatusCondition>();
		condition.duration = duration;
		DoTStatusEffect effect = condition.GetComponentInParent<DoTStatusEffect>();
		if (effect != null)
		{
			effect.percentOfMaxHP = percentOfMaxHP;
			effect.minimumDamage = minimumDamage;
			effect.canKnockOut = canKnockOut;
		}
		return 0;
	}
}
