using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomAbilityPicker : BaseAbilityPicker
{
	public List<BaseAbilityPicker> pickers;

	public override void Pick (PlanOfAttack plan)
	{
		if (plan == null || pickers == null || pickers.Count == 0)
			return;

		List<BaseAbilityPicker> valid = new List<BaseAbilityPicker>();
		for (int i = 0; i < pickers.Count; ++i)
		{
			if (pickers[i] != null)
				valid.Add(pickers[i]);
		}

		if (valid.Count == 0)
			return;

		int index = Random.Range(0, valid.Count);
		valid[index].Pick(plan);
	}
}
