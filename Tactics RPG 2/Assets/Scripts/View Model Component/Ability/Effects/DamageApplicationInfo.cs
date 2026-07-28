using UnityEngine;

public class DamageApplicationInfo
{
    public Unit attacker;
    public Unit defender;
    public DamageAbilityEffect source;
    public Tile targetTile;
    public int damageAmount;
    public bool cancelDamage;

    public DamageApplicationInfo(Unit attacker, Unit defender, DamageAbilityEffect source, Tile targetTile, int damageAmount)
    {
        this.attacker = attacker;
        this.defender = defender;
        this.source = source;
        this.targetTile = targetTile;
        this.damageAmount = damageAmount;
        this.cancelDamage = false;
    }
}
