using UnityEngine;

public class RewindTimeAbilityEffect : BaseAbilityEffect
{
    public bool fallbackToBeforeLastDamage = true;

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        RewindTimeMemory memory = target.content.GetComponent<RewindTimeMemory>();
        if (memory != null && memory.RestoreSnapshot())
            return 0;

        if (fallbackToBeforeLastDamage)
        {
            LastDamageMemory damageMemory = target.content.GetComponent<LastDamageMemory>();
            if (damageMemory != null)
                damageMemory.RestoreBeforeLastDamage();
        }
        return 0;
    }
}
