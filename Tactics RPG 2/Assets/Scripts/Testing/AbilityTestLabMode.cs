using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Drop this on the Battle Controller in a duplicated scene to turn that scene
/// into an ability/status/turn-order sandbox.
///
/// InitBattleState detects this component and uses it instead of the normal
/// story/test battle spawner.
/// </summary>
public class AbilityTestLabMode : MonoBehaviour
{
    [Serializable]
    public class LabUnitSlot
    {
        public string recipeName = "Rein";
        public string displayLabel = "";
        public Alliances alliance = Alliances.Hero;
        public int level = 20;
        public int x = 0;
        public int z = 0;
        public Directions facing = Directions.North;

        [Header("Testing Helpers")]
        public bool grantAllAbilities = false;
        public bool infiniteMana = false;
        public bool autoSkipTurn = false;

        [Header("Stat Overrides")]
        public bool overrideStats = true;
        public int maxHP = 999;
        public int maxMP = 999;
        public int speed = 10;
        public int move = 5;
        public int jump = 4;
    }

    [Header("Lab Units")]
    public LabUnitSlot[] unitSlots = DefaultSlots();

    [Header("Catalog Test Setup")]
    public bool includeEveryCatalogRecipeAbility = true;
    public bool includeUncataloguedAbilityPrefabs = true;
    public bool hideOriginalCatalogsOnTester = true;

    [Header("Analyze Test Setup")]
    [Tooltip("When enabled, Rein keeps his normal catalog only, while a separate controllable ally gets all abilities. This makes Analyze easier to test.")]
    public bool useAnalyzeTestingLayout = true;
    public bool keepReinCatalogLimited = true;
    public bool ensureAnalyzeTeacher = true;
    public string analyzeTeacherRecipeName = "Lucy";
    public string analyzeTeacherDisplayLabel = "Analyze Teacher - All Abilities";

    [Header("Battle Setup")]
    public bool clearExistingUnits = true;
    public bool addNeverEndingVictoryCondition = true;
    public bool addAutoStatusController = true;
    public bool selectFirstNonSkippingHero = true;

    [Header("Debug")]
    public bool logSpawnSummary = true;

    public void BuildLab(BattleController owner, Board board)
    {
        if (owner == null || board == null)
            return;

        if (clearExistingUnits)
            ClearRuntimeUnits(owner);

        if (addAutoStatusController && owner.GetComponent<AutoStatusController>() == null)
            owner.gameObject.AddComponent<AutoStatusController>();

        if (addNeverEndingVictoryCondition && owner.GetComponent<BaseVictoryCondition>() == null)
            owner.gameObject.AddComponent<AbilityTestLabVictoryCondition>();

        owner.units.Clear();

        GameObject container = new GameObject("Ability Test Lab Units");
        container.transform.SetParent(owner.transform, false);

        LabUnitSlot[] slots = unitSlots;
        if (slots == null || slots.Length == 0)
            slots = DefaultSlots();

        if (useAnalyzeTestingLayout)
            slots = BuildAnalyzeTestingSlots(slots);

        Unit firstSelectable = null;
        for (int i = 0; i < slots.Length; ++i)
        {
            Unit unit = SpawnSlot(slots[i], owner, board, container.transform);
            if (unit == null)
                continue;

            owner.units.Add(unit);

            if (firstSelectable == null && (!selectFirstNonSkippingHero || IsSelectableHero(slots[i], unit)))
                firstSelectable = unit;
        }

        if (firstSelectable == null && owner.units.Count > 0)
            firstSelectable = owner.units[0];

        if (firstSelectable != null)
            SelectStartingTile(owner, firstSelectable.tile);

        if (logSpawnSummary)
        {
            string summary = useAnalyzeTestingLayout
                ? "Ability Test Lab spawned {0} units. Analyze mode: Rein keeps his normal catalog; Analyze Teacher has all abilities and infinite MP."
                : "Ability Test Lab spawned {0} units. Main tester should have all available ability prefabs and infinite MP.";
            Debug.Log(string.Format(summary, owner.units.Count));
        }
    }

