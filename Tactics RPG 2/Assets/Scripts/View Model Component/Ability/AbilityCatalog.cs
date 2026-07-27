using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A unit's ability catalog can now contain every ability the unit may ever
/// learn, while only showing abilities that are both unlocked and equipped.
///
/// - Locked future abilities are hidden from the battle menu.
/// - Unequipped abilities are also hidden, which lets flexible characters like
///   Lazuli carry a large learned spell list but bring only a chosen loadout.
/// - Abilities can still fail CanPerform because of MP, Silence, etc.; those
///   are shown but disabled by the normal menu lock behavior.
/// </summary>
public class AbilityCatalog : MonoBehaviour 
{
<<<<<<< Updated upstream
	public const string ChangedNotification = "AbilityCatalog.ChangedNotification";
=======
>>>>>>> Stashed changes
	#region Types
	public enum JobTier
	{
		None = 0,
		Base = 1,
		Advanced = 2,
		Master = 3
	}

	class JobLine
	{
		public readonly string baseJob;
		public readonly string advancedJob;
		public readonly string masterJob;
		readonly string[] baseAbilities;
		readonly string[] advancedAbilities;
		readonly string[] masterAbilities;

		public JobLine (string baseJob, string advancedJob, string masterJob, string[] baseAbilities, string[] advancedAbilities, string[] masterAbilities)
		{
			this.baseJob = baseJob;
			this.advancedJob = advancedJob;
			this.masterJob = masterJob;
			this.baseAbilities = baseAbilities;
			this.advancedAbilities = advancedAbilities;
			this.masterAbilities = masterAbilities;
		}

		public JobTier TierForJob (string jobName)
		{
			if (SameName(baseJob, jobName))
				return JobTier.Base;
			if (SameName(advancedJob, jobName))
				return JobTier.Advanced;
			if (SameName(masterJob, jobName))
				return JobTier.Master;
			return JobTier.None;
		}

		public bool TryGetRequiredTier (string abilityName, out JobTier requiredTier)
		{
			if (ContainsAbility(baseAbilities, abilityName))
			{
				requiredTier = JobTier.Base;
				return true;
			}
			if (ContainsAbility(advancedAbilities, abilityName))
			{
				requiredTier = JobTier.Advanced;
				return true;
			}
			if (ContainsAbility(masterAbilities, abilityName))
			{
				requiredTier = JobTier.Master;
				return true;
			}

			requiredTier = JobTier.None;
			return false;
		}
	}

	class AbilityRecord
	{
		public string categoryName;
		public string entryName;
		public string leafName;
		public string categoryLeafPath;

		public AbilityRecord (string categoryName, string entryName)
		{
			this.categoryName = categoryName;
			this.entryName = entryName;

			string normalized = NormalizeAbilityResourcePath(entryName);
			leafName = GetResourceLeafName(normalized);
			categoryLeafPath = string.IsNullOrEmpty(categoryName) ? leafName : categoryName + "/" + leafName;
		}
	}
	#endregion

	#region Fields
	public string recipeName;

	// If false, job progression decides what is unlocked.
	// If true, only the runtime/manual unlock list decides what is unlocked.
	public bool useManualUnlockList;

	// Runtime/manual unlock entries. These can be edited in a recipe at spawn,
	// then changed by Analyze, story rewards, job promotion, etc.
	public List<string> unlockedEntries = new List<string>();

	// If false, every unlocked ability is shown.
	// If true, only unlocked abilities listed in equippedEntries are shown.
	public bool useLoadout;

	// Runtime/manual equipped entries. This is the fast-swap list for characters
	// like Lazuli who can know many abilities but only show selected ones.
	public List<string> equippedEntries = new List<string>();

	Dictionary<Ability, AbilityRecord> records = new Dictionary<Ability, AbilityRecord>();

