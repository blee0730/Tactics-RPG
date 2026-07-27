using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Turn 
{
	public Unit actor;
	public bool hasUnitMoved;
	public bool hasUnitActed;
	public bool lockMove;
	public Ability ability;
	public List<Tile> targets;
	public PlanOfAttack plan;
	Tile startTile;
	Directions startDir;

	public int movesRemaining { get; private set; }
	public int actionsRemaining { get; private set; }
	public int maxMovesThisTurn { get; private set; }
	public int maxActionsThisTurn { get; private set; }

	public void Change (Unit current)
	{
		actor = current;
		hasUnitMoved = false;
		hasUnitActed = false;
		lockMove = false;
		startTile = actor.tile;
		startDir = actor.dir;
		plan = null;

		SetCommandBudgets(1, 1);

		AccelStatusEffect accel = actor != null ? actor.GetComponentInChildren<AccelStatusEffect>() : null;
		if (accel != null)
			accel.ApplyToTurn(this);
	}

	public void SetCommandBudgets (int moves, int actions)
	{
		maxMovesThisTurn = Mathf.Max(0, moves);
		maxActionsThisTurn = Mathf.Max(0, actions);
		movesRemaining = maxMovesThisTurn;
		actionsRemaining = maxActionsThisTurn;
	}

	public bool CanMove ()
	{
		return movesRemaining > 0;
	}

	public bool CanAct ()
	{
		return actionsRemaining > 0;
	}

	public void ConsumeMove ()
	{
		hasUnitMoved = true;
		movesRemaining = Mathf.Max(0, movesRemaining - 1);
	}

	public void ConsumeAction ()
	{
		hasUnitActed = true;
		actionsRemaining = Mathf.Max(0, actionsRemaining - 1);
	}

	public void ConsumeRemainingCommands ()
	{
		movesRemaining = 0;
		actionsRemaining = 0;
		hasUnitMoved = true;
		hasUnitActed = true;
		lockMove = true;
	}

	public bool HasRemainingCommands ()
	{
		return CanMove() || CanAct();
	}

	public void UndoMove ()
	{
		hasUnitMoved = false;
		movesRemaining = maxMovesThisTurn;
		actor.Place(startTile);
		actor.dir = startDir;
		actor.Match();
	}
}
