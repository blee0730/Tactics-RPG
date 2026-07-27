using UnityEngine;
using System.Collections;

public class AbilityCatalogRecipe : ScriptableObject 
{
	[System.Serializable]
	public class Category
	{
		public string name;

		// Full potential ability list for this category. These can include future
		// abilities that should not appear until unlocked.
		public string[] entries;
	}

	// If false, the catalog uses the active job progression to decide what is
	// unlocked. This is best for normal class lines like Thief, Shield, Sword, etc.
	// If true, only initiallyUnlockedEntries/runtime UnlockAbility calls decide
	// what is unlocked. This is best for Rein's Analyze and Lazuli's custom magic.
	public bool useManualUnlockList;
	public string[] initiallyUnlockedEntries;

	// If false, every unlocked ability appears in battle.
	// If true, only unlocked abilities listed here appear in battle.
	public bool useLoadout;
	public string[] startingEquippedEntries;

	public Category[] categories;
}
