using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageAndKnockbackAbilityEffect : DamageAbilityEffect
{
    public int distance = 1;
    public bool dealFallDamage = true;
    public float fallDamagePercentPerHeight = 0.1f;

    protected override int OnApply(Tile target)
    {
        int value = base.OnApply(target);
        Knockback(target);
        return value;
    }

    void Knockback(Tile target)
    {
        if (target == null || target.content == null)
            return;

        Unit attacker = GetComponentInParent<Unit>();
        Unit defender = target.content.GetComponent<Unit>();
        Board board = GameObject.FindObjectOfType<Board>();
        if (attacker == null || defender == null || board == null || defender.tile == null || attacker.tile == null)
            return;

        Point normal = (defender.tile.pos - attacker.tile.pos).GetDirection().GetNormal();
        Tile startTile = defender.tile;
        Tile destination = startTile;

        for (int i = 0; i < distance; ++i)
        {
            Tile next = board.GetTile(destination.pos + normal);
            if (next == null || next.content != null)
                break;
            destination = next;
        }

        if (destination == startTile)
            return;

        defender.Place(destination);
        defender.Match();

        if (dealFallDamage)
            ApplyFallDamage(defender, startTile, destination);
    }

    void ApplyFallDamage(Unit defender, Tile from, Tile to)
    {
        float fallDistance = from.height - to.height;
        if (fallDistance <= 0f)
            return;

        Stats stats = defender.GetComponent<Stats>();
        if (stats == null || stats[StatTypes.JMP] >= fallDistance)
            return;

        int currentHP = stats[StatTypes.HP];
        int maxHP = stats[StatTypes.MHP];
        int reduce = Mathf.Min(currentHP, Mathf.FloorToInt(maxHP * fallDamagePercentPerHeight * fallDistance));
        stats.SetValue(StatTypes.HP, currentHP - reduce, false);
    }
}
