using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class HealAbilityEffect : BaseAbilityEffect 
{
	private Ability ability;

    void Awake()
    {
        ability = GetComponentInParent<Ability>();
    }
	public override int Predict(Tile target)
{
    Unit attacker = GetComponentInParent<Unit>();
    Unit defender = target.content.GetComponent<Unit>();

    int value =
        GetStat(attacker, defender,
        GetPowerNotification, 0);

    if (ability != null)
    {
        AbilityMastery mastery =
            ability.GetComponent<AbilityMastery>();

        if (mastery != null)
        {
            value += mastery.level * 2;
        }
    }

    return value;
}

	protected override int OnApply (Tile target)
	{
		Unit defender = target.content.GetComponent<Unit>();
		
		// Start with the predicted value
		int value = Predict(target);
		
		// Add some random variance
		value = Mathf.FloorToInt(value * UnityEngine.Random.Range(0.9f, 1.1f));
		
		// Clamp the amount to a range
		value = Mathf.Clamp(value, minDamage, maxDamage);
		
		// Apply the amount to the target
		Unit healer = GetComponentInParent<Unit>();

		Stats healerStats = healer.GetOrAddComponent<Stats>();

		value += healerStats[StatTypes.MAG] /4;

		Stats s = defender.GetComponent<Stats>();

		s[StatTypes.HP] += value;

		return value;
	}
}