using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class InflictAbilityEffect : BaseAbilityEffect 
{
	public string statusName;
	public int duration;

	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Type statusType = ResolveStatusType(statusName);
		if (statusType == null || !statusType.IsSubclassOf(typeof(StatusEffect)))
		{
			Debug.LogError("Invalid Status Type: " + statusName);
			return 0;
		}

		Status status = target.content.GetComponent<Status>();
		if (status == null)
			status = target.content.GetComponentInChildren<Status>();
		if (status == null)
		{
			Debug.LogError("Target has no Status component: " + target.content.name);
			return 0;
		}

		MethodInfo mi = typeof(Status).GetMethod("Add");
		Type[] types = new Type[]{ statusType, typeof(DurationStatusCondition) };
		MethodInfo constructed = mi.MakeGenericMethod(types);

		object retValue = constructed.Invoke(status, null);

		DurationStatusCondition condition = retValue as DurationStatusCondition;
		if (condition != null)
			condition.duration = duration;
		return 0;
	}

	Type ResolveStatusType (string typeName)
	{
		if (string.IsNullOrEmpty(typeName))
			return null;

		Type type = Type.GetType(typeName);
		if (type != null)
			return type;

		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; ++i)
		{
			type = assemblies[i].GetType(typeName);
			if (type != null)
				return type;
		}

		return null;
	}
}
