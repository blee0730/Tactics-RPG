using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseAbilityMenuState : BattleState
{
	protected string menuTitle;
	protected List<string> menuOptions;

	public override void Enter ()
	{
		base.Enter ();
		SelectTile(turn.actor.tile);
		if (driver.Current == Drivers.Human)
			LoadMenu();
	}

	public override void Exit ()
	{
		base.Exit ();
		abilityMenuPanelController.Hide();
	}

	protected override void OnFire (object sender, InfoEventArgs<int> e)
	{
		if (e.info == 0)
			Confirm();
		else
			Cancel();
	}

	protected override void OnMove (object sender, InfoEventArgs<Point> e)
	{
		if (cameraRig.transform.rotation.eulerAngles.y >= 270 && cameraRig.transform.rotation.eulerAngles.y <= 360 && (e.info.x > 0 || e.info.y < 0))
			abilityMenuPanelController.Next();
		else if(cameraRig.transform.rotation.eulerAngles.y >= 0 && cameraRig.transform.rotation.eulerAngles.y < 90 && (e.info.x < 0 || e.info.y > 0))
			abilityMenuPanelController.Next();
		else if(cameraRig.transform.rotation.eulerAngles.y >= 90 && cameraRig.transform.rotation.eulerAngles.y < 180 && (e.info.x > 0 || e.info.y > 0))
			abilityMenuPanelController.Next();
		else if(cameraRig.transform.rotation.eulerAngles.y >= 180 && cameraRig.transform.rotation.eulerAngles.y < 270 && (e.info.x > 0 || e.info.y < 0))
			abilityMenuPanelController.Next();
		else
			abilityMenuPanelController.Previous();
	}

	protected abstract void LoadMenu ();
	protected abstract void Confirm ();
	protected abstract void Cancel ();
}