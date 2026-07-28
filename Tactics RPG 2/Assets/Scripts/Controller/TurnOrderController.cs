using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Static round-based turn order controller.
///
/// Units act once per round. The next actor is always the living unit with the
/// highest current SPD that has not acted this round yet. There is no CTR build-up
/// and no turn-cost penalty for moving or acting.
///
/// Because the next actor is recalculated after every completed turn, the queue
/// stays stable while stats are unchanged, but immediately re-sorts when SPD or a
/// speed-affecting status such as Haste/Slow changes before a unit has acted.
/// </summary>
public class TurnOrderController : MonoBehaviour 
{
	#region Notifications
	public const string RoundBeganNotification = "TurnOrderController.roundBegan";
	public const string TurnCheckNotification = "TurnOrderController.turnCheck";
	public const string TurnBeganNotification = "TurnOrderController.TurnBeganNotification";
	public const string TurnCompletedNotification = "TurnOrderController.turnCompleted";
	public const string RoundEndedNotification = "TurnOrderController.roundEnded";
	public const string TurnOrderChangedNotification = "TurnOrderController.turnOrderChanged";
	#endregion

	#region Fields / Properties
	BattleController battleController;	
	HashSet<Unit> actedThisRound = new HashSet<Unit>();
	Unit activeTurnUnit;
	int roundNumber;

	public Unit CurrentActor
	{
		get
		{
			if (battleController == null || battleController.turn == null)
				return null;
			return battleController.turn.actor;
		}
	}

	public int RoundNumber
	{
		get { return roundNumber; }
	}
	#endregion

	#region MonoBehaviour
	void Awake ()
	{
		battleController = GetComponent<BattleController>();
	}

	void OnEnable ()
	{
		this.AddObserver(OnTurnOrderRelevantChange, Stats.DidChangeNotification(StatTypes.SPD));
		this.AddObserver(OnTurnOrderRelevantChange, Stats.DidChangeNotification(StatTypes.HP));
		this.AddObserver(OnTurnOrderRelevantChange, Status.AddedNotification);
		this.AddObserver(OnTurnOrderRelevantChange, Status.RemovedNotification);
	}

	void OnDisable ()
	{
		this.RemoveObserver(OnTurnOrderRelevantChange, Stats.DidChangeNotification(StatTypes.SPD));
		this.RemoveObserver(OnTurnOrderRelevantChange, Stats.DidChangeNotification(StatTypes.HP));
		this.RemoveObserver(OnTurnOrderRelevantChange, Status.AddedNotification);
		this.RemoveObserver(OnTurnOrderRelevantChange, Status.RemovedNotification);
	}
	#endregion

	#region Public
	public IEnumerator Round ()
	{
		if (battleController == null)
			battleController = GetComponent<BattleController>();

		while (true)
		{
			roundNumber += 1;
			actedThisRound.Clear();
			activeTurnUnit = null;
			this.PostNotification(RoundBeganNotification);
			BroadcastTurnOrderChanged();

			while (HasRemainingUnitsThisRound())
			{
				Unit current = GetNextUnitForTurn();
				if (current == null)
					break;

				activeTurnUnit = current;
				battleController.turn.Change(current);

				// Start-of-turn effects such as DoT should resolve before checking
				// whether a status prevents the unit from acting this turn.
				current.PostNotification(TurnBeganNotification);
				BroadcastTurnOrderChanged();

				if (!CanTakeTurn(current))
				{
					actedThisRound.Add(current);
					current.PostNotification(TurnCompletedNotification);
					activeTurnUnit = null;
					BroadcastTurnOrderChanged();
					continue;
				}

				yield return current;

				actedThisRound.Add(current);
				current.PostNotification(TurnCompletedNotification);
				activeTurnUnit = null;
				BroadcastTurnOrderChanged();
			}

			activeTurnUnit = null;
			BroadcastTurnOrderChanged();
			this.PostNotification(RoundEndedNotification);
		}
	}

	public List<Unit> GetTurnQueuePreview (int count)
	{
		return GetTurnQueuePreview(count, true);
	}

	public List<Unit> GetTurnQueuePreview (int count, bool includeCurrentActor)
	{
		List<Unit> result = new List<Unit>();
		if (count <= 0)
			return result;

		if (battleController == null)
			battleController = GetComponent<BattleController>();
		if (battleController == null)
			return result;

		// Show the current actor first while their turn is open.
		if (includeCurrentActor && activeTurnUnit != null && IsPreviewable(activeTurnUnit) && !actedThisRound.Contains(activeTurnUnit))
			result.Add(activeTurnUnit);

		AddRemainingCurrentRoundTurns(result, count);

		// If the panel asks for more entries than remain this round, append the next
		// round in the same static SPD order so the player can see the loop.
		while (result.Count < count)
		{
			List<Unit> nextRound = GetSortedPreviewableUnits(false);
			if (nextRound.Count == 0)
				break;

			for (int i = 0; i < nextRound.Count && result.Count < count; ++i)
				result.Add(nextRound[i]);
		}

		return result;
	}

