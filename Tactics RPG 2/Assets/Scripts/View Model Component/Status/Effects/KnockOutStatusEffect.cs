using UnityEngine;
using System.Collections;

public class KnockOutStatusEffect : StatusEffect
{
	Unit owner;
	Stats stats;
	Status status;
	
	void Awake ()
	{
		owner = GetComponentInParent<Unit>();
		stats = owner.GetComponent<Stats>();
		status = owner.GetComponentInChildren<Status>();
	}
	
	void OnEnable ()
	{
		if(TryAutoRevive())
			return;
			
		owner.transform.localScale = new Vector3(0.75f, 0.1f, 0.75f);
		this.AddObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification, owner);
		this.AddObserver(OnStatCounterWillChange, Stats.WillChangeNotification(StatTypes.CTR), stats); 
	}
	
	void OnDisable ()
	{
		owner.transform.localScale = Vector3.one;
		this.RemoveObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification, owner);
		this.RemoveObserver(OnStatCounterWillChange, Stats.WillChangeNotification(StatTypes.CTR), stats);
	}
	
	void OnTurnCheck (object sender, object args)
	{
		// Dont allow a KO'd unit to take turns
		BaseException exc = args as BaseException;
		if (exc.defaultToggle == true)
			exc.FlipToggle();
	}
	
	void OnStatCounterWillChange (object sender, object args)
	{
		// Dont allow a KO'd unit to increment the turn order counter
		ValueChangeException exc = args as ValueChangeException;
		if (exc.toValue > exc.fromValue)
			exc.FlipToggle();
	}

	bool TryAutoRevive()
	{
    	AutoReviveStatusEffect revive = GetComponentInParent<Status>().GetComponentInChildren<AutoReviveStatusEffect>();

    	if (revive == null)
        	return false;

    	stats[StatTypes.HP] = Mathf.FloorToInt(stats[StatTypes.MHP] * revive.revivePercent);

    	revive.charges--;

		if (revive.charges <= 0)
		{
    		Destroy(revive.gameObject);
		}

    	Destroy(gameObject);

    	return true;
	}
}