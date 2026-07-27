using UnityEngine;
using System.Collections;

public class StopStatusEffect : StatusEffect 
{
<<<<<<< Updated upstream
	Stats myStats;

	void OnEnable ()
	{
		myStats = GetComponentInParent<Stats>();
		if (myStats)
			this.AddObserver( OnCounterWillChange, Stats.WillChangeNotification(StatTypes.CTR), myStats );
		this.AddObserver( OnAutomaticHitCheck, HitRate.AutomaticHitCheckNotification );
=======
	Unit owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		if (owner)
			this.AddObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification, owner);
		this.AddObserver(OnAutomaticHitCheck, HitRate.AutomaticHitCheckNotification);
>>>>>>> Stashed changes
	}
	
	void OnDisable ()
	{
		if (owner)
			this.RemoveObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification, owner);
		this.RemoveObserver(OnAutomaticHitCheck, HitRate.AutomaticHitCheckNotification);
	}

	void OnTurnCheck (object sender, object args)
	{
<<<<<<< Updated upstream
		ValueChangeException exc = args as ValueChangeException;
		exc.FlipToggle();
=======
		BaseException exc = args as BaseException;
		if (exc != null && exc.defaultToggle == true)
			exc.FlipToggle();
>>>>>>> Stashed changes
	}

	void OnAutomaticHitCheck (object sender, object args)
	{
<<<<<<< Updated upstream
		Unit owner = GetComponentInParent<Unit>();
		MatchException exc = args as MatchException;
		if (owner == exc.target)
=======
		MatchException exc = args as MatchException;
		if (exc != null && exc.target == owner && exc.defaultToggle == false)
>>>>>>> Stashed changes
			exc.FlipToggle();
	}
}
