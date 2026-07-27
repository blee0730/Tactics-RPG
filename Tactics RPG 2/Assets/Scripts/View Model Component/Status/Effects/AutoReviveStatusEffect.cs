using UnityEngine;
using System.Collections;

public class AutoReviveStatusEffect : StatusEffect
{
    public float percent = 0.5f;

    public bool TryRevive(Stats stats)
    {
        if (stats == null)
            return false;

        if (stats[StatTypes.HP] > 0)
            return false;

        AutoReviveStatusCondition condition = GetComponentInChildren<AutoReviveStatusCondition>();
        if (condition == null)
            return false;

        float clampedPercent = Mathf.Clamp01(percent);
        int amount = Mathf.Max(1, Mathf.FloorToInt(stats[StatTypes.MHP] * clampedPercent));

        // Remove the one-use flag before restoring HP so the unit does not keep the revive.
        condition.Remove();

        stats.SetValue(StatTypes.HP, amount, false);
        return true;
    }
}
