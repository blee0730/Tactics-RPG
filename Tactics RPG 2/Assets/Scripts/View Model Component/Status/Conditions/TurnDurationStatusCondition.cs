using UnityEngine;
using System.Collections;

public class TurnDurationStatusCondition : StatusCondition
{
	public int duration = 1;
	Unit owner;
	bool activeThisTurn;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		if (owner)
		{
			this.AddObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
			this.AddObserver(OnTurnCompleted, TurnOrderController.TurnCompletedNotification, owner);
		}
	}

	void OnDisable ()
	{
		if (owner)
		{
			this.RemoveObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
			this.RemoveObserver(OnTurnCompleted, TurnOrderController.TurnCompletedNotification, owner);
		}
	}

	void OnTurnBegan (object sender, object args)
	{
		activeThisTurn = true;
	}

	void OnTurnCompleted (object sender, object args)
	{
		if (!activeThisTurn)
			return;

		activeThisTurn = false;
		duration--;
		if (duration <= 0)
			Remove();
	}
}