    LabUnitSlot[] BuildAnalyzeTestingSlots(LabUnitSlot[] source)
    {
        List<LabUnitSlot> result = new List<LabUnitSlot>();

        bool hasRein = false;
        bool hasTeacher = false;

        if (source != null)
        {
            for (int i = 0; i < source.Length; ++i)
            {
                LabUnitSlot copy = CloneSlot(source[i]);
                if (copy == null)
                    continue;

                if (IsReinSlot(copy))
                {
                    hasRein = true;
                    NormalizeReinAnalyzerSlot(copy);
                    result.Add(copy);

                    if (ensureAnalyzeTeacher && !hasTeacher)
                    {
                        result.Add(CreateAnalyzeTeacherSlot(copy));
                        hasTeacher = true;
                    }
                    continue;
                }

                if (IsAnalyzeTeacherSlot(copy))
                {
                    hasTeacher = true;
                    NormalizeAnalyzeTeacherSlot(copy);
                }

                result.Add(copy);
            }
        }

        if (!hasRein)
        {
            LabUnitSlot rein = CreateDefaultReinAnalyzerSlot();
            result.Insert(0, rein);
            hasRein = true;

            if (ensureAnalyzeTeacher && !hasTeacher)
            {
                result.Insert(1, CreateAnalyzeTeacherSlot(rein));
                hasTeacher = true;
            }
        }
        else if (ensureAnalyzeTeacher && !hasTeacher)
        {
            result.Insert(Mathf.Min(1, result.Count), CreateAnalyzeTeacherSlot(result[0]));
        }

        return result.ToArray();
    }

    LabUnitSlot CloneSlot(LabUnitSlot source)
    {
        if (source == null)
            return null;

        return new LabUnitSlot
        {
            recipeName = source.recipeName,
            displayLabel = source.displayLabel,
            alliance = source.alliance,
            level = source.level,
            x = source.x,
            z = source.z,
            facing = source.facing,
            grantAllAbilities = source.grantAllAbilities,
            infiniteMana = source.infiniteMana,
            autoSkipTurn = source.autoSkipTurn,
            overrideStats = source.overrideStats,
            maxHP = source.maxHP,
            maxMP = source.maxMP,
            speed = source.speed,
            move = source.move,
            jump = source.jump
        };
    }

    void NormalizeReinAnalyzerSlot(LabUnitSlot slot)
    {
        if (slot == null)
            return;

        if (string.IsNullOrEmpty(slot.displayLabel) || slot.displayLabel.Contains("Tester"))
            slot.displayLabel = "Rein - Analyzer";

        slot.alliance = Alliances.Hero;
        slot.autoSkipTurn = false;
        slot.infiniteMana = true;
        slot.maxMP = Mathf.Max(slot.maxMP, 9999);

        if (keepReinCatalogLimited)
            slot.grantAllAbilities = false;
    }

    void NormalizeAnalyzeTeacherSlot(LabUnitSlot slot)
    {
        if (slot == null)
            return;

        if (string.IsNullOrEmpty(slot.recipeName))
            slot.recipeName = string.IsNullOrEmpty(analyzeTeacherRecipeName) ? "Lucy" : analyzeTeacherRecipeName;
        if (string.IsNullOrEmpty(slot.displayLabel))
            slot.displayLabel = string.IsNullOrEmpty(analyzeTeacherDisplayLabel) ? "Analyze Teacher - All Abilities" : analyzeTeacherDisplayLabel;

        slot.alliance = Alliances.Hero;
        slot.grantAllAbilities = true;
        slot.infiniteMana = true;
        slot.autoSkipTurn = false;
        slot.overrideStats = true;
        slot.maxHP = Mathf.Max(slot.maxHP, 999);
        slot.maxMP = Mathf.Max(slot.maxMP, 9999);
        slot.speed = Mathf.Max(slot.speed, 13);
        slot.move = Mathf.Max(slot.move, 6);
        slot.jump = Mathf.Max(slot.jump, 5);
    }

    LabUnitSlot CreateDefaultReinAnalyzerSlot()
    {
        LabUnitSlot rein = new LabUnitSlot
        {
            recipeName = "Rein",
            displayLabel = "Rein - Analyzer",
            alliance = Alliances.Hero,
            level = 30,
            x = 1,
            z = 5,
            facing = Directions.East,
            grantAllAbilities = false,
            infiniteMana = true,
            autoSkipTurn = false,
            overrideStats = true,
            maxHP = 999,
            maxMP = 9999,
            speed = 12,
            move = 6,
            jump = 5
        };
        return rein;
    }

    LabUnitSlot CreateAnalyzeTeacherSlot(LabUnitSlot reinSlot)
    {
        LabUnitSlot teacher = new LabUnitSlot
        {
            recipeName = string.IsNullOrEmpty(analyzeTeacherRecipeName) ? "Lucy" : analyzeTeacherRecipeName,
            displayLabel = string.IsNullOrEmpty(analyzeTeacherDisplayLabel) ? "Analyze Teacher - All Abilities" : analyzeTeacherDisplayLabel,
            alliance = Alliances.Hero,
            level = reinSlot != null ? Mathf.Max(30, reinSlot.level) : 30,
            x = reinSlot != null ? reinSlot.x : 1,
            z = reinSlot != null ? reinSlot.z - 1 : 4,
            facing = Directions.East,
            grantAllAbilities = true,
            infiniteMana = true,
            autoSkipTurn = false,
            overrideStats = true,
            maxHP = 999,
            maxMP = 9999,
            speed = 14,
            move = 6,
            jump = 5
        };
        return teacher;
    }

