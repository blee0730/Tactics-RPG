using UnityEngine;
using System.Collections;

public class RegenStatusEffect : StatusEffect
{
<<<<<<< Updated upstream
	public const string TickNotification = "RegenStatusEffect.TickNotification";

=======
>>>>>>> Stashed changes
	public int amount = 0;
	public float percentOfMaxMP = 0.1f;

	Unit owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		if (owner)
<<<<<<< Updated upstream
			this.AddObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
=======
			this.AddObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, owner);
>>>>>>> Stashed changes
	}

	void OnDisable ()
	{
		if (owner)
<<<<<<< Updated upstream
			this.RemoveObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
	}

	void OnTurnBegan (object sender, object args)
=======
			this.RemoveObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, owner);
	}

	void OnNewTurn (object sender, object args)
>>>>>>> Stashed changes
	{
		Stats stats = GetComponentInParent<Stats>();
		if (stats == null)
			return;

		int restore = amount;
		if (percentOfMaxMP > 0f)
			restore += Mathf.Max(1, Mathf.FloorToInt(stats[StatTypes.MMP] * percentOfMaxMP));
<<<<<<< Updated upstream

		if (restore <= 0)
			return;

		Mana mana = stats.GetComponent<Mana>();
		if (mana != null)
			mana.MP += restore;
		else
			stats[StatTypes.MP] = Mathf.Clamp(stats[StatTypes.MP] + restore, 0, stats[StatTypes.MMP]);

		this.PostNotification(TickNotification, restore);
=======
		stats[StatTypes.MP] += restore;
>>>>>>> Stashed changes
	}
}
