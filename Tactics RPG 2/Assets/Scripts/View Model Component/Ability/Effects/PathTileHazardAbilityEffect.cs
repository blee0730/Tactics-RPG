using UnityEngine;

public class PathTileHazardAbilityEffect : SpawnHazardZoneAbilityEffect
{
    protected override int OnApply(Tile target)
    {
        PathAbilityArea area = GetComponentInParent<Ability>().GetComponent<PathAbilityArea>();
        if (area == null || area.tiles == null)
            return 0;

        for (int i = 0; i < area.tiles.Count; ++i)
            base.OnApply(area.tiles[i]);
        return 0;
    }
}