    bool IsReinSlot(LabUnitSlot slot)
    {
        if (slot == null)
            return false;

        string recipe = AbilityCatalog.CleanName(slot.recipeName);
        string label = AbilityCatalog.CleanName(slot.displayLabel);
        return recipe.Contains("rein") || label.Contains("rein");
    }

    bool IsAnalyzeTeacherSlot(LabUnitSlot slot)
    {
        if (slot == null)
            return false;

        string label = AbilityCatalog.CleanName(slot.displayLabel);
        return label.Contains("analyze teacher") || label.Contains("all abilities teacher");
    }

    Unit SpawnSlot(LabUnitSlot slot, BattleController owner, Board board, Transform container)
    {
        if (slot == null || string.IsNullOrEmpty(slot.recipeName))
            return null;

        GameObject instance = UnitFactory.Create(slot.recipeName, Mathf.Max(1, slot.level));
        if (instance == null)
            return null;

        instance.transform.SetParent(container, false);

        Unit unit = instance.GetComponent<Unit>();
        if (unit == null)
            return null;

        ApplyPresentation(unit, slot);
        ApplyAlliance(unit, slot.alliance);
        ApplyStats(unit, slot);

        Tile tile = FindBestOpenTile(board, slot.x, slot.z);
        unit.Place(tile);
        unit.dir = slot.facing;
        unit.Match();

        if (slot.grantAllAbilities)
            GrantAllAbilities(unit);

        if (slot.infiniteMana && unit.GetComponent<AbilityTestInfiniteMana>() == null)
        {
            AbilityTestInfiniteMana infiniteMana = unit.gameObject.AddComponent<AbilityTestInfiniteMana>();
            infiniteMana.manaAmount = Mathf.Max(999, slot.maxMP);
        }

        if (slot.autoSkipTurn && unit.GetComponent<AbilityTestAutoSkipTurn>() == null)
            unit.gameObject.AddComponent<AbilityTestAutoSkipTurn>();

        return unit;
    }

    void ApplyPresentation(Unit unit, LabUnitSlot slot)
    {
        UnitProfile profile = unit.GetComponent<UnitProfile>();
        if (profile == null)
            profile = unit.gameObject.AddComponent<UnitProfile>();

        if (!string.IsNullOrEmpty(slot.displayLabel))
            profile.displayName = slot.displayLabel;
    }

    void ApplyAlliance(Unit unit, Alliances allianceType)
    {
        Alliance alliance = unit.GetComponent<Alliance>();
        if (alliance == null)
            alliance = unit.gameObject.AddComponent<Alliance>();
        alliance.type = allianceType;
    }

    void ApplyStats(Unit unit, LabUnitSlot slot)
    {
        if (!slot.overrideStats)
            return;

        Stats stats = unit.GetComponent<Stats>();
        if (stats == null)
            return;

        int hp = Mathf.Max(1, slot.maxHP);
        int mp = Mathf.Max(1, slot.maxMP);

        stats.SetValue(StatTypes.MHP, hp, false);
        stats.SetValue(StatTypes.HP, hp, false);
        stats.SetValue(StatTypes.MMP, mp, false);
        stats.SetValue(StatTypes.MP, mp, false);
        stats.SetValue(StatTypes.SPD, Mathf.Max(0, slot.speed), false);
        stats.SetValue(StatTypes.MOV, Mathf.Max(0, slot.move), false);
        stats.SetValue(StatTypes.JMP, Mathf.Max(0, slot.jump), false);
    }

    void GrantAllAbilities(Unit unit)
    {
        if (unit == null)
            return;

        if (hideOriginalCatalogsOnTester)
        {
            AbilityCatalog[] existingCatalogs = unit.GetComponentsInChildren<AbilityCatalog>(true);
            for (int i = 0; i < existingCatalogs.Length; ++i)
            {
                if (existingCatalogs[i] != null)
                    existingCatalogs[i].gameObject.SetActive(false);
            }
        }

        GameObject root = new GameObject("Ability Test Catalog - All Abilities");
        root.transform.SetParent(unit.transform, false);

        AbilityCatalog catalog = root.AddComponent<AbilityCatalog>();
        catalog.recipeName = "Ability Test Lab";
        catalog.useManualUnlockList = true;
        catalog.useLoadout = false;
        catalog.unlockedEntries.Clear();
        catalog.equippedEntries.Clear();

        Dictionary<string, bool> added = new Dictionary<string, bool>();

        if (includeEveryCatalogRecipeAbility)
            AddAbilitiesFromCatalogRecipes(catalog, root.transform, added);

        if (includeUncataloguedAbilityPrefabs)
            AddUncataloguedAbilityPrefabs(catalog, root.transform, added);
    }

