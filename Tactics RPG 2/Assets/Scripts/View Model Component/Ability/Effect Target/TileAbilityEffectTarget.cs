using UnityEngine;
using System.Collections;

public class TileAbilityEffectTarget : AbilityEffectTarget 
{
	public override bool IsTarget (Tile tile)
	{
		if (tile == null)
			return false;

		return tile;
	}
}