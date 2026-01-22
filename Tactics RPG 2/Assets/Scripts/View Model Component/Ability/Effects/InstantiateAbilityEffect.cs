using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class InstantiateAbilityEffect : BaseAbilityEffect 
{
	public float height;
	public GameObject prefab;
	public bool flame;
	public bool wet;
	public bool summon;

	public override int Predict(Tile target)
	{
		return 0;
	}

	protected override int OnApply(Tile target)
	{
		if ((target.isFlammable == flame && target.isWet == wet) || summon)
		{
			Vector3 position = target.center + new Vector3(0, height, 0);
			Quaternion rotation = target.transform.rotation;
			GameObject instance = Instantiate(prefab, position, rotation);
			instance.transform.SetParent(target.transform);
		}
		return 0;
	}
}