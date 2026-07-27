using UnityEngine;
using System.Collections;

public class UntargetableStatusEffect : StatusEffect
{
	Unit owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		this.AddObserver(OnAutomaticMissCheck, HitRate.AutomaticMissCheckNotification);
	}

	void OnDisable ()
	{
		this.RemoveObserver(OnAutomaticMissCheck, HitRate.AutomaticMissCheckNotification);
	}

	void OnAutomaticMissCheck (object sender, object args)
	{
		MatchException exc = args as MatchException;
		if (exc == null || exc.target != owner)
			return;

		MonoBehaviour hitRate = sender as MonoBehaviour;
		Ability ability = hitRate != null ? hitRate.GetComponentInParent<Ability>() : null;
		AbilityMetadata metadata = ability != null ? ability.GetComponent<AbilityMetadata>() : null;
		if (metadata != null && metadata.canTargetUntargetable)
			return;

		if (exc.defaultToggle == false)
			exc.FlipToggle();
	}
}
