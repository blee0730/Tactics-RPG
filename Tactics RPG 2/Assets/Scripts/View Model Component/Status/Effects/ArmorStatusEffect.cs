using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArmorStatusEffect : StatusEffect
{
    public float defenseMultiplier = 1.5f;
    Unit owner;

    void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        this.AddObserver(OnGetDefense, BaseAbilityEffect.GetDefenseNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnGetDefense, BaseAbilityEffect.GetDefenseNotification);
    }

    void OnGetDefense(object sender, object args)
    {
        Info<Unit, Unit, List<ValueModifier>> info = args as Info<Unit, Unit, List<ValueModifier>>;
        if (info == null || info.arg1 != owner)
            return;

        info.arg2.Add(new MultValueModifier(200, defenseMultiplier));
    }
}
