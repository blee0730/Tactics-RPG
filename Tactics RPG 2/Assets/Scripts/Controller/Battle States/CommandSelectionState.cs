using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandSelectionState : BaseAbilityMenuState 
{
	public override void Enter ()
	{
		base.Enter ();
		statPanelController.ShowPrimary(turn.actor.gameObject);
		if (driver.Current == Drivers.Computer)
			StartCoroutine( ComputerTurn() );
	}

	public override void Exit ()
	{
		base.Exit ();
		statPanelController.HidePrimary();
	}

	protected override void LoadMenu ()
	{
		if (menuOptions == null)
		{
			menuTitle = "Commands";
			menuOptions = new List<string>(3);
			menuOptions.Add("Move");
			menuOptions.Add("Action");
			menuOptions.Add("Wait");
		}

		abilityMenuPanelController.Show(menuTitle, menuOptions);
		abilityMenuPanelController.SetLocked(0, !turn.CanMove() || turn.actor.cantMove);
		abilityMenuPanelController.SetLocked(1, !turn.CanAct() || turn.actor.cantAct);
	}

	protected override void Confirm ()
	{
		switch (abilityMenuPanelController.selection)
		{
		case 0: // Move
			if (turn.CanMove() && !turn.actor.cantMove)
				owner.ChangeState<MoveTargetState>();
			break;
		case 1: // Action
			if (turn.CanAct() && !turn.actor.cantAct)
				owner.ChangeState<CategorySelectionState>();
			break;
		case 2: // Wait
			owner.ChangeState<EndFacingState>();
			break;
		}
	}

	protected override void Cancel ()
	{
		if (turn.hasUnitMoved && !turn.lockMove)
		{
			turn.UndoMove();
			abilityMenuPanelController.SetLocked(0, false);
			SelectTile(turn.actor.tile);
		}
		else
		{
			owner.ChangeState<ExploreState>();
		}
	}

	IEnumerator ComputerTurn ()
	{
		if (turn.plan == null)
		{
			turn.plan = owner.cpu.Evaluate();
			turn.ability = turn.plan.ability;
		}

		yield return new WaitForSeconds (1f);

		if (turn.CanMove() && !turn.actor.cantMove && turn.plan.moveLocation != turn.actor.tile.pos)
			owner.ChangeState<MoveTargetState>();
<<<<<<< Updated upstream
		else if (turn.hasUnitActed == false && turn.plan.ability != null)
=======
		else if (turn.CanAct() && !turn.actor.cantAct && turn.plan.ability != null)
>>>>>>> Stashed changes
			owner.ChangeState<AbilityTargetState>();
		else
			owner.ChangeState<EndFacingState>();
	}
}
