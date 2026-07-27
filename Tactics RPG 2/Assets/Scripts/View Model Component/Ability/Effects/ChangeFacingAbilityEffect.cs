using UnityEngine;
using System.Collections;

public enum FacingChangeMode
{
	North,
	East,
	South,
	West,
	FaceUser,
	FaceAwayFromUser,
	TurnLeft,
	TurnRight,
	TurnAround,
	Random
}

public class ChangeFacingAbilityEffect : BaseAbilityEffect
{
	public FacingChangeMode facingMode = FacingChangeMode.TurnAround;

	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		if (target == null || target.content == null)
			return 0;

		Unit targetUnit = target.content.GetComponent<Unit>();
		Unit user = GetComponentInParent<Unit>();
		if (targetUnit == null)
			return 0;

		targetUnit.dir = GetFacing(targetUnit, user, target);
		targetUnit.Match();
		return 0;
	}

	Directions GetFacing (Unit targetUnit, Unit user, Tile target)
	{
		switch (facingMode)
		{
		case FacingChangeMode.North:
			return Directions.North;
		case FacingChangeMode.East:
			return Directions.East;
		case FacingChangeMode.South:
			return Directions.South;
		case FacingChangeMode.West:
			return Directions.West;
		case FacingChangeMode.FaceUser:
			if (user != null && user.tile != null)
				return (user.tile.pos - target.pos).GetDirection();
			return targetUnit.dir;
		case FacingChangeMode.FaceAwayFromUser:
			if (user != null && user.tile != null)
				return (target.pos - user.tile.pos).GetDirection();
			return targetUnit.dir;
		case FacingChangeMode.TurnLeft:
			return Rotate(targetUnit.dir, -1);
		case FacingChangeMode.TurnRight:
			return Rotate(targetUnit.dir, 1);
		case FacingChangeMode.Random:
			return (Directions)UnityEngine.Random.Range(0, 4);
		default: // TurnAround
			return Rotate(targetUnit.dir, 2);
		}
	}

	Directions Rotate (Directions dir, int steps)
	{
		int value = ((int)dir + steps) % 4;
		if (value < 0)
			value += 4;
		return (Directions)value;
	}
}
