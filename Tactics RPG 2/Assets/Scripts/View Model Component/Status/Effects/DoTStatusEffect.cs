using UnityEngine;
using System.Collections;

public class DoTStatusEffect : StatusEffect 
{
	public float percentOfMaxHP = 0.1f;
	public int minimumDamage = 1;
	public bool canKnockOut = true;

	Unit owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		if (owner)
			this.AddObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, owner);
	}

	void OnDisable ()
	{
		if (owner)
			this.RemoveObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, owner);
	}

	void OnNewTurn (object sender, object args)
	{
		Stats s = GetComponentInParent<Stats>();
		if (s == null)
			return;

		int currentHP = s[StatTypes.HP];
		if (currentHP <= 0)
			return;

		int maxHP = s[StatTypes.MHP];
		int reduce = Mathf.Max(minimumDamage, Mathf.FloorToInt(maxHP * percentOfMaxHP));
		int floor = canKnockOut ? 0 : 1;
		int nextHP = Mathf.Max(floor, currentHP - reduce);
		s.SetValue(StatTypes.HP, nextHP, false);
	}
}
