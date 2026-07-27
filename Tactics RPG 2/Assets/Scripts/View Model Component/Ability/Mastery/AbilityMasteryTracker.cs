using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Tracks how much a unit has personally practiced each ability.
/// Every unit can have this component. It listens for Ability.DidPerformNotification
/// from abilities owned by this unit, then applies mastery bonuses back to that
/// unit's copies of the ability.
///
/// V1 effects:
/// - +1 use whenever the owner successfully performs an ability.
/// - Mastery levels are based on configurable use thresholds.
/// - Each mastery level increases power and reduces MP cost.
/// - The data is runtime-only for now; later it can be saved to roster data.
/// </summary>
public class AbilityMasteryTracker : MonoBehaviour
{
    public const string ChangedNotification = "AbilityMasteryTracker.ChangedNotification";

    [Serializable]
    public class MasteryRecord
    {
        public string key;
        public string abilityName;
        public string categoryName;
        public string entryName;
        public int useCount;
        public int masteryLevel;
        public int usesAtCurrentLevel;
        public int usesForNextLevel;
        public bool maxed;
    }

    [Header("Progression")]
    public int maxMasteryLevel = 12;
    public int firstLevelUseRequirement = 10;
    public float requirementGrowthMultiplier = 2f;

    [Header("Tuple Unlocks")]
    public bool unlockTupleByMastery = true;
    public int maxTupleCount = 12;

    [Header("Battle Effects")]
    public bool applyPowerBonus = true;
    public float powerBonusPerLevel = 0.05f;
    public bool applyMagicCostReduction = true;
    public float magicCostReductionPerLevel = 0.05f;
    public float minimumMagicCostMultiplier = 0.50f;

    [Header("Runtime Data")]
    public List<MasteryRecord> records = new List<MasteryRecord>();

    Unit cachedUnit;

    public Unit OwnerUnit
    {
        get
        {
            if (cachedUnit == null)
                cachedUnit = GetComponent<Unit>();
            return cachedUnit;
        }
    }

    void OnEnable()
    {
        this.AddObserver(OnAbilityDidPerform, Ability.DidPerformNotification);
        this.AddObserver(OnGetPower, BaseAbilityEffect.GetPowerNotification);
        this.AddObserver(OnGetMagicCost, AbilityMagicCost.GetCostNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnAbilityDidPerform, Ability.DidPerformNotification);
        this.RemoveObserver(OnGetPower, BaseAbilityEffect.GetPowerNotification);
        this.RemoveObserver(OnGetMagicCost, AbilityMagicCost.GetCostNotification);
    }

    public void ConfigureForBattle(bool testLabMode)
    {
        if (testLabMode)
        {
            firstLevelUseRequirement = 3;
            requirementGrowthMultiplier = 1.5f;
        }
        else
        {
            firstLevelUseRequirement = 10;
            requirementGrowthMultiplier = 2f;
        }

        maxMasteryLevel = Mathf.Max(1, maxMasteryLevel);
        maxTupleCount = Mathf.Clamp(maxTupleCount, 1, TupleAbilityModifier.MaxTupleCount);
        firstLevelUseRequirement = Mathf.Max(1, firstLevelUseRequirement);
        requirementGrowthMultiplier = Mathf.Max(1f, requirementGrowthMultiplier);
    }

    void OnAbilityDidPerform(object sender, object args)
    {
        Ability ability = sender as Ability;
        if (!IsOwnedAbility(ability))
            return;

        // Rein's Analyze partial copies are intentionally practice tools for Analyze,
        // not true mastery training. They only begin earning normal mastery once the
        // ability is fully learned / completed.
        if (IsIncompleteAnalyzeCopy(ability))
            return;

        AddUse(ability, 1);
    }

    void OnGetPower(object sender, object args)
    {
        if (!applyPowerBonus)
            return;

        Ability ability = AbilityFromSender(sender);
        if (!IsOwnedAbility(ability))
            return;

        if (IsIncompleteAnalyzeCopy(ability))
            return;

        int level = GetMasteryLevel(ability);
        if (level <= 0)
            return;

        Info<Unit, Unit, List<ValueModifier>> info = args as Info<Unit, Unit, List<ValueModifier>>;
        if (info == null || info.arg2 == null)
            return;

        info.arg2.Add(new MultValueModifier(1100, GetPowerMultiplier(level)));
    }

