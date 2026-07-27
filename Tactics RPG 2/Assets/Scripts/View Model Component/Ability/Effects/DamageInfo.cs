using UnityEngine;
using System.Collections;

public class DamageInfo
{
    public readonly Unit attacker;
    public readonly Unit defender;
    public readonly BaseAbilityEffect sourceEffect;
    public readonly Tile targetTile;
    public readonly int value;

    public DamageInfo(Unit attacker, Unit defender, BaseAbilityEffect sourceEffect, Tile targetTile, int value)
    {
        this.attacker = attacker;
        this.defender = defender;
        this.sourceEffect = sourceEffect;
        this.targetTile = targetTile;
        this.value = value;
    }
}