    void AddAbilitiesFromCatalogRecipes(AbilityCatalog catalog, Transform root, Dictionary<string, bool> added)
    {
        AbilityCatalogRecipe[] recipes = Resources.LoadAll<AbilityCatalogRecipe>("Ability Catalog Recipes");
        for (int r = 0; r < recipes.Length; ++r)
        {
            AbilityCatalogRecipe recipe = recipes[r];
            if (recipe == null || recipe.categories == null)
                continue;

            for (int c = 0; c < recipe.categories.Length; ++c)
            {
                AbilityCatalogRecipe.Category category = recipe.categories[c];
                if (category == null || category.entries == null)
                    continue;

                string categoryName = string.IsNullOrEmpty(category.name) ? recipe.name : category.name;
                Transform categoryRoot = FindOrCreateCategory(root, categoryName);

                for (int e = 0; e < category.entries.Length; ++e)
                    TryAddAbility(catalog, categoryRoot, categoryName, category.entries[e], added);
            }
        }
    }

    void AddUncataloguedAbilityPrefabs(AbilityCatalog catalog, Transform root, Dictionary<string, bool> added)
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Abilities");
        if (prefabs == null || prefabs.Length == 0)
            return;

        Transform categoryRoot = FindOrCreateCategory(root, "Uncatalogued");
        for (int i = 0; i < prefabs.Length; ++i)
        {
            GameObject prefab = prefabs[i];
            if (prefab == null || prefab.GetComponent<Ability>() == null)
                continue;

            TryAddAbilityPrefab(catalog, categoryRoot, "Uncatalogued", prefab.name, prefab, added);
        }
    }

    void TryAddAbility(AbilityCatalog catalog, Transform categoryRoot, string categoryName, string entryName, Dictionary<string, bool> added)
    {
        if (string.IsNullOrEmpty(entryName))
            return;

        GameObject prefab = AbilityCatalog.LoadAbilityPrefab(categoryName, entryName);
        if (prefab == null)
            return;

        TryAddAbilityPrefab(catalog, categoryRoot, categoryName, entryName, prefab, added);
    }

    void TryAddAbilityPrefab(AbilityCatalog catalog, Transform categoryRoot, string categoryName, string entryName, GameObject prefab, Dictionary<string, bool> added)
    {
        if (catalog == null || categoryRoot == null || prefab == null)
            return;

        Ability prefabAbility = prefab.GetComponent<Ability>();
        if (prefabAbility == null)
            return;

        string key = AbilityCatalog.CleanName(prefab.name);
        if (added.ContainsKey(key))
            return;
        added.Add(key, true);

        GameObject instance = Instantiate(prefab);
        instance.name = prefab.name;
        instance.transform.SetParent(categoryRoot, false);

        Ability ability = instance.GetComponent<Ability>();
        if (ability == null)
            return;

        catalog.RegisterAbility(ability, categoryName, entryName);
        catalog.UnlockAbility(entryName);
        catalog.UnlockAbility(prefab.name);
        catalog.EquipAbility(entryName);
        catalog.EquipAbility(prefab.name);
    }

    Transform FindOrCreateCategory(Transform root, string categoryName)
    {
        if (root == null)
            return null;

        if (string.IsNullOrEmpty(categoryName))
            categoryName = "Abilities";

        for (int i = 0; i < root.childCount; ++i)
        {
            Transform child = root.GetChild(i);
            if (child.name == categoryName)
                return child;
        }

        GameObject obj = new GameObject(categoryName);
        obj.transform.SetParent(root, false);
        return obj.transform;
    }

    Tile FindBestOpenTile(Board board, int x, int z)
    {
        if (board == null)
            return null;

        Point requested = new Point(x, z);
        Tile exact = board.GetTile(requested);
        if (exact != null && exact.content == null)
            return exact;

        List<Tile> all = board.GetAllSelectableTiles();
        Tile best = null;
        float bestScore = float.MaxValue;
        for (int i = 0; i < all.Count; ++i)
        {
            Tile tile = all[i];
            if (tile == null || tile.content != null)
                continue;

            float dx = tile.pos.x - x;
            float dz = tile.pos.y - z;
            float score = dx * dx + dz * dz + Mathf.Abs(tile.height) * 0.01f;
            if (best == null || score < bestScore)
            {
                best = tile;
                bestScore = score;
            }
        }

        return best != null ? best : exact;
    }

    void SelectStartingTile(BattleController owner, Tile tile)
    {
        if (owner == null || tile == null)
            return;

        owner.selectedTile = tile;
        owner.pos = tile.pos;
        if (owner.tileSelectionIndicator != null)
            owner.tileSelectionIndicator.localPosition = tile.center;
    }

    bool IsSelectableHero(LabUnitSlot slot, Unit unit)
    {
        return slot != null && unit != null && slot.alliance == Alliances.Hero && !slot.autoSkipTurn;
    }

    void ClearRuntimeUnits(BattleController owner)
    {
        if (owner == null)
            return;

        Transform oldUnits = owner.transform.Find("Units");
        if (oldUnits != null)
            Destroy(oldUnits.gameObject);

        Transform oldLabUnits = owner.transform.Find("Ability Test Lab Units");
        if (oldLabUnits != null)
            Destroy(oldLabUnits.gameObject);
    }

    public static LabUnitSlot[] DefaultSlots()
    {
        return new LabUnitSlot[]
        {
            new LabUnitSlot
            {
                recipeName = "Rein",
                displayLabel = "Rein - Analyzer",
                alliance = Alliances.Hero,
                level = 30,
                x = 1,
                z = 5,
                facing = Directions.East,
                grantAllAbilities = false,
                infiniteMana = true,
                autoSkipTurn = false,
                overrideStats = true,
                maxHP = 999,
                maxMP = 9999,
                speed = 12,
                move = 6,
                jump = 5
            },
            new LabUnitSlot
            {
                recipeName = "Lucy",
                displayLabel = "Analyze Teacher - All Abilities",
                alliance = Alliances.Hero,
                level = 30,
                x = 1,
                z = 4,
                facing = Directions.East,
                grantAllAbilities = true,
                infiniteMana = true,
                autoSkipTurn = false,
                overrideStats = true,
                maxHP = 999,
                maxMP = 9999,
                speed = 14,
                move = 6,
                jump = 5
            },
            new LabUnitSlot
            {
                recipeName = "Lucy",
                displayLabel = "Ally Clock Fast",
                alliance = Alliances.Hero,
                level = 20,
                x = 1,
                z = 3,
                facing = Directions.East,
                grantAllAbilities = false,
                infiniteMana = false,
                autoSkipTurn = true,
                overrideStats = true,
                maxHP = 500,
                maxMP = 100,
                speed = 15,
                move = 5,
                jump = 4
            },
            new LabUnitSlot
            {
                recipeName = "Rosemary",
                displayLabel = "Ally Clock Slow",
                alliance = Alliances.Hero,
                level = 20,
                x = 1,
                z = 7,
                facing = Directions.East,
                grantAllAbilities = false,
                infiniteMana = false,
                autoSkipTurn = true,
                overrideStats = true,
                maxHP = 800,
                maxMP = 100,
                speed = 6,
                move = 4,
                jump = 3
            },
            new LabUnitSlot
            {
                recipeName = "Enemy Warrior",
                displayLabel = "Dummy A - Fast",
                alliance = Alliances.Enemy,
                level = 20,
                x = 6,
                z = 4,
                facing = Directions.West,
                grantAllAbilities = false,
                infiniteMana = false,
                autoSkipTurn = true,
                overrideStats = true,
                maxHP = 999,
                maxMP = 100,
                speed = 14,
                move = 0,
                jump = 0
            },
            new LabUnitSlot
            {
                recipeName = "Enemy Rogue",
                displayLabel = "Dummy B - Medium",
                alliance = Alliances.Enemy,
                level = 20,
                x = 6,
                z = 5,
                facing = Directions.West,
                grantAllAbilities = false,
                infiniteMana = false,
                autoSkipTurn = true,
                overrideStats = true,
                maxHP = 999,
                maxMP = 100,
                speed = 10,
                move = 0,
                jump = 0
            },
            new LabUnitSlot
            {
                recipeName = "Enemy Wizard",
                displayLabel = "Dummy C - Slow",
                alliance = Alliances.Enemy,
                level = 20,
                x = 6,
                z = 6,
                facing = Directions.West,
                grantAllAbilities = false,
                infiniteMana = false,
                autoSkipTurn = true,
                overrideStats = true,
                maxHP = 999,
                maxMP = 999,
                speed = 5,
                move = 0,
                jump = 0
            }
        };
    }
}