    void OnGetMagicCost(object sender, object args)
    {
        if (!applyMagicCostReduction)
            return;

        AbilityMagicCost cost = sender as AbilityMagicCost;
        if (cost == null)
            return;

        Ability ability = cost.GetComponent<Ability>();
        if (!IsOwnedAbility(ability))
            return;

        if (IsIncompleteAnalyzeCopy(ability))
            return;

        int level = GetMasteryLevel(ability);
        if (level <= 0)
            return;

        List<ValueModifier> modifiers = args as List<ValueModifier>;
        if (modifiers == null)
            return;

        modifiers.Add(new MultValueModifier(1100, GetMagicCostMultiplier(level)));
    }

    public MasteryRecord AddUse(Ability ability, int amount)
    {
        if (ability == null || amount <= 0)
            return null;

        MasteryRecord record = GetRecordForAbility(ability, true);
        if (record == null)
            return null;

        record.useCount += amount;
        RecalculateRecord(record);
        this.PostNotification(ChangedNotification, record);
        return record;
    }

    public MasteryRecord GetRecordForAbility(Ability ability, bool createIfMissing)
    {
        if (ability == null)
            return null;

        AbilityCatalog catalog = ability.GetComponentInParent<AbilityCatalog>();
        string categoryName = catalog != null ? catalog.GetAbilityCategoryName(ability) : string.Empty;
        string entryName = catalog != null ? catalog.GetAbilityEntryName(ability) : ability.name;
        string abilityName = catalog != null ? catalog.GetAbilityLeafName(ability) : ability.name;

        if (string.IsNullOrEmpty(entryName))
            entryName = ability.name;
        if (string.IsNullOrEmpty(abilityName))
            abilityName = ability.name;

        string key = AbilityCatalog.BuildAbilityAnalyzeKey(categoryName, entryName);
        if (string.IsNullOrEmpty(key))
            key = AbilityCatalog.CleanName(ability.name);

        MasteryRecord existing = GetRecord(key);
        if (existing != null || !createIfMissing)
            return existing;

        MasteryRecord record = new MasteryRecord();
        record.key = key;
        record.abilityName = CleanDisplayName(abilityName);
        record.categoryName = categoryName;
        record.entryName = entryName;
        record.useCount = 0;
        records.Add(record);
        RecalculateRecord(record);
        return record;
    }

    public MasteryRecord GetRecord(string key)
    {
        if (string.IsNullOrEmpty(key) || records == null)
            return null;

        string clean = AbilityCatalog.CleanName(key);
        for (int i = 0; i < records.Count; ++i)
        {
            MasteryRecord record = records[i];
            if (record != null && AbilityCatalog.CleanName(record.key) == clean)
                return record;
        }
        return null;
    }

    public int GetMasteryLevel(Ability ability)
    {
        MasteryRecord record = GetRecordForAbility(ability, false);
        return record != null ? record.masteryLevel : 0;
    }


    public int GetMaxTupleCount(Ability ability)
    {
        if (!unlockTupleByMastery)
            return 1;

        if (IsIncompleteAnalyzeCopy(ability))
            return 1;

        return GetMaxTupleCountForMasteryLevel(GetMasteryLevel(ability));
    }

    public int GetMaxTupleCountForMasteryLevel(int masteryLevel)
    {
        if (!unlockTupleByMastery)
            return 1;

        int level = Mathf.Max(0, masteryLevel);
        int cap = Mathf.Clamp(maxTupleCount, 1, TupleAbilityModifier.MaxTupleCount);
        return Mathf.Clamp(level + 1, 1, cap);
    }

    public string GetTupleSummaryForMasteryLevel(int masteryLevel)
    {
        int maxTuple = GetMaxTupleCountForMasteryLevel(masteryLevel);
        if (maxTuple <= 1)
            return "Tuple: Single only";
        return string.Format("Tuple: Single through x{0}", maxTuple);
    }

    public float GetPowerMultiplier(int masteryLevel)
    {
        int level = Mathf.Clamp(masteryLevel, 0, Mathf.Max(1, maxMasteryLevel));
        return 1f + powerBonusPerLevel * level;
    }

    public float GetMagicCostMultiplier(int masteryLevel)
    {
        int level = Mathf.Clamp(masteryLevel, 0, Mathf.Max(1, maxMasteryLevel));
        float multiplier = 1f - magicCostReductionPerLevel * level;
        return Mathf.Clamp(multiplier, Mathf.Clamp01(minimumMagicCostMultiplier), 1f);
    }

