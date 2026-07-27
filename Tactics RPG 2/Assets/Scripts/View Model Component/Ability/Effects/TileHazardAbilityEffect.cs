using UnityEngine;
using System.Collections;

public enum TileHazardType
{
	Fire,
	Ice,
	Trap
}

public class TileHazardAbilityEffect : BaseAbilityEffect
{
	public TileHazardType hazardType = TileHazardType.Fire;
	public int duration = 3;
	public bool replaceExistingSameType = true;

	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		if (target == null)
			return 0;

		System.Type componentType = GetHazardComponentType();
		if (componentType == null)
			return 0;

		Component existing = target.GetComponentInChildren(componentType);
		if (existing != null && replaceExistingSameType)
			Destroy(existing.gameObject);
		else if (existing != null)
			return 0;

		GameObject go = new GameObject(hazardType.ToString());
		go.transform.SetParent(target.transform, false);
		go.transform.localPosition = Vector3.zero;
		go.AddComponent(componentType);
		TileEffectDuration timer = go.AddComponent<TileEffectDuration>();
		timer.duration = duration;
		return 0;
	}

	System.Type GetHazardComponentType ()
	{
		switch (hazardType)
		{
		case TileHazardType.Fire: return typeof(Fire);
		case TileHazardType.Ice: return typeof(Ice);
		case TileHazardType.Trap: return typeof(Trap);
		default: return null;
		}
	}
}
