using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConfirmAbilityTargetState : BattleState
{
	List<Tile> tiles;
	AbilityArea aa;
	int index = 0;

	public override void Enter ()
	{
		base.Enter ();
		aa = turn.ability.GetComponent<AbilityArea>();
<<<<<<< Updated upstream
		tiles = aa.GetTilesInArea(board, pos);
=======
		attacker = aa != null ? aa.GetComponentInParent<Unit>() : null;
		if (aa == null)
		{
			tiles = new List<Tile>();
			turn.targets = new List<Tile>();
			statPanelController.HideSecondary();
			hitSuccessIndicator.Hide();
			return;
		}
		tiles = aa.GetTilesInArea(board, owner.currentTile);
		if (tiles == null)
			tiles = aa.tiles;
		if (tiles == null)
			tiles = new List<Tile>();
		tiles.RemoveAll(t => t == null);
>>>>>>> Stashed changes
		board.SelectTiles(tiles);
		FindTargets();
		statPanelController.ShowPrimary(turn.actor.gameObject);
		if (turn.targets.Count > 0)
		{
			if (driver.Current == Drivers.Human)
				hitSuccessIndicator.Show();
			SetTarget(0);
		}
		else
		{
			statPanelController.HideSecondary();
			hitSuccessIndicator.Hide();
			if (driver.Current == Drivers.Human)
				StartCoroutine(RejectInvalidTargetSelection());
		}
		if (driver.Current == Drivers.Computer)
			StartCoroutine(ComputerDisplayAbilitySelection());
	}

	public override void Exit ()
	{
		base.Exit ();
		board.DeSelectTiles(tiles);
		statPanelController.HidePrimary();
		statPanelController.HideSecondary();
		hitSuccessIndicator.Hide();
	}

	protected override void OnMove (object sender, InfoEventArgs<Point> e)
	{
		if (e.info.y > 0 || e.info.x > 0)
			SetTarget(index + 1);
		else
			SetTarget(index - 1);
	}

	protected override void OnFire (object sender, InfoEventArgs<int> e)
	{
		if (e.info == 0)
		{
			if (turn.targets.Count > 0)
				owner.ChangeState<PerformAbilityState>();
		}
		else
			owner.ChangeState<AbilityTargetState>();
	}


	IEnumerator RejectInvalidTargetSelection ()
	{
		if (owner.battleMessageController != null)
			owner.battleMessageController.Display("No valid targets");

		yield return new WaitForSeconds(0.35f);

		// Stay in the selected ability's targeting flow instead of leaving the
		// player at an empty confirm state where clicking does nothing.
		if (owner.CurrentState == this)
		{
			if (aa != null)
				aa.counter = Mathf.Max(1, aa.count);
			owner.ChangeState<AbilityTargetState>();
		}
	}

	void FindTargets ()
	{
		turn.targets = new List<Tile>();
		for (int i = 0; i < tiles.Count; ++i)
			if (tiles[i] != null && turn.ability.IsTarget(tiles[i]))
				turn.targets.Add(tiles[i]);
	}

	void SetTarget (int target)
	{
		if (turn.targets == null || turn.targets.Count == 0)
			return;

		index = target;
		if (index < 0)
			index = turn.targets.Count - 1;
		if (index >= turn.targets.Count)
			index = 0;

<<<<<<< Updated upstream
		if (turn.targets.Count > 0)
		{
			RefreshSecondaryStatPanel(turn.targets[index].pos);
			UpdateHitSuccessIndicator ();
		}
	}

=======
		Tile selected = turn.targets[index];
		Unit selectedUnit = selected != null && selected.content != null ? selected.content.GetComponent<Unit>() : null;
		if (selectedUnit != null && selectedUnit != turn.actor)
			RefreshSecondaryStatPanel(selected.pos);
		else
			statPanelController.HideSecondary();

		UpdateHitSuccessIndicator();
	}
	
>>>>>>> Stashed changes
	void UpdateHitSuccessIndicator ()
	{
		if (turn.targets == null || turn.targets.Count == 0 || index < 0 || index >= turn.targets.Count)
			return;

		int chance = 0;
		int amount = 0;
		Tile target = turn.targets[index];

		Transform obj = turn.ability.transform;
		for (int i = 0; i < obj.childCount; ++i)
		{
			AbilityEffectTarget targeter = obj.GetChild(i).GetComponent<AbilityEffectTarget>();
			if (targeter == null || !targeter.IsTarget(target))
				continue;

			HitRate hitRate = targeter.GetComponent<HitRate>();
			if (hitRate != null)
				chance = hitRate.Calculate(target);

			BaseAbilityEffect effect = targeter.GetComponent<BaseAbilityEffect>();
			if (effect != null)
				amount = effect.Predict(target);
			break;
		}

		hitSuccessIndicator.SetStats(chance, amount);
	}

	IEnumerator ComputerDisplayAbilitySelection ()
	{
		owner.battleMessageController.Display(turn.ability.name);
		yield return new WaitForSeconds (2f);
		owner.ChangeState<PerformAbilityState>();
	}
}
