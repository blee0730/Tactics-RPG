using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime knowledge tracker for Rein's Analyze mechanic.
///
/// V1 rule: every catalog ability is learnable. If the learner sees another unit
/// perform a catalog ability enough times, the ability is copied into the
/// learner's AbilityCatalog, unlocked, and equipped.
///
/// Partial learning rule: before the full requirement is met, Analyze can grant
/// weak partial versions at milestone stages. Test Lab uses thirds for 1/3,
/// 2/3, 3/3. Normal 100-observation learning uses fourths: 25/50/75/100.
/// Partial versions have reduced power and increased MP cost.
///
/// Self-practice rule: using a partial Analyze copy also improves Analyze progress.
/// The gain scales with the current partial power: 25% power gives +0.25 progress,
/// 50% gives +0.50, 75% gives +0.75, etc. Fully learned abilities no longer gain
/// Analyze progress from self-use; they should later feed normal mastery/use XP.
/// </summary>
public class AnalyzeLearner : MonoBehaviour
{
    public const string ObservedNotification = "AnalyzeLearner.ObservedNotification";
    public const string LearnedNotification = "AnalyzeLearner.LearnedNotification";
    public const string PartialUnlockedNotification = "AnalyzeLearner.PartialUnlockedNotification";
    public const string SelfPracticedNotification = "AnalyzeLearner.SelfPracticedNotification";

    [System.Serializable]
    public class ObservationRecord
    {
        public string key;
        public string abilityName;
        public string categoryName;
        public string entryName;
        public float observationCount;
        public int observationRequirement;
        public bool learned;
    }

    [Header("Analyze Rules")]
    public int defaultObservationRequirement = 100;
    public bool observeOwnerAbilities = false;
    public bool requireCatalogSource = true;
    public bool autoUnlockLearnedAbilities = true;
    public bool autoEquipLearnedAbilities = true;

    [Header("Partial Learning")]
    public bool grantPartialAbilities = true;
    public int normalPartialStages = 4;
    public bool useRequirementAsStagesWhenSmall = true;
    public int smallRequirementStageLimit = 4;
    public bool autoEquipPartialAbilities = true;

    [Header("Self Practice")]
    public bool selfPracticePartialAbilities = true;
    public bool selfPracticeGainScalesWithPartialPower = true;
    public float flatSelfPracticeObservationGain = 0.25f;
    public bool selfPracticeCanCompleteLearning = true;

    [Header("Runtime Data")]
    public List<ObservationRecord> observations = new List<ObservationRecord>();

    AbilityCatalog cachedCatalog;
    Unit cachedUnit;

    public AbilityCatalog Catalog
    {
        get
        {
            if (cachedCatalog == null || !cachedCatalog.gameObject.activeInHierarchy)
                cachedCatalog = GetComponentInChildren<AbilityCatalog>();
            return cachedCatalog;
        }
    }

    public Unit OwnerUnit
    {
        get
        {
            if (cachedUnit == null)
                cachedUnit = GetComponent<Unit>();
            return cachedUnit;
        }
    }

    public bool ObserveAbility(Ability sourceAbility, Unit performer)
    {
        if (sourceAbility == null)
            return false;

        Unit owner = OwnerUnit;
        bool performedByOwner = owner != null && performer == owner;

        // Rein practicing a partial Analyze copy should still teach him, even if
        // observeOwnerAbilities is false. Fully learned/self-owned normal abilities
        // still do not count as Analyze observations.
        if (performedByOwner)
        {
            if (TrySelfPracticePartialAbility(sourceAbility, out bool learnedFromPractice))
                return learnedFromPractice;

            if (!observeOwnerAbilities)
                return false;
        }

        AbilityCatalog sourceCatalog = sourceAbility.GetComponentInParent<AbilityCatalog>();
        if (requireCatalogSource && sourceCatalog == null)
            return false;

        string categoryName = sourceCatalog != null ? sourceCatalog.GetAbilityCategoryName(sourceAbility) : "Analyzed";
        string entryName = sourceCatalog != null ? sourceCatalog.GetAbilityEntryName(sourceAbility) : sourceAbility.name;
        string abilityName = sourceCatalog != null ? sourceCatalog.GetAbilityLeafName(sourceAbility) : sourceAbility.name;

        if (string.IsNullOrEmpty(entryName))
            entryName = sourceAbility.name;
        if (string.IsNullOrEmpty(abilityName))
            abilityName = sourceAbility.name;

        string key = AbilityCatalog.BuildAbilityAnalyzeKey(categoryName, entryName);
        if (string.IsNullOrEmpty(key))
            return false;

        ObservationRecord record = GetOrCreateRecord(key, abilityName, categoryName, entryName);

        if (record.learned)
            return false;

        if (IsAbilityAlreadyFullyKnown(sourceAbility, categoryName, entryName))
        {
            record.learned = true;
            record.observationCount = Mathf.Max(record.observationCount, record.observationRequirement);
            UpdatePartialAbility(sourceAbility, record, true);
            this.PostNotification(ObservedNotification, record);
            return false;
        }

        return AddObservationProgress(sourceAbility, record, 1f, true, true);
    }

