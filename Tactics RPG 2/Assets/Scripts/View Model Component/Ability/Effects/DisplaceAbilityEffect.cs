using UnityEngine;
using System.Collections;

public enum DisplaceDirectionMode
{
	AwayFromUser,
	TowardUser,
	UserFacing,
	TargetFacing,
	TargetBack,
<<<<<<< Updated upstream
	TowardAreaCenter,
	AwayFromAreaCenter,
=======
>>>>>>> Stashed changes
	Absolute
}

public class DisplaceAbilityEffect : BaseAbilityEffect
{
	[Header("Displacement")]
	public int distance = 1;
	public DisplaceDirectionMode directionMode = DisplaceDirectionMode.AwayFromUser;
	public bool north = false;
	public bool east = false;
	public bool south = false;
	public bool west = false;

	[Header("Allowed Target Content")]
	public bool moveUnits = true;
	public bool moveObjects = true;
	public bool rotateUnitToMoveDirection = false;

	[Header("Collision / Height")]
	public bool stopAtOccupiedTile = true;
	public bool stopAtMissingTile = true;
	public bool dealFallDamage = true;
	public float fallDamagePercentPerHeight = 0.1f;

	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		if (target == null || target.content == null || distance <= 0)
			return 0;

		Unit attacker = GetComponentInParent<Unit>();
		Board board = GameObject.FindObjectOfType<Board>();
		if (attacker == null || board == null)
			return 0;

		Unit targetUnit = target.content.GetComponent<Unit>();
		if (targetUnit != null && !moveUnits)
			return 0;
		if (targetUnit == null && !moveObjects)
			return 0;

		Point normal;
		if (!TryGetDirection(attacker, target, targetUnit, out normal))
			return 0;

		Tile startTile = targetUnit != null ? targetUnit.tile : target;
		if (startTile == null)
			return 0;

		Tile destination = FindDestination(board, startTile, normal);
		if (destination == null || destination == startTile)
			return 0;

		MoveContent(target.content, targetUnit, startTile, destination, normal.GetDirection());

		if (dealFallDamage && targetUnit != null)
			ApplyFallDamage(targetUnit, startTile, destination);

		return 0;
	}

	bool TryGetDirection (Unit attacker, Tile target, Unit targetUnit, out Point normal)
	{
		normal = new Point(0, 0);

		if (directionMode == DisplaceDirectionMode.Absolute)
			return TryGetAbsoluteDirection(out normal);

		if (target == null || attacker == null || attacker.tile == null)
			return false;

		switch (directionMode)
		{
		case DisplaceDirectionMode.TowardUser:
			normal = (attacker.tile.pos - target.pos).GetDirection().GetNormal();
			return true;
		case DisplaceDirectionMode.UserFacing:
			normal = attacker.dir.GetNormal();
			return true;
		case DisplaceDirectionMode.TargetFacing:
			if (targetUnit == null)
				return false;
			normal = targetUnit.dir.GetNormal();
			return true;
		case DisplaceDirectionMode.TargetBack:
			if (targetUnit == null)
				return false;
			normal = GetOpposite(targetUnit.dir).GetNormal();
			return true;
<<<<<<< Updated upstream
		case DisplaceDirectionMode.TowardAreaCenter:
			return TryGetAreaCenterDirection(target, true, out normal);
		case DisplaceDirectionMode.AwayFromAreaCenter:
			return TryGetAreaCenterDirection(target, false, out normal);
=======
>>>>>>> Stashed changes
		default: // AwayFromUser
			normal = (target.pos - attacker.tile.pos).GetDirection().GetNormal();
			return true;
		}
	}

<<<<<<< Updated upstream
	bool TryGetAreaCenterDirection(Tile target, bool towardCenter, out Point normal)
	{
		normal = new Point(0, 0);
		Ability ability = GetComponentInParent<Ability>();
		AbilityArea area = ability != null ? ability.GetComponent<AbilityArea>() : null;
		Tile center = null;
		if (area != null && area.tiles != null && area.tiles.Count > 0)
			center = area.tiles[0];
		if (center == null || target == null || center == target)
			return false;
		normal = towardCenter ? (center.pos - target.pos).GetDirection().GetNormal() : (target.pos - center.pos).GetDirection().GetNormal();
		return true;
	}

=======
>>>>>>> Stashed changes
	bool TryGetAbsoluteDirection (out Point normal)
	{
		normal = new Point(0, 0);
		if (north)
		{
			normal = Directions.North.GetNormal();
			return true;
		}
		if (east)
		{
			normal = Directions.East.GetNormal();
			return true;
		}
		if (south)
		{
			normal = Directions.South.GetNormal();
			return true;
		}
		if (west)
		{
			normal = Directions.West.GetNormal();
			return true;
		}
		return false;
	}

	Tile FindDestination (Board board, Tile startTile, Point normal)
	{
		Tile destination = startTile;
		for (int i = 0; i < distance; ++i)
		{
			Tile next = board.GetTile(destination.pos + normal);
			if (next == null)
				return stopAtMissingTile ? destination : null;
			if (stopAtOccupiedTile && next.content != null)
				return destination;
			destination = next;
		}
		return destination;
	}

	void MoveContent (GameObject content, Unit targetUnit, Tile from, Tile to, Directions moveDirection)
	{
		if (targetUnit != null)
		{
			if (rotateUnitToMoveDirection)
				targetUnit.dir = moveDirection;
			targetUnit.Place(to);
			targetUnit.Match();
			return;
		}

		if (from != null && from.content == content)
			from.content = null;
		to.content = content;
		content.transform.localPosition = to.center;
	}

	void ApplyFallDamage (Unit targetUnit, Tile from, Tile to)
	{
		float fallDistance = from.height - to.height;
		if (fallDistance <= 0f)
			return;

		Stats stats = targetUnit.GetComponent<Stats>();
		if (stats == null || stats[StatTypes.JMP] >= fallDistance)
			return;

		int currentHP = stats[StatTypes.HP];
		int maxHP = stats[StatTypes.MHP];
		int reduce = Mathf.Min(currentHP, Mathf.FloorToInt(maxHP * fallDamagePercentPerHeight * fallDistance));
		stats.SetValue(StatTypes.HP, currentHP - reduce, false);
	}

	Directions GetOpposite (Directions dir)
	{
		switch (dir)
		{
		case Directions.North:
			return Directions.South;
		case Directions.East:
			return Directions.West;
		case Directions.South:
			return Directions.North;
		default:
			return Directions.East;
		}
	}
}
