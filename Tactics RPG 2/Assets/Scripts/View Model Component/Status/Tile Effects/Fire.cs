using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    Tile owner;
    void OnEnable()
    {
        owner = GetComponentInParent<Tile>();
        if (owner)
            this.AddObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, owner);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, owner);
    }

    void OnTriggerEnter(Collider unit)
    {
        Tile tile = GetComponentInParent<Tile>();
        Stats s = unit.GetComponentInParent<Stats>();
        TickDamage(s);
    }

    void OnNewTurn(object sender, object args)
    {
        Stats s = owner.content.GetComponentInParent<Stats>();
        TickDamage(s);
    }

    void TickDamage(Stats s)
    {
        int currentHP = s[StatTypes.HP];
        int maxHP = s[StatTypes.MHP];
        int reduce = Mathf.Min(currentHP, Mathf.FloorToInt(maxHP * 0.1f));
        s.SetValue(StatTypes.HP, (currentHP - reduce), false);
    }
}
