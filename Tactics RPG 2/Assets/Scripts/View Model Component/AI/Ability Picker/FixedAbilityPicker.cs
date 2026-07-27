using UnityEngine;
using System.Collections;

public class FixedAbilityPicker : BaseAbilityPicker
{
	public Targets target;
	public string ability;

	public override void Pick (PlanOfAttack plan)
	{
		if (plan == null)
			return;

		plan.target = target;
		plan.ability = Find(ability);

		if (plan.ability == null || !plan.ability.CanPerform())
		{
			plan.ability = Default();
			plan.target = plan.ability != null ? Targets.Foe : Targets.None;
		}
	}
}
