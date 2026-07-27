using UnityEngine;
using System.Collections;

public class ApplyTemporaryStatModifierAbilityEffect : BaseAbilityEffect
{
	public int duration = 3;
	public StatTypes statType = StatTypes.STR;
	public float multiplier = 1f;
	public int flatBonus = 0;
	public int sortOrder = 100;

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

		DurationStatusCondition condition = status.Add<TemporaryStatModifierStatusEffect, DurationStatusCondition>();
		condition.duration = duration;
		TemporaryStatModifierStatusEffect effect = condition.GetComponentInParent<TemporaryStatModifierStatusEffect>();
		if (effect != null)
		{
			effect.statType = statType;
			effect.multiplier = multiplier;
			effect.flatBonus = flatBonus;
			effect.sortOrder = sortOrder;
		}
		return 0;
	}
}
