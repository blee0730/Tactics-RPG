using UnityEngine;

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
}
