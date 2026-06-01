using UnityEngine;
using System.Collections;

public class AutoReviveAbilityEffect : BaseAbilityEffect
{
    public float revivePercent = 0.25f;

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        Unit defender =
            target.content.GetComponent<Unit>();

        if (defender == null)
            return 0;

        Status status =
            defender.GetComponentInChildren<Status>();

        DurationStatusCondition condition =
            status.Add
            <
                AutoReviveStatusEffect,
                DurationStatusCondition
            >();

        AutoReviveStatusEffect effect =
            condition.GetComponentInParent
            <AutoReviveStatusEffect>();

        effect.revivePercent =
            revivePercent;

        condition.duration = 999;

        return 0;
    }
}