using UnityEngine;
using System;
using System.Reflection;

public class ApplyChanceStatusAbilityEffect : BaseAbilityEffect
{
    public string statusName = "StopStatusEffect";
    public int duration = 1;
    public int chance = 30;

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        if (target == null || target.content == null)
            return 0;
        if (UnityEngine.Random.Range(0, 100) >= chance)
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
            status = target.content.AddComponent<Status>();

        MethodInfo mi = typeof(Status).GetMethod("Add");
        MethodInfo constructed = mi.MakeGenericMethod(new Type[] { statusType, typeof(DurationStatusCondition) });
        object retValue = constructed.Invoke(status, null);
        DurationStatusCondition condition = retValue as DurationStatusCondition;
        if (condition != null)
            condition.duration = duration;
        return 0;
    }

    Type ResolveStatusType(string typeName)
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