	// Hardcoded from Tactics RPG Jobs List.docx.
	// Ability order rule:
	//   entries 1-3 = base tier
	//   entry 4     = advanced tier
	//   entry 5     = master tier
	static readonly JobLine[] jobLines = new JobLine[]
	{
		new JobLine("Hunter", "Archer", "Sniper",
			new string[] { "Set Trap", "Piercing Shot", "Double Shot" },
			new string[] { "Raining Arrows" },
			new string[] { "Never Miss" }),

		new JobLine("Light Mage", "Light Sage", "Lumomancer",
			new string[] { "Light", "Distort Light", "Laser Beam" },
			new string[] { "Light Mirage", "Sun Beam" },
			new string[] { "Summon Einherjar" }),

		new JobLine("Dark Mage", "Dark Sage", "Necromancer",
			new string[] { "Darkness", "Shadow Shroud", "Dark Orb" },
			new string[] { "Suck Lifeforce" },
			new string[] { "Shadow Warriors" }),

		new JobLine("Beast Trainer", "Beast Tamer", "Beast Master",
			new string[] { "Call Beast", "Restrain", "Pull" },
			new string[] { "Tame Beast" },
			new string[] { "Mimic Beast" }),

		new JobLine("Fire Mage", "Fire Sage", "Pyromancer",
			new string[] { "Flame Lance", "Firewall", "Flame Arrow" },
			new string[] { "Fireball" },
			new string[] { "Flame Walk" }),

		new JobLine("Water Mage", "Water Sage", "Hydromancer",
			new string[] { "Water Ball", "Water Wall", "Water Jet" },
			new string[] { "Water Prison" },
			new string[] { "Tsunami" }),

		new JobLine("Ice Mage", "Ice Sage", "Cryomancer",
			new string[] { "Piercing Icicle", "Ice Wall", "Ice Shield" },
			new string[] { "Blizzard", "Hail" },
			new string[] { "Ice Walk" }),

		new JobLine("Earth Mage", "Earth Sage", "Terramancer",
			new string[] { "Pitfall", "Earth Wall", "Rock Shot" },
			new string[] { "Rockslide", "Earthquake" },
			new string[] { "Terraform" }),

		new JobLine("Lightning Mage", "Lightning Sage", "Electromancer",
			new string[] { "Lightning Bolt", "Lightning Blast", "Electromagnetism" },
			new string[] { "Fast as Lightning" },
			new string[] { "Thunderstorm" }),

		new JobLine("Wind Mage", "Wind Sage", "Aeromancer",
			new string[] { "Air Slash", "Wind Shield", "Vacuum Prison" },
			new string[] { "Tornado" },
			new string[] { "Air Walking" }),

		new JobLine("Time Mage", "Time Sage", "Chronomancer",
			new string[] { "Past Injuries", "Haste", "Slow" },
			new string[] { "Freeze Time" },
			new string[] { "Rewind Time" }),

		new JobLine("Space Mage", "Space Sage", "Spatiomancer",
			new string[] { "Reposition", "Swap Positions", "Vertical Drop" },
			new string[] { "Black Hole" },
			new string[] { "Portal" }),

		new JobLine("Curate", "Cleric", "Bishop",
			new string[] { "Heal", "Cure", "Regen" },
			new string[] { "Recover" },
			new string[] { "Revive" }),

		new JobLine("Neophyte", "Alchemist", "Master Alchemist",
			new string[] { "Concoct", "Throw", "Recreate" },
			new string[] { "Transmutation" },
			new string[] { "Disintegration" }),

		new JobLine("Mercenary", "Myrmidon", "Swordmaster",
			new string[] { "Slash", "Thrust", "Spin" },
			new string[] { "Energy Blade" },
			new string[] { "Counterattack" }),

		new JobLine("Soldier", "Cavalier", "Paladin",
			new string[] { "Hilt Hit", "Javelin", "Pierce Defense" },
			new string[] { "Bulldoze" },
			new string[] { "Cardinal Thrust" }),

		new JobLine("Soldier", "Knight", "General",
			new string[] { "Hilt Hit", "Javelin", "Pierce Defense" },
			new string[] { "Phalanx" },
			new string[] { "Iron Wall" }),

		new JobLine("Fighter", "Brawler", "Monk",
			new string[] { "Kick Back", "Single Target Combo", "Multi Target Combo" },
			new string[] { "Play With Your Food", "Toy With Opponent" },
			new string[] { "Redirect Projectile" }),

		new JobLine("Bandit", "Brigand", "Berserker",
			new string[] { "Smash", "Enhance", "Enhance or Bulk Up", "Axe Toss" },
			new string[] { "Earthquake", "Ground Slam" },
			new string[] { "Rampage" }),

		new JobLine("Griffon Knight", "Pegasus Knight", "Valkyrie",
			new string[] {},
			new string[] { "Crash Landing" },
			new string[] { "Divine Smite" }),

		new JobLine("Griffon Knight", "Wyvern Knight", "Dracoknight",
			new string[] {},
			new string[] { "Carry", "Carry Drop", "Carry/Drop", "Carry or Drop" },
			new string[] { "Breath" }),

		new JobLine("Thief", "Rogue", "Assassin",
			new string[] { "Stealth", "Throwing Knife", "Accel" },
			new string[] { "Pass By" },
			new string[] { "Assassinate" }),

		new JobLine("Artisan", "Blacksmith", "Flectomancer",
			new string[] { "Maintenance", "Build", "Daze" },
			new string[] { "Forge" },
			new string[] { "Automaton" }),

		new JobLine("Performer", "Dancer", "Master Dancer",
			new string[] { "Charm", "Dance", "Rally" },
			new string[] { "Fan Dance" },
			new string[] { "Blade Dance" }),

		new JobLine("Shield Bearer", "Guardian", "Master Guardian",
			new string[] { "Armor", "Taunt", "Shield Bash" },
			new string[] { "Jump to Ally" },
			new string[] { "Invulnerable" }),
	};
	#endregion

