using UnityEngine;

/// <summary>
/// Keeps a test unit's MP full so ability prefabs can be tested without cost setup.
/// </summary>
public class AbilityTestInfiniteMana : MonoBehaviour
{
    public int manaAmount = 9999;

    Unit owner;
    Stats stats;

    void Awake()
    {
        owner = GetComponent<Unit>();
        stats = GetComponent<Stats>();
    }

    void OnEnable()
    {
        RestoreMana();
        this.AddObserver(OnAbilityDidPerform, Ability.DidPerformNotification);
        if (owner != null)
            this.AddObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnAbilityDidPerform, Ability.DidPerformNotification);
        if (owner != null)
            this.RemoveObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
    }

    void LateUpdate()
    {
        RestoreMana();
    }

    void OnTurnBegan(object sender, object args)
    {
        RestoreMana();
    }

    void OnAbilityDidPerform(object sender, object args)
    {
        Ability ability = sender as Ability;
        if (ability == null || owner == null)
            return;

        Unit abilityOwner = ability.GetComponentInParent<Unit>();
        if (abilityOwner == owner)
            RestoreMana();
    }

    void RestoreMana()
    {
        if (stats == null)
            stats = GetComponent<Stats>();
        if (stats == null)
            return;

        int amount = Mathf.Max(1, manaAmount);
        if (stats[StatTypes.MMP] != amount)
            stats.SetValue(StatTypes.MMP, amount, false);
        if (stats[StatTypes.MP] != amount)
            stats.SetValue(StatTypes.MP, amount, false);
    }
}
