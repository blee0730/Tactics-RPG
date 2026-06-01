using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class SpawnObjectAbilityEffect : BaseAbilityEffect 
{
	public float height;
	public GameObject prefab;
	public SpawnRequirement requirement;

	public enum SpawnRequirement
	{
		None,
		Flammable,
		Wet,
		SummonOnly
	}

	public override int Predict(Tile target)
	{
		return 0;
	}

	protected override int OnApply(Tile target)
	{
		Ability ability = GetComponentInParent<Ability>();

		bool canSpawn = false;

		switch(requirement)
		{
    		case SpawnRequirement.None:
        		canSpawn = true;
        		break;

    		case SpawnRequirement.Flammable:
        		canSpawn = target.isFlammable;
        		break;

    		case SpawnRequirement.Wet:
        		canSpawn = target.isWet;
        		break;

    		case SpawnRequirement.SummonOnly:
        		canSpawn = true;
        		break;
		}

		if(canSpawn)
		{
			Vector3 position = target.center + new Vector3(0, height, 0);
			Quaternion rotation = target.transform.rotation;
			GameObject instance = Instantiate(prefab, position, rotation);

			Summon summon = instance.GetComponent<Summon>();

			if (summon != null)
    			summon.owner = GetComponentInParent<Unit>();
	
			Unit summoned = instance.GetComponent<Unit>();

			if (summoned != null && ability != null)
			{
    			AbilityMastery mastery = ability.GetComponent<AbilityMastery>();

    			if (mastery != null)
    			{
        			Stats stats = summoned.GetComponent<Stats>();

        			stats[StatTypes.MHP] += mastery.level * 10;
    			}
			}
		}
		return 0;
	}
}