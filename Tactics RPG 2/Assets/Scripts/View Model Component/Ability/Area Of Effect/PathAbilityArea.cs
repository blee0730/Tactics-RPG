using UnityEngine;
using System.Collections.Generic;

public class PathAbilityArea : AbilityArea
{
    public int maxSteps = 5;
    public int vertical = 2;
    public bool allowBacktracking = false;
    public bool requireEmptyEndpoint = true;
    public bool includeStartingTile = false;
    public bool allowThroughAllies = true;
    public bool allowThroughEnemies = false;
    public bool allowThroughNeutral = false;

    public override List<Tile> GetTilesInArea(Board board, Point pos)
    {
        if (tiles == null)
            tiles = new List<Tile>();
        return new List<Tile>(tiles);
    }

    public List<Tile> GetSelectableTiles(Board board, Unit actor)
    {
        List<Tile> result = new List<Tile>();
        if (board == null || actor == null || actor.tile == null)
            return result;

        Tile from = tiles != null && tiles.Count > 0 ? tiles[tiles.Count - 1] : actor.tile;

        // Manual walk-style paths use the same edge-based layer rule as normal
        // movement: no direct upper<->lower switch in the same X/Z column.
        // The player must jump/drop to an adjacent lower/upper tile first.
        Point[] dirs = new Point[] { new Point(0, 1), new Point(1, 0), new Point(0, -1), new Point(-1, 0) };
        for (int i = 0; i < dirs.Length; ++i)
        {
            List<Tile> stack = board.GetSelectableTiles(from.pos + dirs[i]);
            for (int j = 0; j < stack.Count; ++j)
            {
                Tile next = stack[j];
                if (CanAddTile(board, actor, next) && !result.Contains(next))
                    result.Add(next);
            }
        }

        return result;
    }

    public void ResetPath()
    {
        if (tiles == null)
            tiles = new List<Tile>();
        tiles.Clear();
    }

    public bool CanAddTile(Board board, Unit actor, Tile tile)
    {
        if (board == null || actor == null || actor.tile == null || tile == null)
            return false;
        if (tiles == null)
            tiles = new List<Tile>();
        if (tiles.Count >= maxSteps)
            return false;
        if (!allowBacktracking && tiles.Contains(tile))
            return false;

        Tile from = tiles.Count > 0 ? tiles[tiles.Count - 1] : actor.tile;
        if (tile == from)
            return false;

        bool sameColumn = tile.pos == from.pos;
        int dx = Mathf.Abs(tile.pos.x - from.pos.x);
        int dy = Mathf.Abs(tile.pos.y - from.pos.y);

        // No direct upper/lower switch inside one stacked column.
        // To reach a tile under a bridge/top tile, the path must step to an
        // adjacent lower tile first, then move horizontally underneath.
        if (sameColumn)
            return false;

        if (dx + dy != 1)
            return false;

        if (Mathf.Abs(tile.height - from.height) > vertical)
            return false;

        // Same edge-clipping rule as normal walking movement. This prevents manual
        // walk-style abilities from drawing a path that jumps through a stacked
        // upper tile to reach the next upper tile.
        if (board.BlocksLayerTransitionThroughStack(from, tile))
            return false;

        return CanPassThrough(actor, tile, false);
    }

    public bool CanFinish(Unit actor)
    {
        if (tiles == null || tiles.Count == 0)
            return false;
        Tile endpoint = tiles[tiles.Count - 1];
        if (requireEmptyEndpoint && endpoint.content != null)
            return false;
        return true;
    }

    public bool TryAddTile(Board board, Unit actor, Tile tile)
    {
        if (!CanAddTile(board, actor, tile))
            return false;
        tiles.Add(tile);
        return true;
    }

    public void RemoveLast()
    {
        if (tiles != null && tiles.Count > 0)
            tiles.RemoveAt(tiles.Count - 1);
    }

    bool CanPassThrough(Unit actor, Tile tile, bool endpoint)
    {
        if (tile.content == null)
            return true;
        Unit other = tile.content.GetComponent<Unit>();
        if (other == null)
            return allowThroughNeutral;
        Alliance actorAlliance = actor.GetComponentInChildren<Alliance>();
        Alliance otherAlliance = other.GetComponentInChildren<Alliance>();
        if (actorAlliance == null || otherAlliance == null)
            return false;
        if (actorAlliance.IsMatch(otherAlliance, Targets.Ally))
            return allowThroughAllies;
        if (actorAlliance.IsMatch(otherAlliance, Targets.Foe))
            return allowThroughEnemies;
        return allowThroughNeutral;
    }
}
