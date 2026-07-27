using UnityEngine;
using System.Collections;

public abstract class BaseAbilityPicker : MonoBehaviour
{
	#region Fields
	protected Unit owner;
	protected AbilityCatalog ac;
	#endregion

	#region MonoBehaviour
	void Start ()
	{
		CacheReferences();
	}
	#endregion
	
	#region Public
	public abstract void Pick (PlanOfAttack plan);
	#endregion
	
	#region Protected
	protected void CacheReferences ()
	{
		if (owner == null)
			owner = GetComponentInParent<Unit>();
		if (ac == null && owner != null)
			ac = owner.GetComponentInChildren<AbilityCatalog>();
	}

	protected Ability Find (string abilityName)
	{
		CacheReferences();
		if (ac != null)
			return ac.FindAbility(abilityName, true);
		return null;
	}

	protected Ability Default ()
	{
		CacheReferences();
		if (owner == null)
			return null;

		Ability[] abilities = owner.GetComponentsInChildren<Ability>();
		Ability firstUsable = null;
		for (int i = 0; i < abilities.Length; ++i)
		{
			Ability ability = abilities[i];
			if (ability == null || !ability.CanPerform())
				continue;

			if (firstUsable == null)
				firstUsable = ability;

			if (AbilityCatalog.CleanName(ability.name) == AbilityCatalog.CleanName("Attack"))
				return ability;
		}

		return firstUsable;
	}
	#endregion
}
