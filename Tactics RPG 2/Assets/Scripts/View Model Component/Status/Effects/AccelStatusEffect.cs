using UnityEngine;
using System.Collections;

public class AccelStatusEffect : StatusEffect
{
	public int movesPerTurn = 2;
	public int actionsPerTurn = 2;
	Unit owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
	}

	public void ApplyToTurn (Turn turn)
	{
		if (turn == null || turn.actor == null || owner == null || turn.actor != owner)
			return;

		turn.SetCommandBudgets(movesPerTurn, actionsPerTurn);
	}
}
