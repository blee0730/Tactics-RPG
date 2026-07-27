using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CureAbilityEffect : BaseAbilityEffect 
{
	static HashSet<Type> CurableTypes
	{
		get
		{
			if (_curableTypes == null)
			{
				_curableTypes = new HashSet<Type>();
				_curableTypes.Add( typeof(PoisonStatusEffect) );
				_curableTypes.Add( typeof(BlindStatusEffect) );
				_curableTypes.Add( typeof(SlowStatusEffect) );
				_curableTypes.Add( typeof(StopStatusEffect) );
				_curableTypes.Add( typeof(SilenceStatusEffect) );
				_curableTypes.Add( typeof(MovementLockStatusEffect) );
				_curableTypes.Add( typeof(ActionLockStatusEffect) );
			}
			return _curableTypes;
		}
	}
	static HashSet<Type> _curableTypes;

	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Unit defender = target.content.GetComponent<Unit>();
		if (defender == null)
			return 0;

		Status status = defender.GetComponentInChildren<Status>();
		if (status == null)
			return 0;

		DurationStatusCondition[] candidates = status.GetComponentsInChildren<DurationStatusCondition>();
		for (int i = candidates.Length - 1; i >= 0; --i)
		{
			StatusEffect effect = candidates[i].GetComponentInParent<StatusEffect>();
			if (effect != null && CurableTypes.Contains( effect.GetType() ))
				candidates[i].Remove();
		}
		return 0;
	}
}
