using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime modifier used by Tuple System v1.
///
/// A tuple cast is selected from the ability menu as x2, x3, etc. The selected
/// tuple count is stored on the ability until the cast finishes. While active,
/// this modifier scales normal power-based effects and MP cost by the tuple count.
///
/// V1 intentionally uses the existing power/cost notification pipeline instead
/// of trying to custom-repeat every possible ability effect. Damage and healing
/// abilities get the expected larger numbers immediately. Utility/status/terrain
/// abilities still pay the higher tuple cost, and can get custom tuple behavior later.
/// </summary>
public class TupleAbilityModifier : MonoBehaviour
{
    public const int MinTupleCount = 1;
    public const int MaxTupleCount = 12;

    [Header("Runtime Tuple State")]
    [SerializeField] int activeTupleCount = 1;
    [SerializeField] int previewTupleCount = 1;
    [SerializeField] bool previewMode;

    Ability owner;

    public int ActiveTupleCount
    {
        get { return Mathf.Clamp(activeTupleCount, MinTupleCount, MaxTupleCount); }
    }

    public bool HasActiveTuple
    {
        get { return ActiveTupleCount > 1; }
    }

    void Awake()
    {
        owner = GetComponent<Ability>();
    }

    void OnEnable()
    {
        if (owner == null)
            owner = GetComponent<Ability>();

        this.AddObserver(OnGetPower, BaseAbilityEffect.GetPowerNotification);
        this.AddObserver(OnGetMagicCost, AbilityMagicCost.GetCostNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnGetPower, BaseAbilityEffect.GetPowerNotification);
        this.RemoveObserver(OnGetMagicCost, AbilityMagicCost.GetCostNotification);
    }

    public void SetActiveTuple(int tupleCount)
    {
        activeTupleCount = Mathf.Clamp(tupleCount, MinTupleCount, MaxTupleCount);
    }

    public void ClearActiveTuple()
    {
        activeTupleCount = MinTupleCount;
        previewMode = false;
        previewTupleCount = MinTupleCount;
    }

    public void BeginPreview(int tupleCount)
    {
        previewTupleCount = Mathf.Clamp(tupleCount, MinTupleCount, MaxTupleCount);
        previewMode = true;
    }

    public void EndPreview()
    {
        previewMode = false;
        previewTupleCount = MinTupleCount;
    }

    int CurrentTupleCount()
    {
        return Mathf.Clamp(previewMode ? previewTupleCount : activeTupleCount, MinTupleCount, MaxTupleCount);
    }

    void OnGetPower(object sender, object args)
    {
        if (!IsMyAbilityEffect(sender))
            return;

        int tupleCount = CurrentTupleCount();
        if (tupleCount <= 1)
            return;

        Info<Unit, Unit, List<ValueModifier>> info = args as Info<Unit, Unit, List<ValueModifier>>;
        if (info == null || info.arg2 == null)
            return;

        // Sort order 1050 intentionally comes after Analyze partial scaling (1000)
        // and before mastery bonus/cost reduction (1100).
        info.arg2.Add(new MultValueModifier(1050, tupleCount));
    }

    void OnGetMagicCost(object sender, object args)
    {
        if (!IsMyMagicCost(sender))
            return;

        int tupleCount = CurrentTupleCount();
        if (tupleCount <= 1)
            return;

        List<ValueModifier> modifiers = args as List<ValueModifier>;
        if (modifiers == null)
            return;

        // Tuple cost is multiplied first, then mastery can reduce that final cost.
        modifiers.Add(new MultValueModifier(1050, tupleCount));
    }

    bool IsMyAbilityEffect(object sender)
    {
        if (owner == null)
            owner = GetComponent<Ability>();
        if (owner == null)
            return false;

        MonoBehaviour behaviour = sender as MonoBehaviour;
        if (behaviour == null)
            return false;

        Ability ability = behaviour.GetComponentInParent<Ability>();
        return ability == owner;
    }

    bool IsMyMagicCost(object sender)
    {
        if (owner == null)
            owner = GetComponent<Ability>();
        if (owner == null)
            return false;

        AbilityMagicCost cost = sender as AbilityMagicCost;
        if (cost == null)
            return false;

        Ability ability = cost.GetComponent<Ability>();
        return ability == owner;
    }

    public static TupleAbilityModifier Get(Ability ability)
    {
        return ability != null ? ability.GetComponent<TupleAbilityModifier>() : null;
    }

    public static TupleAbilityModifier GetOrAdd(Ability ability)
    {
        if (ability == null)
            return null;

        TupleAbilityModifier modifier = ability.GetComponent<TupleAbilityModifier>();
        if (modifier == null)
            modifier = ability.gameObject.AddComponent<TupleAbilityModifier>();
        return modifier;
    }

    public static void SetActive(Ability ability, int tupleCount)
    {
        TupleAbilityModifier modifier = GetOrAdd(ability);
        if (modifier != null)
            modifier.SetActiveTuple(tupleCount);
    }

    public static void ClearActive(Ability ability)
    {
        TupleAbilityModifier modifier = Get(ability);
        if (modifier != null)
            modifier.ClearActiveTuple();
    }

    public static bool CanPerformWithTuple(Ability ability, int tupleCount)
    {
        if (ability == null)
            return false;

        TupleAbilityModifier modifier = GetOrAdd(ability);
        if (modifier == null)
            return ability.CanPerform();

        modifier.BeginPreview(tupleCount);
        bool canPerform = ability.CanPerform();
        modifier.EndPreview();
        return canPerform;
    }

    public static int GetEffectiveMagicCost(Ability ability, int tupleCount)
    {
        if (ability == null)
            return 0;

        AbilityMagicCost cost = ability.GetComponent<AbilityMagicCost>();
        if (cost == null)
            return 0;

        TupleAbilityModifier modifier = GetOrAdd(ability);
        if (modifier == null)
            return cost.EffectiveAmount;

        modifier.BeginPreview(tupleCount);
        int amount = cost.EffectiveAmount;
        modifier.EndPreview();
        return amount;
    }

    public static string GetMenuSuffix(int tupleCount)
    {
        tupleCount = Mathf.Clamp(tupleCount, MinTupleCount, MaxTupleCount);
        return tupleCount > 1 ? string.Format(" x{0}", tupleCount) : string.Empty;
    }
}
