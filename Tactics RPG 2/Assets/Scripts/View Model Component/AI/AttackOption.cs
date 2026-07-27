using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackOption 
{
	#region Classes
	class Mark
	{
		public Tile tile;
		public bool isMatch;
		
		public Mark (Tile tile, bool isMatch)
		{
			this.tile = tile;
			this.isMatch = isMatch;
		}
	}
	#endregion

	#region Fields
	public Tile target;
	public Directions direction;
	public List<Tile> areaTargets = new List<Tile>();
	public bool isCasterMatch;
	public Tile bestMoveTile { get; private set; }
	public int bestAngleBasedScore { get; private set; }
	List<Mark> marks = new List<Mark>();
	List<Tile> moveTargets = new List<Tile>();
	#endregion

	#region Public
	public void AddMoveTarget (Tile tile)
	{
		if (tile == null)
			return;

		if (!isCasterMatch && areaTargets != null && areaTargets.Contains(tile))
			return;
		if (!moveTargets.Contains(tile))
			moveTargets.Add(tile);
	}

	public void AddMark (Tile tile, bool isMatch)
	{
		if (tile == null)
			return;
		marks.Add (new Mark(tile, isMatch));
	}

	// Scores the option based on how many of the targets are of the desired type
	public int GetScore (Unit caster, Ability ability)
	{
		bestMoveTile = null;
		bestAngleBasedScore = 0;

		GetBestMoveTarget(caster, ability);
		if (bestMoveTile == null)
			return 0;

		int score = 0;
		for (int i = 0; i < marks.Count; ++i)
		{
			if (marks[i].isMatch)
				score++;
			else
				score--;
		}

		if (isCasterMatch && areaTargets != null && areaTargets.Contains(bestMoveTile))
			score++;

		return score;
	}
	#endregion

	#region Private
	// Returns the tile which is the most effective point for the caster to attack from
	void GetBestMoveTarget (Unit caster, Ability ability)
	{
		if (moveTargets.Count == 0 || caster == null || ability == null)
			return;
		
		if (IsAbilityAngleBased(ability))
		{
			bestAngleBasedScore = int.MinValue;
			Tile startTile = caster.tile;
			Directions startDirection = caster.dir;
			caster.dir = direction;

			List<Tile> bestOptions = new List<Tile>();
			for (int i = 0; i < moveTargets.Count; ++i)
			{
				Tile option = moveTargets[i];
				if (option == null)
					continue;

				caster.Place(option);
				int score = GetAngleBasedScore(caster);
				if (score > bestAngleBasedScore)
				{
					bestAngleBasedScore = score;
					bestOptions.Clear();
				}

				if (score == bestAngleBasedScore)
				{
					bestOptions.Add(option);
				}
			}
			
			caster.Place(startTile);
			caster.dir = startDirection;

			FilterBestMoves(bestOptions);
			if (bestOptions.Count > 0)
				bestMoveTile = bestOptions[ UnityEngine.Random.Range(0, bestOptions.Count) ];
		}
		else
		{
			bestMoveTile = moveTargets[ UnityEngine.Random.Range(0, moveTargets.Count) ];
		}
	}

	// Indicates whether the angle of attack is an important factor in the
	// application of this ability
	bool IsAbilityAngleBased (Ability ability)
	{
		if (ability == null)
			return false;

		for (int i = 0; i < ability.transform.childCount; ++i)
		{
			HitRate hr = ability.transform.GetChild(i).GetComponent<HitRate>();
			if (hr != null && hr.IsAngleBased)
				return true;
		}
		return false;
	}

	// Scores the option based on how many of the targets are a match
	// and considers the angle of attack to each mark
	int GetAngleBasedScore (Unit caster)
	{
		int score = 0;
		for (int i = 0; i < marks.Count; ++i)
		{
			if (marks[i] == null || marks[i].tile == null)
				continue;

			int value = marks[i].isMatch ? 1 : -1;
			int multiplier = MultiplierForAngle(caster, marks[i].tile);
			score += value * multiplier;
		}
		return score;
	}

	void FilterBestMoves (List<Tile> list)
	{
		if (!isCasterMatch || list == null || areaTargets == null)
			return;

		bool canTargetSelf = false;
		for (int i = 0; i < list.Count; ++i)
		{
			if (areaTargets.Contains(list[i]))
			{
				canTargetSelf = true;
				break;
			}
		}

		if (canTargetSelf)
		{
			for (int i = list.Count - 1; i >= 0; --i)
			{
				if (!areaTargets.Contains(list[i]))
					list.RemoveAt(i);
			}
		}
	}

	int MultiplierForAngle (Unit caster, Tile tile)
	{
		if (caster == null || tile == null || tile.content == null)
			return 0;

		Unit defender = tile.content.GetComponentInChildren<Unit>();
		if (defender == null)
			return 0;

		Facings facing = caster.GetFacing(defender);
		if (facing == Facings.Back)
			return 90;
		if (facing == Facings.Side)
			return 75;
		return 50;
	}
	#endregion
}
