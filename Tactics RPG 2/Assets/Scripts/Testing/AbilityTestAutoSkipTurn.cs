using UnityEngine;

/// <summary>
/// Test-lab helper for practice dummies and clock units.
/// The unit remains visible in turn order, receives TurnBegan/RoundBegan ticks,
/// then immediately skips its own controllable turn.
/// </summary>
public class AbilityTestAutoSkipTurn : MonoBehaviour
{
    public bool lockMoveAndAction = true;

    Unit owner;

    void OnEnable()
    {
        owner = GetComponent<Unit>();
        if (owner != null)
        {
            this.AddObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification, owner);
            ApplyLocks();
        }
    }

    void OnDisable()
    {
        if (owner != null)
        {
            this.RemoveObserver(OnTurnCheck, TurnOrderController.TurnCheckNotification, owner);

            if (lockMoveAndAction)
            {
                owner.cantMove = false;
                owner.cantAct = false;
            }
        }
    }

    void Update()
    {
        // Some status effects can toggle these flags. Keep test dummies inert.
        ApplyLocks();
    }

    void ApplyLocks()
    {
        if (!lockMoveAndAction || owner == null)
            return;

        owner.cantMove = true;
        owner.cantAct = true;
    }

    void OnTurnCheck(object sender, object args)
    {
        BaseException exc = args as BaseException;
        if (exc == null)
            return;

        // TurnOrderController created the exception with default true.
        // Flip once to make this unit's turn auto-skip.
        if (exc.toggle == exc.defaultToggle)
            exc.FlipToggle();
    }
}
