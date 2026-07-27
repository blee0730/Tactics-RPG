using UnityEngine;
<<<<<<< Updated upstream

public class PathEndpointAbilityEffectTarget : AbilityEffectTarget
{
    public bool requireEmptyEndpoint = true;

    public override bool IsTarget(Tile tile)
    {
        PathAbilityArea area = GetComponentInParent<Ability>().GetComponent<PathAbilityArea>();
        if (area == null || area.tiles == null || area.tiles.Count == 0 || tile == null)
            return false;
        Tile endpoint = area.tiles[area.tiles.Count - 1];
        if (tile != endpoint)
            return false;
        if (requireEmptyEndpoint && tile.content != null)
            return false;
        return true;
    }
=======
using System.Collections;

public class PathEndpointAbilityEffectTarget : AbilityEffectTarget
{
	public bool requireEmptyEndpoint = true;

	public override bool IsTarget (Tile tile)
	{
		PathAbilityArea pathArea = GetComponentInParent<PathAbilityArea>();
		if (pathArea == null || tile == null)
			return false;

		Tile endpoint = pathArea.Endpoint;
		if (endpoint == null || endpoint != tile)
			return false;

		return !requireEmptyEndpoint || tile.content == null;
	}
>>>>>>> Stashed changes
}
