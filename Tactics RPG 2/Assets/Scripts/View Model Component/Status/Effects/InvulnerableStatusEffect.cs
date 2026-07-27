using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InvulnerableStatusEffect : StatusEffect
{
	Unit owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		this.AddObserver(OnTweakDamage, BaseAbilityEffect.TweakDamageNotification);
	}

	void OnDisable ()
	{
		this.RemoveObserver(OnTweakDamage, BaseAbilityEffect.TweakDamageNotification);
	}

	void OnTweakDamage (object sender, object args)
	{
		Info<Unit, Unit, List<ValueModifier>> info = args as Info<Unit, Unit, List<ValueModifier>>;
		if (info == null || info.arg1 != owner)
			return;

		MonoBehaviour effect = sender as MonoBehaviour;
		Ability ability = effect != null ? effect.GetComponentInParent<Ability>() : null;
		AbilityMetadata metadata = ability != null ? ability.GetComponent<AbilityMetadata>() : null;
		if (metadata != null && metadata.ignoresInvulnerability)
			return;

		info.arg2.Add(new MultValueModifier(1000, 0));
	}
}
