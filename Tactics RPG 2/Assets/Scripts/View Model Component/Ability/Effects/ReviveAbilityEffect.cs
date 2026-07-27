using UnityEngine;
using System.Collections;

public class ReviveAbilityEffect : BaseAbilityEffect 
{
	public float percent;

	public override int Predict (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Stats s = target.content.GetComponent<Stats>();
		return s != null ? Mathf.FloorToInt(s[StatTypes.MHP] * percent) : 0;
	}

	protected override int OnApply (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Stats s = target.content.GetComponent<Stats>();
		if (s == null)
			return 0;

		int value = s[StatTypes.HP] = Predict(target);
		return value;
	}
}
