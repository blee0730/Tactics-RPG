using UnityEngine;
using System.Collections;

public class StopStatusEffect : StatusEffect 
{
	Unit owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		if (owner)
			this.AddObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification, owner);
		this.AddObserver(OnAutomaticHitCheck, HitRate.AutomaticHitCheckNotification);
	}
	
	void OnDisable ()
	{
		if (owner)
			this.RemoveObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification, owner);
		this.RemoveObserver(OnAutomaticHitCheck, HitRate.AutomaticHitCheckNotification);
	}

	void OnTurnCheck (object sender, object args)
	{
		BaseException exc = args as BaseException;
		if (exc != null && exc.defaultToggle == true)
			exc.FlipToggle();
	}

	void OnAutomaticHitCheck (object sender, object args)
	{
		MatchException exc = args as MatchException;
		if (exc != null && exc.target == owner && exc.defaultToggle == false)
			exc.FlipToggle();
	}
}
