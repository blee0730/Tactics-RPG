using UnityEngine;
using System.Collections;

public class TauntStatusEffect : StatusEffect
{
    public Unit taunter;

    public Unit GetForcedTarget(Unit actor)
    {
        if (actor == null || taunter == null)
            return null;

        Stats stats = taunter.GetComponent<Stats>();
        if (stats == null || stats[StatTypes.HP] <= 0)
            return null;

        Alliance actorAlliance = actor.GetComponentInChildren<Alliance>();
        Alliance taunterAlliance = taunter.GetComponentInChildren<Alliance>();
        if (actorAlliance == null || taunterAlliance == null)
            return null;

        return actorAlliance.IsMatch(taunterAlliance, Targets.Foe) ? taunter : null;
    }
}
