using UnityEngine;
<<<<<<< Updated upstream

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
=======
using System.Collections;

public class DrainDamageAbilityEffect : DamageAbilityEffect
{
	public float healPercentOfDamage = 0.5f;
	public bool restoreHP = true;
	public bool restoreMP = false;

	protected override int OnApply (Tile target)
	{
		int damageValue = base.OnApply(target);
		if (damageValue >= 0)
			return damageValue;

		Unit user = GetComponentInParent<Unit>();
		Stats stats = user != null ? user.GetComponent<Stats>() : null;
		if (stats == null)
			return damageValue;

		int heal = Mathf.FloorToInt(Mathf.Abs(damageValue) * healPercentOfDamage);
		if (restoreHP)
			stats[StatTypes.HP] = Mathf.Min(stats[StatTypes.MHP], stats[StatTypes.HP] + heal);
		if (restoreMP)
			stats[StatTypes.MP] = Mathf.Min(stats[StatTypes.MMP], stats[StatTypes.MP] + heal);

		return damageValue;
	}
>>>>>>> Stashed changes
}
