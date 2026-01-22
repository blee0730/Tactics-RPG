using UnityEngine;
using System.Collections;

public class StatModifierFeature : Feature
{
	#region Fields / Properties
	public StatTypes type;
	public int amount;
	public WeaponType weaponType;
	public enum WeaponType
	{
		none,
		sword,
		spear,
		bow,
		axe,
		staff,
		whip,
		bottle,
		gauntlet,
		shield,
		dagger,
		hammer,
		fan,
	}

	Stats stats 
	{ 
		get 
		{ 
			return _target.GetComponentInParent<Stats>();
		}
	}
	#endregion

	#region Protected
	protected override void OnApply ()
	{
		stats[type] += amount;
	}

	protected override void OnRemove ()
	{
		stats[type] -= amount;
	}
	#endregion
}