using UnityEngine;

public class VerticalDropAbilityEffect : BaseAbilityEffect
{
    public float heightOffset = 4f;
    public float damagePercentPerHeight = 0.1f;

    public override int Predict(Tile target)
    {
        if (target == null || target.content == null)
            return 0;
        Stats stats = target.content.GetComponent<Stats>();
        if (stats == null)
            return 0;
        int damage = Mathf.Max(1, Mathf.FloorToInt(stats[StatTypes.MHP] * damagePercentPerHeight * heightOffset));
        return -damage;
    }

    protected override int OnApply(Tile target)
    {
        if (target == null || target.content == null)
            return 0;
        Unit unit = target.content.GetComponent<Unit>();
        Stats stats = target.content.GetComponent<Stats>();
        if (unit == null || stats == null)
            return 0;
        int damage = Mathf.Abs(Predict(target));
        stats[StatTypes.HP] -= Mathf.Min(stats[StatTypes.HP], damage);
        unit.Match();
        return -damage;
    }
}
