using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WalkMovement : Movement 
{
	#region Protected
	protected override bool ExpandSearch (Tile from, Tile to)
	{
		// Skip if the distance in height between the two tiles is more than the unit can jump.
		if (Mathf.Abs(from.height - to.height) > jumpHeight)
			return false;

		// Multilayer rule:
		// Do not let a unit switch between stacked surfaces in the exact same X/Z
		// column. That looked like the character falling through the bridge/floor.
		// Layer changes must happen by jumping/dropping to an adjacent tile first:
		// upper(P) -> lower(Q) -> lower(P), or lower(Q) -> upper(P) -> upper(...).
		if (IsDirectSameColumnLayerSwitch(from, to))
			return false;

		// Edge transition rule:
		// If a layer-changing step would pass through a stacked floor/ceiling at
		// the edge, block it. Example: lower B -> upper C is illegal when upper B
		// exists; the unit must have jumped lower A -> upper B first.
		if (searchBoard != null && searchBoard.BlocksLayerTransitionThroughStack(from, to))
			return false;

		// Same-alliance units may be passed through, but occupied tiles are still
		// removed from the final destination list by Movement.Filter.
		if (!CanPassThrough(to))
			return false;

		return base.ExpandSearch(from, to);
	}
	
	protected override bool CanPassThrough(Tile tile)
	{
		if (tile == null || tile.content == null)
			return true;

		Unit other = tile.content.GetComponent<Unit>();
		if (other == null || other == unit)
			return true;

		Alliance mine = unit != null ? unit.GetComponentInChildren<Alliance>() : null;
		Alliance theirs = other.GetComponentInChildren<Alliance>();
		return mine != null && theirs != null && mine.IsMatch(theirs, Targets.Ally);
	}

	public override IEnumerator Traverse (Tile tile)
	{
		unit.Place(tile);

		// Build a list of way points from the unit's 
		// starting tile to the destination tile
		List<Tile> targets = new List<Tile>();
		while (tile != null)
		{
			targets.Insert(0, tile);
			tile = tile.prev;
		}

		// Move to each way point in succession
		for (int i = 1; i < targets.Count; ++i)
		{
			Tile from = targets[i-1];
			Tile to = targets[i];

			if (from.pos != to.pos)
			{
				Directions dir = from.GetDirection(to);
				if (unit.dir != dir)
					yield return StartCoroutine(Turn(dir));
			}

			if (Mathf.Approximately(from.height, to.height))
				yield return StartCoroutine(Walk(to));
			else
				yield return StartCoroutine(Jump(to));
		}

		yield return null;
	}
	#endregion

	#region Private
	IEnumerator Walk (Tile target)
	{
		Tweener tweener = transform.MoveTo(target.center, 0.5f, EasingEquations.Linear);
		while (tweener != null)
			yield return null;
	}

	IEnumerator Jump (Tile to)
	{
		Tweener tweener = transform.MoveTo(to.center, 0.5f, EasingEquations.Linear);

		Tweener t2 = jumper.MoveToLocal(new Vector3(0, 0.25f * 2f, 0), tweener.duration / 2f, EasingEquations.EaseOutQuad);
		t2.loopCount = 1;
		t2.loopType = EasingControl.LoopType.PingPong;

		while (tweener != null)
			yield return null;
	}
	#endregion
}
