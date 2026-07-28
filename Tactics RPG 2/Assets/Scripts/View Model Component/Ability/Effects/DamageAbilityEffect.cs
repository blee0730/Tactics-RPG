using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageAbilityEffect : BaseAbilityEffect 
{
	public const string WillApplyDamageNotification = "DamageAbilityEffect.WillApplyDamageNotification";
	public const string DamageAppliedNotification = "DamageAbilityEffect.DamageAppliedNotification";

	#region Public
	public override int Predict (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Unit attacker = GetComponentInParent<Unit>();
		Unit defender = target.content.GetComponent<Unit>();
		if (attacker == null || defender == null)
			return 0;

		// Get the attackers base attack stat considering
		// mission items, support check, status check, and equipment, etc
		int attack = GetStat(attacker, defender, GetAttackNotification, 0);

		// Get the targets base defense stat considering
		// mission items, support check, status check, and equipment, etc
		int defense = GetStat(attacker, defender, GetDefenseNotification, 0);

		// Calculate base damage
		int damage = attack - (defense / 2);
		damage = Mathf.Max(damage, 1);

		// Get the abilities power stat considering possible variations
		int power = GetStat(attacker, defender, GetPowerNotification, 0);

		// Apply power bonus
		damage = power * damage / 100;
		damage = Mathf.Max(damage, 1);

		// Tweak the damage based on a variety of other checks like
		// Elemental damage, Critical Hits, Damage multipliers, etc.
		damage = GetStat(attacker, defender, TweakDamageNotification, damage);

		// Clamp the damage to a range
		damage = Mathf.Clamp(damage, minDamage, maxDamage);
		return -damage;
	}
	
	protected override int OnApply (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Unit defender = target.content.GetComponent<Unit>();
		if (defender == null)
			return 0;

		// Start with the predicted damage value
		int value = Predict(target);

		// Add some random variance
		value = Mathf.FloorToInt(value * UnityEngine.Random.Range(0.9f, 1.1f));

		// Clamp the damage to a range
		value = Mathf.Clamp(value, minDamage, maxDamage);

		Unit attacker = GetComponentInParent<Unit>();

		// Give reactive statuses such as Counterattack a chance to cancel or modify
		// the damage before HP changes. Negative values are damage, positive values
		// are healing.
		DamageApplicationInfo info = new DamageApplicationInfo(attacker, defender, this, target, value);
		this.PostNotification(WillApplyDamageNotification, info);
		if (info.cancelDamage)
			return 0;
		value = info.damageAmount;

		// Apply the damage to the target
		Stats s = defender.GetComponent<Stats>();
		if (s != null)
		{
			if (value < 0)
			{
				LastDamageMemory memory = defender.GetComponent<LastDamageMemory>();
				if (memory == null)
					memory = defender.gameObject.AddComponent<LastDamageMemory>();
				memory.RecordBeforeDamage(attacker, value);
			}

			s[StatTypes.HP] += value;
			this.PostNotification(DamageAppliedNotification, new DamageInfo(attacker, defender, this, target, value));
		}
		return value;
	}
	#endregion
}
