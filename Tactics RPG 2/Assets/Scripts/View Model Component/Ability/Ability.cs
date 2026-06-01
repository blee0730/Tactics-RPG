using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ability : MonoBehaviour 
{
	public AbilityData data;
	public List<AbilityEvolution> evolutions = new List<AbilityEvolution>();
	public const string CanPerformCheck = "Ability.CanPerformCheck";
	public const string FailedNotification = "Ability.FailedNotification";
	public const string DidPerformNotification = "Ability.DidPerformNotification";

	public int MasteryLevel
	{
    	get
    	{
        	AbilityMastery mastery = GetComponent<AbilityMastery>();

        	return mastery != null ? mastery.level : 0;
    	}
	}
	public bool CanPerform ()
	{
		BaseException exc = new BaseException(true);
		this.PostNotification(CanPerformCheck, exc);
		return exc.toggle;
	}

	public void Perform (List<Tile> targets)
	{
		if (!CanPerform())
		{
			this.PostNotification(FailedNotification);
			return;
		}

		for (int i = 0; i < targets.Count; ++i)
			Perform(targets[i]);

		AbilityMastery mastery = GetComponent<AbilityMastery>();

		if (mastery != null)
		{
    		mastery.RegisterUse();
		}

		this.PostNotification("AbilityUsed", this);

		this.PostNotification(DidPerformNotification);
	}

	public bool IsTarget (Tile tile)
	{
		Transform obj = transform;
		for (int i = 0; i < obj.childCount; ++i)
		{
			AbilityEffectTarget targeter = obj.GetChild(i).GetComponent<AbilityEffectTarget>();
			if (targeter.IsTarget(tile))
				return true;
		}
		return false;
	}

	private BaseAbilityEffect[] cachedEffects;

	void Awake()
	{
    	cachedEffects = GetComponentsInChildren<BaseAbilityEffect>();
	}

	void Perform(Tile target)
	{
    	for (int i = 0; i < cachedEffects.Length; i++)
    	{
       		cachedEffects[i].Apply(target);
    	}
	}
}