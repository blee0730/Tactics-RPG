using UnityEngine;
using System;
using System.Reflection;

public enum HazardZoneType
{
    Fire,
    Ice,
    Hail,
    Earthquake,
    Thunderstorm,
    Vacuum,
    Darkness,
    Generic
}

public class HazardZoneTileEffect : MonoBehaviour
{
    public HazardZoneType hazardType = HazardZoneType.Generic;
    public int durationRounds = 3;
    public float percentOfMaxHP = 0.1f;
    public int flatDamage = 0;
    public int minimumDamage = 1;
    public bool canKnockOut = true;
    public bool damageOnEnter = true;
    public bool damageOnTurnStart = true;
    public bool removeWhenExpired = true;

    public string statusName = "";
    public int statusDuration = 1;
    public int statusChance = 100;
    public bool applyStatusOnEnter = false;
    public bool applyStatusOnTurnStart = false;

    public int darknessAccuracyPenalty = 20;

    Tile owner;

    void OnEnable()
    {
        owner = GetComponentInParent<Tile>();
        this.AddObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification);
        this.AddObserver(OnRoundEnded, TurnOrderController.RoundEndedNotification);
        if (hazardType == HazardZoneType.Darkness)
            this.AddObserver(OnHitRateStatusCheck, HitRate.StatusCheckNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification);
        this.RemoveObserver(OnRoundEnded, TurnOrderController.RoundEndedNotification);
        if (hazardType == HazardZoneType.Darkness)
            this.RemoveObserver(OnHitRateStatusCheck, HitRate.StatusCheckNotification);
    }

    void OnTriggerEnter(Collider other)
    {
        Unit unit = other.GetComponentInParent<Unit>();
        if (unit == null)
            return;
        if (damageOnEnter)
            TickDamage(unit);
        if (applyStatusOnEnter)
            TryApplyStatus(unit);
    }

    void OnTurnBegan(object sender, object args)
    {
        Unit unit = sender as Unit;
        if (unit == null || owner == null || unit.tile != owner)
            return;
        if (damageOnTurnStart)
            TickDamage(unit);
        if (applyStatusOnTurnStart)
            TryApplyStatus(unit);
    }

    void OnRoundEnded(object sender, object args)
    {
        if (durationRounds < 0)
            return;
        durationRounds--;
        if (durationRounds <= 0 && removeWhenExpired)
            Destroy(gameObject);
    }

    void OnHitRateStatusCheck(object sender, object args)
    {
        Info<Unit, Unit, int> info = args as Info<Unit, Unit, int>;
        if (info == null || info.arg1 == null || info.arg1.tile == null || owner == null)
            return;

        // Darkness protects a unit standing in it. A shooter standing in darkness is
        // not penalized merely for shooting out of it.
        if (info.arg1.tile == owner)
            info.arg2 += darknessAccuracyPenalty;
    }

    void TickDamage(Unit unit)
    {
        if (unit == null)
            return;
        Stats stats = unit.GetComponent<Stats>();
        if (stats == null)
            return;
        int currentHP = stats[StatTypes.HP];
        int maxHP = stats[StatTypes.MHP];
        int damage = flatDamage + Mathf.FloorToInt(maxHP * percentOfMaxHP);
        damage = Mathf.Max(minimumDamage, damage);
        if (!canKnockOut)
            damage = Mathf.Min(damage, Mathf.Max(0, currentHP - 1));
        else
            damage = Mathf.Min(damage, currentHP);
        stats.SetValue(StatTypes.HP, currentHP - damage, false);
    }

    void TryApplyStatus(Unit unit)
    {
        if (unit == null || string.IsNullOrEmpty(statusName))
            return;
        if (UnityEngine.Random.Range(0, 100) >= statusChance)
            return;

        Type statusType = ResolveStatusType(statusName);
        if (statusType == null || !statusType.IsSubclassOf(typeof(StatusEffect)))
            return;

        Status status = unit.GetComponent<Status>();
        if (status == null)
            status = unit.GetComponentInChildren<Status>();
        if (status == null)
            status = unit.gameObject.AddComponent<Status>();

        MethodInfo mi = typeof(Status).GetMethod("Add");
        MethodInfo constructed = mi.MakeGenericMethod(new Type[] { statusType, typeof(DurationStatusCondition) });
        object retValue = constructed.Invoke(status, null);
        DurationStatusCondition condition = retValue as DurationStatusCondition;
        if (condition != null)
            condition.duration = statusDuration;
    }

    Type ResolveStatusType(string typeName)
    {
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
