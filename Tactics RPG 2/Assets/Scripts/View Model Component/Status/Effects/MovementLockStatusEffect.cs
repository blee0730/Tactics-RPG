using UnityEngine;
using System.Collections;

public class MovementLockStatusEffect : StatusEffect
{
	Unit owner;

	void OnEnable ()
	{
		owner = GetComponentInParent<Unit>();
		if (owner)
			owner.cantMove = true;
	}

	void OnDisable ()
	{
		if (owner)
			owner.cantMove = false;
	}
}
