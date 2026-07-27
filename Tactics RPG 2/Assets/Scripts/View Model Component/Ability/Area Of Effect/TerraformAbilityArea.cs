using UnityEngine;
using System.Collections.Generic;

public class TerraformAbilityArea : AbilityArea
{
    public int maxOperations = 5;
    public int horizontal = 5;
    public int vertical = 4;
    public float heightStep = 0.25f;
    public bool requireEmptyTiles = false;

    public List<int> operations = new List<int>(); // +1 raise, -1 lower

    public List<Tile> GetSelectableTiles(Board board, Unit actor)
    {
        List<Tile> result = new List<Tile>();
        if (board == null || actor == null || actor.tile == null)
            return result;
        foreach (Tile tile in board.topTiles.Values)
        {
            if (CanSelect(actor, tile))
                result.Add(tile);
        }
        return result;
    }

    public override List<Tile> GetTilesInArea(Board board, Point pos)
    {
        if (tiles == null)
            tiles = new List<Tile>();
        return new List<Tile>(tiles);
    }

    public void ResetOperations()
    {
        if (tiles == null)
            tiles = new List<Tile>();
        tiles.Clear();
        operations.Clear();
    }

    public bool CanSelect(Unit actor, Tile tile)
    {
        if (actor == null || actor.tile == null || tile == null)
            return false;
        if (tiles == null)
            tiles = new List<Tile>();
        if (operations.Count >= maxOperations)
            return false;
        int distance = Mathf.Abs(actor.tile.pos.x - tile.pos.x) + Mathf.Abs(actor.tile.pos.y - tile.pos.y);
        if (distance > horizontal)
            return false;
        if (Mathf.Abs(actor.tile.height - tile.height) > vertical)
            return false;
        if (requireEmptyTiles && tile.content != null)
            return false;
        return true;
    }

    public bool AddOperation(Unit actor, Tile tile, int operation)
    {
        if (!CanSelect(actor, tile))
            return false;
        tiles.Add(tile);
        operations.Add(operation >= 0 ? 1 : -1);
        return true;
    }

    public void RemoveLast()
    {
        if (tiles != null && tiles.Count > 0)
            tiles.RemoveAt(tiles.Count - 1);
        if (operations.Count > 0)
            operations.RemoveAt(operations.Count - 1);
    }

    public bool CanFinish()
    {
        return tiles != null && tiles.Count > 0 && operations.Count == tiles.Count;
    }

    public int GetOperation(Tile tile)
    {
        if (tile == null || tiles == null)
            return 0;
        for (int i = 0; i < tiles.Count; ++i)
            if (tiles[i] == tile && i < operations.Count)
                return operations[i];
        return 0;
    }
}