	#region MonoBehaviour
	void OnEnable ()
	{
		this.AddObserver(OnAbilityCanPerformCheck, Ability.CanPerformCheck);
	}

	void OnDisable ()
	{
		this.RemoveObserver(OnAbilityCanPerformCheck, Ability.CanPerformCheck);
	}
	#endregion

	#region Setup
	public void ApplyRecipeSettings (AbilityCatalogRecipe recipe)
	{
		if (recipe == null)
			return;

		useManualUnlockList = recipe.useManualUnlockList;
		useLoadout = recipe.useLoadout;

		unlockedEntries.Clear();
		AddEntries(unlockedEntries, recipe.initiallyUnlockedEntries);

		equippedEntries.Clear();
		AddEntries(equippedEntries, recipe.startingEquippedEntries);
	}

	public void RegisterAbility (Ability ability, string categoryName, string entryName)
	{
		if (ability == null)
			return;

		records[ability] = new AbilityRecord(categoryName, entryName);
	}
	#endregion

	#region Ability Loading
	public static GameObject LoadAbilityPrefab (string categoryName, string entryName)
	{
		if (string.IsNullOrEmpty(entryName))
			return null;

		string entryPath = NormalizeAbilityResourcePath(entryName);
		string entryLeafName = GetResourceLeafName(entryPath);
		string categoryPath = NormalizeAbilityResourcePath(categoryName);

		GameObject prefab = null;
		if (entryPath.StartsWith("Abilities/"))
			prefab = Resources.Load<GameObject>(entryPath);
		if (prefab != null)
			return prefab;

		if (entryPath.Contains("/"))
		{
			prefab = Resources.Load<GameObject>("Abilities/" + entryPath);
			if (prefab != null)
				return prefab;
		}

		if (!string.IsNullOrEmpty(categoryPath))
		{
			prefab = Resources.Load<GameObject>("Abilities/" + categoryPath + "/" + entryLeafName);
			if (prefab != null)
				return prefab;
		}

		prefab = Resources.Load<GameObject>("Abilities/" + entryLeafName);
		if (prefab != null)
			return prefab;

		return FindAbilityPrefabByName(entryLeafName);
	}

	static GameObject[] abilityPrefabCache;
	static Dictionary<string, GameObject> abilityPrefabLookup;

	static GameObject FindAbilityPrefabByName (string abilityName)
	{
		if (abilityPrefabLookup == null)
			BuildAbilityPrefabLookup();

		GameObject prefab;
		abilityPrefabLookup.TryGetValue(CleanName(abilityName), out prefab);
		return prefab;
	}

