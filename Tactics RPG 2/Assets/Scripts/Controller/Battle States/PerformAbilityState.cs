using UnityEngine;
using System.Collections;

public class PerformAbilityState : BattleState 
{
	AbilityArea aa;
	public override void Enter()
	{
		base.Enter();
		aa = turn.ability.GetComponent<AbilityArea>();
		turn.hasUnitActed = true;
		if (turn.hasUnitMoved)
			turn.lockMove = true;
		if (aa.count > 1)
		{
			aa.tiles.Clear();
			aa.counter = aa.count;
		}
			StartCoroutine(Animate());
	}
	
	IEnumerator Animate ()
	{
		// TODO play animations, etc
		yield return null;
		ApplyAbility();
		
		if (IsBattleOver())
			owner.ChangeState<CutSceneState>();
		else if (!UnitHasControl())
			owner.ChangeState<SelectUnitState>();
		else if (turn.hasUnitMoved)
			owner.ChangeState<EndFacingState>();
		else
			owner.ChangeState<CommandSelectionState>();
	}
	
	void ApplyAbility ()
	{
		turn.ability.Perform(turn.targets);
	}
	
	bool UnitHasControl ()
	{
		return turn.actor.GetComponentInChildren<KnockOutStatusEffect>() == null;
	}
}