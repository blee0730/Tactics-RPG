using UnityEngine;
using System.Collections;

public class PerformAbilityState : BattleState
{
	public override void Enter()
	{
		base.Enter();

		// Consumes the current unit's action budget. The old code only set
		// hasUnitActed, which left actionsRemaining > 0 and could let a unit
		// act again after using an ability.
		turn.ConsumeAction();
		if (turn.hasUnitMoved)
			turn.lockMove = true;

		StartCoroutine(Animate());
	}
	
	IEnumerator Animate ()
	{
		// TODO play animations, etc
		yield return null;

		if (turn.ability != null)
		{
			turn.ability.Perform(turn.targets);
			TupleAbilityModifier.ClearActive(turn.ability);
		}

		if (IsBattleOver())
			owner.ChangeState<CutSceneState>();
		else if (!UnitHasControl())
			owner.ChangeState<SelectUnitState>();
		else if (turn.hasUnitMoved)
			owner.ChangeState<EndFacingState>();
		else
			owner.ChangeState<CommandSelectionState>();
	}
	
	bool UnitHasControl ()
	{
		if (turn.actor == null)
			return false;

		if (turn.actor.GetComponentInChildren<KnockOutStatusEffect>() != null)
			return false;

		Health health = turn.actor.GetComponent<Health>();
		if (health != null && health.HP <= health.MinHP)
			return false;

		return true;
	}
}
