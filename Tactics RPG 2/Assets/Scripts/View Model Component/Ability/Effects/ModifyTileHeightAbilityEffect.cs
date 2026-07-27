using UnityEngine;
<<<<<<< Updated upstream

public enum TileHeightChangeMode
{
    Raise,
    Lower,
    Set,
    ToggleRaiseLower
=======
using System.Collections;

public enum TileHeightChangeMode
{
	Raise,
	Lower,
	Set
>>>>>>> Stashed changes
}

public class ModifyTileHeightAbilityEffect : BaseAbilityEffect
{
<<<<<<< Updated upstream
    public TileHeightChangeMode changeMode = TileHeightChangeMode.Raise;
    public float amount = 0.25f;
    public float setHeight = 0f;
    public bool requireEmptyTile = false;
    public float togglePivotHeight = 0f;

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        if (target == null)
            return 0;
        if (requireEmptyTile && target.content != null)
            return 0;

        Board board = GameObject.FindObjectOfType<Board>();
        if (board == null)
            return 0;

        float newHeight = target.height;
        switch (changeMode)
        {
        case TileHeightChangeMode.Lower:
            newHeight = target.height - amount;
            break;
        case TileHeightChangeMode.Set:
            newHeight = setHeight;
            break;
        case TileHeightChangeMode.ToggleRaiseLower:
            newHeight = target.height <= togglePivotHeight ? target.height + amount : target.height - amount;
            break;
        default:
            newHeight = target.height + amount;
            break;
        }

        board.SetTileHeight(target, newHeight);
        return 0;
    }
=======
	public TileHeightChangeMode changeMode = TileHeightChangeMode.Raise;
	public int steps = 1;
	public float setHeight = 0f;
	public bool requireEmptyTile = true;

	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		if (target == null)
			return 0;
		if (requireEmptyTile && target.content != null)
			return 0;

		switch (changeMode)
		{
		case TileHeightChangeMode.Lower:
			for (int i = 0; i < steps; ++i)
				target.Shrink();
			break;
		case TileHeightChangeMode.Set:
			target.height = setHeight;
			target.Load(target.pos, target.height);
			break;
		default:
			for (int i = 0; i < steps; ++i)
				target.Grow();
			break;
		}
		return 0;
	}
>>>>>>> Stashed changes
}
