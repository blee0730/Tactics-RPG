using UnityEngine;

public class SpawnWallTileAbilityEffect : BaseAbilityEffect
{
    public Tile.TileType tileType = Tile.TileType.water;
    public float targetHeight = 2f;
    public bool requireEmptyTile = true;
    public bool transferContentIfOccupied = false;

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

        board.ReplaceTopTile(target.pos, tileType, targetHeight, transferContentIfOccupied);
        return 0;
    }
}
