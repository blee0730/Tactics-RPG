using UnityEngine;
using System.Collections;

public class ActionLockStatusEffect : StatusEffect
{
	Unit owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		if (owner)
			owner.cantAct = true;
	}

	void OnDisable ()
	{
		if (owner)
			owner.cantAct = false;
	}
}
