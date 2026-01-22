using UnityEngine;
using System.Collections;

public class StopStatusEffect : StatusEffect 
{
	Stats myStats;
	Unit actor;

	void OnEnable ()
	{
		myStats = GetComponentInParent<Stats>();
		actor = GetComponentInParent<Unit>();
		if (myStats)
			this.AddObserver(OnCounterWillChange, Stats.WillChangeNotification(StatTypes.CTR), myStats);
		this.AddObserver( OnAutomaticHitCheck, HitRate.AutomaticHitCheckNotification );
	}
	
	void OnDisable ()
	{
		this.RemoveObserver( OnCounterWillChange, Stats.WillChangeNotification(StatTypes.CTR), myStats );
		this.RemoveObserver( OnAutomaticHitCheck, HitRate.AutomaticHitCheckNotification );
	}
	
	void OnCounterWillChange (object sender, object args)
	{
		actor.cantMove = true;
	}

	void OnAutomaticHitCheck (object sender, object args)
	{
		actor.cantMove = true;
	}
}
