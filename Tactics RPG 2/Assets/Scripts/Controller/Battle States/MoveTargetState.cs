using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveTargetState : BattleState
{
	List<Tile> tiles;
	
	public override void Enter ()
	{
		base.Enter ();
		Movement mover = turn.actor.GetComponent<Movement>();
		tiles = mover.GetTilesInRange(board);
		board.SelectTiles(tiles);
		RefreshPrimaryStatPanel(pos);
		if (driver.Current == Drivers.Computer)
			StartCoroutine(ComputerHighlightMoveTarget());
	}
	
	public override void Exit ()
	{
		base.Exit ();
		board.DeSelectTiles(tiles);
		tiles = null;
		statPanelController.HidePrimary();
		statPanelController.HideDetail();
	}
	
	protected override void OnMove (object sender, InfoEventArgs<Point> e)
	{
		if (statPanelController.IsDetailVisible)
			return;

		// Cursor movement should not be limited to legal move endpoints.
		// Occupied ally/enemy tiles are often removed from the destination list,
		// but the player still needs to be able to move the cursor across them
		// to reach highlighted tiles on the far side.
		SelectTile(e.info + pos);
		RefreshPrimaryStatPanel(pos);
	}

	protected override void OnCycleLayer (object sender, InfoEventArgs<int> e)
	{
<<<<<<< Updated upstream
		if (statPanelController.IsDetailVisible)
			return;

		// Layer cycling is a cursor-selection feature, not a movement confirmation.
		// Allow cycling through any selectable top/splitTop at this X/Z point.
		// The Fire/confirm step below still only allows actual movement to tiles
		// returned by Movement.GetTilesInRange.
		CycleTileLayer(e.info);
=======
		CycleTileLayer(e.info, tiles);
>>>>>>> Stashed changes
		RefreshPrimaryStatPanel(pos);
	}
	
	protected override void OnFire (object sender, InfoEventArgs<int> e)
	{
		if (statPanelController.IsDetailVisible)
		{
			if (e.info == 0)
				statPanelController.CycleDetailPage();
			else
				statPanelController.HideDetail();
			return;
		}

		if (e.info == 0)
		{
			Unit inspected = GetUnit(pos);
			if (inspected != null)
			{
				statPanelController.ShowDetail(inspected.gameObject);
				return;
			}

			if (tiles.Contains(owner.currentTile))
				owner.ChangeState<MoveSequenceState>();
		}
		else
		{
			if (statPanelController.IsDetailVisible)
			{
				statPanelController.HideDetail();
				return;
			}

			owner.ChangeState<CommandSelectionState>();
		}
	}

	IEnumerator ComputerHighlightMoveTarget ()
	{
		Point cursorPos = pos;
		while (cursorPos != turn.plan.moveLocation)
		{
			if (cursorPos.x < turn.plan.moveLocation.x) cursorPos.x++;
			if (cursorPos.x > turn.plan.moveLocation.x) cursorPos.x--;
			if (cursorPos.y < turn.plan.moveLocation.y) cursorPos.y++;
			if (cursorPos.y > turn.plan.moveLocation.y) cursorPos.y--;
			SelectTile(cursorPos);
			yield return new WaitForSeconds(0.25f);
		}
		yield return new WaitForSeconds(0.5f);
		owner.ChangeState<MoveSequenceState>();
	}
}
