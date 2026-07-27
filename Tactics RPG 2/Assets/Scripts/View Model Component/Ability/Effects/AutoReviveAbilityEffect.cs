using UnityEngine;
using System.Collections;

public class AutoReviveAbilityEffect : BaseAbilityEffect
{
    public float percent = 0.5f;

    public override int Predict(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Stats stats = target.content.GetComponent<Stats>();
        if (stats == null)
            return 0;

        float clampedPercent = Mathf.Clamp01(percent);
        return Mathf.Max(1, Mathf.FloorToInt(stats[StatTypes.MHP] * clampedPercent));
    }

    protected override int OnApply(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Status status = target.content.GetComponentInChildren<Status>();
        if (status == null)
            return 0;

        AutoReviveStatusEffect effect = status.GetComponentInChildren<AutoReviveStatusEffect>();
        AutoReviveStatusCondition condition = null;

        if (effect != null)
            condition = effect.GetComponentInChildren<AutoReviveStatusCondition>();

        // Revive is a one-use flag. Recasting refreshes the existing flag instead of stacking it.
        if (effect == null || condition == null)
        {
            condition = status.Add<AutoReviveStatusEffect, AutoReviveStatusCondition>();
            effect = condition.GetComponentInParent<AutoReviveStatusEffect>();
        }

        effect.percent = Mathf.Clamp01(percent);
        return Predict(target);
    }
}
