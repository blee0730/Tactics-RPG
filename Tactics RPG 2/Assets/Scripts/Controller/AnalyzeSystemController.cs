using UnityEngine;

/// <summary>
/// Battle-level coordinator for Analyze.
/// Watches ability use globally and feeds observations into any active AnalyzeLearner.
/// </summary>
public class AnalyzeSystemController : MonoBehaviour
{
    [Header("Analyze Requirements")]
    public int defaultObservationRequirement = 100;
    public int abilityTestLabObservationRequirement = 3;

    [Header("Feedback")]
    public bool showLearnMessages = true;
    public bool logAnalyzeProgress = false;

    BattleController battle;

    void Awake()
    {
        battle = GetComponent<BattleController>();
    }

    void OnEnable()
    {
        this.AddObserver(OnAbilityDidPerform, Ability.DidPerformNotification);
    }

    void Start()
    {
        EnsureAnalyzeLearners();
    }

    void OnDisable()
    {
        this.RemoveObserver(OnAbilityDidPerform, Ability.DidPerformNotification);
    }

    public void EnsureAnalyzeLearners()
    {
        if (battle == null)
            battle = GetComponent<BattleController>();
        if (battle == null || battle.units == null)
            return;

        int requirement = GetActiveObservationRequirement();
        for (int i = 0; i < battle.units.Count; ++i)
        {
            Unit unit = battle.units[i];
            if (unit == null || !ShouldHaveAnalyze(unit))
                continue;

            AnalyzeLearner learner = unit.GetComponent<AnalyzeLearner>();
            if (learner == null)
                learner = unit.gameObject.AddComponent<AnalyzeLearner>();

            learner.defaultObservationRequirement = requirement;
        }
    }

    void OnAbilityDidPerform(object sender, object args)
    {
        Ability ability = sender as Ability;
        if (ability == null)
            return;

        Unit performer = ability.GetComponentInParent<Unit>();
        if (performer == null)
            return;

        EnsureAnalyzeLearners();

        AnalyzeLearner[] learners = GetComponentsInChildren<AnalyzeLearner>(true);
        for (int i = 0; i < learners.Length; ++i)
        {
            AnalyzeLearner learner = learners[i];
            if (learner == null || !learner.enabled || !learner.gameObject.activeInHierarchy)
                continue;

            bool learned = learner.ObserveAbility(ability, performer);
            if (learned)
                ShowLearnedMessage(learner, ability);
            else if (logAnalyzeProgress)
                Debug.Log("Analyze observed: " + ability.name + " by " + learner.name);
        }
    }

    int GetActiveObservationRequirement()
    {
        AbilityTestLabMode lab = GetComponent<AbilityTestLabMode>();
        if (lab != null && lab.enabled)
            return Mathf.Max(1, abilityTestLabObservationRequirement);

        return Mathf.Max(1, defaultObservationRequirement);
    }

    bool ShouldHaveAnalyze(Unit unit)
    {
        if (unit == null)
            return false;

        if (unit.GetComponent<AnalyzeLearner>() != null)
            return true;

        UnitProfile profile = unit.GetComponent<UnitProfile>();
        if (profile != null && AbilityCatalog.CleanName(profile.DisplayName).Contains("rein"))
            return true;

        if (AbilityCatalog.CleanName(unit.name).Contains("rein"))
            return true;

        AbilityCatalog[] catalogs = unit.GetComponentsInChildren<AbilityCatalog>(true);
        for (int i = 0; i < catalogs.Length; ++i)
        {
            if (catalogs[i] != null && AbilityCatalog.CleanName(catalogs[i].recipeName) == "rein")
                return true;
        }

        Job[] jobs = unit.GetComponentsInChildren<Job>(true);
        for (int i = 0; i < jobs.Length; ++i)
        {
            if (jobs[i] == null)
                continue;

            string jobName = AbilityCatalog.CleanName(jobs[i].name);
            if (jobName == "master of none" || jobName == "jack of all trades" || jobName == "master of all")
                return true;
        }

        return false;
    }

    void ShowLearnedMessage(AnalyzeLearner learner, Ability ability)
    {
        if (!showLearnMessages || battle == null || battle.battleMessageController == null || ability == null)
            return;

        UnitProfile profile = learner != null ? learner.GetComponent<UnitProfile>() : null;
        string learnerName = profile != null ? profile.DisplayName : (learner != null ? learner.name : "Analyze");
        battle.battleMessageController.Display(learnerName + " learned " + ability.name + "!");
    }
}
