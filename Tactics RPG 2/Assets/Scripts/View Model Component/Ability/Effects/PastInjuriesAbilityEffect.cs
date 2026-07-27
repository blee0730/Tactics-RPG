using UnityEngine;

public class PastInjuriesAbilityEffect : BaseAbilityEffect
{
    public float damageMultiplier = 1f;
    public int minimumDamage = 1;

    public override int Predict(Tile target)
    {
        Unit caster = GetComponentInParent<Unit>();
        LastDamageMemory memory = caster != null ? caster.GetComponent<LastDamageMemory>() : null;
        if (memory == null || !memory.hasLastDamage)
            return 0;
        return -Mathf.Max(minimumDamage, Mathf.RoundToInt(memory.lastDamageReceived * damageMultiplier));
    }

    protected override int OnApply(Tile target)
    {
        if (target == null || target.content == null)
            return 0;
        Stats stats = target.content.GetComponent<Stats>();
        if (stats == null)
            return 0;

        int value = Predict(target);
        if (value >= 0)
            return 0;
        stats[StatTypes.HP] += value;
        return value;
    }
}