	public int GetEffectiveSpeed (Unit unit)
	{
		Stats stats = unit != null ? unit.GetComponent<Stats>() : null;
		if (stats == null)
			return 0;

		float speed = stats[StatTypes.SPD];

		HasteStatusEffect[] hastes = unit.GetComponentsInChildren<HasteStatusEffect>();
		for (int i = 0; i < hastes.Length; ++i)
		{
			if (hastes[i] != null && hastes[i].type == StatTypes.SPD)
				speed *= 2f;
		}

		SlowStatusEffect[] slows = unit.GetComponentsInChildren<SlowStatusEffect>();
		for (int i = 0; i < slows.Length; ++i)
		{
			if (slows[i] != null && slows[i].type == StatTypes.SPD)
				speed *= 0.5f;
		}

		return Mathf.Max(0, Mathf.RoundToInt(speed));
	}
	#endregion

	#region Private
	bool HasRemainingUnitsThisRound ()
	{
		if (battleController == null || battleController.units == null)
			return false;

		for (int i = 0; i < battleController.units.Count; ++i)
		{
			Unit unit = battleController.units[i];
			if (IsPreviewable(unit) && !actedThisRound.Contains(unit))
				return true;
		}

		return false;
	}

	Unit GetNextUnitForTurn ()
	{
		List<Unit> candidates = GetSortedPreviewableUnits(true);
		return candidates.Count > 0 ? candidates[0] : null;
	}

	void AddRemainingCurrentRoundTurns (List<Unit> result, int count)
	{
		List<Unit> remaining = GetSortedPreviewableUnits(true);
		for (int i = 0; i < remaining.Count && result.Count < count; ++i)
		{
			Unit unit = remaining[i];
			if (unit == activeTurnUnit && result.Contains(unit))
				continue;
			result.Add(unit);
		}
	}

	List<Unit> GetSortedPreviewableUnits (bool excludeActedThisRound)
	{
		List<Unit> units = new List<Unit>();
		if (battleController == null || battleController.units == null)
			return units;

		for (int i = 0; i < battleController.units.Count; ++i)
		{
			Unit unit = battleController.units[i];
			if (!IsPreviewable(unit))
				continue;

			if (excludeActedThisRound && actedThisRound.Contains(unit))
				continue;

			if (excludeActedThisRound && unit == activeTurnUnit)
				continue;

			units.Add(unit);
		}

		units.Sort(CompareTurnOrder);
		return units;
	}

	int CompareTurnOrder (Unit a, Unit b)
	{
		int compare = GetEffectiveSpeed(b).CompareTo(GetEffectiveSpeed(a));
		if (compare != 0)
			return compare;

		Stats aStats = a != null ? a.GetComponent<Stats>() : null;
		Stats bStats = b != null ? b.GetComponent<Stats>() : null;
		int aSkill = aStats != null ? aStats[StatTypes.SKL] : 0;
		int bSkill = bStats != null ? bStats[StatTypes.SKL] : 0;
		compare = bSkill.CompareTo(aSkill);
		if (compare != 0)
			return compare;

		int aIndex = battleController.units.IndexOf(a);
		int bIndex = battleController.units.IndexOf(b);
		return aIndex.CompareTo(bIndex);
	}

	bool CanTakeTurn (Unit target)
	{
		if (!IsPreviewable(target))
			return false;

		// A unit that has both command channels locked has nothing meaningful to do,
		// so skip straight to the next unit rather than forcing a Wait/Facing step.
		if (target.cantMove && target.cantAct)
			return false;

		BaseException exc = new BaseException(true);
		target.PostNotification(TurnCheckNotification, exc);
		return exc.toggle;
	}

	bool IsPreviewable (Unit unit)
	{
		if (unit == null || unit.gameObject == null || !unit.gameObject.activeInHierarchy)
			return false;

		if (unit.GetComponentInChildren<KnockOutStatusEffect>() != null)
			return false;

		Health health = unit.GetComponent<Health>();
		if (health != null && health.HP <= health.MinHP)
			return false;

		Stats stats = unit.GetComponent<Stats>();
		if (stats != null && stats[StatTypes.HP] <= 0)
			return false;

		return true;
	}

	void OnTurnOrderRelevantChange (object sender, object args)
	{
		BroadcastTurnOrderChanged();
	}

	void BroadcastTurnOrderChanged ()
	{
		this.PostNotification(TurnOrderChangedNotification, GetTurnQueuePreview(12, true));
	}
	#endregion
}
