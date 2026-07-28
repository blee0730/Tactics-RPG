using UnityEngine;

public class CounterattackStatusEffect : StatusEffect
{
    public int baseChance = 50;
    public int skillDifferenceMultiplier = 5;
    public int minChance = 10;
    public int maxChance = 90;
    public float counterDamagePercent = 1f;
    public bool onlyCounterAdjacentAttackers = true;
    public bool consumeOnSuccess = false;
    public bool consumeOnFailure = false;

    Unit owner;

    void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        this.AddObserver(OnWillApplyDamage, DamageAbilityEffect.WillApplyDamageNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnWillApplyDamage, DamageAbilityEffect.WillApplyDamageNotification);
    }

    void OnWillApplyDamage(object sender, object args)
    {
        DamageApplicationInfo info = args as DamageApplicationInfo;
        if (owner == null || info == null || info.defender != owner || info.attacker == null)
            return;
        if (info.damageAmount >= 0)
            return;
        if (onlyCounterAdjacentAttackers && !IsAdjacent(owner, info.attacker))
            return;

        bool success = RollCounter(info.attacker);
        if (success)
        {
            info.cancelDamage = true;
            ApplyCounterDamage(info.attacker, Mathf.Abs(info.damageAmount));
            if (consumeOnSuccess)
                RemoveOneCondition();
        }
        else if (consumeOnFailure)
        {
            RemoveOneCondition();
        }
    }

    bool IsAdjacent(Unit a, Unit b)
    {
        if (a.tile == null || b.tile == null)
            return false;
        int dx = Mathf.Abs(a.tile.pos.x - b.tile.pos.x);
        int dy = Mathf.Abs(a.tile.pos.y - b.tile.pos.y);
        return dx + dy == 1;
    }

    bool RollCounter(Unit attacker)
    {
        Stats ownerStats = owner.GetComponent<Stats>();
        Stats attackerStats = attacker.GetComponent<Stats>();
        int ownerSkill = ownerStats != null ? ownerStats[StatTypes.SKL] : 0;
        int attackerSkill = attackerStats != null ? attackerStats[StatTypes.SKL] : 0;
        int chance = baseChance + ((ownerSkill - attackerSkill) * skillDifferenceMultiplier);
        chance = Mathf.Clamp(chance, minChance, maxChance);
        return UnityEngine.Random.Range(0, 100) < chance;
    }

    void ApplyCounterDamage(Unit attacker, int incomingDamage)
    {
        Stats stats = attacker.GetComponent<Stats>();
        if (stats == null)
            return;
        int damage = Mathf.Max(1, Mathf.RoundToInt(incomingDamage * counterDamagePercent));
        stats[StatTypes.HP] -= damage;
    }

    void RemoveOneCondition()
    {
        StatusCondition condition = GetComponentInChildren<StatusCondition>();
        Status status = GetComponentInParent<Status>();
        if (condition != null && status != null)
            status.Remove(condition);
    }
}
