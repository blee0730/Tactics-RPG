using UnityEngine;
using System.Collections;

public class ApplyRegenAbilityEffect : BaseAbilityEffect
{
    public int duration = 0;
    public int amount = 0;
    public float percentOfMaxMP = 0f;

    public override int Predict(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Stats stats = target.content.GetComponent<Stats>();
        if (stats == null)
            return 0;

        int restore = amount;
        if (percentOfMaxMP > 0f)
            restore += Mathf.Max(1, Mathf.FloorToInt(stats[StatTypes.MMP] * percentOfMaxMP));
        return restore;
    }

    protected override int OnApply(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Status status = target.content.GetComponentInChildren<Status>();
        if (status == null)
            return 0;

        DurationStatusCondition condition = status.Add<RegenStatusEffect, DurationStatusCondition>();
        condition.duration = duration;

        RegenStatusEffect effect = condition.GetComponentInParent<RegenStatusEffect>();
        effect.amount = amount;
        effect.percentOfMaxMP = percentOfMaxMP;
        return Predict(target);
    }
}
