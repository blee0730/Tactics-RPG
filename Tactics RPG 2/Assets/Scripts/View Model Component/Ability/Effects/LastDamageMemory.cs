using UnityEngine;

public class LastDamageMemory : MonoBehaviour
{
    public int lastDamageReceived;
    public Unit lastAttacker;
    public Tile lastDamageTile;
    public int hpBeforeLastDamage;
    public int mpBeforeLastDamage;
    public Tile tileBeforeLastDamage;
    public Directions facingBeforeLastDamage;
    public bool hasLastDamage;

    public void RecordBeforeDamage(Unit attacker, int incomingDamage)
    {
        Unit owner = GetComponent<Unit>();
        Stats stats = GetComponent<Stats>();
        if (owner == null || stats == null)
            return;

        lastAttacker = attacker;
        lastDamageReceived = Mathf.Abs(incomingDamage);
        lastDamageTile = owner.tile;
        hpBeforeLastDamage = stats[StatTypes.HP];
        mpBeforeLastDamage = stats[StatTypes.MP];
        tileBeforeLastDamage = owner.tile;
        facingBeforeLastDamage = owner.dir;
        hasLastDamage = true;
    }

    public bool RestoreBeforeLastDamage()
    {
        if (!hasLastDamage)
            return false;

        Unit owner = GetComponent<Unit>();
        Stats stats = GetComponent<Stats>();
        if (owner == null || stats == null)
            return false;

        stats.SetValue(StatTypes.HP, Mathf.Clamp(hpBeforeLastDamage, 0, stats[StatTypes.MHP]), false);
        stats.SetValue(StatTypes.MP, Mathf.Clamp(mpBeforeLastDamage, 0, stats[StatTypes.MMP]), false);
        if (tileBeforeLastDamage != null && (tileBeforeLastDamage.content == null || tileBeforeLastDamage.content == owner.gameObject))
        {
            owner.Place(tileBeforeLastDamage);
            owner.dir = facingBeforeLastDamage;
            owner.Match();
        }
        return true;
    }
}
