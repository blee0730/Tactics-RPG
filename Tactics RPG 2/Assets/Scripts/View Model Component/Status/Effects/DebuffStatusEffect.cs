using UnityEngine;
using System.Collections;

public class SlowStatusEffect : StatusEffect 
{
	Stats myStats;
	public StatTypes type;
	void OnEnable ()
	{
		myStats = GetComponentInParent<Stats>();
		if (myStats)
			this.AddObserver( OnCounterWillChange, Stats.WillChangeNotification(type), myStats );
	}
	
	void OnDisable ()
	{
		this.RemoveObserver( OnCounterWillChange, Stats.WillChangeNotification(type), myStats );
	}
	
	void OnCounterWillChange (object sender, object args)
	{
		ValueChangeException exc = args as ValueChangeException;
		MultDeltaModifier m = new MultDeltaModifier(0, 0.5f);
		exc.AddModifier(m);
	}
}