using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityTargetState : BattleState 
{
	List<Tile> tiles;
	AbilityRange ar;
<<<<<<< Updated upstream
=======
	AbilityArea aa;
	PathAbilityArea pathArea;
	TerraformAbilityArea terraformArea;
>>>>>>> Stashed changes
	
	public override void Enter ()
	{
		base.Enter ();
		ar = turn.ability.GetComponent<AbilityRange>();
<<<<<<< Updated upstream
		SelectTiles ();
=======
		aa = turn.ability.GetComponent<AbilityArea>();
		pathArea = aa as PathAbilityArea;
		terraformArea = aa as TerraformAbilityArea;
		ResetAreaStateForNewTargeting();
		SelectTiles();
>>>>>>> Stashed changes
		statPanelController.ShowPrimary(turn.actor.gameObject);
		RefreshDirectionalTargetSelection();
		if (driver.Current == Drivers.Computer)
			StartCoroutine(ComputerHighlightTarget());
	}
	
	public override void Exit ()
	{
		base.Exit ();
		board.DeSelectTiles(tiles);
		statPanelController.HidePrimary();
		statPanelController.HideSecondary();
	}
	
	protected override void OnMove (object sender, InfoEventArgs<Point> e)
	{
		if (pathArea != null || terraformArea != null)
		{
			SelectTile(e.info + pos, tiles);
			RefreshSecondaryStatPanel(pos);
			return;
		}

		if (ar.directionOriented)
		{
			ChangeDirection(e.info);
		}
		else
		{
			SelectTile(e.info + pos, tiles);
			RefreshSecondaryStatPanel(pos);
		}
	}

	protected override void OnCycleLayer (object sender, InfoEventArgs<int> e)
	{
		if (ar != null && ar.directionOriented)
			return;

		CycleTileLayer(e.info, tiles);
		RefreshSecondaryStatPanel(pos);
	}
	
	protected override void OnFire (object sender, InfoEventArgs<int> e)
	{
		if (pathArea != null)
		{
			HandlePathFire(e.info);
			return;
		}

		if (terraformArea != null)
		{
			HandleTerraformFire(e.info);
			return;
		}

		if (e.info == 0)
		{
<<<<<<< Updated upstream
			if (ar.directionOriented || tiles.Contains(board.GetTile(pos)))
				owner.ChangeState<ConfirmAbilityTargetState>();
		}
		else
		{
			owner.ChangeState<CategorySelectionState>();
=======
			Tile selectedTile = ar.directionOriented ? GetDirectionalTargetTile() : owner.currentTile;
			if (selectedTile == null)
				return;

			bool validSelection = ar.directionOriented || tiles.Contains(selectedTile);
			if (aa.counter >= 1 && validSelection)
			{
				if (aa.tiles == null)
					aa.tiles = new List<Tile>();

				pos = selectedTile.pos;
				aa.counter--;
				aa.tiles.Add(selectedTile);
				if (aa.counter == 0)
					owner.ChangeState<ConfirmAbilityTargetState>();
			}
		}
		else
		{
			ResetAreaStateForCancel();
			TupleAbilityModifier.ClearActive(turn.ability);
			owner.ChangeState<ActionSelectionState>();
		}
	}

	void HandlePathFire(int button)
	{
		if (button == 0)
		{
			Tile selectedTile = owner.currentTile;
			if (pathArea.TryAddTile(board, turn.actor, selectedTile))
			{
				board.DeSelectTiles(tiles);
				SelectTiles();
			}
		}
		else if (button == 1)
		{
			if (pathArea.tiles != null && pathArea.tiles.Count > 0)
			{
				pathArea.RemoveLast();
				board.DeSelectTiles(tiles);
				SelectTiles();
			}
			else
			{
				ResetAreaStateForCancel();
				TupleAbilityModifier.ClearActive(turn.ability);
				owner.ChangeState<ActionSelectionState>();
			}
		}
		else if (button == 2)
		{
			if (pathArea.CanFinish(turn.actor))
				owner.ChangeState<ConfirmAbilityTargetState>();
		}
	}

	void HandleTerraformFire(int button)
	{
		if (button == 0 || button == 2)
		{
			Tile selectedTile = owner.currentTile;
			int op = button == 0 ? 1 : -1;
			if (terraformArea.AddOperation(turn.actor, selectedTile, op))
			{
				board.DeSelectTiles(tiles);
				SelectTiles();
			}
		}
		else if (button == 1)
		{
			if (terraformArea.CanFinish())
				owner.ChangeState<ConfirmAbilityTargetState>();
			else
			{
				ResetAreaStateForCancel();
				TupleAbilityModifier.ClearActive(turn.ability);
				owner.ChangeState<ActionSelectionState>();
			}
>>>>>>> Stashed changes
		}
	}
	

	void ResetAreaStateForNewTargeting ()
	{
		if (aa == null)
			return;

		// AbilityArea lives on the runtime ability object, so values like counter
		// persist after a cast. If counter was left at 0 by the previous use, the
		// range still highlights but left-click cannot select/fire anything. Reset
		// it every time this targeting state opens.
		aa.counter = Mathf.Max(1, aa.count);

		if (aa.tiles == null)
			aa.tiles = new List<Tile>();
		else
			aa.tiles.Clear();

		if (pathArea != null)
			pathArea.ResetPath();
		if (terraformArea != null)
			terraformArea.ResetOperations();
	}

	void ResetAreaStateForCancel ()
	{
		if (aa == null)
			return;

		aa.counter = Mathf.Max(1, aa.count);
		if (aa.tiles != null)
			aa.tiles.Clear();
		if (terraformArea != null && terraformArea.operations != null)
			terraformArea.operations.Clear();
	}

	void ChangeDirection (Point p)
	{
		Directions dir = p.GetDirection();
		if (turn.actor.dir != dir)
		{
			board.DeSelectTiles(tiles);
			turn.actor.dir = dir;
			turn.actor.Match();
			SelectTiles ();
			RefreshDirectionalTargetSelection();
		}
	}
	
	void SelectTiles ()
	{
		if (pathArea != null)
			tiles = pathArea.GetSelectableTiles(board, turn.actor);
		else if (terraformArea != null)
			tiles = terraformArea.GetSelectableTiles(board, turn.actor);
		else if (ar != null)
			tiles = ar.GetTilesInRange(board);
		else
			tiles = new List<Tile>();

		if (tiles == null)
			tiles = new List<Tile>();
		board.SelectTiles(tiles);
	}

	void RefreshDirectionalTargetSelection ()
	{
		if (ar == null || !ar.directionOriented)
			return;

		Tile targetTile = GetDirectionalTargetTile();
		if (targetTile != null)
		{
			SelectTile(targetTile);
			RefreshSecondaryStatPanel(pos);
		}
		else
		{
			statPanelController.HideSecondary();
		}
	}

	Tile GetDirectionalTargetTile ()
	{
		if (tiles == null || tiles.Count == 0)
			return null;

		for (int i = 0; i < tiles.Count; ++i)
		{
			Tile tile = tiles[i];
			if (tile != null)
				return tile;
		}
		return null;
	}

	IEnumerator ComputerHighlightTarget ()
	{
		if (ar.directionOriented)
		{
			ChangeDirection(turn.plan.attackDirection.GetNormal());
			yield return new WaitForSeconds(0.25f);
		}
		else
		{
			Point cursorPos = pos;
			while (cursorPos != turn.plan.fireLocation)
			{
				if (cursorPos.x < turn.plan.fireLocation.x) cursorPos.x++;
				if (cursorPos.x > turn.plan.fireLocation.x) cursorPos.x--;
				if (cursorPos.y < turn.plan.fireLocation.y) cursorPos.y++;
				if (cursorPos.y > turn.plan.fireLocation.y) cursorPos.y--;
				SelectTile(cursorPos);
				yield return new WaitForSeconds(0.25f);
			}
		}
		yield return new WaitForSeconds(0.5f);
		owner.ChangeState<ConfirmAbilityTargetState>();
	}
}