	static void BuildAbilityPrefabLookup ()
	{
		abilityPrefabCache = Resources.LoadAll<GameObject>("Abilities");
		abilityPrefabLookup = new Dictionary<string, GameObject>();

		for (int i = 0; i < abilityPrefabCache.Length; ++i)
		{
			GameObject prefab = abilityPrefabCache[i];
			if (prefab == null)
				continue;

			string key = CleanName(prefab.name);
			if (abilityPrefabLookup.ContainsKey(key))
			{
				Debug.LogWarning(string.Format(
					"Duplicate ability prefab name found under Resources/Abilities: {0}. " +
					"Use an explicit recipe entry path such as 'Fire/{0}' to avoid ambiguity.",
					prefab.name));
				continue;
			}

			abilityPrefabLookup.Add(key, prefab);
		}
	}

	static string NormalizeAbilityResourcePath (string value)
	{
		if (string.IsNullOrEmpty(value))
			return string.Empty;

		string result = value.Trim();
		result = result.Replace("\\", "/");

		if (result.ToLowerInvariant().StartsWith("resources/"))
			result = result.Substring("resources/".Length);
		if (result.ToLowerInvariant().EndsWith(".prefab"))
			result = result.Substring(0, result.Length - ".prefab".Length);

		while (result.StartsWith("/"))
			result = result.Substring(1);
		while (result.EndsWith("/"))
			result = result.Substring(0, result.Length - 1);

		return result;
	}

	static string GetResourceLeafName (string value)
	{
		if (string.IsNullOrEmpty(value))
			return string.Empty;

		int index = value.LastIndexOf('/');
		return index >= 0 ? value.Substring(index + 1) : value;
	}
	#endregion

	#region Public - Raw Catalog Access
	public int CategoryCount ()
	{
		return transform.childCount;
	}

	public GameObject GetCategory (int index)
	{
		if (index < 0 || index >= transform.childCount)
			return null;
		return transform.GetChild(index).gameObject;
	}

	public int AbilityCount (GameObject category)
	{
		return category != null ? category.transform.childCount : 0;
	}

	public Ability GetAbility (int categoryIndex, int abilityIndex)
	{
		GameObject category = GetCategory(categoryIndex);
		if (category == null || abilityIndex < 0 || abilityIndex >= category.transform.childCount)
			return null;
		return category.transform.GetChild(abilityIndex).GetComponent<Ability>();
	}
	#endregion

	#region Public - Visible Battle Menu Access
	public int VisibleCategoryCount ()
	{
		return GetVisibleCategories().Count;
	}

	public GameObject GetVisibleCategory (int visibleIndex)
	{
		List<GameObject> visible = GetVisibleCategories();
		if (visibleIndex < 0 || visibleIndex >= visible.Count)
			return null;
		return visible[visibleIndex];
	}

	public int VisibleAbilityCount (GameObject category)
	{
		return GetVisibleAbilities(category).Count;
	}

	public Ability GetVisibleAbility (int visibleCategoryIndex, int visibleAbilityIndex)
	{
		GameObject category = GetVisibleCategory(visibleCategoryIndex);
		List<Ability> visible = GetVisibleAbilities(category);
		if (visibleAbilityIndex < 0 || visibleAbilityIndex >= visible.Count)
			return null;
		return visible[visibleAbilityIndex];
	}

	public bool IsAbilityVisible (Ability ability)
	{
		return ability != null && IsAbilityUnlocked(ability) && IsAbilityEquipped(ability);
	}

	public Ability FindAbility (string abilityName, bool visibleOnly)
	{
		for (int i = 0; i < transform.childCount; ++i)
		{
			Transform category = transform.GetChild(i);
			for (int j = 0; j < category.childCount; ++j)
			{
				Ability ability = category.GetChild(j).GetComponent<Ability>();
				if (ability == null)
					continue;

				if (!AbilityMatchesEntry(ability, abilityName))
					continue;

				if (visibleOnly && !IsAbilityVisible(ability))
					continue;

				return ability;
			}
		}
		return null;
	}
<<<<<<< Updated upstream


	public string GetAbilityCategoryName (Ability ability)
	{
		AbilityRecord record = GetRecord(ability);
		return record != null ? record.categoryName : string.Empty;
	}

