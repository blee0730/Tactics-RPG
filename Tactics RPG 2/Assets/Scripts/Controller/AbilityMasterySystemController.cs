using UnityEngine;

/// <summary>
/// Runtime helper that makes sure every battle unit has an AbilityMasteryTracker.
/// This keeps mastery script-only for now; later the same data can be saved into
/// roster/unit progression files.
/// </summary>
public class AbilityMasterySystemController : MonoBehaviour
{
    [Header("Normal Battle Defaults")]
    public int normalFirstLevelUseRequirement = 10;
    public float normalRequirementGrowthMultiplier = 2f;

    [Header("Ability Test Lab Defaults")]
    public int testLabFirstLevelUseRequirement = 3;
    public float testLabRequirementGrowthMultiplier = 1.5f;

    [Header("Mastery Effects")]
    public int maxMasteryLevel = 12;
    public float powerBonusPerLevel = 0.05f;
    public float magicCostReductionPerLevel = 0.05f;
    public float minimumMagicCostMultiplier = 0.50f;

    public void EnsureMasteryTrackers(bool testLabMode)
    {
        Unit[] units = GetComponentsInChildren<Unit>(true);
        for (int i = 0; i < units.Length; ++i)
            EnsureMasteryTracker(units[i], testLabMode);
    }

    public AbilityMasteryTracker EnsureMasteryTracker(Unit unit, bool testLabMode)
    {
        if (unit == null)
            return null;

        AbilityMasteryTracker tracker = unit.GetComponent<AbilityMasteryTracker>();
        if (tracker == null)
            tracker = unit.gameObject.AddComponent<AbilityMasteryTracker>();

        tracker.maxMasteryLevel = Mathf.Max(1, maxMasteryLevel);
        tracker.powerBonusPerLevel = Mathf.Max(0f, powerBonusPerLevel);
        tracker.magicCostReductionPerLevel = Mathf.Max(0f, magicCostReductionPerLevel);
        tracker.minimumMagicCostMultiplier = Mathf.Clamp01(minimumMagicCostMultiplier);

        if (testLabMode)
        {
            tracker.firstLevelUseRequirement = Mathf.Max(1, testLabFirstLevelUseRequirement);
            tracker.requirementGrowthMultiplier = Mathf.Max(1f, testLabRequirementGrowthMultiplier);
        }
        else
        {
            tracker.firstLevelUseRequirement = Mathf.Max(1, normalFirstLevelUseRequirement);
            tracker.requirementGrowthMultiplier = Mathf.Max(1f, normalRequirementGrowthMultiplier);
        }

        tracker.RecalculateAllRecords();
        return tracker;
    }
}
