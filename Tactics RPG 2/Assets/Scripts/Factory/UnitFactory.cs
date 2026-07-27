using UnityEngine;
using System.IO;
using System.Collections;

public static class UnitFactory
{
	#region Public
	public static GameObject Create (string name, int level)
	{
		UnitRecipe recipe = Resources.Load<UnitRecipe>("Unit Recipes/" + name);
		if (recipe == null)
		{
			Debug.LogError("No Unit Recipe for name: " + name);
			return null;
		}
		return Create(recipe, level);
	}

	public static GameObject Create (UnitRecipe recipe, int level)
	{
		GameObject obj = InstantiatePrefab("Units/" + recipe.model);
		obj.name = recipe.name;
		AddProfile(obj, recipe);
		obj.AddComponent<Unit>();
		AddStats(obj);
		AddLocomotion(obj, recipe.locomotion);
		obj.AddComponent<Status>();
		obj.AddComponent<Equipment>();
		AddJob(obj, recipe.job);
		AddRank(obj, level);
		obj.AddComponent<Health>();
		obj.AddComponent<Mana>();
		obj.AddComponent<LastDamageMemory>();
		obj.AddComponent<RewindTimeMemory>();
		AddAttack(obj, recipe.attack);
		AddAbilityCatalog(obj, recipe.abilityCatalog);
		AddAlliance(obj, recipe.alliance);
		AddAttackPattern(obj, recipe.strategy);
		return obj;
	}
	#endregion

	#region Private
	static GameObject InstantiatePrefab (string name)
	{
		GameObject prefab = LoadPrefabFlexible(name);
		return InstantiatePrefabObject(prefab, name);
	}

	static GameObject LoadPrefabFlexible (string name)
	{
		GameObject prefab = Resources.Load<GameObject>(name);
		if (prefab != null)
			return prefab;

		int slash = name.LastIndexOf('/');
		if (slash < 0)
			return null;

		string folder = name.Substring(0, slash);
		string leaf = name.Substring(slash + 1);
		GameObject[] options = Resources.LoadAll<GameObject>(folder);
		string cleanLeaf = AbilityCatalog.CleanName(leaf);
		for (int i = 0; i < options.Length; ++i)
		{
			if (options[i] == null)
				continue;
			if (AbilityCatalog.CleanName(options[i].name) == cleanLeaf)
				return options[i];
		}

		return null;
	}

	static GameObject InstantiatePrefabObject (GameObject prefab, string fallbackName)
	{
		if (prefab == null)
		{
			Debug.LogError("No Prefab for name: " + fallbackName);
			return new GameObject(fallbackName);
		}
		GameObject instance = GameObject.Instantiate(prefab);
		instance.name = instance.name.Replace("(Clone)", "");
		return instance;
	}

	static GameObject InstantiateAbilityPrefab (string categoryName, string abilityEntry)
	{
		GameObject prefab = AbilityCatalog.LoadAbilityPrefab(categoryName, abilityEntry);
		if (prefab == null)
		{
			Debug.LogError(string.Format(
				"No Ability Prefab found for catalog category '{0}' entry '{1}'. " +
				"Expected Resources/Abilities/{0}/{1}, Resources/Abilities/{1}, or a recursive match under Resources/Abilities.",
				categoryName, abilityEntry));
			return null;
		}

		return InstantiatePrefabObject(prefab, abilityEntry);
	}


	static void AddProfile (GameObject obj, UnitRecipe recipe)
	{
		UnitProfile profile = obj.GetComponent<UnitProfile>();
		if (profile == null)
			profile = obj.AddComponent<UnitProfile>();

		profile.displayName = !string.IsNullOrEmpty(recipe.displayName) ? recipe.displayName : recipe.name;
		profile.statusPortrait = recipe.statusPortrait;
		profile.dialogueCharacterName = !string.IsNullOrEmpty(recipe.dialogueCharacterName) ? recipe.dialogueCharacterName : profile.displayName;
	}

	static void AddStats (GameObject obj)
	{
		Stats s = obj.AddComponent<Stats>();
		s.SetValue(StatTypes.LVL, 1, false);
	}

	static void AddJob (GameObject obj, string name)
	{
		GameObject instance = InstantiatePrefab("Jobs/" + name);
		instance.transform.SetParent(obj.transform);
		Job job = instance.GetComponent<Job>();
		job.Employ();
		job.LoadDefaultStats();
	}

	static void AddLocomotion (GameObject obj, Locomotions type)
	{
		switch (type)
		{
		case Locomotions.Walk:
			obj.AddComponent<WalkMovement>();
			break;
		case Locomotions.Fly:
			obj.AddComponent<FlyMovement>();
			break;
		case Locomotions.Teleport:
			obj.AddComponent<TeleportMovement>();
			break;
		}
	}

	static void AddAlliance (GameObject obj, Alliances type)
	{
		Alliance alliance = obj.AddComponent<Alliance>();
		alliance.type = type;
	}

	static void AddRank (GameObject obj, int level)
	{
		Rank rank = obj.AddComponent<Rank>();
		rank.Init(level);
	}

	static void AddAttack (GameObject obj, string name)
	{
		GameObject instance = InstantiatePrefab("Abilities/" + name);
		instance.transform.SetParent(obj.transform);
	}

	static void AddAbilityCatalog (GameObject obj, string name)
	{
		GameObject main = new GameObject("Ability Catalog");
		main.transform.SetParent(obj.transform);
		AbilityCatalog catalog = main.AddComponent<AbilityCatalog>();
		catalog.recipeName = name;

		AbilityCatalogRecipe recipe = Resources.Load<AbilityCatalogRecipe>("Ability Catalog Recipes/" + name);
		if (recipe == null)
		{
			Debug.LogError("No Ability Catalog Recipe Found: " + name);
			return;
		}

		catalog.ApplyRecipeSettings(recipe);

		for (int i = 0; i < recipe.categories.Length; ++i)
		{
			GameObject category = new GameObject( recipe.categories[i].name );
			category.transform.SetParent(main.transform);

			for (int j = 0; j < recipe.categories[i].entries.Length; ++j)
			{
				GameObject ability = InstantiateAbilityPrefab(recipe.categories[i].name, recipe.categories[i].entries[j]);
				if (ability == null)
					continue;

				ability.transform.SetParent(category.transform);
				catalog.RegisterAbility(ability.GetComponent<Ability>(), recipe.categories[i].name, recipe.categories[i].entries[j]);
			}
		}
	}

	static void AddAttackPattern (GameObject obj, string name)
	{
		Driver driver = obj.AddComponent<Driver>();
		if (string.IsNullOrEmpty(name))
		{
			driver.normal = Drivers.Human;
		}
		else
		{
			driver.normal = Drivers.Computer;
			GameObject instance = InstantiatePrefab("Attack Pattern/" + name);
			instance.transform.SetParent(obj.transform);
		}
	}
	#endregion
}