    bool TrySelfPracticePartialAbility(Ability ability, out bool learned)
    {
        learned = false;

        if (!selfPracticePartialAbilities || ability == null)
            return false;

        AnalyzePartialAbilityModifier modifier = ability.GetComponent<AnalyzePartialAbilityModifier>();
        if (modifier == null || modifier.IsFullyLearned)
            return false;

        ObservationRecord record = GetRecord(modifier.analyzeKey);
        if (record == null || record.learned)
            return false;

        float gain = GetSelfPracticeObservationGain(record);
        if (gain <= 0f)
            return false;

        learned = AddObservationProgress(ability, record, gain, true, false);
        this.PostNotification(SelfPracticedNotification, record);
        return true;
    }

    bool AddObservationProgress(Ability sourceAbility, ObservationRecord record, float amount, bool canComplete, bool externalObservation)
    {
        if (sourceAbility == null || record == null || amount <= 0f)
            return false;

        if (record.learned)
            return false;

        int previousStage = GetPartialStageIndex(record);
        record.observationCount = Mathf.Min(record.observationRequirement, record.observationCount + amount);
        this.PostNotification(ObservedNotification, record);

        int currentStage = GetPartialStageIndex(record);
        if (grantPartialAbilities && currentStage > 0)
        {
            bool becameFull = record.observationCount >= record.observationRequirement;
            UpdatePartialAbility(sourceAbility, record, becameFull);

            if (currentStage > previousStage && !becameFull)
                this.PostNotification(PartialUnlockedNotification, record);
        }

        if (record.observationCount >= record.observationRequirement)
        {
            if (canComplete || selfPracticeCanCompleteLearning)
                return LearnAbility(sourceAbility, record);
        }

        return false;
    }

    public ObservationRecord GetRecord(string key)
    {
        if (string.IsNullOrEmpty(key) || observations == null)
            return null;

        string cleanKey = AbilityCatalog.CleanName(key);
        for (int i = 0; i < observations.Count; ++i)
        {
            ObservationRecord record = observations[i];
            if (record != null && AbilityCatalog.CleanName(record.key) == cleanKey)
                return record;
        }
        return null;
    }

    public int GetPartialStageCount(ObservationRecord record)
    {
        int requirement = record != null ? Mathf.Max(1, record.observationRequirement) : Mathf.Max(1, defaultObservationRequirement);
        if (useRequirementAsStagesWhenSmall && requirement <= Mathf.Max(1, smallRequirementStageLimit))
            return requirement;
        return Mathf.Max(1, normalPartialStages);
    }

    public int GetPartialStageIndex(ObservationRecord record)
    {
        if (record == null)
            return 0;

        int requirement = Mathf.Max(1, record.observationRequirement);
        int stages = GetPartialStageCount(record);

        if (record.learned || record.observationCount >= requirement)
            return stages;

        int stage = Mathf.FloorToInt(record.observationCount * (float)stages / (float)requirement);
        return Mathf.Clamp(stage, 0, stages);
    }

    public float GetPartialPowerMultiplier(ObservationRecord record)
    {
        int stages = GetPartialStageCount(record);
        int stage = GetPartialStageIndex(record);
        if (stage >= stages)
            return 1f;
        return Mathf.Clamp01((float)stage / (float)Mathf.Max(1, stages));
    }

    public float GetPartialMagicCostMultiplier(ObservationRecord record)
    {
        int stages = GetPartialStageCount(record);
        int stage = Mathf.Clamp(GetPartialStageIndex(record), 1, stages);
        if (stage >= stages)
            return 1f;
        return Mathf.Max(1f, (float)(stages - stage + 1));
    }

    public float GetSelfPracticeObservationGain(ObservationRecord record)
    {
        if (record == null || record.learned)
            return 0f;

        int stage = GetPartialStageIndex(record);
        if (stage <= 0)
            return 0f;

        float gain = selfPracticeGainScalesWithPartialPower ? GetPartialPowerMultiplier(record) : Mathf.Max(0f, flatSelfPracticeObservationGain);
        return Mathf.Clamp(gain, 0f, 0.999f);
    }

