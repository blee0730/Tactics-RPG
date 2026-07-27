using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyMovement : Movement 
{
	#region Protected
	protected override bool ExpandSearch (Tile from, Tile to)
	{
		// Flying ignores normal jump-height limits, but it should still obey the
		// multilayer no-clipping graph. A flying unit should not phase straight
		// through an upper floor to land on a lower splitTop. It follows the same
		// upper/lower route shape as walking, just without touching hazards.
		if (IsDirectSameColumnLayerSwitch(from, to))
			return false;

		if (searchBoard != null && searchBoard.BlocksLayerTransitionThroughStack(from, to))
			return false;

		return base.ExpandSearch(from, to);
	}
	#endregion

	#region Public
	public override IEnumerator Traverse (Tile tile)
	{
		if (tile == null)
			yield break;

		// Use the Board.Search prev chain, just like WalkMovement. This keeps the
		// actual flight trajectory on the same legal layered path that was previewed
		// by the cursor instead of drawing a straight line through stacked floors.
		List<Tile> targets = new List<Tile>();
		Tile step = tile;
		while (step != null)
		{
			targets.Insert(0, step);
			step = step.prev;
		}

		unit.Place(tile);

		if (targets.Count == 0)
			yield break;

		float hoverHeight = 0.75f;
		Tweener tweener = jumper.MoveToLocal(new Vector3(0, hoverHeight, 0), 0.25f, EasingEquations.EaseInOutQuad);
		while (tweener != null)
			yield return null;

		for (int i = 1; i < targets.Count; ++i)
		{
			Tile from = targets[i - 1];
			Tile to = targets[i];

			if (from.pos != to.pos)
			{
				Directions dir = from.GetDirection(to);
				if (unit.dir != dir)
					yield return StartCoroutine(Turn(dir));
			}

			float dist = Mathf.Sqrt(Mathf.Pow(to.pos.x - from.pos.x, 2) + Mathf.Pow(to.pos.y - from.pos.y, 2));
			float vertical = Mathf.Abs(to.center.y - from.center.y);
			float duration = Mathf.Max(0.25f, (dist + vertical) * 0.35f);
			tweener = transform.MoveTo(to.center, duration, EasingEquations.EaseInOutQuad);
			while (tweener != null)
				yield return null;
		}

		// Land on the destination only after the path is complete, so tile hazards
		// along the route are not touched by the flyer during traversal.
		tweener = jumper.MoveToLocal(Vector3.zero, 0.25f, EasingEquations.EaseInOutQuad);
		while (tweener != null)
			yield return null;
	}
	#endregion
}
