using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityMagicCost : MonoBehaviour 
{
	public const string GetCostNotification = "AbilityMagicCost.GetCostNotification";

	#region Fields
	public int amount;
	Ability owner;
	#endregion

	#region Properties
	public int EffectiveAmount
	{
		get { return GetEffectiveAmount(); }
	}
	#endregion

	#region MonoBehaviour
	void Awake ()
	{
		owner = GetComponent<Ability>();
	}

	void OnEnable ()
	{
		this.AddObserver(OnCanPerformCheck, Ability.CanPerformCheck, owner);
		this.AddObserver(OnDidPerformNotification, Ability.DidPerformNotification, owner);
	}

	void OnDisable ()
	{
		this.RemoveObserver(OnCanPerformCheck, Ability.CanPerformCheck, owner);
		this.RemoveObserver(OnDidPerformNotification, Ability.DidPerformNotification, owner);
	}
	#endregion

	#region Public
	public int GetEffectiveAmount ()
	{
		List<ValueModifier> modifiers = new List<ValueModifier>();
		this.PostNotification(GetCostNotification, modifiers);
		modifiers.Sort(Compare);

		float value = amount;
		for (int i = 0; i < modifiers.Count; ++i)
			value = modifiers[i].Modify(amount, value);

		return Mathf.Max(0, Mathf.CeilToInt(value));
	}
	#endregion

	#region Notification Handlers
	void OnCanPerformCheck (object sender, object args)
	{
		Stats s = GetComponentInParent<Stats>();
		int cost = GetEffectiveAmount();
		if (s[StatTypes.MP] < cost)
		{
			BaseException exc = (BaseException)args;
			exc.FlipToggle();
		}
	}

	void OnDidPerformNotification (object sender, object args)
	{
		Stats s = GetComponentInParent<Stats>();
		s[StatTypes.MP] -= GetEffectiveAmount();
	}
	#endregion

	#region Private
	int Compare (ValueModifier x, ValueModifier y)
	{
		return x.sortOrder.CompareTo(y.sortOrder);
	}
	#endregion
}
