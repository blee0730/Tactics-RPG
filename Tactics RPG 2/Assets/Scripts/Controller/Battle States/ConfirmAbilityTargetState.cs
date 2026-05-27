using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConfirmAbilityTargetState : BattleState
{
	Unit attacker;
	List<Tile> tiles;
	AbilityArea aa;
	int index = 0;

	public override void Enter ()
	{
		base.Enter ();
		aa = turn.ability.GetComponent<AbilityArea>();
		attacker = aa.GetComponentInParent<Unit>();
		aa.dir = attacker.dir;
		if(aa.displace == true && aa.direction == true)
        {
            switch(attacker.dir)
            {
				case Directions.North:
					aa.dir = Directions.South;
					break;
				case Directions.East:
					aa.dir = Directions.West;
					break;
				case Directions.South:
					aa.dir = Directions.North;
					break;
				default: //West
					aa.dir = Directions.East;
					break;
            }
        }
		tiles = aa.GetTilesInArea(board, pos);
		if (tiles == null)
			tiles = aa.tiles;
		board.SelectTiles(tiles);
		FindTargets();
		RefreshPrimaryStatPanel(turn.actor.tile.pos);
		if (turn.targets.Count > 0)
		{
			if (driver.Current == Drivers.Human)
				hitSuccessIndicator.Show();
			SetTarget(0);
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
		if (aa.displace == true && aa.direction == true)
			ChangeDirection(e.info);
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
			{
				owner.ChangeState<PerformAbilityState>();
			}
		}
		else
		{
			owner.ChangeState<AbilityTargetState>();
			aa.counter = aa.count;
		}
	}

	void FindTargets ()
	{
		turn.targets = new List<Tile>();
		for (int i = 0; i < tiles.Count; ++i)
			if (turn.ability.IsTarget(tiles[i]))
				turn.targets.Add(tiles[i]);
	}

	void SetTarget(int target)
	{
		index = target;
		if (index < 0)
			index = turn.targets.Count - 1;
		if (index >= turn.targets.Count)
			index = 0;

		if (turn.targets.Count > 0)
		{
			RefreshSecondaryStatPanel(turn.targets[index].pos);
			UpdateHitSuccessIndicator();
		}
	}
	
	void ChangeDirection(Point p)
    {
		Directions dir = p.GetDirection();
		if (dir == Directions.North && dir != attacker.dir)
			aa.dir = dir;
		if (dir == Directions.East && dir != attacker.dir)
			aa.dir = dir;
		if (dir == Directions.South && dir != attacker.dir)
			aa.dir = dir;
		if (dir == Directions.West && dir != attacker.dir)
			aa.dir = dir;
    }

	void UpdateHitSuccessIndicator ()
	{
		int chance = 0;
		int amount = 0;
		Tile target = turn.targets[index];

		Transform obj = turn.ability.transform;
		for (int i = 0; i < obj.childCount; ++i)
		{
			AbilityEffectTarget targeter = obj.GetChild(i).GetComponent<AbilityEffectTarget>();
			if (targeter.IsTarget(target))
			{
				HitRate hitRate = targeter.GetComponent<HitRate>();
				chance = hitRate.Calculate(target);

				BaseAbilityEffect effect = targeter.GetComponent<BaseAbilityEffect>();
				amount = effect.Predict(target);
				break;
			}
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