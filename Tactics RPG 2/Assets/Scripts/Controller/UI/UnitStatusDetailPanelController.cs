using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Fire Emblem-style full unit page used for battle testing and tactical inspection.
/// Built entirely at runtime so the existing scene/prefabs do not need to be rewired.
/// Includes a dedicated side page panel for Ability Mastery on every unit, and Analyze on Rein.
/// </summary>
public class UnitStatusDetailPanelController : MonoBehaviour
{
    [Header("Runtime UI")]
    [SerializeField] Canvas canvas;
    [SerializeField] RectTransform panelRoot;
    [SerializeField] RectTransform analyzePanelRoot;
    [SerializeField] Image portraitImage;
    [SerializeField] Text nameLabel;
    [SerializeField] Text jobLabel;
    [SerializeField] Text levelLabel;
    [SerializeField] Text hpLabel;
    [SerializeField] Text mpLabel;
    [SerializeField] Text moveLabel;
    [SerializeField] Text statusSummaryLabel;
    [SerializeField] Text bodyLabel;
    [SerializeField] Text sidePanelTitleLabel;
    [SerializeField] Text analyzeSummaryLabel;
    [SerializeField] Text analyzeBodyLabel;

    [Header("Layout")]
    [SerializeField] Vector2 panelSize = new Vector2(760f, 720f);
    [SerializeField] Vector2 panelOffset = new Vector2(26f, 0f);
    [SerializeField] Vector2 analyzePanelSize = new Vector2(360f, 720f);
    [SerializeField] Vector2 analyzePanelOffset = new Vector2(800f, 0f);
    [SerializeField] float refreshInterval = 0.15f;

    enum SidePage
    {
        Mastery,
        Analyze
    }

    SidePage sidePage = SidePage.Mastery;

    readonly Dictionary<StatTypes, Text> statValueLabels = new Dictionary<StatTypes, Text>();
    readonly List<Text> weaponMasteryLabels = new List<Text>();
    readonly List<Text> inventorySlotLabels = new List<Text>();

    GameObject target;
    Stats observedStats;
    Status observedStatus;
    Equipment observedEquipment;
    AnalyzeLearner observedAnalyzeLearner;
    AbilityMasteryTracker observedMasteryTracker;
    float nextRefreshTime;

    static readonly StatTypes[] displayedStats = new StatTypes[]
    {
        StatTypes.STR,
        StatTypes.MAG,
        StatTypes.SKL,
        StatTypes.SPD,
        StatTypes.DEF,
        StatTypes.LCK,
        StatTypes.RES,
        StatTypes.FRT,
        StatTypes.JMP
    };

    static readonly StatModifierFeature.WeaponType[] displayedWeaponTypes = new StatModifierFeature.WeaponType[]
    {
        StatModifierFeature.WeaponType.sword,
        StatModifierFeature.WeaponType.spear,
        StatModifierFeature.WeaponType.bow,
        StatModifierFeature.WeaponType.axe,
        StatModifierFeature.WeaponType.staff,
        StatModifierFeature.WeaponType.whip,
        StatModifierFeature.WeaponType.bottle,
        StatModifierFeature.WeaponType.gauntlet,
        StatModifierFeature.WeaponType.shield,
        StatModifierFeature.WeaponType.dagger,
        StatModifierFeature.WeaponType.hammer,
        StatModifierFeature.WeaponType.fan
    };

    public bool IsShowing
    {
        get { return panelRoot != null && panelRoot.gameObject.activeSelf; }
    }

    void Awake()
    {
        BuildUIIfNeeded();
        Hide();
    }

    void OnDisable()
    {
        RemoveObservers();
    }

    void Update()
    {
        if (!IsShowing)
            return;

        if (target == null)
        {
            Hide();
            return;
        }

        if (Time.unscaledTime >= nextRefreshTime)
        {
            nextRefreshTime = Time.unscaledTime + Mathf.Max(0.05f, refreshInterval);
            Refresh();
        }
    }

    public void Show(GameObject unit)
    {
        BuildUIIfNeeded();
        RemoveObservers();

        target = unit;
        observedStats = target != null ? target.GetComponent<Stats>() : null;
        observedStatus = target != null ? target.GetComponent<Status>() : null;
        observedEquipment = target != null ? target.GetComponent<Equipment>() : null;
        observedAnalyzeLearner = target != null ? target.GetComponent<AnalyzeLearner>() : null;
        observedMasteryTracker = target != null ? target.GetComponent<AbilityMasteryTracker>() : null;
        sidePage = SidePage.Mastery;

        AddObservers();

        if (panelRoot != null)
            panelRoot.gameObject.SetActive(true);
        if (canvas != null)
            canvas.gameObject.SetActive(true);

        Refresh();
    }

    public void Hide()
    {
        RemoveObservers();
        target = null;
        observedStats = null;
        observedStatus = null;
        observedEquipment = null;
        observedAnalyzeLearner = null;
        observedMasteryTracker = null;

        if (panelRoot != null)
            panelRoot.gameObject.SetActive(false);
        if (analyzePanelRoot != null)
            analyzePanelRoot.gameObject.SetActive(false);
    }

    public void Refresh()
    {
        if (target == null)
            return;

        UnitProfile profile = target.GetComponent<UnitProfile>();
        Stats stats = target.GetComponent<Stats>();

        string displayName = profile != null ? profile.DisplayName : target.name;
        if (nameLabel != null)
            nameLabel.text = displayName;

        if (portraitImage != null)
        {
            Sprite portrait = profile != null ? profile.statusPortrait : null;
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
        }

        if (jobLabel != null)
            jobLabel.text = GetJobName(target);

        if (levelLabel != null)
            levelLabel.text = stats != null ? "LV  " + stats[StatTypes.LVL].ToString() : "LV  --";

        if (hpLabel != null)
            hpLabel.text = stats != null ? "HP  " + stats[StatTypes.HP] + " / " + stats[StatTypes.MHP] : "HP  -- / --";

        if (mpLabel != null)
            mpLabel.text = stats != null ? "MP  " + stats[StatTypes.MP] + " / " + stats[StatTypes.MMP] : "MP  -- / --";

        if (moveLabel != null)
            moveLabel.text = stats != null ? "MV  " + stats[StatTypes.MOV].ToString() : "MV  --";

        RefreshStatGrid(stats);
        RefreshWeaponMastery(target);
        RefreshInventorySlots(target);
        RefreshStatusSummary(target);
        RefreshSidePanel(target);

        if (bodyLabel != null)
            bodyLabel.text = BuildDebugBody(target);
    }

