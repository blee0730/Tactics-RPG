using UnityEngine;
using System.Collections;

public class StatusTypeHitRate : HitRate
{
	public override bool IsAngleBased { get { return false; }}

	public override int Calculate (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Unit attacker = GetComponentInParent<Unit>();
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

	int GetHit (Unit attacker)
	{
		Stats s = attacker != null ? attacker.GetComponentInParent<Stats>() : null;
		return s != null ? Mathf.Clamp(s[StatTypes.SKL], 0, 100) : 0;
	}

	int GetEvade (Unit defender)
	{
		Stats s = defender != null ? defender.GetComponentInParent<Stats>() : null;
		return s != null ? Mathf.Clamp(s[StatTypes.FRT], 0, 100) : 0;
	}

	protected override float AdjustForRelativeFacing (Unit attacker, Unit target)
	{
		return 1f;
	}
}
