using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    void OnTriggerEnter(Collider unit)
    {
        Tile tile = GetComponentInParent<Tile>();
        Unit target = unit.GetComponentInParent<Unit>();
        Stats s = unit.GetComponentInParent<Stats>();
        target.Place(tile);
        TickDamage(s);
        target.cantMove = true;
        Destroy(gameObject);
    }

    void TickDamage(Stats s)
    {
        int currentHP = s[StatTypes.HP];
        int maxHP = s[StatTypes.MHP];
        int reduce = Mathf.Min(currentHP, Mathf.FloorToInt(maxHP * 0.1f));
        s.SetValue(StatTypes.HP, currentHP - reduce, false);
    }
}