    void AddObservers()
    {
        if (observedStats != null)
        {
            for (int i = 0; i < (int)StatTypes.Count; ++i)
                this.AddObserver(OnObservedUnitChanged, Stats.DidChangeNotification((StatTypes)i), observedStats);
        }

        if (observedStatus != null)
        {
            this.AddObserver(OnObservedUnitChanged, Status.AddedNotification, observedStatus);
            this.AddObserver(OnObservedUnitChanged, Status.RemovedNotification, observedStatus);
        }

        if (observedEquipment != null)
        {
            this.AddObserver(OnObservedUnitChanged, Equipment.EquippedNotification, observedEquipment);
            this.AddObserver(OnObservedUnitChanged, Equipment.UnEquippedNotification, observedEquipment);
        }

        if (observedAnalyzeLearner != null)
        {
            this.AddObserver(OnObservedUnitChanged, AnalyzeLearner.ObservedNotification, observedAnalyzeLearner);
            this.AddObserver(OnObservedUnitChanged, AnalyzeLearner.PartialUnlockedNotification, observedAnalyzeLearner);
            this.AddObserver(OnObservedUnitChanged, AnalyzeLearner.SelfPracticedNotification, observedAnalyzeLearner);
            this.AddObserver(OnObservedUnitChanged, AnalyzeLearner.LearnedNotification, observedAnalyzeLearner);
        }

        if (observedMasteryTracker != null)
            this.AddObserver(OnObservedUnitChanged, AbilityMasteryTracker.ChangedNotification, observedMasteryTracker);
    }

    void RemoveObservers()
    {
        if (observedStats != null)
        {
            for (int i = 0; i < (int)StatTypes.Count; ++i)
                this.RemoveObserver(OnObservedUnitChanged, Stats.DidChangeNotification((StatTypes)i), observedStats);
        }

        if (observedStatus != null)
        {
            this.RemoveObserver(OnObservedUnitChanged, Status.AddedNotification, observedStatus);
            this.RemoveObserver(OnObservedUnitChanged, Status.RemovedNotification, observedStatus);
        }

        if (observedEquipment != null)
        {
            this.RemoveObserver(OnObservedUnitChanged, Equipment.EquippedNotification, observedEquipment);
            this.RemoveObserver(OnObservedUnitChanged, Equipment.UnEquippedNotification, observedEquipment);
        }

        if (observedAnalyzeLearner != null)
        {
            this.RemoveObserver(OnObservedUnitChanged, AnalyzeLearner.ObservedNotification, observedAnalyzeLearner);
            this.RemoveObserver(OnObservedUnitChanged, AnalyzeLearner.PartialUnlockedNotification, observedAnalyzeLearner);
            this.RemoveObserver(OnObservedUnitChanged, AnalyzeLearner.SelfPracticedNotification, observedAnalyzeLearner);
            this.RemoveObserver(OnObservedUnitChanged, AnalyzeLearner.LearnedNotification, observedAnalyzeLearner);
        }

        if (observedMasteryTracker != null)
            this.RemoveObserver(OnObservedUnitChanged, AbilityMasteryTracker.ChangedNotification, observedMasteryTracker);
    }

    void OnObservedUnitChanged(object sender, object args)
    {
        Refresh();
    }


    public void CycleSidePage()
    {
        if (!IsShowing)
            return;

        if (observedAnalyzeLearner != null)
            sidePage = sidePage == SidePage.Mastery ? SidePage.Analyze : SidePage.Mastery;
        else
            sidePage = SidePage.Mastery;

        RefreshSidePanel(target);
    }

    public bool CanCycleSidePage
    {
        get { return observedAnalyzeLearner != null; }
    }

    void RefreshStatGrid(Stats stats)
    {
        for (int i = 0; i < displayedStats.Length; ++i)
        {
            Text label;
            if (!statValueLabels.TryGetValue(displayedStats[i], out label) || label == null)
                continue;

            label.text = stats != null ? stats[displayedStats[i]].ToString() : "--";
        }
    }

    void RefreshWeaponMastery(GameObject unitObject)
    {
        StatModifierFeature.WeaponType equippedType = GetEquippedWeaponType(unitObject);

        for (int i = 0; i < weaponMasteryLabels.Count && i < displayedWeaponTypes.Length; ++i)
        {
            StatModifierFeature.WeaponType type = displayedWeaponTypes[i];
            string grade = GetWeaponGrade(unitObject, type);
            string prefix = type == equippedType && type != StatModifierFeature.WeaponType.none ? "> " : "";
            weaponMasteryLabels[i].text = prefix + WeaponSymbol(type) + "  " + grade;
        }
    }

    void RefreshInventorySlots(GameObject unitObject)
    {
        Equipment equipment = unitObject != null ? unitObject.GetComponent<Equipment>() : null;
        IList<Equippable> items = equipment != null ? equipment.items : null;

        for (int i = 0; i < inventorySlotLabels.Count; ++i)
        {
            string text = "Slot " + (i + 1) + ":  -----";
            if (items != null && i < items.Count && items[i] != null)
            {
                text = "Slot " + (i + 1) + ":  " + CleanRuntimeName(items[i].name);
                if (items[i].slots != EquipSlots.None)
                    text += "  [" + items[i].slots + "]";
            }
            inventorySlotLabels[i].text = text;
        }
    }

