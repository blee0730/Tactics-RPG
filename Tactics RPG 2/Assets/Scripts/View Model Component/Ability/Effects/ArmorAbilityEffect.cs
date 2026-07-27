using UnityEngine;
using System.Collections;

public class ArmorAbilityEffect : BaseAbilityEffect
{
    public int duration = 5;
    public float defenseMultiplier = 1.5f;

    public override int Predict(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Stats stats = target.content.GetComponent<Stats>();
        if (stats == null)
            return 0;

        return Mathf.FloorToInt(stats[StatTypes.DEF] * (defenseMultiplier - 1f));
    }

    protected override int OnApply(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Status status = target.content.GetComponentInChildren<Status>();
        if (status == null)
            return 0;

        DurationStatusCondition condition = status.Add<ArmorStatusEffect, DurationStatusCondition>();
        condition.duration = duration;

        ArmorStatusEffect effect = condition.GetComponentInParent<ArmorStatusEffect>();
        effect.defenseMultiplier = defenseMultiplier;
        return Predict(target);
    }
}
