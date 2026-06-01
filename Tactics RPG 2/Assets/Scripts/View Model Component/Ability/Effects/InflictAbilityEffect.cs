using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class InflictAbilityEffect : BaseAbilityEffect 
{
	public string statusName;
	public int duration;
	public int baseChance = 100;

	private Ability ability;

    void Awake()
    {
        ability = GetComponentInParent<Ability>();
    }

    public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		Type statusType = Type.GetType(statusName);
		if (statusType == null || !statusType.IsSubclassOf(typeof(StatusEffect)))
		{
			Debug.LogError("Invalid Status Type");
			return 0;
		}

		MethodInfo mi = typeof(Status).GetMethod("Add");
		Type[] types = new Type[]{ statusType, typeof(DurationStatusCondition) };
		MethodInfo constructed = mi.MakeGenericMethod(types);

		Status status = target.content.GetComponent<Status>();

		int chance = baseChance;

		Unit attacker = GetComponentInParent<Unit>();

		Unit defender = target.content.GetComponent<Unit>();

		if (attacker != null)
		{
    		Stats atkStats = attacker.GetComponent<Stats>();

    		chance += atkStats[StatTypes.MAG] / 5;
		}

		if (defender != null)
		{
    		Stats defStats = defender.GetComponent<Stats>();

    		chance -= defStats[StatTypes.RES] / 5;
		}

		if (ability != null)
		{
    		AbilityMastery mastery = ability.GetComponent<AbilityMastery>();

    		if (mastery != null)
    		{
        		chance += mastery.level;
    		}
		}

		chance = Mathf.Clamp(chance, 5, 100);

		if (UnityEngine.Random.Range(0,100) >= chance)
		{
    		return 0;
		}

		object retValue = constructed.Invoke(status, null);

		DurationStatusCondition condition = retValue as DurationStatusCondition;

		condition.duration = duration;

		return 0;
	}
}