using UnityEngine;
using System.Collections;

public class AssassinateAbilityEffect : DamageAbilityEffect
{
    public int baseInstantKillChance = 25;
    public int skillBonusPercent = 2;
    public int defenderSkillPenaltyPercent = 2;
    public int minInstantKillChance = 5;
    public int maxInstantKillChance = 75;
    public float failureDamageMultiplier = 1.5f;

    public override int Predict(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Unit defender = target.content.GetComponent<Unit>();
        Stats stats = defender != null ? defender.GetComponent<Stats>() : null;
        if (stats == null)
            return base.Predict(target);

        int chance = CalculateInstantKillChance(target);
        int fallback = Mathf.RoundToInt(base.Predict(target) * failureDamageMultiplier);
        // Preview the possible instant kill as a negative HP value when chance is strong.
        if (chance >= 50)
            return -stats[StatTypes.HP];
        return fallback;
    }

    protected override int OnApply(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Unit defender = target.content.GetComponent<Unit>();
        Stats stats = defender != null ? defender.GetComponent<Stats>() : null;
        if (stats == null)
            return 0;

        int chance = CalculateInstantKillChance(target);
        if (UnityEngine.Random.Range(0, 100) < chance)
        {
            int current = stats[StatTypes.HP];
            stats[StatTypes.HP] = 0;
            return -current;
        }

        int value = base.OnApply(target);
        int bonus = Mathf.RoundToInt(value * (failureDamageMultiplier - 1f));
        if (bonus != 0)
        {
            stats[StatTypes.HP] += bonus;
            value += bonus;
        }
        return value;
    }

    int CalculateInstantKillChance(Tile target)
    {
        Unit attacker = GetComponentInParent<Unit>();
        Unit defender = target != null && target.content != null ? target.content.GetComponent<Unit>() : null;
        Stats attackerStats = attacker != null ? attacker.GetComponent<Stats>() : null;
        Stats defenderStats = defender != null ? defender.GetComponent<Stats>() : null;

        int chance = baseInstantKillChance;
        if (attackerStats != null)
            chance += attackerStats[StatTypes.SKL] * skillBonusPercent;
        if (defenderStats != null)
            chance -= defenderStats[StatTypes.SKL] * defenderSkillPenaltyPercent;

        return Mathf.Clamp(chance, minInstantKillChance, maxInstantKillChance);
    }
}