    public string GetSummaryForAbility(Ability ability)
    {
        if (IsIncompleteAnalyzeCopy(ability))
            return "Partial Analyze copy — mastery starts after full learning.";

        MasteryRecord record = GetRecordForAbility(ability, false);
        if (record == null)
        {
            int firstReq = UsesRequiredForLevel(1);
            return string.Format("Mastery Lv 0   Uses 0/{0}   Power +0%   MP -0%", firstReq);
        }

        string nextText = record.maxed ? "MAX" : string.Format("{0}/{1}", record.usesAtCurrentLevel, record.usesForNextLevel);
        int powerPercent = Mathf.RoundToInt((GetPowerMultiplier(record.masteryLevel) - 1f) * 100f);
        int costPercent = Mathf.RoundToInt((1f - GetMagicCostMultiplier(record.masteryLevel)) * 100f);
        return string.Format("Mastery Lv {0}   Uses {1}   Next {2}   Power +{3}%   MP -{4}%   {5}",
            record.masteryLevel,
            record.useCount,
            nextText,
            powerPercent,
            costPercent,
            GetTupleSummaryForMasteryLevel(record.masteryLevel));
    }

    public void RecalculateAllRecords()
    {
        if (records == null)
            return;

        for (int i = 0; i < records.Count; ++i)
            RecalculateRecord(records[i]);
    }

    void RecalculateRecord(MasteryRecord record)
    {
        if (record == null)
            return;

        int maxLevel = Mathf.Max(1, maxMasteryLevel);
        int uses = Mathf.Max(0, record.useCount);
        int level = 0;

        for (int i = 1; i <= maxLevel; ++i)
        {
            if (uses >= TotalUsesRequiredForLevel(i))
                level = i;
            else
                break;
        }

        record.masteryLevel = level;
        record.maxed = level >= maxLevel;

        if (record.maxed)
        {
            record.usesAtCurrentLevel = uses;
            record.usesForNextLevel = 0;
        }
        else
        {
            int currentFloor = level <= 0 ? 0 : TotalUsesRequiredForLevel(level);
            int nextCeiling = TotalUsesRequiredForLevel(level + 1);
            record.usesAtCurrentLevel = uses - currentFloor;
            record.usesForNextLevel = Mathf.Max(1, nextCeiling - currentFloor);
        }
    }

    public int TotalUsesRequiredForLevel(int level)
    {
        level = Mathf.Clamp(level, 1, Mathf.Max(1, maxMasteryLevel));

        int total = 0;
        for (int i = 1; i <= level; ++i)
            total += UsesRequiredForLevel(i);
        return total;
    }

    public int UsesRequiredForLevel(int level)
    {
        level = Mathf.Max(1, level);
        float value = Mathf.Max(1, firstLevelUseRequirement) * Mathf.Pow(Mathf.Max(1f, requirementGrowthMultiplier), level - 1);
        return Mathf.Max(1, Mathf.CeilToInt(value));
    }

    bool IsOwnedAbility(Ability ability)
    {
        if (ability == null)
            return false;

        Unit owner = OwnerUnit;
        if (owner == null)
            return false;

        Unit abilityOwner = ability.GetComponentInParent<Unit>();
        return abilityOwner == owner;
    }

    bool IsIncompleteAnalyzeCopy(Ability ability)
    {
        AnalyzePartialAbilityModifier modifier = AnalyzePartialAbilityModifier.Get(ability);
        return modifier != null && !modifier.IsFullyLearned;
    }

    Ability AbilityFromSender(object sender)
    {
        MonoBehaviour behaviour = sender as MonoBehaviour;
        if (behaviour == null)
            return null;
        return behaviour.GetComponentInParent<Ability>();
    }

    string CleanDisplayName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "Unknown Ability";
        return value.Replace("(Clone)", "").Trim();
    }

    public static AbilityMasteryTracker GetTrackerForAbility(Ability ability)
    {
        if (ability == null)
            return null;

        Unit unit = ability.GetComponentInParent<Unit>();
        return unit != null ? unit.GetComponent<AbilityMasteryTracker>() : null;
    }

    public static string GetMenuSuffix(Ability ability)
    {
        AnalyzePartialAbilityModifier partial = AnalyzePartialAbilityModifier.Get(ability);
        if (partial != null && !partial.IsFullyLearned)
            return string.Empty;

        AbilityMasteryTracker tracker = GetTrackerForAbility(ability);
        if (tracker == null)
            return string.Empty;

        MasteryRecord record = tracker.GetRecordForAbility(ability, false);
        if (record == null || record.masteryLevel <= 0)
            return string.Empty;

        return string.Format(" [M{0}]", record.masteryLevel);
    }
}
