using UnityEngine;
using System.Collections;

public class PoisonStatusEffect : StatusEffect 
{
	public const string TickNotification = "DoTStatusEffect.TickNotification";

	public float percentOfMaxHP = 0.1f;
	public int minimumDamage = 1;
	public bool canKnockOut = true;

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

		Health health = stats.GetComponent<Health>();
		int currentHP = stats[StatTypes.HP];
		int defeatThreshold = health != null ? health.MinHP : 0;
		if (currentHP <= defeatThreshold)
			return;

		int maxHP = stats[StatTypes.MHP];
		int damage = Mathf.Max(minimumDamage, Mathf.FloorToInt(maxHP * percentOfMaxHP));
		int floor = canKnockOut ? defeatThreshold : Mathf.Max(defeatThreshold + 1, 1);
		int nextHP = Mathf.Max(floor, currentHP - damage);

		if (health != null)
			health.HP = nextHP;
		else
			stats[StatTypes.HP] = nextHP;

		this.PostNotification(TickNotification, damage);
	}
}
