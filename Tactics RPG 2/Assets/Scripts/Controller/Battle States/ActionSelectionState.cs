using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionSelectionState : BaseAbilityMenuState 
{
	public static int category;
	AbilityCatalog catalog;
	List<Ability> menuAbilities = new List<Ability>();
	List<int> menuTupleCounts = new List<int>();

	public override void Enter ()
	{
		base.Enter ();
		statPanelController.ShowPrimary(turn.actor.gameObject);
	}
	
	public override void Exit ()
	{
		base.Exit ();
		statPanelController.HidePrimary();
	}

	protected override void LoadMenu ()
	{
		catalog = turn.actor.GetComponentInChildren<AbilityCatalog>();
		GameObject container = catalog != null ? catalog.GetVisibleCategory(category) : null;
		menuTitle = container != null ? container.name : "Action";

<<<<<<< Updated upstream
		if (menuOptions == null)
			menuOptions = new List<string>();
		else
			menuOptions.Clear();

		menuAbilities.Clear();
		menuTupleCounts.Clear();

		int count = container != null ? catalog.VisibleAbilityCount(container) : 0;
		if (count == 0)
		{
			menuOptions.Add("No Abilities");
			menuAbilities.Add(null);
			menuTupleCounts.Add(1);
=======
		int count = container != null ? catalog.VisibleAbilityCount(container) : 0;
		if (menuOptions == null)
			menuOptions = new List<string>(Mathf.Max(count, 1));
		else
			menuOptions.Clear();

		if (count == 0)
		{
			menuOptions.Add("No Abilities");
>>>>>>> Stashed changes
			abilityMenuPanelController.Show(menuTitle, menuOptions);
			abilityMenuPanelController.SetLocked(0, true);
			return;
		}

<<<<<<< Updated upstream
=======
		bool[] locks = new bool[count];
>>>>>>> Stashed changes
		for (int i = 0; i < count; ++i)
		{
			Ability ability = catalog.GetVisibleAbility(category, i);
			if (ability == null)
			{
				menuOptions.Add("Missing Ability");
<<<<<<< Updated upstream
				menuAbilities.Add(null);
				menuTupleCounts.Add(1);
				continue;
			}

			int maxTuple = GetMaxTupleCount(ability);
			for (int tuple = 1; tuple <= maxTuple; ++tuple)
			{
				menuOptions.Add(BuildAbilityLabel(ability, tuple));
				menuAbilities.Add(ability);
				menuTupleCounts.Add(tuple);
			}
		}

		if (menuOptions.Count == 0)
		{
			menuOptions.Add("No Abilities");
			menuAbilities.Add(null);
			menuTupleCounts.Add(1);
=======
				locks[i] = true;
				continue;
			}

			AbilityMagicCost cost = ability.GetComponent<AbilityMagicCost>();
			if (cost)
				menuOptions.Add(string.Format("{0}: {1}", ability.name, cost.amount));
			else
				menuOptions.Add(ability.name);

			// At this point the ability is visible/unlocked/equipped. A lock here
			// now means normal combat restrictions like not enough MP, Silence, etc.
			locks[i] = !ability.CanPerform();
>>>>>>> Stashed changes
		}

		abilityMenuPanelController.Show(menuTitle, menuOptions);
		for (int i = 0; i < menuAbilities.Count; ++i)
		{
			Ability ability = menuAbilities[i];
			int tuple = menuTupleCounts[i];
			bool locked = ability == null || !TupleAbilityModifier.CanPerformWithTuple(ability, tuple);
			abilityMenuPanelController.SetLocked(i, locked);
		}
	}

	int GetMaxTupleCount(Ability ability)
	{
		if (ability == null)
			return 1;

		AnalyzePartialAbilityModifier partial = AnalyzePartialAbilityModifier.Get(ability);
		if (partial != null && !partial.IsFullyLearned)
			return 1;

		AbilityMasteryTracker mastery = AbilityMasteryTracker.GetTrackerForAbility(ability);
		if (mastery == null)
			return 1;

		return mastery.GetMaxTupleCount(ability);
	}

	string BuildAbilityLabel(Ability ability, int tupleCount)
	{
		string label = ability.name;
		label += AnalyzePartialAbilityModifier.GetMenuSuffix(ability);
		label += AbilityMasteryTracker.GetMenuSuffix(ability);
		label += TupleAbilityModifier.GetMenuSuffix(tupleCount);

		AbilityMagicCost cost = ability.GetComponent<AbilityMagicCost>();
		if (cost)
			return string.Format("{0}: {1}", label, TupleAbilityModifier.GetEffectiveMagicCost(ability, tupleCount));

		return label;
	}

	protected override void Confirm ()
	{
<<<<<<< Updated upstream
		int selection = abilityMenuPanelController.selection;
		turn.ability = selection >= 0 && selection < menuAbilities.Count ? menuAbilities[selection] : null;
		int tupleCount = selection >= 0 && selection < menuTupleCounts.Count ? menuTupleCounts[selection] : 1;

		if (turn.ability == null)
			return;

		TupleAbilityModifier.SetActive(turn.ability, tupleCount);
		if (!turn.ability.CanPerform())
		{
			TupleAbilityModifier.ClearActive(turn.ability);
			return;
		}

=======
		turn.ability = catalog != null ? catalog.GetVisibleAbility(category, abilityMenuPanelController.selection) : null;
		if (turn.ability == null || !turn.ability.CanPerform())
			return;

>>>>>>> Stashed changes
		owner.ChangeState<AbilityTargetState>();
	}

	protected override void Cancel ()
	{
		owner.ChangeState<CategorySelectionState>();
	}
}
