using UnityEngine;
using System.Collections;

public class EmptyTileAbilityEffectTarget : AbilityEffectTarget
{
    public override bool IsTarget(Tile tile)
    {
        return tile != null && tile.content == null;
    }
}
