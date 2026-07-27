using UnityEngine;
using System.Collections;

public class GrantExtraTurnAbilityEffect : BaseAbilityEffect
{
    // Kept with the old class name so the existing Accel prefab does not need rewiring.
    // This now applies an Accel status instead of refreshing the current turn immediately.
    public int duration = 3;
    public int movesPerTurn = 2;
    public int actionsPerTurn = 2;
    public bool consumeCurrentTurn = true;

    Ability owner;

    void Awake ()
    {
        owner = GetComponentInParent<Ability>();
    }

    void OnEnable ()
    {
        if (owner == null)
            owner = GetComponentInParent<Ability>();
        if (owner != null)
            this.AddObserver(OnCanPerformCheck, Ability.CanPerformCheck, owner);
    }

    void OnDisable ()
    {
        if (owner != null)
            this.RemoveObserver(OnCanPerformCheck, Ability.CanPerformCheck, owner);
    }

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        Unit caster = GetComponentInParent<Unit>();
        Unit recipient = GetTargetUnit(target);
        if (recipient == null)
            recipient = caster;
        if (recipient == null)
            return 0;

        Status status = recipient.GetComponent<Status>();
        if (status == null)
            status = recipient.GetComponentInChildren<Status>();
        if (status == null)
            return 0;

        TurnDurationStatusCondition condition = status.Add<AccelStatusEffect, TurnDurationStatusCondition>();
        condition.duration = duration;

        AccelStatusEffect effect = condition.GetComponentInParent<AccelStatusEffect>();
        effect.movesPerTurn = Mathf.Max(1, movesPerTurn);
        effect.actionsPerTurn = Mathf.Max(1, actionsPerTurn);

        BattleController battle = GameObject.FindObjectOfType<BattleController>();
        if (consumeCurrentTurn && battle != null && battle.turn != null && battle.turn.actor == caster)
            battle.turn.ConsumeRemainingCommands();

        return 0;
    }

    void OnCanPerformCheck (object sender, object args)
    {
        Ability ability = sender as Ability;
        BaseException exc = args as BaseException;
        if (ability == null || exc == null || ability != owner)
            return;

        Unit caster = ability.GetComponentInParent<Unit>();
        if (caster == null)
            return;

        if (caster.GetComponentInChildren<AccelStatusEffect>() != null && exc.toggle == true)
            exc.FlipToggle();
    }

    Unit GetTargetUnit (Tile target)
    {
        if (target == null || target.content == null)
            return null;
        return target.content.GetComponent<Unit>();
    }
}
