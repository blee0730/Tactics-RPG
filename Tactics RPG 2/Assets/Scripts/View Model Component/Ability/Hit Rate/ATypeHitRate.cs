using UnityEngine;
using System.Collections;

public class ATypeHitRate : HitRate 
{
	public override int Calculate (Tile target)
	{
<<<<<<< Updated upstream
=======
		if (target == null || target.content == null)
			return 0;

		Unit attacker = GetComponentInParent<Unit>();
>>>>>>> Stashed changes
		Unit defender = target.content.GetComponent<Unit>();
		if (attacker == null || defender == null)
			return 0;

		if (AutomaticHit(defender))
		    return Final(0);

		if (AutomaticMiss(defender))
			return Final(100);

		int evade = GetEvade(defender);
		evade = AdjustForRelativeFacing(defender, evade);
		evade = AdjustForStatusEffects(defender, evade);
		evade = Mathf.Clamp(evade, 5, 95);
		return Final(evade);
	}

<<<<<<< Updated upstream
	int GetEvade (Unit target)
	{
		Stats s = target.GetComponentInParent<Stats>();
		return Mathf.Clamp(s[StatTypes.EVD], 0, 100);
	}

	int AdjustForRelativeFacing (Unit target, int rate)
	{
		switch (attacker.GetFacing(target))
		{
		case Facings.Front:
			return rate;
		case Facings.Side:
			return rate / 2;
		default:
			return rate / 4;
		}
=======
	int GetHit(Unit attacker)
	{
		Stats s = attacker != null ? attacker.GetComponentInParent<Stats>() : null;
		return s != null ? Mathf.Clamp(s[StatTypes.SKL], 0, 100) : 0;
	}
	
	int GetEvade (Unit target)
	{
		Stats s = target != null ? target.GetComponentInParent<Stats>() : null;
		return s != null ? Mathf.Clamp(s[StatTypes.SKL], 0, 100) : 0;
>>>>>>> Stashed changes
	}
}
