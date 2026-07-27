using UnityEngine;
using System.Collections;

public class AllyAbilityEffectTarget : AbilityEffectTarget
{
    Alliance alliance;

    void Start()
    {
        alliance = GetComponentInParent<Alliance>();
    }

    public override bool IsTarget(Tile tile)
    {
        if (tile == null || tile.content == null)
            return false;

        if (alliance == null)
            alliance = GetComponentInParent<Alliance>();

        Alliance other = tile.content.GetComponentInChildren<Alliance>();
        Stats stats = tile.content.GetComponent<Stats>();
        return alliance != null && other != null && alliance.IsMatch(other, Targets.Ally) && stats != null && stats[StatTypes.HP] > 0;
    }
}