	public string GetAbilityEntryName (Ability ability)
	{
		AbilityRecord record = GetRecord(ability);
		return record != null ? record.entryName : (ability != null ? ability.name : string.Empty);
	}

	public string GetAbilityLeafName (Ability ability)
	{
		AbilityRecord record = GetRecord(ability);
		return record != null ? record.leafName : (ability != null ? ability.name : string.Empty);
	}

	public string GetAbilityAnalyzeKey (Ability ability)
	{
		AbilityRecord record = GetRecord(ability);
		if (record == null)
			return string.Empty;
		return BuildAbilityAnalyzeKey(record.categoryName, record.entryName);
	}

	public Ability AddRuntimeAbilityCopy (Ability sourceAbility, string categoryName, string entryName, bool unlock, bool equip)
	{
		if (sourceAbility == null && string.IsNullOrEmpty(entryName))
			return null;

		if (string.IsNullOrEmpty(entryName) && sourceAbility != null)
			entryName = sourceAbility.name;
		if (string.IsNullOrEmpty(categoryName))
			categoryName = "Analyzed";

		Ability existing = FindAbility(entryName, false);
		if (existing == null && !string.IsNullOrEmpty(categoryName))
			existing = FindAbility(categoryName + "/" + entryName, false);

		if (existing != null)
		{
			RegisterAbility(existing, categoryName, entryName);
			if (unlock)
			{
				UnlockAbility(entryName);
				UnlockAbility(existing.name);
				UnlockAbility(categoryName + "/" + entryName);
			}
			if (equip)
			{
				EquipAbility(entryName);
				EquipAbility(existing.name);
				EquipAbility(categoryName + "/" + entryName);
			}
			NotifyChanged(existing);
			return existing;
		}

		GameObject prefab = LoadAbilityPrefab(categoryName, entryName);
		GameObject instance = null;
		if (prefab != null)
			instance = Instantiate(prefab);
		else if (sourceAbility != null)
			instance = Instantiate(sourceAbility.gameObject);

		if (instance == null)
		{
			Debug.LogWarning("Analyze could not copy ability because no prefab/runtime source was available: " + entryName);
			return null;
		}

		instance.name = instance.name.Replace("(Clone)", "").Trim();
		Transform category = FindOrCreateCategory(categoryName);
		instance.transform.SetParent(category, false);

		Ability ability = instance.GetComponent<Ability>();
		if (ability == null)
		{
			Debug.LogWarning("Analyze copied object without an Ability component: " + instance.name);
			Destroy(instance);
			return null;
		}

		RegisterAbility(ability, categoryName, entryName);

		if (unlock)
		{
			UnlockAbility(entryName);
			UnlockAbility(ability.name);
			UnlockAbility(categoryName + "/" + entryName);
		}

		if (equip)
		{
			EquipAbility(entryName);
			EquipAbility(ability.name);
			EquipAbility(categoryName + "/" + entryName);
		}

		NotifyChanged(ability);
		return ability;
	}

=======
>>>>>>> Stashed changes
	#endregion

	#region Public - Unlocks and Loadout
	public bool IsAbilityUnlocked (Ability ability)
	{
		JobTier requiredTier;
		return IsAbilityUnlocked(ability, out requiredTier);
	}

	public bool IsAbilityUnlocked (Ability ability, out JobTier requiredTier)
	{
		requiredTier = JobTier.None;
		if (ability == null)
			return false;

		if (EntryListContains(unlockedEntries, ability))
			return true;

		if (useManualUnlockList)
			return false;

		string abilityName = CleanName(ability.name);
		if (!IsKnownProgressionAbility(abilityName, out requiredTier))
			return true;

		string jobName = GetCurrentJobName();
		if (string.IsNullOrEmpty(jobName))
			return false;

		for (int i = 0; i < jobLines.Length; ++i)
		{
			JobTier activeTier = jobLines[i].TierForJob(jobName);
			if (activeTier == JobTier.None)
				continue;

			JobTier lineRequiredTier;
			if (!jobLines[i].TryGetRequiredTier(abilityName, out lineRequiredTier))
				continue;

			requiredTier = lineRequiredTier;
			return activeTier >= lineRequiredTier;
		}

		// Known progression ability, but not in this unit's current line.
		return false;
	}

