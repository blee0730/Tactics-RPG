using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ComputerPlayer : MonoBehaviour 
{
	#region Fields
	BattleController bc;
	Unit actor { get { return bc != null && bc.turn != null ? bc.turn.actor : null; }}
	Alliance alliance { get { return actor != null ? actor.GetComponent<Alliance>() : null; }}
	Unit nearestFoe;
	Unit forcedTauntTarget;
	#endregion
	
	#region MonoBehaviour
	void Awake ()
	{
		bc = GetComponent<BattleController>();
	}
	#endregion
	
	#region Public
	public PlanOfAttack Evaluate ()
	{
		PlanOfAttack poa = CreateSafePlan();

		try
		{
			if (actor == null || bc == null || bc.board == null)
				return poa;

			forcedTauntTarget = GetForcedTauntTarget();
			AttackPattern pattern = actor.GetComponentInChildren<AttackPattern>();
			if (pattern != null)
				pattern.Pick(poa);
			else
				DefaultAttackPattern(poa);

			if (!IsUsablePlanAbility(poa.ability))
				DefaultAttackPattern(poa);

			if (IsUsablePlanAbility(poa.ability))
			{
				if (IsPositionIndependent(poa))
					PlanPositionIndependent(poa);
				else if (IsDirectionIndependent(poa))
					PlanDirectionIndependent(poa);
				else
					PlanDirectionDependent(poa);
			}

			if (poa.ability == null)
				MoveTowardOpponent(poa);
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Format("[ComputerPlayer] AI plan failed for {0}. The unit will wait this turn instead of freezing the battle.\n{1}",
				actor != null ? actor.name : "<null actor>", ex));
			poa = CreateSafePlan();
		}

		return poa;
	}

	public Directions DetermineEndFacingDirection ()
	{
		Directions dir = (Directions)UnityEngine.Random.Range(0, 4);

		try
		{
			if (actor == null)
				return dir;

			FindNearestFoe();
			if (nearestFoe != null)
			{
				Directions start = actor.dir;
				for (int i = 0; i < 4; ++i)
				{
					actor.dir = (Directions)i;
					if (nearestFoe.GetFacing(actor) == Facings.Front)
					{
						dir = actor.dir;
						break;
					}
				}
				actor.dir = start;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Format("[ComputerPlayer] Failed to determine end facing for {0}.\n{1}",
				actor != null ? actor.name : "<null actor>", ex));
		}

		return dir;
	}
	#endregion
	
	#region Private
	PlanOfAttack CreateSafePlan ()
	{
		PlanOfAttack poa = new PlanOfAttack();
		if (actor != null && actor.tile != null)
		{
			poa.moveLocation = actor.tile.pos;
			poa.fireLocation = actor.tile.pos;
			poa.attackDirection = actor.dir;
		}
		return poa;
	}

	void DefaultAttackPattern (PlanOfAttack poa)
	{
		if (poa == null)
			return;

		poa.ability = GetDefaultAbility();
		poa.target = poa.ability != null ? Targets.Foe : Targets.None;
	}

	Ability GetDefaultAbility ()
	{
		if (actor == null)
			return null;

		Ability[] abilities = actor.GetComponentsInChildren<Ability>();
		Ability firstUsable = null;
		for (int i = 0; i < abilities.Length; ++i)
		{
			Ability ability = abilities[i];
			if (!IsUsablePlanAbility(ability))
				continue;

			if (firstUsable == null)
				firstUsable = ability;

			if (AbilityCatalog.CleanName(ability.name) == AbilityCatalog.CleanName("Attack"))
				return ability;
		}

		return firstUsable;
	}

	bool IsUsablePlanAbility (Ability ability)
	{
		if (ability == null)
			return false;
		if (ability.GetComponent<AbilityRange>() == null)
			return false;
		if (ability.GetComponent<AbilityArea>() == null)
			return false;
		return ability.CanPerform();
	}

	bool IsPositionIndependent (PlanOfAttack poa)
	{
		AbilityRange range = poa != null && poa.ability != null ? poa.ability.GetComponent<AbilityRange>() : null;
		return range != null && range.positionOriented == false;
	}
	
	bool IsDirectionIndependent (PlanOfAttack poa)
	{
		AbilityRange range = poa != null && poa.ability != null ? poa.ability.GetComponent<AbilityRange>() : null;
		return range != null && !range.directionOriented;
	}
	
	void PlanPositionIndependent (PlanOfAttack poa)
	{
		List<Tile> moveOptions = GetMoveOptions();
		if (moveOptions.Count == 0)
		{
			poa.ability = null;
			return;
		}

		Tile tile = moveOptions[UnityEngine.Random.Range(0, moveOptions.Count)];
		if (tile == null)
		{
			poa.ability = null;
			return;
		}

		poa.moveLocation = poa.fireLocation = tile.pos;
	}
	
	void PlanDirectionIndependent (PlanOfAttack poa)
	{
		Tile startTile = actor.tile;
		Dictionary<Tile, AttackOption> map = new Dictionary<Tile, AttackOption>();
		AbilityRange ar = poa.ability.GetComponent<AbilityRange>();
		List<Tile> moveOptions = GetMoveOptions();
		
		for (int i = 0; i < moveOptions.Count; ++i)
		{
			Tile moveTile = moveOptions[i];
			if (moveTile == null)
				continue;

			actor.Place(moveTile);
			List<Tile> fireOptions = ar.GetTilesInRange(bc.board);
			if (fireOptions == null)
				continue;
			
			for (int j = 0; j < fireOptions.Count; ++j)
			{
				Tile fireTile = fireOptions[j];
				if (fireTile == null)
					continue;

				AttackOption ao = null;
				if (map.ContainsKey(fireTile))
				{
					ao = map[fireTile];
				}
				else
				{
					ao = new AttackOption();
					map[fireTile] = ao;
					ao.target = fireTile;
					ao.direction = actor.dir;
					RateFireLocation(poa, ao);
				}

				ao.AddMoveTarget(moveTile);
			}
		}
		
		actor.Place(startTile);
		List<AttackOption> list = new List<AttackOption>(map.Values);
		PickBestOption(poa, list);
	}
	
	void PlanDirectionDependent (PlanOfAttack poa)
	{
		Tile startTile = actor.tile;
		Directions startDirection = actor.dir;
		List<AttackOption> list = new List<AttackOption>();
		List<Tile> moveOptions = GetMoveOptions();
		
		for (int i = 0; i < moveOptions.Count; ++i)
		{
			Tile moveTile = moveOptions[i];
			if (moveTile == null)
				continue;

			actor.Place(moveTile);
			
			for (int j = 0; j < 4; ++j)
			{
				actor.dir = (Directions)j;
				AttackOption ao = new AttackOption();
				ao.target = moveTile;
				ao.direction = actor.dir;
				RateFireLocation(poa, ao);
				ao.AddMoveTarget(moveTile);
				list.Add(ao);
			}
		}
		
		actor.Place(startTile);
		actor.dir = startDirection;
		PickBestOption(poa, list);
	}

	bool IsAbilityTargetMatch (PlanOfAttack poa, Tile tile)
	{
		if (poa == null || tile == null)
			return false;

		if (forcedTauntTarget != null && poa.target == Targets.Foe)
			return tile.content == forcedTauntTarget.gameObject;

		if (poa.target == Targets.Tile)
			return true;

		if (poa.target == Targets.None || tile.content == null)
			return false;

		Alliance ownAlliance = alliance;
		Alliance other = tile.content.GetComponentInChildren<Alliance>();
		return ownAlliance != null && other != null && ownAlliance.IsMatch(other, poa.target);
	}
	
	List<Tile> GetMoveOptions ()
	{
		List<Tile> result = new List<Tile>();
		if (actor == null)
			return result;

		Movement movement = actor.GetComponent<Movement>();
		if (movement != null)
		{
			List<Tile> options = movement.GetTilesInRange(bc.board);
			if (options != null)
				result.AddRange(options);
		}

		if (actor.tile != null && !result.Contains(actor.tile))
			result.Add(actor.tile);

		return result;
	}
	
	void RateFireLocation (PlanOfAttack poa, AttackOption option)
	{
		if (poa == null || option == null || poa.ability == null || option.target == null)
			return;

		AbilityArea area = poa.ability.GetComponent<AbilityArea>();
		if (area == null)
			return;

		List<Tile> tiles = area.GetTilesInArea(bc.board, option.target.pos);
		if (tiles == null)
			tiles = new List<Tile>();

		option.areaTargets = tiles;
		option.isCasterMatch = IsAbilityTargetMatch(poa, actor.tile);

		for (int i = 0; i < tiles.Count; ++i)
		{
			Tile tile = tiles[i];
			if (tile == null || actor.tile == tile || !poa.ability.IsTarget(tile))
				continue;
			
			bool isMatch = IsAbilityTargetMatch(poa, tile);
			option.AddMark(tile, isMatch);
		}
	}
	
	void PickBestOption (PlanOfAttack poa, List<AttackOption> list)
	{
		if (poa == null || list == null || list.Count == 0)
		{
			if (poa != null)
				poa.ability = null;
			return;
		}

		int bestScore = 1;
		List<AttackOption> bestOptions = new List<AttackOption>();
		for (int i = 0; i < list.Count; ++i)
		{
			AttackOption option = list[i];
			if (option == null)
				continue;

			int score = option.GetScore(actor, poa.ability);
			if (score > bestScore)
			{
				bestScore = score;
				bestOptions.Clear();
				bestOptions.Add(option);
			}
			else if (score == bestScore)
			{
				bestOptions.Add(option);
			}
		}

		if (bestOptions.Count == 0)
		{
			poa.ability = null;
			return;
		}

		List<AttackOption> finalPicks = new List<AttackOption>();
		bestScore = int.MinValue;
		for (int i = 0; i < bestOptions.Count; ++i)
		{
			AttackOption option = bestOptions[i];
			if (option == null || option.bestMoveTile == null || option.target == null)
				continue;

			int score = option.bestAngleBasedScore;
			if (score > bestScore)
			{
				bestScore = score;
				finalPicks.Clear();
				finalPicks.Add(option);
			}
			else if (score == bestScore)
			{
				finalPicks.Add(option);
			}
		}

		if (finalPicks.Count == 0)
		{
			poa.ability = null;
			return;
		}
		
		AttackOption choice = finalPicks[UnityEngine.Random.Range(0, finalPicks.Count)];
		poa.fireLocation = choice.target.pos;
		poa.attackDirection = choice.direction;
		poa.moveLocation = choice.bestMoveTile.pos;
	}

	Unit GetForcedTauntTarget ()
	{
		if (actor == null)
			return null;

		TauntStatusEffect taunt = actor.GetComponentInChildren<TauntStatusEffect>();
		return taunt != null ? taunt.GetForcedTarget(actor) : null;
	}

	void FindNearestFoe ()
	{
		nearestFoe = null;
		forcedTauntTarget = GetForcedTauntTarget();
		if (forcedTauntTarget != null)
		{
			nearestFoe = forcedTauntTarget;
			return;
		}

		if (actor == null || actor.tile == null || bc == null || bc.board == null || alliance == null)
			return;

		bc.board.Search(actor.tile, delegate(Tile arg1, Tile arg2) {
			if (nearestFoe == null && arg2 != null && arg2.content != null)
			{
				Alliance other = arg2.content.GetComponentInChildren<Alliance>();
				if (other != null && alliance.IsMatch(other, Targets.Foe))
				{
					Unit unit = other.GetComponent<Unit>();
					Stats stats = unit != null ? unit.GetComponent<Stats>() : null;
					if (stats != null && stats[StatTypes.HP] > 0)
					{
						nearestFoe = unit;
						return true;
					}
				}
			}
			return nearestFoe == null;
		});
	}

	void MoveTowardOpponent (PlanOfAttack poa)
	{
		if (poa == null || actor == null || actor.tile == null)
			return;

		List<Tile> moveOptions = GetMoveOptions();
		FindNearestFoe();
		if (nearestFoe != null)
		{
			Tile toCheck = nearestFoe.tile;
			while (toCheck != null)
			{
				if (moveOptions.Contains(toCheck))
				{
					poa.moveLocation = toCheck.pos;
					return;
				}
				toCheck = toCheck.prev;
			}
		}

		poa.moveLocation = actor.tile.pos;
	}
	#endregion
}