    void RefreshStatusSummary(GameObject unitObject)
    {
        if (statusSummaryLabel == null)
            return;

        StatusEffect[] effects = unitObject.GetComponentsInChildren<StatusEffect>(true);
        if (effects == null || effects.Length == 0)
        {
            statusSummaryLabel.text = "STATUS: None";
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("STATUS: ");
        int count = 0;
        for (int i = 0; i < effects.Length; ++i)
        {
            if (effects[i] == null)
                continue;

            if (count > 0)
                sb.Append(", ");
            sb.Append(CleanStatusName(effects[i].GetType().Name));
            count++;
        }

        if (count == 0)
            sb.Append("None");

        statusSummaryLabel.text = sb.ToString();
    }

    void RefreshSidePanel(GameObject unitObject)
    {
        if (analyzePanelRoot != null)
            analyzePanelRoot.gameObject.SetActive(IsShowing);

        if (!IsShowing)
            return;

        if (sidePage == SidePage.Analyze && observedAnalyzeLearner == null)
            sidePage = SidePage.Mastery;

        if (sidePanelTitleLabel != null)
            sidePanelTitleLabel.text = sidePage == SidePage.Mastery ? "ABILITY MASTERY" : "ANALYZE";

        if (sidePage == SidePage.Mastery)
        {
            AbilityMasteryTracker mastery = unitObject != null ? unitObject.GetComponent<AbilityMasteryTracker>() : null;
            if (analyzeSummaryLabel != null)
                analyzeSummaryLabel.text = BuildMasterySummary(mastery);
            if (analyzeBodyLabel != null)
                analyzeBodyLabel.text = BuildMasteryBody(mastery);
        }
        else
        {
            AnalyzeLearner learner = unitObject != null ? unitObject.GetComponent<AnalyzeLearner>() : null;
            if (analyzeSummaryLabel != null)
                analyzeSummaryLabel.text = BuildAnalyzeSummary(learner);
            if (analyzeBodyLabel != null)
                analyzeBodyLabel.text = BuildAnalyzeBody(learner);
        }
    }

    string BuildMasterySummary(AbilityMasteryTracker mastery)
    {
        if (mastery == null)
            return "No mastery tracker.\nLeft click: next page    Right click: close";

        int records = mastery.records != null ? mastery.records.Count : 0;
        int totalUses = 0;
        int highestLevel = 0;
        int highestTuple = 1;
        if (mastery.records != null)
        {
            for (int i = 0; i < mastery.records.Count; ++i)
            {
                var record = mastery.records[i];
                if (record == null)
                    continue;
                totalUses += Mathf.Max(0, record.useCount);
                highestLevel = Mathf.Max(highestLevel, record.masteryLevel);
                highestTuple = Mathf.Max(highestTuple, mastery.GetMaxTupleCountForMasteryLevel(record.masteryLevel));
            }
        }

        string pageHint = observedAnalyzeLearner != null ? "Left click: Analyze page" : "Left click: Mastery page";
        return string.Format("Records: {0}    Total Uses: {1}    Highest Lv: {2}    Best Tuple: x{3}\n{4}    Right click: close",
            records,
            totalUses,
            highestLevel,
            highestTuple,
            pageHint);
    }

    string BuildMasteryBody(AbilityMasteryTracker mastery)
    {
        StringBuilder sb = new StringBuilder(2048);
        if (mastery == null)
        {
            sb.AppendLine("No AbilityMasteryTracker on this unit.");
            return sb.ToString();
        }

        if (mastery.records == null || mastery.records.Count == 0)
        {
            sb.AppendLine("No ability uses recorded yet.");
            sb.Append("First mastery level requires ");
            sb.Append(mastery.TotalUsesRequiredForLevel(1));
            sb.AppendLine(" use(s). Test Lab requirements are lower.");
            sb.AppendLine();
            sb.AppendLine("Use an ability with this unit to create a mastery record here.");
            return sb.ToString();
        }

        for (int i = 0; i < mastery.records.Count; ++i)
        {
            AbilityMasteryTracker.MasteryRecord record = mastery.records[i];
            if (record == null)
                continue;

            sb.Append("• ");
            sb.Append(record.abilityName);
            if (!string.IsNullOrEmpty(record.categoryName))
            {
                sb.Append("   [");
                sb.Append(record.categoryName);
                sb.Append("]");
            }
            sb.AppendLine();

            sb.Append("   Lv ");
            sb.Append(record.masteryLevel);
            sb.Append(" / ");
            sb.Append(mastery.maxMasteryLevel);
            sb.Append("   Uses ");
            sb.Append(record.useCount);

            if (record.maxed)
            {
                sb.Append("   MAX");
            }
            else
            {
                sb.Append("   Next ");
                sb.Append(record.usesAtCurrentLevel);
                sb.Append("/");
                sb.Append(record.usesForNextLevel);
            }
            sb.AppendLine();

            int powerPercent = Mathf.RoundToInt((mastery.GetPowerMultiplier(record.masteryLevel) - 1f) * 100f);
            int mpPercent = Mathf.RoundToInt((1f - mastery.GetMagicCostMultiplier(record.masteryLevel)) * 100f);
            sb.Append("   Power +");
            sb.Append(powerPercent);
            sb.Append("%   MP -");
            sb.Append(mpPercent);
            sb.AppendLine("%");

            sb.Append("   ");
            sb.AppendLine(mastery.GetTupleSummaryForMasteryLevel(record.masteryLevel));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    string BuildAnalyzeSummary(AnalyzeLearner learner)
    {
        if (learner == null)
            return "No Analyze data on this unit.\nLeft click: Mastery page    Right click: close";

        int observedCount = learner.observations != null ? learner.observations.Count : 0;
        int partialCount = 0;
        int waitingCount = 0;
        int learnedCount = 0;

        if (learner.observations != null)
        {
            for (int i = 0; i < learner.observations.Count; ++i)
            {
                var record = learner.observations[i];
                if (record == null)
                    continue;
                if (record.learned)
                    learnedCount++;
                else if (learner.GetPartialStageIndex(record) > 0)
                    partialCount++;
                else
                    waitingCount++;
            }
        }

        return string.Format("Req: {0}    Records: {1}    Partial: {2}    Waiting: {3}    Learned: {4}\nLeft click: Mastery page    Right click: close",
            learner.defaultObservationRequirement,
            observedCount,
            partialCount,
            waitingCount,
            learnedCount);
    }

    string BuildDebugBody(GameObject unitObject)
    {
        StringBuilder sb = new StringBuilder(4096);

        sb.AppendLine("STATUS EFFECTS");
        sb.AppendLine("------------------------------");
        AppendStatuses(sb, unitObject);

        sb.AppendLine();
        sb.AppendLine("ABILITIES");
        sb.AppendLine("------------------------------");
        AppendAbilities(sb, unitObject);

        sb.AppendLine();
        sb.AppendLine("BATTLE STATE");
        sb.AppendLine("------------------------------");
        AppendBattleState(sb, unitObject);

        return sb.ToString();
    }

    string BuildAnalyzeBody(AnalyzeLearner learner)
    {
        StringBuilder sb = new StringBuilder(2048);
        if (learner == null)
        {
            sb.AppendLine("No Analyze data on this unit.");
            return sb.ToString();
        }

        List<AnalyzeLearner.ObservationRecord> notUsable = new List<AnalyzeLearner.ObservationRecord>();
        List<AnalyzeLearner.ObservationRecord> inProgress = new List<AnalyzeLearner.ObservationRecord>();
        List<AnalyzeLearner.ObservationRecord> learned = new List<AnalyzeLearner.ObservationRecord>();

        if (learner.observations != null)
        {
            for (int i = 0; i < learner.observations.Count; ++i)
            {
                var record = learner.observations[i];
                if (record == null)
                    continue;

                if (record.learned)
                    learned.Add(record);
                else if (learner.GetPartialStageIndex(record) > 0)
                    inProgress.Add(record);
                else
                    notUsable.Add(record);
            }
        }

        sb.AppendLine("PARTIAL / USABLE");
        sb.AppendLine("------------------------------");
        if (inProgress.Count == 0)
        {
            sb.AppendLine("None");
        }
        else
        {
            for (int i = 0; i < inProgress.Count; ++i)
                AppendAnalyzeRecordLine(sb, learner, inProgress[i], "OBSERVING");
        }

        sb.AppendLine();
        sb.AppendLine("SEEN, NOT USABLE YET");
        sb.AppendLine("------------------------------");
        if (notUsable.Count == 0)
        {
            sb.AppendLine("None");
        }
        else
        {
            for (int i = 0; i < notUsable.Count; ++i)
                AppendAnalyzeRecordLine(sb, learner, notUsable[i], "WAITING");
        }

        sb.AppendLine();
        sb.AppendLine("LEARNED / FULL POWER");
        sb.AppendLine("------------------------------");
        if (learned.Count == 0)
        {
            sb.AppendLine("None");
        }
        else
        {
            for (int i = 0; i < learned.Count; ++i)
                AppendAnalyzeRecordLine(sb, learner, learned[i], "LEARNED");
        }

        return sb.ToString();
    }

    void AppendAnalyzeRecordLine(StringBuilder sb, AnalyzeLearner learner, AnalyzeLearner.ObservationRecord record, string label)
    {
        if (record == null)
            return;

        sb.Append("• ");
        sb.Append(record.abilityName);
        sb.Append("   ");
        sb.Append(learner.GetObservationProgressText(record));
        sb.Append("/");
        sb.Append(record.observationRequirement);
        sb.Append("   ");
        sb.Append(label);
        sb.AppendLine();

        sb.Append("   ");
        sb.AppendLine(learner.GetPartialStatusText(record));
        sb.Append("   ");
        sb.AppendLine(learner.GetSelfPracticeStatusText(record));

        if (!string.IsNullOrEmpty(record.categoryName))
        {
            sb.Append("   Category: ");
            sb.AppendLine(record.categoryName);
        }
    }

    void AppendBattleState(StringBuilder sb, GameObject unitObject)
    {
        Unit unit = unitObject.GetComponent<Unit>();
        Health health = unitObject.GetComponent<Health>();
        Driver driver = unitObject.GetComponent<Driver>();
        Movement movement = unitObject.GetComponent<Movement>();
        Alliance alliance = unitObject.GetComponent<Alliance>();
        AbilityCatalog catalog = unitObject.GetComponentInChildren<AbilityCatalog>();

        if (alliance != null)
        {
            sb.Append("Alliance: ");
            sb.Append(alliance.type.ToString());
            if (alliance.confused)
                sb.Append(" / Confused");
            sb.AppendLine();
        }

        if (catalog != null)
        {
            sb.Append("Catalog: ");
            sb.AppendLine(string.IsNullOrEmpty(catalog.recipeName) ? "--" : catalog.recipeName);
        }

        if (unit != null)
        {
            sb.Append("Tile: ");
            if (unit.tile != null)
            {
                sb.Append("(");
                sb.Append(unit.tile.pos.x);
                sb.Append(", ");
                sb.Append(unit.tile.pos.y);
                sb.Append(")  Height: ");
                sb.Append(unit.tile.height.ToString("0.##"));
            }
            else
            {
                sb.Append("--");
            }
            sb.AppendLine();

            sb.Append("Facing: ");
            sb.Append(unit.dir.ToString());
            sb.Append("   Cant Move: ");
            sb.Append(unit.cantMove ? "YES" : "No");
            sb.Append("   Cant Act: ");
            sb.AppendLine(unit.cantAct ? "YES" : "No");
        }

        if (driver != null)
        {
            sb.Append("Driver: ");
            sb.AppendLine(driver.Current.ToString());
        }

        if (movement != null)
        {
            sb.Append("Movement: ");
            sb.AppendLine(CleanRuntimeName(movement.GetType().Name));
        }

        if (health != null)
        {
            sb.Append("Defeat / Min HP Threshold: ");
            sb.AppendLine(health.MinHP.ToString());
        }
    }

    void AppendStatuses(StringBuilder sb, GameObject unitObject)
    {
        StatusEffect[] effects = unitObject.GetComponentsInChildren<StatusEffect>(true);
        if (effects == null || effects.Length == 0)
        {
            sb.AppendLine("None");
            return;
        }

        for (int i = 0; i < effects.Length; ++i)
        {
            StatusEffect effect = effects[i];
            if (effect == null)
                continue;

            sb.Append("• ");
            sb.AppendLine(CleanStatusName(effect.GetType().Name));

            AppendPublicFields(sb, effect, "   ");

            StatusCondition[] conditions = effect.GetComponentsInChildren<StatusCondition>(true);
            for (int j = 0; j < conditions.Length; ++j)
            {
                StatusCondition condition = conditions[j];
                if (condition == null)
                    continue;

                sb.Append("   - Condition: ");
                sb.AppendLine(CleanRuntimeName(condition.GetType().Name));
                AppendPublicFields(sb, condition, "      ");
            }
        }
    }

    void AppendMastery(StringBuilder sb, GameObject unitObject)
    {
        AbilityMasteryTracker mastery = unitObject != null ? unitObject.GetComponent<AbilityMasteryTracker>() : null;
        if (mastery == null)
        {
            sb.AppendLine("No AbilityMasteryTracker on this unit.");
            return;
        }

        if (mastery.records == null || mastery.records.Count == 0)
        {
            sb.AppendLine("No mastered/used abilities yet.");
            sb.Append("First mastery level requires ");
            sb.Append(mastery.TotalUsesRequiredForLevel(1));
            sb.AppendLine(" use(s).");
            return;
        }

        for (int i = 0; i < mastery.records.Count; ++i)
        {
            AbilityMasteryTracker.MasteryRecord record = mastery.records[i];
            if (record == null)
                continue;

            sb.Append("• ");
            sb.Append(record.abilityName);
            sb.Append("   Lv ");
            sb.Append(record.masteryLevel);
            sb.Append("   Uses ");
            sb.Append(record.useCount);

            if (record.maxed)
            {
                sb.Append("   MAX");
            }
            else
            {
                sb.Append("   Next ");
                sb.Append(record.usesAtCurrentLevel);
                sb.Append("/");
                sb.Append(record.usesForNextLevel);
            }

            int powerPercent = Mathf.RoundToInt((mastery.GetPowerMultiplier(record.masteryLevel) - 1f) * 100f);
            int mpPercent = Mathf.RoundToInt((1f - mastery.GetMagicCostMultiplier(record.masteryLevel)) * 100f);
            sb.Append("   Power +");
            sb.Append(powerPercent);
            sb.Append("%   MP -");
            sb.Append(mpPercent);
            sb.AppendLine("%");

            sb.Append("   ");
            sb.AppendLine(mastery.GetTupleSummaryForMasteryLevel(record.masteryLevel));
        }
    }

    void AppendAbilities(StringBuilder sb, GameObject unitObject)
    {
        AbilityCatalog catalog = unitObject.GetComponentInChildren<AbilityCatalog>();
        if (catalog == null)
        {
            sb.AppendLine("No Ability Catalog");
            return;
        }

        for (int c = 0; c < catalog.CategoryCount(); ++c)
        {
            GameObject category = catalog.GetCategory(c);
            if (category == null)
                continue;

            sb.AppendLine(CleanRuntimeName(category.name));

            for (int a = 0; a < catalog.AbilityCount(category); ++a)
            {
                Ability ability = catalog.GetAbility(c, a);
                if (ability == null)
                    continue;

                AbilityCatalog.JobTier requiredTier;
                bool unlocked = catalog.IsAbilityUnlocked(ability, out requiredTier);
                bool equipped = catalog.IsAbilityEquipped(ability);
                bool visible = catalog.IsAbilityVisible(ability);
                bool canPerform = ability.CanPerform();

                sb.Append("  • ");
                sb.Append(CleanRuntimeName(ability.name));
                sb.Append("  [");
                sb.Append(visible ? "Shown" : "Hidden");
                sb.Append(unlocked ? ", Unlocked" : ", Locked");
                sb.Append(equipped ? ", Equipped" : ", Unequipped");
                sb.Append(canPerform ? ", Usable" : ", Blocked");
                if (requiredTier != AbilityCatalog.JobTier.None)
                {
                    sb.Append(", Tier: ");
                    sb.Append(requiredTier.ToString());
                }
                sb.AppendLine("]");

                AbilityMasteryTracker mastery = unitObject.GetComponent<AbilityMasteryTracker>();
                if (mastery != null)
                {
                    sb.Append("     ");
                    sb.AppendLine(mastery.GetSummaryForAbility(ability));
                }
            }
        }
    }

    void AppendPublicFields(StringBuilder sb, object instance, string indent)
    {
        FieldInfo[] fields = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < fields.Length; ++i)
        {
            FieldInfo field = fields[i];
            if (field == null)
                continue;

            object value = field.GetValue(instance);
            sb.Append(indent);
            sb.Append(field.Name);
            sb.Append(": ");
            sb.AppendLine(FormatFieldValue(value));
        }
    }

    string FormatFieldValue(object value)
    {
        if (value == null)
            return "null";

        UnityEngine.Object unityObject = value as UnityEngine.Object;
        if (unityObject != null)
            return unityObject.name;

        return value.ToString();
    }

    string GetJobName(GameObject unitObject)
    {
        Job job = unitObject != null ? unitObject.GetComponentInChildren<Job>() : null;
        return job != null ? CleanRuntimeName(job.gameObject.name) : "No Job";
    }

    StatModifierFeature.WeaponType GetEquippedWeaponType(GameObject unitObject)
    {
        Equipment equipment = unitObject != null ? unitObject.GetComponent<Equipment>() : null;
        if (equipment == null)
            return StatModifierFeature.WeaponType.none;

        Equippable primary = equipment.GetItem(EquipSlots.Primary);
        StatModifierFeature.WeaponType type = GetItemWeaponType(primary);
        if (type != StatModifierFeature.WeaponType.none)
            return type;

        Equippable secondary = equipment.GetItem(EquipSlots.Secondary);
        return GetItemWeaponType(secondary);
    }

    StatModifierFeature.WeaponType GetItemWeaponType(Equippable item)
    {
        if (item == null)
            return StatModifierFeature.WeaponType.none;

        StatModifierFeature[] features = item.GetComponentsInChildren<StatModifierFeature>(true);
        for (int i = 0; i < features.Length; ++i)
        {
            if (features[i] != null && features[i].weaponType != StatModifierFeature.WeaponType.none)
                return features[i].weaponType;
        }

        return StatModifierFeature.WeaponType.none;
    }

    string GetWeaponGrade(GameObject unitObject, StatModifierFeature.WeaponType type)
    {
        string reflected = TryReflectWeaponGrade(unitObject, type);
        return !string.IsNullOrEmpty(reflected) ? reflected : "E";
    }

    string TryReflectWeaponGrade(GameObject unitObject, StatModifierFeature.WeaponType type)
    {
        if (unitObject == null || type == StatModifierFeature.WeaponType.none)
            return string.Empty;

        MonoBehaviour[] behaviours = unitObject.GetComponentsInChildren<MonoBehaviour>(true);
        string weaponName = type.ToString().ToLowerInvariant();

        for (int i = 0; i < behaviours.Length; ++i)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null)
                continue;

            string className = behaviour.GetType().Name.ToLowerInvariant();
            if (!className.Contains("mastery") && !className.Contains("proficiency") && !className.Contains("rank"))
                continue;

            FieldInfo[] fields = behaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int f = 0; f < fields.Length; ++f)
            {
                string fieldName = fields[f].Name.ToLowerInvariant();
                if (!fieldName.Contains(weaponName))
                    continue;
                if (!fieldName.Contains("grade") && !fieldName.Contains("rank") && !fieldName.Contains("mastery"))
                    continue;

                object value = fields[f].GetValue(behaviour);
                if (value != null)
                    return value.ToString();
            }
        }

