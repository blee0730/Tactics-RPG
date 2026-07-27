using UnityEngine;
<<<<<<< Updated upstream

public class UntilOwnerNextTurnStatusCondition : StatusCondition
{
    Unit owner;
    int createdFrame;

    void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        createdFrame = Time.frameCount;
        if (owner != null)
            this.AddObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
    }

    void OnDisable()
    {
        if (owner != null)
            this.RemoveObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
    }

    void OnTurnBegan(object sender, object args)
    {
        if (Time.frameCount == createdFrame)
            return;
        Status status = GetComponentInParent<Status>();
        if (status != null)
            status.Remove(this);
    }
=======
using System.Collections;

public class UntilOwnerNextTurnStatusCondition : StatusCondition
{
	Unit owner;
	bool armed;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		armed = true;
		if (owner != null)
			this.AddObserver(OnOwnerTurnBegan, TurnOrderController.TurnBeganNotification, owner);
	}

	void OnDisable ()
	{
		if (owner != null)
			this.RemoveObserver(OnOwnerTurnBegan, TurnOrderController.TurnBeganNotification, owner);
	}

	void OnOwnerTurnBegan (object sender, object args)
	{
		if (!armed)
			return;

		Remove();
	}
>>>>>>> Stashed changes
}
