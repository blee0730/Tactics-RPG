using UnityEngine;
using System.Collections;

public class AutoStatusController : MonoBehaviour 
{
	void OnEnable ()
	{
		this.AddObserver(OnHPDidChangeNotification, Stats.DidChangeNotification(StatTypes.HP));
	}
	
	void OnDisable ()
	{
		this.RemoveObserver(OnHPDidChangeNotification, Stats.DidChangeNotification(StatTypes.HP));
	}
	
	void OnHPDidChangeNotification (object sender, object args)
	{
		Stats stats = sender as Stats;
<<<<<<< Updated upstream
		if (stats == null)
			return;

		Unit unit = stats.GetComponent<Unit>();
		if (unit == null)
			return;

		Health health = stats.GetComponent<Health>();
		int defeatThreshold = health != null ? health.MinHP : 0;

		if (stats[StatTypes.HP] > defeatThreshold)
			return;

		Status status = stats.GetComponent<Status>();
		if (status == null)
			status = stats.gameObject.AddComponent<Status>();

		if (status.GetComponentInChildren<KnockOutStatusEffect>() != null)
			return;

		AutoReviveStatusEffect autoRevive = status.GetComponentInChildren<AutoReviveStatusEffect>();
		if (autoRevive != null && autoRevive.TryRevive(stats))
			return;

		StatComparisonCondition condition = status.Add<KnockOutStatusEffect, StatComparisonCondition>();
		condition.Init(StatTypes.HP, defeatThreshold, condition.LessThanOrEqualTo);
=======
		if (stats[StatTypes.HP] == 0)
		{
			Status status = stats.GetComponentInChildren<Status>();
			if (status == null)
				return;

			AutoReviveStatusEffect autoRevive = status.GetComponentInChildren<AutoReviveStatusEffect>();
			if (autoRevive != null && autoRevive.TryRevive(stats))
				return;

			StatComparisonCondition c = status.Add<KnockOutStatusEffect, StatComparisonCondition>();
			c.Init(StatTypes.HP, 0, c.EqualTo);
		}
>>>>>>> Stashed changes
	}
}
