using UnityEngine;
using System.Collections;

public class ATypeHitRate : HitRate
{
	public override int Calculate(Tile target)
	{
		Unit attacker = GetComponentInParent<Unit>();
		Unit defender = target.content.GetComponent<Unit>();
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

	int GetHit(Unit attacker)
	{
		Stats s = attacker.GetComponentInParent<Stats>();
		return Mathf.Clamp(s[StatTypes.SKL], 0, 100);
	}
	
	int GetEvade (Unit target)
	{
		Stats s = target.GetComponentInParent<Stats>();
		return Mathf.Clamp(s[StatTypes.SKL], 0, 100);
	}
}