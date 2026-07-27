using UnityEngine;

public class DrainDamageAbilityEffect : DamageAbilityEffect
{
    public float healPercentOfDamage = 0.5f;
    public bool restoreHP = true;
    public bool restoreMP = false;

    protected override int OnApply(Tile target)
    {
        int value = base.OnApply(target);
        int damageDone = Mathf.Abs(Mathf.Min(0, value));
        if (damageDone <= 0)
            return value;

        Unit caster = GetComponentInParent<Unit>();
        Stats stats = caster != null ? caster.GetComponent<Stats>() : null;
        if (stats == null)
            return value;

        int amount = Mathf.Max(1, Mathf.RoundToInt(damageDone * healPercentOfDamage));
        if (restoreHP)
            stats.SetValue(StatTypes.HP, Mathf.Min(stats[StatTypes.MHP], stats[StatTypes.HP] + amount), false);
        if (restoreMP)
            stats.SetValue(StatTypes.MP, Mathf.Min(stats[StatTypes.MMP], stats[StatTypes.MP] + amount), false);
        return value;
    }
}
