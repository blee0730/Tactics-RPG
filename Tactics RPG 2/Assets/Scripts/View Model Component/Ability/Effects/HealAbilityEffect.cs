using UnityEngine;
using System.Collections;

public class HealAbilityEffect : BaseAbilityEffect 
{
	public override int Predict (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Unit attacker = GetComponentInParent<Unit>();
		Unit defender = target.content.GetComponent<Unit>();
		if (attacker == null || defender == null)
			return 0;

		return GetStat(attacker, defender, GetPowerNotification, 0);
	}

	protected override int OnApply (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Unit defender = target.content.GetComponent<Unit>();
		if (defender == null)
			return 0;
		
		// Start with the predicted value
		int value = Predict(target);
		
		// Add some random variance
		value = Mathf.FloorToInt(value * UnityEngine.Random.Range(0.9f, 1.1f));
		
		// Clamp the amount to a range
		value = Mathf.Clamp(value, minDamage, maxDamage);
		
		// Apply the amount to the target
		Stats s = defender.GetComponent<Stats>();
		if (s != null)
			s[StatTypes.HP] += value;
		return value;
	}
}
