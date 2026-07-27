using UnityEngine;
using System.Collections.Generic;

public class TemporaryStatModifierStatusEffect : StatusEffect
{
	public StatTypes statType = StatTypes.STR;
	public float multiplier = 1f;
	public int flatBonus = 0;
	public int sortOrder = 100;

	Stats owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Stats>();
		if (owner != null)
			this.AddObserver(OnStatWillChange, Stats.WillChangeNotification(statType), owner);
	}

	void OnDisable ()
	{
		if (owner != null)
			this.RemoveObserver(OnStatWillChange, Stats.WillChangeNotification(statType), owner);
	}

	void OnStatWillChange (object sender, object args)
	{
		ValueChangeException exc = args as ValueChangeException;
		if (exc == null)
			return;

		if (!Mathf.Approximately(multiplier, 1f))
			exc.AddModifier(new MultValueModifier(sortOrder, multiplier));
		if (flatBonus != 0)
			exc.AddModifier(new AddValueModifier(sortOrder + 1, flatBonus));
	}
}
