using UnityEngine;
<<<<<<< Updated upstream

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
=======
using System.Collections;

public class PathTileHazardAbilityEffect : TileHazardAbilityEffect
{
	protected override int OnApply (Tile target)
	{
		PathAbilityArea pathArea = GetComponentInParent<PathAbilityArea>();
		if (pathArea == null)
			return base.OnApply(target);

		int applied = 0;
		for (int i = 0; i < pathArea.SelectedPath.Count; ++i)
			applied += base.OnApply(pathArea.SelectedPath[i]);
		return applied;
	}
>>>>>>> Stashed changes
}