    public string GetProgressText(ObservationRecord record)
    {
        if (record == null)
            return string.Empty;

        if (record.learned)
            return record.abilityName + "  LEARNED";

        string text = string.Format("{0}  {1}/{2}", record.abilityName, GetObservationProgressText(record), record.observationRequirement);
        int stage = GetPartialStageIndex(record);
        if (stage > 0)
            text += "  " + GetPartialStatusText(record);
        return text;
    }

    public string GetObservationProgressText(ObservationRecord record)
    {
        if (record == null)
            return "0";

        float value = record.observationCount;
        if (Mathf.Approximately(value, Mathf.Round(value)))
            return Mathf.RoundToInt(value).ToString();

        return value.ToString("0.##");
    }

    public string GetSelfPracticeStatusText(ObservationRecord record)
    {
        if (record == null)
            return string.Empty;

        if (record.learned)
            return "Self-use: mastered; future uses should feed mastery XP.";

        int stage = GetPartialStageIndex(record);
        if (stage <= 0)
            return "Self-use: not available until a partial copy unlocks.";

        return string.Format("Self-use progress: +{0}/use", GetSelfPracticeObservationGain(record).ToString("0.##"));
    }

    public string GetPartialStatusText(ObservationRecord record)
    {
        if (record == null)
            return string.Empty;

        int stages = GetPartialStageCount(record);
        int stage = GetPartialStageIndex(record);
        if (record.learned || stage >= stages)
            return "FULL POWER / MPx1";
        if (stage <= 0)
            return "NOT USABLE YET";

        int percent = Mathf.RoundToInt(GetPartialPowerMultiplier(record) * 100f);
        return string.Format("PARTIAL {0}/{1}  {2}% POWER / MPx{3:0.#}", stage, stages, percent, GetPartialMagicCostMultiplier(record));
    }

    ObservationRecord GetOrCreateRecord(string key, string abilityName, string categoryName, string entryName)
    {
        ObservationRecord record = GetRecord(key);
        if (record != null)
            return record;

        record = new ObservationRecord();
        record.key = key;
        record.abilityName = CleanDisplayName(abilityName);
        record.categoryName = categoryName;
        record.entryName = entryName;
        record.observationRequirement = Mathf.Max(1, defaultObservationRequirement);
        record.observationCount = 0f;
        record.learned = false;
        observations.Add(record);
        return record;
    }

    bool LearnAbility(Ability sourceAbility, ObservationRecord record)
    {
        AbilityCatalog catalog = Catalog;
        if (catalog == null || record == null)
            return false;

        Ability learned = catalog.AddRuntimeAbilityCopy(sourceAbility, record.categoryName, record.entryName, autoUnlockLearnedAbilities, autoEquipLearnedAbilities);
        if (learned == null)
            return false;

        record.learned = true;
        record.observationCount = Mathf.Max(record.observationCount, record.observationRequirement);
        ConfigurePartialModifier(learned, record, true);
        this.PostNotification(LearnedNotification, record);
        return true;
    }

    void UpdatePartialAbility(Ability sourceAbility, ObservationRecord record, bool full)
    {
        AbilityCatalog catalog = Catalog;
        if (catalog == null || sourceAbility == null || record == null)
            return;

        int stage = full ? GetPartialStageCount(record) : GetPartialStageIndex(record);
        if (stage <= 0)
            return;

        Ability partial = catalog.AddRuntimeAbilityCopy(sourceAbility, record.categoryName, record.entryName, true, autoEquipPartialAbilities);
        if (partial == null)
            return;

        ConfigurePartialModifier(partial, record, full);
    }

    void ConfigurePartialModifier(Ability ability, ObservationRecord record, bool full)
    {
        if (ability == null || record == null)
            return;

        AnalyzePartialAbilityModifier modifier = ability.GetComponent<AnalyzePartialAbilityModifier>();
        if (modifier == null)
            modifier = ability.gameObject.AddComponent<AnalyzePartialAbilityModifier>();

        int stages = GetPartialStageCount(record);
        int stage = full ? stages : GetPartialStageIndex(record);
        modifier.Configure(record, stage, stages, full || record.learned);
    }

    bool IsAbilityAlreadyFullyKnown(Ability sourceAbility, string categoryName, string entryName)
    {
        AbilityCatalog catalog = Catalog;
        if (catalog == null)
            return false;

        Ability existing = catalog.FindAbility(entryName, false);
        if (existing == null && !string.IsNullOrEmpty(categoryName))
            existing = catalog.FindAbility(categoryName + "/" + entryName, false);

        if (existing == null || !catalog.IsAbilityUnlocked(existing))
            return false;

        AnalyzePartialAbilityModifier partial = existing.GetComponent<AnalyzePartialAbilityModifier>();
        return partial == null || partial.IsFullyLearned;
    }

    string CleanDisplayName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "Unknown Ability";

        value = value.Replace("(Clone)", "").Trim();
        return value;
    }
}
