using UnityEngine;
<<<<<<< Updated upstream

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
=======
using System.Collections;

public class DamageApplicationInfo
{
	public readonly DamageAbilityEffect source;
	public readonly Tile targetTile;
	public readonly Unit attacker;
	public readonly Unit defender;
	public int amount;
	public bool cancel;

	public DamageApplicationInfo (DamageAbilityEffect source, Tile targetTile, Unit attacker, Unit defender, int amount)
	{
		this.source = source;
		this.targetTile = targetTile;
		this.attacker = attacker;
		this.defender = defender;
		this.amount = amount;
		this.cancel = false;
	}
>>>>>>> Stashed changes
}