        return string.Empty;
    }

    string WeaponSymbol(StatModifierFeature.WeaponType type)
    {
        switch (type)
        {
            case StatModifierFeature.WeaponType.sword: return "Sw";
            case StatModifierFeature.WeaponType.spear: return "Sp";
            case StatModifierFeature.WeaponType.bow: return "Bw";
            case StatModifierFeature.WeaponType.axe: return "Ax";
            case StatModifierFeature.WeaponType.staff: return "St";
            case StatModifierFeature.WeaponType.whip: return "Wh";
            case StatModifierFeature.WeaponType.bottle: return "Bt";
            case StatModifierFeature.WeaponType.gauntlet: return "Gt";
            case StatModifierFeature.WeaponType.shield: return "Sh";
            case StatModifierFeature.WeaponType.dagger: return "Dg";
            case StatModifierFeature.WeaponType.hammer: return "Hm";
            case StatModifierFeature.WeaponType.fan: return "Fn";
        }
        return "--";
    }

    string CleanStatusName(string name)
    {
        name = CleanRuntimeName(name);
        name = name.Replace("Status Effect", "");
        return name.Trim();
    }

    string CleanRuntimeName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "--";

        name = name.Replace("(Clone)", "");
        name = name.Replace("StatusEffect", "Status Effect");
        name = name.Replace("StatusCondition", "Status Condition");
        name = name.Replace("Ability", " Ability");
        return SplitCamelCase(name).Trim();
    }

    string SplitCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        StringBuilder sb = new StringBuilder(text.Length + 8);
        sb.Append(text[0]);
        for (int i = 1; i < text.Length; ++i)
        {
            char c = text[i];
            char previous = text[i - 1];
            if (char.IsUpper(c) && !char.IsWhiteSpace(previous) && !char.IsUpper(previous))
                sb.Append(' ');
            sb.Append(c);
        }
        return sb.ToString();
    }

    void BuildUIIfNeeded()
    {
        if (panelRoot != null)
            return;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statValueLabels.Clear();
        weaponMasteryLabels.Clear();
        inventorySlotLabels.Clear();

        GameObject canvasObject = new GameObject("Unit Status Detail Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 35;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        BuildMainPanel(font, canvasObject.transform);
        BuildAnalyzePanel(font, canvasObject.transform);
    }

    void BuildMainPanel(Font font, Transform canvasParent)
    {
        GameObject panelObject = new GameObject("Unit Status FE Panel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasParent, false);
        panelRoot = panelObject.GetComponent<RectTransform>();
        panelRoot.anchorMin = new Vector2(0f, 0.5f);
        panelRoot.anchorMax = new Vector2(0f, 0.5f);
        panelRoot.pivot = new Vector2(0f, 0.5f);
        panelRoot.anchoredPosition = panelOffset;
        panelRoot.sizeDelta = panelSize;

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.70f, 0.50f, 0.28f, 0.96f);

        CreateImageBox("Inner Parchment", panelRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-20f, -20f), new Color(0.86f, 0.68f, 0.39f, 0.92f));

        Image namePlate = CreateImageBox("Name Plate", panelRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -12f), new Vector2(264f, 42f), new Color(0.12f, 0.18f, 0.32f, 0.96f));
        nameLabel = CreateText("Name", namePlate.rectTransform, font, 26, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch(nameLabel.rectTransform, 8f, 4f, 8f, 4f);

        Image jobPlate = CreateImageBox("Job Plate", panelRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(286f, -12f), new Vector2(-420f, 42f), new Color(0.93f, 0.72f, 0.38f, 0.96f));
        jobLabel = CreateText("Job", jobPlate.rectTransform, font, 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.18f, 0.10f, 0.04f, 1f));
        Stretch(jobLabel.rectTransform, 8f, 4f, 8f, 4f);

        Image levelPlate = CreateImageBox("Level Plate", panelRoot, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-14f, -12f), new Vector2(128f, 42f), new Color(0.95f, 0.78f, 0.48f, 0.96f));
        levelLabel = CreateText("Level", levelPlate.rectTransform, font, 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.18f, 0.10f, 0.04f, 1f));
        Stretch(levelLabel.rectTransform, 8f, 4f, 8f, 4f);

        CreateTopInfoBoxes(font);
        CreatePortraitArea(font);
        CreateStatGrid(font);
        CreateWeaponMasteryArea(font);
        CreateInventoryArea(font);
        CreateStatusAndDebugArea(font);

        Text hintLabel = CreateText("Close Hint", panelRoot, font, 14, FontStyle.Italic, TextAnchor.UpperRight, new Color(0.18f, 0.10f, 0.04f, 0.85f));
        RectTransform hintRect = hintLabel.rectTransform;
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(1f, 0f);
        hintRect.anchoredPosition = new Vector2(-20f, 12f);
        hintRect.sizeDelta = new Vector2(-40f, 22f);
        hintLabel.text = "Left click changes side page | Right click / Cancel closes";
    }

    void BuildAnalyzePanel(Font font, Transform canvasParent)
    {
        GameObject panelObject = new GameObject("Detail Side Page Panel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasParent, false);
        analyzePanelRoot = panelObject.GetComponent<RectTransform>();
        analyzePanelRoot.anchorMin = new Vector2(0f, 0.5f);
        analyzePanelRoot.anchorMax = new Vector2(0f, 0.5f);
        analyzePanelRoot.pivot = new Vector2(0f, 0.5f);
        analyzePanelRoot.anchoredPosition = analyzePanelOffset;
        analyzePanelRoot.sizeDelta = analyzePanelSize;

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.22f, 0.19f, 0.34f, 0.97f);

        CreateImageBox("Analyze Inner", analyzePanelRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-18f, -18f), new Color(0.82f, 0.75f, 0.90f, 0.94f));

        Image titlePlate = CreateImageBox("Side Page Title Plate", analyzePanelRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(-24f, 42f), new Color(0.18f, 0.18f, 0.40f, 0.97f));
        sidePanelTitleLabel = CreateText("Side Page Title", titlePlate.rectTransform, font, 24, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch(sidePanelTitleLabel.rectTransform, 6f, 4f, 6f, 4f);
        sidePanelTitleLabel.text = "ABILITY MASTERY";

        analyzeSummaryLabel = CreateText("Analyze Summary", analyzePanelRoot, font, 15, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.16f, 0.08f, 0.28f, 1f));
        RectTransform summaryRect = analyzeSummaryLabel.rectTransform;
        summaryRect.anchorMin = new Vector2(0f, 1f);
        summaryRect.anchorMax = new Vector2(1f, 1f);
        summaryRect.pivot = new Vector2(0.5f, 1f);
        summaryRect.anchoredPosition = new Vector2(0f, -68f);
        summaryRect.sizeDelta = new Vector2(-32f, 44f);
        analyzeSummaryLabel.text = "Req: --   Records: 0   Partial: 0   Waiting: 0   Learned: 0";

        Text help = CreateText("Analyze Help", analyzePanelRoot, font, 13, FontStyle.Italic, TextAnchor.UpperLeft, new Color(0.20f, 0.10f, 0.28f, 0.92f));
        RectTransform helpRect = help.rectTransform;
        helpRect.anchorMin = new Vector2(0f, 1f);
        helpRect.anchorMax = new Vector2(1f, 1f);
        helpRect.pivot = new Vector2(0.5f, 1f);
        helpRect.anchoredPosition = new Vector2(0f, -110f);
        helpRect.sizeDelta = new Vector2(-32f, 38f);
        help.text = "Partial Analyze grants weak usable versions at milestones. Full learning removes the penalties.";

        Image listPlate = CreateImageBox("Side Page List Plate", analyzePanelRoot, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(-32f, 560f), new Color(0.96f, 0.94f, 0.99f, 0.80f));

        GameObject scrollViewObject = new GameObject("Side Page Scroll View", typeof(RectTransform), typeof(ScrollRect));
        scrollViewObject.transform.SetParent(listPlate.rectTransform, false);
        RectTransform scrollViewRect = scrollViewObject.GetComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0f, 0f);
        scrollViewRect.anchorMax = new Vector2(1f, 1f);
        scrollViewRect.offsetMin = new Vector2(8f, 8f);
        scrollViewRect.offsetMax = new Vector2(-8f, -8f);

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollViewRect, false);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        Mask mask = viewportObject.GetComponent<Mask>();
        mask.showMaskGraphic = false;

        analyzeBodyLabel = CreateText("Side Page Body", viewportRect, font, 16, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.12f, 0.05f, 0.22f, 1f));
        RectTransform bodyRect = analyzeBodyLabel.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0f, 1f);
        bodyRect.anchoredPosition = Vector2.zero;
        bodyRect.sizeDelta = new Vector2(0f, 1100f);
        analyzeBodyLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
        analyzeBodyLabel.verticalOverflow = VerticalWrapMode.Overflow;

        ContentSizeFitter fitter = analyzeBodyLabel.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scrollViewObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = bodyRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.scrollSensitivity = 28f;
    }

    void CreateTopInfoBoxes(Font font)
    {
        hpLabel = CreateInfoBox("HP Box", "HP", new Vector2(286f, -62f), new Vector2(180f, 42f), font);
        mpLabel = CreateInfoBox("MP Box", "MP", new Vector2(476f, -62f), new Vector2(160f, 42f), font);
        moveLabel = CreateInfoBox("Move Box", "MV", new Vector2(646f, -62f), new Vector2(92f, 42f), font);
    }

    Text CreateInfoBox(string name, string prefix, Vector2 position, Vector2 size, Font font)
    {
        Image box = CreateImageBox(name, panelRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), position, size, new Color(0.95f, 0.78f, 0.48f, 0.92f));
        Text text = CreateText(name + " Text", box.rectTransform, font, 21, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.18f, 0.10f, 0.04f, 1f));
        Stretch(text.rectTransform, 6f, 2f, 6f, 2f);
        text.text = prefix;
        return text;
    }

    void CreatePortraitArea(Font font)
    {
        Image portraitFrame = CreateImageBox("Portrait Frame", panelRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -64f), new Vector2(250f, 276f), new Color(0.22f, 0.16f, 0.12f, 0.78f));

        GameObject portraitObject = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
        portraitObject.transform.SetParent(portraitFrame.rectTransform, false);
        portraitImage = portraitObject.GetComponent<Image>();
        portraitImage.preserveAspect = true;
        portraitImage.color = Color.white;
        Stretch(portraitImage.rectTransform, 8f, 8f, 8f, 8f);
    }

    void CreateStatGrid(Font font)
    {
        Text title = CreateSectionTitle("Stats Title", "STATS", new Vector2(18f, -350f), new Vector2(250f, 26f), font);
        title.alignment = TextAnchor.MiddleCenter;

        float x = 24f;
        float y = -382f;
        float rowHeight = 34f;
        float colWidth = 118f;

        for (int i = 0; i < displayedStats.Length; ++i)
        {
            int col = i % 2;
            int row = i / 2;
            CreateStatBox(displayedStats[i], x + col * (colWidth + 8f), y - row * rowHeight, colWidth, 28f, font);
        }
    }

    void CreateStatBox(StatTypes type, float x, float y, float width, float height, Font font)
    {
        Image box = CreateImageBox(type.ToString() + " Stat Box", panelRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y), new Vector2(width, height), new Color(0.95f, 0.78f, 0.48f, 0.88f));

        Text label = CreateText(type.ToString() + " Label", box.rectTransform, font, 17, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.80f, 0.22f, 0.16f, 1f));
        label.rectTransform.anchorMin = new Vector2(0f, 0f);
        label.rectTransform.anchorMax = new Vector2(0.55f, 1f);
        label.rectTransform.offsetMin = new Vector2(8f, 0f);
        label.rectTransform.offsetMax = new Vector2(0f, 0f);
        label.text = type.ToString();

        Text value = CreateText(type.ToString() + " Value", box.rectTransform, font, 19, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.02f, 0.55f, 0.16f, 1f));
        value.rectTransform.anchorMin = new Vector2(0.45f, 0f);
        value.rectTransform.anchorMax = new Vector2(1f, 1f);
        value.rectTransform.offsetMin = new Vector2(0f, 0f);
        value.rectTransform.offsetMax = new Vector2(-8f, 0f);
        value.text = "--";

        statValueLabels[type] = value;
    }

    void CreateWeaponMasteryArea(Font font)
    {
        Text title = CreateSectionTitle("Weapon Mastery Title", "WEAPON MASTERY", new Vector2(286f, -118f), new Vector2(452f, 26f), font);
        title.alignment = TextAnchor.MiddleCenter;

        float startX = 294f;
        float startY = -154f;
        float boxW = 104f;
        float boxH = 30f;
        float gapX = 8f;
        float gapY = 7f;

        for (int i = 0; i < displayedWeaponTypes.Length; ++i)
        {
            int col = i % 4;
            int row = i / 4;
            Image box = CreateImageBox("Weapon Mastery " + displayedWeaponTypes[i], panelRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(startX + col * (boxW + gapX), startY - row * (boxH + gapY)), new Vector2(boxW, boxH), new Color(0.93f, 0.72f, 0.38f, 0.86f));
            Text text = CreateText("Weapon Text", box.rectTransform, font, 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.18f, 0.10f, 0.04f, 1f));
            Stretch(text.rectTransform, 4f, 2f, 4f, 2f);
            text.text = WeaponSymbol(displayedWeaponTypes[i]) + "  E";
            weaponMasteryLabels.Add(text);
        }
    }

    void CreateInventoryArea(Font font)
    {
        Text title = CreateSectionTitle("Inventory Title", "INVENTORY / ITEM SLOTS", new Vector2(286f, -282f), new Vector2(452f, 26f), font);
        title.alignment = TextAnchor.MiddleCenter;

        for (int i = 0; i < 5; ++i)
        {
            Image slot = CreateImageBox("Inventory Slot " + (i + 1), panelRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(294f, -318f - i * 36f), new Vector2(436f, 30f), new Color(0.96f, 0.78f, 0.50f, 0.82f));
            Text text = CreateText("Inventory Slot Text", slot.rectTransform, font, 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.18f, 0.10f, 0.04f, 1f));
            Stretch(text.rectTransform, 10f, 2f, 10f, 2f);
            text.text = "Slot " + (i + 1) + ":  -----";
            inventorySlotLabels.Add(text);
        }
    }

    void CreateStatusAndDebugArea(Font font)
    {
        statusSummaryLabel = CreateText("Status Summary", panelRoot, font, 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.18f, 0.10f, 0.04f, 1f));
        RectTransform statusRect = statusSummaryLabel.rectTransform;
        statusRect.anchorMin = new Vector2(0f, 1f);
        statusRect.anchorMax = new Vector2(0f, 1f);
        statusRect.pivot = new Vector2(0f, 1f);
        statusRect.anchoredPosition = new Vector2(24f, -570f);
        statusRect.sizeDelta = new Vector2(706f, 28f);
        statusSummaryLabel.text = "STATUS: None";

        Text title = CreateSectionTitle("Details Title", "STATUS / ABILITY DETAILS", new Vector2(24f, -604f), new Vector2(706f, 24f), font);
        title.alignment = TextAnchor.MiddleCenter;

        GameObject scrollViewObject = new GameObject("Details Scroll View", typeof(RectTransform), typeof(ScrollRect));
        scrollViewObject.transform.SetParent(panelRoot, false);
        RectTransform scrollViewRect = scrollViewObject.GetComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0f, 0f);
        scrollViewRect.anchorMax = new Vector2(1f, 0f);
        scrollViewRect.pivot = new Vector2(0.5f, 0f);
        scrollViewRect.anchoredPosition = new Vector2(0f, 48f);
        scrollViewRect.sizeDelta = new Vector2(-48f, 88f);

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollViewRect, false);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        Mask mask = viewportObject.GetComponent<Mask>();
        mask.showMaskGraphic = false;

        bodyLabel = CreateText("Body", viewportRect, font, 14, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);
        RectTransform bodyRect = bodyLabel.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0f, 1f);
        bodyRect.anchoredPosition = Vector2.zero;
        bodyRect.sizeDelta = new Vector2(0f, 1200f);
        bodyLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
        bodyLabel.verticalOverflow = VerticalWrapMode.Overflow;
        bodyLabel.color = new Color(0.10f, 0.06f, 0.03f, 1f);

        ContentSizeFitter fitter = bodyLabel.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scrollViewObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = bodyRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.scrollSensitivity = 28f;
    }

    Text CreateSectionTitle(string name, string textValue, Vector2 position, Vector2 size, Font font)
    {
        Image box = CreateImageBox(name + " Plate", panelRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), position, size, new Color(0.36f, 0.18f, 0.08f, 0.86f));
        Text text = CreateText(name, box.rectTransform, font, 15, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.92f, 0.72f, 1f));
        Stretch(text.rectTransform, 8f, 2f, 8f, 2f);
        text.text = textValue;
        return text;
    }

    Image CreateImageBox(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    Text CreateText(string name, Transform parent, Font font, int size, FontStyle style, TextAnchor alignment, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        Text text = obj.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    void Stretch(RectTransform rect, float left, float top, float right, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
