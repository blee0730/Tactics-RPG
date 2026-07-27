using UnityEngine;
using System.Collections;

public class SilenceStatusEffect : StatusEffect
{
	Unit owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		this.AddObserver(OnCanPerformCheck, Ability.CanPerformCheck);
	}

	void OnDisable ()
	{
		this.RemoveObserver(OnCanPerformCheck, Ability.CanPerformCheck);
	}

	void OnCanPerformCheck (object sender, object args)
	{
		Ability ability = sender as Ability;
		BaseException exc = args as BaseException;
<<<<<<< Updated upstream
		if (ability == null || exc == null || owner == null || exc.toggle == false)
=======
		if (ability == null || exc == null || owner == null)
>>>>>>> Stashed changes
			return;

		Unit user = ability.GetComponentInParent<Unit>();
		if (user != owner)
			return;

<<<<<<< Updated upstream
		if (IsBlockedBySilence(ability))
			exc.FlipToggle();
	}

	bool IsBlockedBySilence (Ability ability)
	{
		AbilityMetadata metadata = ability.GetComponent<AbilityMetadata>();
		if (metadata != null)
		{
			if (metadata.blockedBySilence)
				return true;
			if (metadata.sourceType == AbilitySourceTypes.Magical)
				return true;
		}

		// Fallback rule for the current project: any ability that uses
		// MagicalAbilityPower is treated as a magic ability, even if the prefab
		// does not have AbilityMetadata yet. This avoids needing to touch every
		// magic prefab by hand.
		return ability.GetComponentInChildren<MagicalAbilityPower>(true) != null;
	}
=======
		AbilityMetadata metadata = ability.GetComponent<AbilityMetadata>();
		if (metadata != null && metadata.blockedBySilence && exc.defaultToggle == true)
			exc.FlipToggle();
	}
>>>>>>> Stashed changes
}
