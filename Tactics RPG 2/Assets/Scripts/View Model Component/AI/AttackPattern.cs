using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackPattern : MonoBehaviour 
{
	public List<BaseAbilityPicker> pickers;
	int index;
	
	public void Pick (PlanOfAttack plan)
	{
		if (plan == null || pickers == null || pickers.Count == 0)
			return;

		int checkedCount = 0;
		while (checkedCount < pickers.Count)
		{
			if (index < 0 || index >= pickers.Count)
				index = 0;

			BaseAbilityPicker picker = pickers[index];
			index++;
			if (index >= pickers.Count)
				index = 0;

			checkedCount++;
			if (picker == null)
				continue;

			picker.Pick(plan);
			return;
		}
	}
}
