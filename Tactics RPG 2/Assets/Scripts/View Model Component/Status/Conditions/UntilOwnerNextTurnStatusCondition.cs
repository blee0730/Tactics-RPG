using UnityEngine;

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
}
