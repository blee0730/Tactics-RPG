using UnityEngine;
using System.Collections;

public class TauntAbilityEffect : BaseAbilityEffect
{
    public int duration = 3;

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Unit source = GetComponentInParent<Unit>();
        Unit defender = target.content.GetComponent<Unit>();
        if (source == null || defender == null)
            return 0;

        Status status = defender.GetComponentInChildren<Status>();
        if (status == null)
            return 0;

        DurationStatusCondition condition = status.Add<TauntStatusEffect, DurationStatusCondition>();
        condition.duration = duration;

        TauntStatusEffect effect = condition.GetComponentInParent<TauntStatusEffect>();
        effect.taunter = source;
        return 0;
    }
}
