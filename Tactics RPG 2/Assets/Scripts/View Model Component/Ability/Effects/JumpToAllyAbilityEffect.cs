using UnityEngine;
using System.Collections;

public class JumpToAllyAbilityEffect : BaseAbilityEffect
{
    public int armorDuration = 3;
    public float defenseMultiplier = 1.5f;
    public bool requireAdjacentLanding = true;

    public override int Predict(Tile target)
    {
        if (target == null || target.content == null)
            return 0;

        Stats stats = target.content.GetComponent<Stats>();
        if (stats == null)
            return 0;

        return Mathf.FloorToInt(stats[StatTypes.DEF] * (defenseMultiplier - 1f));
    }

    protected override int OnApply(Tile target)
    {
        Unit jumper = GetComponentInParent<Unit>();
        Unit ally = target != null && target.content != null ? target.content.GetComponent<Unit>() : null;
        Board board = GameObject.FindObjectOfType<Board>();
        if (jumper == null || ally == null || board == null || jumper.tile == null || ally.tile == null)
            return 0;

        Tile landing = FindLandingTile(board, jumper, ally);
        if (landing == null && requireAdjacentLanding)
            return 0;

        if (landing != null)
        {
            jumper.Place(landing);
            jumper.Match();
        }

        ApplyArmor(jumper);
        ApplyArmor(ally);
        return Predict(target);
    }

    Tile FindLandingTile(Board board, Unit jumper, Unit ally)
    {
        Point[] offsets = new Point[]
        {
            new Point(0, -1),
            new Point(-1, 0),
            new Point(1, 0),
            new Point(0, 1)
        };

        Tile best = null;
        int bestDistance = int.MaxValue;
        for (int i = 0; i < offsets.Length; ++i)
        {
            Tile tile = board.GetTile(ally.tile.pos + offsets[i]);
            if (tile == null || tile.content != null)
                continue;

            Stats jumperStats = jumper.GetComponent<Stats>();
            if (jumperStats != null && Mathf.Abs(tile.height - jumper.tile.height) > jumperStats[StatTypes.JMP])
                continue;

            int distance = Mathf.Abs(tile.pos.x - jumper.tile.pos.x) + Mathf.Abs(tile.pos.y - jumper.tile.pos.y);
            if (distance < bestDistance)
            {
                best = tile;
                bestDistance = distance;
            }
        }
        return best;
    }

    void ApplyArmor(Unit unit)
    {
        if (unit == null)
            return;

        Status status = unit.GetComponentInChildren<Status>();
        if (status == null)
            return;

        DurationStatusCondition condition = status.Add<ArmorStatusEffect, DurationStatusCondition>();
        condition.duration = armorDuration;

        ArmorStatusEffect effect = condition.GetComponentInParent<ArmorStatusEffect>();
        if (effect != null)
            effect.defenseMultiplier = defenseMultiplier;
    }
}
