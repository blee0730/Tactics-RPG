using UnityEngine;
using System.Collections;

public class ManaRestoreAbilityEffect : BaseAbilityEffect
{
	public int amount = 10;
	public float percentOfMaxMP = 0f;

	public override int Predict (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Stats stats = target.content.GetComponent<Stats>();
		if (stats == null)
			return 0;

		int value = amount;
		if (percentOfMaxMP > 0f)
			value += Mathf.FloorToInt(stats[StatTypes.MMP] * percentOfMaxMP);
		return Mathf.Max(0, value);
	}

	protected override int OnApply (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Stats stats = target.content.GetComponent<Stats>();
		if (stats == null)
			return 0;

		int value = Predict(target);
		stats[StatTypes.MP] += value;
		return value;
	}
}
