using UnityEngine;
<<<<<<< Updated upstream
=======
using System.Collections;
>>>>>>> Stashed changes
using System.Collections.Generic;

public class ManualPathMoveAbilityEffect : BaseAbilityEffect
{
<<<<<<< Updated upstream
    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        Unit actor = GetComponentInParent<Unit>();
        PathAbilityArea area = GetComponentInParent<Ability>().GetComponent<PathAbilityArea>();
        if (actor == null || area == null || area.tiles == null || area.tiles.Count == 0)
            return 0;

        Tile destination = area.tiles[area.tiles.Count - 1];
        if (destination == null || destination.content != null)
            return 0;

        for (int i = 0; i < area.tiles.Count; ++i)
        {
            Tile step = area.tiles[i];
            if (step == null)
                continue;
            if (actor.tile != null)
                actor.dir = actor.tile.GetDirection(step);
            actor.Place(step);
            actor.Match();
        }
        return 0;
    }
=======
	public bool faceEachStep = true;
	public bool requireEmptyEndpoint = true;

	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		PathAbilityArea pathArea = GetComponentInParent<PathAbilityArea>();
		Unit mover = GetComponentInParent<Unit>();
		if (pathArea == null || mover == null)
			return 0;

		Tile endpoint = pathArea.Endpoint;
		if (endpoint == null)
			return 0;
		if (requireEmptyEndpoint && endpoint.content != null)
			return 0;

		Tile previous = mover.tile;
		for (int i = 0; i < pathArea.SelectedPath.Count; ++i)
		{
			Tile step = pathArea.SelectedPath[i];
			if (step == null)
				continue;

			if (faceEachStep && previous != null)
				mover.dir = previous.GetDirection(step);
			previous = step;
		}

		mover.Place(endpoint);
		mover.Match();
		return 0;
	}
>>>>>>> Stashed changes
}
