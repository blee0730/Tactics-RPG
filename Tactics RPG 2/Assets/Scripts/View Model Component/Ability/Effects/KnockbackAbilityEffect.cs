using UnityEngine;
using System.Collections;

public class KnockbackAbilityEffect : DisplaceAbilityEffect
{
	void Reset ()
	{
		directionMode = DisplaceDirectionMode.AwayFromUser;
		distance = 1;
		moveUnits = true;
		moveObjects = true;
		dealFallDamage = true;
		fallDamagePercentPerHeight = 0.1f;
	}
}
