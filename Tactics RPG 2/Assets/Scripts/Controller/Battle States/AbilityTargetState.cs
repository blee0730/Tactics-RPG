using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityTargetState : BattleState 
{
	List<Tile> tiles;
	AbilityRange ar;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
	AbilityArea aa;
	PathAbilityArea pathArea;
	TerraformAbilityArea terraformArea;
>>>>>>> Stashed changes
=======
	AbilityArea aa;
	PathAbilityArea pathArea;
>>>>>>> Stashed changes
	
	public override void Enter()
	{
		base.Enter();
		ar = turn.ability.GetComponent<AbilityRange>();
<<<<<<< Updated upstream
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
=======
		aa = turn.ability.GetComponent<AbilityArea>();
		if (aa.tiles == null)
			aa.tiles = new List<Tile>();
		aa.tiles.Clear();
		aa.counter = aa.count;
		pathArea = aa as PathAbilityArea;

		statPanelController.ShowPrimary(turn.actor.gameObject);

		if (pathArea != null)
			BeginPathTargeting();
		else
		{
			SelectTiles();
			RefreshDirectionalTargetSelection();
		}

>>>>>>> Stashed changes
		if (driver.Current == Drivers.Computer)
			StartCoroutine(ComputerHighlightTarget());
	}
	
	public override void Exit ()
	{
		base.Exit ();
		if (tiles != null)
			board.DeSelectTiles(tiles);
		statPanelController.HidePrimary();
		statPanelController.HideSecondary();
	}
	
	protected override void OnMove (object sender, InfoEventArgs<Point> e)
	{
<<<<<<< Updated upstream
		if (pathArea != null || terraformArea != null)
		{
			SelectTile(e.info + pos, tiles);
=======
		if (pathArea != null)
		{
			SelectTile(e.info + pos);
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
		if (ar != null && ar.directionOriented)
			return;

		CycleTileLayer(e.info, tiles);
		RefreshSecondaryStatPanel(pos);
=======
		if (pathArea != null)
		{
			CycleTileLayer(e.info, tiles);
			RefreshSecondaryStatPanel(pos);
		}
>>>>>>> Stashed changes
	}
	
	protected override void OnFire (object sender, InfoEventArgs<int> e)
	{
		if (pathArea != null)
		{
<<<<<<< Updated upstream
			HandlePathFire(e.info);
			return;
		}

		if (terraformArea != null)
		{
			HandleTerraformFire(e.info);
=======
			OnPathFire(e.info);
>>>>>>> Stashed changes
			return;
		}

		if (e.info == 0)
		{
<<<<<<< Updated upstream
<<<<<<< Updated upstream
			if (ar.directionOriented || tiles.Contains(board.GetTile(pos)))
				owner.ChangeState<ConfirmAbilityTargetState>();
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
				owner.selectedTile = selectedTile;
				aa.counter--;
				aa.tiles.Add(selectedTile);
				if (aa.counter == 0)
					owner.ChangeState<ConfirmAbilityTargetState>();
			}
>>>>>>> Stashed changes
		}
		else
		{
			aa.counter = aa.count;
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
			if (tiles != null)
				board.DeSelectTiles(tiles);
			turn.actor.dir = dir;
			turn.actor.Match();
			SelectTiles ();
			RefreshDirectionalTargetSelection();
		}
	}
	
	void SelectTiles ()
	{
<<<<<<< Updated upstream
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
=======
		tiles = ar.GetTilesInRange(board);
		if (tiles == null)
			tiles = new List<Tile>();
		tiles.RemoveAll(t => t == null);
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
=======
	void BeginPathTargeting ()
	{
		pathArea.Begin(turn.actor, board);
		SelectTile(turn.actor.tile);
		RefreshPathSelectionTiles();
		statPanelController.HideSecondary();
	}

	void OnPathFire (int button)
	{
		if (button == 0)
		{
			Tile selected = owner.currentTile;
			if (pathArea.TryAddStep(selected))
			{
				RefreshPathSelectionTiles();
				RefreshSecondaryStatPanel(pos);
			}
		}
		else if (button == 1)
		{
			if (!pathArea.RemoveLastStep())
			{
				pathArea.ResetPath();
				owner.ChangeState<CategorySelectionState>();
				return;
			}
			RefreshPathSelectionTiles();
		}
		else if (button == 2)
		{
			FinishPathIfValid();
		}
	}

	void RefreshPathSelectionTiles ()
	{
		if (tiles != null)
			board.DeSelectTiles(tiles);

		tiles = pathArea.GetSelectableNextSteps(board);
		if (tiles == null)
			tiles = new List<Tile>();
		tiles.RemoveAll(t => t == null);
		board.SelectTiles(tiles);
	}

	void FinishPathIfValid ()
	{
		if (!pathArea.HasValidPath())
			return;

		if (aa.tiles == null)
			aa.tiles = new List<Tile>();
		aa.tiles.Clear();
		aa.tiles.AddRange(pathArea.SelectedPath);
		Tile end = pathArea.Endpoint;
		if (end != null)
			SelectTile(end);
		owner.ChangeState<ConfirmAbilityTargetState>();
	}

>>>>>>> Stashed changes
	IEnumerator ComputerHighlightTarget ()
	{
		if (pathArea != null)
		{
			// AI cannot manually draw paths yet. Use the planned fire location as a one-tile path.
			Tile planned = board.GetTile(turn.plan.fireLocation);
			if (planned != null)
				pathArea.TryAddStep(planned);
			yield return new WaitForSeconds(0.5f);
			FinishPathIfValid();
			yield break;
		}

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