	public bool IsAbilityEquipped (Ability ability)
	{
		if (ability == null)
			return false;
		if (!useLoadout)
			return true;
		return EntryListContains(equippedEntries, ability);
	}

	public void UnlockAbility (string abilityEntry)
	{
<<<<<<< Updated upstream
		int before = unlockedEntries != null ? unlockedEntries.Count : 0;
		AddUniqueEntry(unlockedEntries, abilityEntry);
		if (unlockedEntries != null && unlockedEntries.Count != before)
			NotifyChanged(null);
=======
		AddUniqueEntry(unlockedEntries, abilityEntry);
>>>>>>> Stashed changes
	}

	public void LockAbility (string abilityEntry)
	{
<<<<<<< Updated upstream
		int before = unlockedEntries != null ? unlockedEntries.Count : 0;
		RemoveEntry(unlockedEntries, abilityEntry);
		if (unlockedEntries != null && unlockedEntries.Count != before)
			NotifyChanged(null);
=======
		RemoveEntry(unlockedEntries, abilityEntry);
>>>>>>> Stashed changes
	}

	public void EquipAbility (string abilityEntry)
	{
<<<<<<< Updated upstream
		int before = equippedEntries != null ? equippedEntries.Count : 0;
		AddUniqueEntry(equippedEntries, abilityEntry);
		if (equippedEntries != null && equippedEntries.Count != before)
			NotifyChanged(null);
=======
		AddUniqueEntry(equippedEntries, abilityEntry);
>>>>>>> Stashed changes
	}

	public void UnequipAbility (string abilityEntry)
	{
<<<<<<< Updated upstream
		int before = equippedEntries != null ? equippedEntries.Count : 0;
		RemoveEntry(equippedEntries, abilityEntry);
		if (equippedEntries != null && equippedEntries.Count != before)
			NotifyChanged(null);
=======
		RemoveEntry(equippedEntries, abilityEntry);
>>>>>>> Stashed changes
	}

	public void SetLoadout (string[] abilityEntries)
	{
		equippedEntries.Clear();
		AddEntries(equippedEntries, abilityEntries);
<<<<<<< Updated upstream
		NotifyChanged(null);
=======
>>>>>>> Stashed changes
	}
	#endregion

	#region Notification Handlers
	void OnAbilityCanPerformCheck (object sender, object args)
	{
		Ability ability = sender as Ability;
		BaseException exc = args as BaseException;
		if (ability == null || exc == null)
			return;

		if (!ability.transform.IsChildOf(transform))
			return;

		if (!IsAbilityVisible(ability) && exc.toggle == true)
			exc.FlipToggle();
	}
	#endregion

	#region Private
	List<GameObject> GetVisibleCategories ()
	{
		List<GameObject> result = new List<GameObject>();
		for (int i = 0; i < transform.childCount; ++i)
		{
			GameObject category = transform.GetChild(i).gameObject;
			if (VisibleAbilityCount(category) > 0)
				result.Add(category);
		}
		return result;
	}

	List<Ability> GetVisibleAbilities (GameObject category)
	{
		List<Ability> result = new List<Ability>();
		if (category == null)
			return result;

		for (int i = 0; i < category.transform.childCount; ++i)
		{
			Ability ability = category.transform.GetChild(i).GetComponent<Ability>();
			if (IsAbilityVisible(ability))
				result.Add(ability);
		}
		return result;
	}

	string GetCurrentJobName ()
	{
		Unit unit = GetComponentInParent<Unit>();
		if (unit == null)
			return string.Empty;

		Job[] jobs = unit.GetComponentsInChildren<Job>();
		for (int i = 0; i < jobs.Length; ++i)
		{
			if (jobs[i] == null || !jobs[i].gameObject.activeInHierarchy)
				continue;
			return jobs[i].name;
		}

		return string.Empty;
	}

	static bool IsKnownProgressionAbility (string abilityName, out JobTier requiredTier)
	{
		requiredTier = JobTier.None;
		for (int i = 0; i < jobLines.Length; ++i)
		{
			if (jobLines[i].TryGetRequiredTier(abilityName, out requiredTier))
				return true;
		}
		return false;
	}

