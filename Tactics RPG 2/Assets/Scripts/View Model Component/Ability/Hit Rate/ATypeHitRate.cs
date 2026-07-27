using UnityEngine;
using System.Collections;

public class ATypeHitRate : HitRate
{
	public override int Calculate(Tile target)
	{
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
=======
>>>>>>> Stashed changes
		if (target == null || target.content == null)
			return 0;

		Unit attacker = GetComponentInParent<Unit>();
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
		Unit defender = target.content.GetComponent<Unit>();
		if (attacker == null || defender == null)
			return 0;

		if (AutomaticHit(defender))
			return 100;

		if (AutomaticMiss(defender))
			return 0;

		int proficiency = 1;
		int hit = GetHit(attacker);
		int evade = GetEvade(defender);
		evade = AdjustForStatusEffects(defender, evade);
		return Final(attacker, defender, hit, proficiency, evade);
	}

<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
	int GetHit(Unit attacker)
	{
		Stats s = attacker != null ? attacker.GetComponentInParent<Stats>() : null;
		return s != null ? Mathf.Clamp(s[StatTypes.SKL], 0, 100) : 0;
	}
	
>>>>>>> Stashed changes
	int GetEvade (Unit target)
	{
		Stats s = target != null ? target.GetComponentInParent<Stats>() : null;
		return s != null ? Mathf.Clamp(s[StatTypes.SKL], 0, 100) : 0;
	}
<<<<<<< Updated upstream

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
=======
>>>>>>> Stashed changes
}
