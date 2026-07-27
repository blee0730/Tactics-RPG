using UnityEngine;
using System.Collections;

public class RegenStatusEffect : StatusEffect
{
	public const string TickNotification = "RegenStatusEffect.TickNotification";

	public int amount = 0;
	public float percentOfMaxMP = 0.1f;

	Unit owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		if (owner)
			this.AddObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
	}

	void OnDisable ()
	{
		if (owner)
			this.RemoveObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
	}

	void OnTurnBegan (object sender, object args)
	{
		Stats stats = GetComponentInParent<Stats>();
		if (stats == null)
			return;

		int restore = amount;
		if (percentOfMaxMP > 0f)
			restore += Mathf.Max(1, Mathf.FloorToInt(stats[StatTypes.MMP] * percentOfMaxMP));

		if (restore <= 0)
			return;

		Mana mana = stats.GetComponent<Mana>();
		if (mana != null)
			mana.MP += restore;
		else
			stats[StatTypes.MP] = Mathf.Clamp(stats[StatTypes.MP] + restore, 0, stats[StatTypes.MMP]);

		this.PostNotification(TickNotification, restore);
	}
}
