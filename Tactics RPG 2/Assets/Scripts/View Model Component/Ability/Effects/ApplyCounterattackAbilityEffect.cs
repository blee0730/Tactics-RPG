using UnityEngine;

public class ApplyCounterattackAbilityEffect : BaseAbilityEffect
{
    public int baseChance = 50;
    public int skillDifferenceMultiplier = 5;
    public int minChance = 10;
    public int maxChance = 90;
    public float counterDamagePercent = 1f;
    public bool onlyCounterAdjacentAttackers = true;
    public bool consumeOnSuccess = false;
    public bool consumeOnFailure = false;

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Status status = target.content.GetComponent<Status>();
        if (status == null)
            status = target.content.GetComponentInChildren<Status>();
        if (status == null)
            status = target.content.AddComponent<Status>();

        UntilOwnerNextTurnStatusCondition condition = status.Add<CounterattackStatusEffect, UntilOwnerNextTurnStatusCondition>();
        CounterattackStatusEffect effect = condition.GetComponentInParent<CounterattackStatusEffect>();
        effect.baseChance = baseChance;
        effect.skillDifferenceMultiplier = skillDifferenceMultiplier;
        effect.minChance = minChance;
        effect.maxChance = maxChance;
        effect.counterDamagePercent = counterDamagePercent;
        effect.onlyCounterAdjacentAttackers = onlyCounterAdjacentAttackers;
        effect.consumeOnSuccess = consumeOnSuccess;
        effect.consumeOnFailure = consumeOnFailure;
        return 0;
    }
}
