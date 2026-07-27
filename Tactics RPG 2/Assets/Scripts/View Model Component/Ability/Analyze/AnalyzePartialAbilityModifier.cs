using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime modifier attached to abilities that Rein has only partially learned
/// through Analyze. It scales power downward and MP cost upward until the ability
/// is fully learned.
/// </summary>
public class AnalyzePartialAbilityModifier : MonoBehaviour
{
    [Header("Analyze Progress")]
    public string analyzeKey;
    public string abilityName;
    public string categoryName;
    public float observationCount;
    public int observationRequirement = 1;
    public int stageIndex;
    public int stageCount = 4;
    public bool fullyLearned;

    Ability owner;

    public bool IsFullyLearned
    {
        get { return fullyLearned || stageIndex >= Mathf.Max(1, stageCount); }
    }

    public float PowerMultiplier
    {
        get
        {
            if (IsFullyLearned)
                return 1f;
            return Mathf.Clamp01((float)Mathf.Max(0, stageIndex) / (float)Mathf.Max(1, stageCount));
        }
    }

    public float MagicCostMultiplier
    {
        get
        {
            if (IsFullyLearned)
                return 1f;

            int stages = Mathf.Max(1, stageCount);
            int stage = Mathf.Clamp(stageIndex, 1, stages);
            return Mathf.Max(1f, (float)(stages - stage + 1));
        }
    }

    public int PowerPercent
    {
        get { return Mathf.RoundToInt(PowerMultiplier * 100f); }
    }

    public string MenuSuffix
    {
        get
        {
            if (IsFullyLearned)
                return string.Empty;
            return string.Format(" ({0}% / MPx{1:0.#})", PowerPercent, MagicCostMultiplier);
        }
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

    public void Configure(AnalyzeLearner.ObservationRecord record, int newStageIndex, int newStageCount, bool newFullyLearned)
    {
        if (record != null)
        {
            analyzeKey = record.key;
            abilityName = record.abilityName;
            categoryName = record.categoryName;
            observationCount = record.observationCount;
            observationRequirement = Mathf.Max(1, record.observationRequirement);
        }

        stageCount = Mathf.Max(1, newStageCount);
        stageIndex = Mathf.Clamp(newStageIndex, 0, stageCount);
        fullyLearned = newFullyLearned || stageIndex >= stageCount;
    }

    public void ConfigureFull(AnalyzeLearner.ObservationRecord record)
    {
        int stages = record != null ? Mathf.Max(1, record.observationRequirement) : Mathf.Max(1, stageCount);
        Configure(record, stages, stages, true);
    }

    void OnGetPower(object sender, object args)
    {
        if (!IsMyAbilityEffect(sender))
            return;

        float multiplier = PowerMultiplier;
        if (multiplier >= 0.999f)
            return;

        var info = args as Info<Unit, Unit, List<ValueModifier>>;
        if (info == null || info.arg2 == null)
            return;

        info.arg2.Add(new MultValueModifier(1000, multiplier));
    }

    void OnGetMagicCost(object sender, object args)
    {
        if (!IsMyMagicCost(sender))
            return;

        float multiplier = MagicCostMultiplier;
        if (multiplier <= 1.001f)
            return;

        List<ValueModifier> modifiers = args as List<ValueModifier>;
        if (modifiers == null)
            return;

        modifiers.Add(new MultValueModifier(1000, multiplier));
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

    public static AnalyzePartialAbilityModifier Get(Ability ability)
    {
        return ability != null ? ability.GetComponent<AnalyzePartialAbilityModifier>() : null;
    }

    public static string GetMenuSuffix(Ability ability)
    {
        AnalyzePartialAbilityModifier modifier = Get(ability);
        return modifier != null ? modifier.MenuSuffix : string.Empty;
    }
}
