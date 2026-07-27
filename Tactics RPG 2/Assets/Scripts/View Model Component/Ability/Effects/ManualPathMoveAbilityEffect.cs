using UnityEngine;
using System.Collections.Generic;

public class ManualPathMoveAbilityEffect : BaseAbilityEffect
{
    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        Unit actor = GetComponentInParent<Unit>();
        PathAbilityArea area = GetComponentInParent<Ability>().GetComponent<PathAbilityArea>();
        if (actor == null || area == null || area.tiles == null || area.tiles.Count == 0)
            return 0;

        Tile destination = area.tiles[area.tiles.Count - 1];
        if (destination == null || destination.content != null)
            return 0;

        for (int i = 0; i < area.tiles.Count; ++i)
        {
            Tile step = area.tiles[i];
            if (step == null)
                continue;
            if (actor.tile != null)
                actor.dir = actor.tile.GetDirection(step);
            actor.Place(step);
            actor.Match();
        }
        return 0;
    }
}
