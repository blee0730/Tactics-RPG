using UnityEngine;

public class ParalysisStatusEffect : StatusEffect
{
    Unit owner;

    void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        if (owner != null)
        {
            owner.cantMove = true;
            owner.cantAct = true;
        }
    }

    void OnDisable()
    {
        if (owner != null)
        {
            owner.cantMove = false;
            owner.cantAct = false;
        }
    }
}
