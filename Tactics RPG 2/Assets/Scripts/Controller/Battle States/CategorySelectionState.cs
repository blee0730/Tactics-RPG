using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CategorySelectionState : BaseAbilityMenuState 
{
	AbilityCatalog catalog;

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
		if (menuOptions == null)
			menuOptions = new List<string>();
		else
			menuOptions.Clear();

		menuTitle = "Action";
		menuOptions.Add("Attack");

		catalog = turn.actor.GetComponentInChildren<AbilityCatalog>();
		if (catalog != null)
		{
			for (int i = 0; i < catalog.VisibleCategoryCount(); ++i)
			{
				GameObject category = catalog.GetVisibleCategory(i);
				if (category != null)
					menuOptions.Add(category.name);
			}
		}
		
		abilityMenuPanelController.Show(menuTitle, menuOptions);
	}

	protected override void Confirm ()
	{
		if (abilityMenuPanelController.selection == 0)
			Attack();
		else
			SetCategory(abilityMenuPanelController.selection - 1);
	}
	
	protected override void Cancel ()
	{
		owner.ChangeState<CommandSelectionState>();
	}

	void Attack ()
	{
		turn.ability = turn.actor.GetComponentInChildren<Ability>();
		owner.ChangeState<AbilityTargetState>();
	}

	void SetCategory (int index)
	{
		if (catalog == null || catalog.GetVisibleCategory(index) == null)
			return;

		ActionSelectionState.category = index;
		owner.ChangeState<ActionSelectionState>();
	}
}