	bool EntryListContains (List<string> entries, Ability ability)
	{
		if (entries == null || ability == null)
			return false;

		for (int i = 0; i < entries.Count; ++i)
		{
			if (AbilityMatchesEntry(ability, entries[i]))
				return true;
		}
		return false;
	}

	bool AbilityMatchesEntry (Ability ability, string entry)
	{
		if (ability == null || string.IsNullOrEmpty(entry))
			return false;

		string cleanEntry = CleanName(entry);
		if (cleanEntry == CleanName(ability.name))
			return true;

		AbilityRecord record = GetRecord(ability);
		if (record == null)
			return false;

		return cleanEntry == CleanName(record.entryName)
			|| cleanEntry == CleanName(record.leafName)
			|| cleanEntry == CleanName(record.categoryLeafPath);
	}

	AbilityRecord GetRecord (Ability ability)
	{
		if (ability == null)
			return null;

		AbilityRecord record;
		if (records.TryGetValue(ability, out record))
			return record;

		string categoryName = ability.transform.parent != null ? ability.transform.parent.name : string.Empty;
		record = new AbilityRecord(categoryName, ability.name);
		records[ability] = record;
		return record;
	}

<<<<<<< Updated upstream


	Transform FindOrCreateCategory (string categoryName)
	{
		if (string.IsNullOrEmpty(categoryName))
			categoryName = "Analyzed";

		for (int i = 0; i < transform.childCount; ++i)
		{
			Transform child = transform.GetChild(i);
			if (child != null && CleanName(child.name) == CleanName(categoryName))
				return child;
		}

		GameObject obj = new GameObject(categoryName);
		obj.transform.SetParent(transform, false);
		return obj.transform;
	}

	void NotifyChanged (Ability ability)
	{
		this.PostNotification(ChangedNotification, ability);
	}

	public static string BuildAbilityAnalyzeKey (string categoryName, string entryName)
	{
		if (string.IsNullOrEmpty(entryName))
			return string.Empty;

		string cleanCategory = CleanName(categoryName);
		string cleanEntry = CleanName(entryName);
		if (string.IsNullOrEmpty(cleanCategory))
			return cleanEntry;
		return cleanCategory + "/" + cleanEntry;
	}

=======
>>>>>>> Stashed changes
	static void AddEntries (List<string> list, string[] entries)
	{
		if (list == null || entries == null)
			return;

		for (int i = 0; i < entries.Length; ++i)
			AddUniqueEntry(list, entries[i]);
	}

	static void AddUniqueEntry (List<string> list, string entry)
	{
		if (list == null || string.IsNullOrEmpty(entry))
			return;

		string clean = CleanName(entry);
		for (int i = 0; i < list.Count; ++i)
		{
			if (CleanName(list[i]) == clean)
				return;
		}

		list.Add(entry);
	}

	static void RemoveEntry (List<string> list, string entry)
	{
		if (list == null || string.IsNullOrEmpty(entry))
			return;

		string clean = CleanName(entry);
		for (int i = list.Count - 1; i >= 0; --i)
		{
			if (CleanName(list[i]) == clean)
				list.RemoveAt(i);
		}
	}

	static bool ContainsAbility (string[] abilities, string abilityName)
	{
		if (abilities == null)
			return false;

		for (int i = 0; i < abilities.Length; ++i)
		{
			if (SameName(abilities[i], abilityName))
				return true;
		}
		return false;
	}

	static bool SameName (string a, string b)
	{
		return CleanName(a) == CleanName(b);
	}

	public static string CleanName (string value)
	{
		if (string.IsNullOrEmpty(value))
			return string.Empty;

		string result = value.ToLowerInvariant();
		result = result.Replace("(clone)", "");
		result = result.Replace("/", " ");
		result = result.Replace("-", " ");
		result = result.Replace("_", " ");
		result = result.Replace("(", " ");
		result = result.Replace(")", " ");
		result = result.Replace(",", " ");
		while (result.Contains("  "))
			result = result.Replace("  ", " ");
		return result.Trim();
	}
	#endregion
<<<<<<< Updated upstream
}
=======
}
>>>>>>> Stashed changes
