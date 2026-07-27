using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RecoverAbilityEffect : BaseAbilityEffect
{
    public int regenDuration = 0;
    public int regenAmount = 0;
    public float regenPercentOfMaxMP = 0f;

    static HashSet<Type> CurableTypes
    {
        get
        {
            if (_curableTypes == null)
            {
                _curableTypes = new HashSet<Type>();
                _curableTypes.Add(typeof(DoTStatusEffect));
                _curableTypes.Add(typeof(BlindStatusEffect));
                _curableTypes.Add(typeof(SlowStatusEffect));
                _curableTypes.Add(typeof(StopStatusEffect));
                _curableTypes.Add(typeof(SilenceStatusEffect));
                _curableTypes.Add(typeof(MovementLockStatusEffect));
                _curableTypes.Add(typeof(ActionLockStatusEffect));
            }
            return _curableTypes;
        }
    }
    static HashSet<Type> _curableTypes;

    public override int Predict(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Unit attacker = GetComponentInParent<Unit>();
        Unit defender = target.content.GetComponent<Unit>();
        if (attacker == null || defender == null)
            return 0;

        return GetStat(attacker, defender, GetPowerNotification, 0);
    }

    protected override int OnApply(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Unit defender = target.content.GetComponent<Unit>();
        if (defender == null)
            return 0;

        int healed = ApplyHeal(defender, target);
        ApplyCure(defender);
        ApplyRegen(defender);
        return healed;
    }

    int ApplyHeal(Unit defender, Tile target)
    {
        int value = Predict(target);
        value = Mathf.FloorToInt(value * UnityEngine.Random.Range(0.9f, 1.1f));
        value = Mathf.Clamp(value, minDamage, maxDamage);

        Stats stats = defender.GetComponent<Stats>();
        if (stats != null)
            stats[StatTypes.HP] += value;
        return value;
    }

    void ApplyCure(Unit defender)
    {
        Status status = defender.GetComponentInChildren<Status>();
        if (status == null)
            return;

        DurationStatusCondition[] candidates = status.GetComponentsInChildren<DurationStatusCondition>();
        for (int i = candidates.Length - 1; i >= 0; --i)
        {
            StatusEffect effect = candidates[i].GetComponentInParent<StatusEffect>();
            if (effect != null && CurableTypes.Contains(effect.GetType()))
                candidates[i].Remove();
        }
    }

    void ApplyRegen(Unit defender)
    {
        Status status = defender.GetComponentInChildren<Status>();
        if (status == null)
            return;

        DurationStatusCondition condition = status.Add<RegenStatusEffect, DurationStatusCondition>();
        condition.duration = regenDuration;

        RegenStatusEffect effect = condition.GetComponentInParent<RegenStatusEffect>();
        effect.amount = regenAmount;
        effect.percentOfMaxMP = regenPercentOfMaxMP;
    }
}
