using UnityEngine;
using System.Collections.Generic;

public class PathAbilityArea : AbilityArea
{
<<<<<<< Updated upstream
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
=======
	public int maxSteps = 5;
	public int vertical = 2;
	public bool allowBacktracking = false;
	public bool requireEmptyEndpoint = true;
	public bool includeStartingTile = false;
	public bool allowThroughAllies = true;
	public bool allowThroughEnemies = false;
	public bool allowThroughNeutral = false;

	readonly List<Tile> selectedPath = new List<Tile>();
	Unit actor;
	Board board;

	public IList<Tile> SelectedPath { get { return selectedPath.AsReadOnly(); } }
	public Tile Endpoint { get { return selectedPath.Count > 0 ? selectedPath[selectedPath.Count - 1] : null; } }

	public void Begin (Unit actor, Board board)
	{
		this.actor = actor;
		this.board = board;
		ResetPath();
	}

	public void ResetPath ()
	{
		selectedPath.Clear();
		tiles = selectedPath;
		counter = 1;
	}

	public bool TryAddStep (Tile tile)
	{
		if (!CanAddStep(tile))
			return false;

		selectedPath.Add(tile);
		tiles = selectedPath;
		return true;
	}

	public bool RemoveLastStep ()
	{
		if (selectedPath.Count == 0)
			return false;

		selectedPath.RemoveAt(selectedPath.Count - 1);
		return true;
	}

	public bool HasValidPath ()
	{
		if (selectedPath.Count == 0)
			return false;

		Tile end = Endpoint;
		if (end == null)
			return false;

		return !requireEmptyEndpoint || end.content == null;
	}

	public List<Tile> GetSelectableNextSteps (Board b)
	{
		if (board == null)
			board = b;

		List<Tile> result = new List<Tile>();
		if (actor == null || actor.tile == null || board == null)
			return result;

		Tile from = selectedPath.Count > 0 ? selectedPath[selectedPath.Count - 1] : actor.tile;
		AddSelectableStack(result, from.pos + new Point(0, 1));
		AddSelectableStack(result, from.pos + new Point(1, 0));
		AddSelectableStack(result, from.pos + new Point(0, -1));
		AddSelectableStack(result, from.pos + new Point(-1, 0));
		return result;
	}

	public override List<Tile> GetTilesInArea (Board board, Point pos)
	{
		List<Tile> result = new List<Tile>();
		if (includeStartingTile && actor != null && actor.tile != null)
			result.Add(actor.tile);
		for (int i = 0; i < selectedPath.Count; ++i)
			if (selectedPath[i] != null && !result.Contains(selectedPath[i]))
				result.Add(selectedPath[i]);
		return result;
	}

	bool CanAddStep (Tile tile)
	{
		if (tile == null || actor == null || actor.tile == null)
			return false;

		if (selectedPath.Count >= maxSteps)
			return false;

		Tile from = selectedPath.Count > 0 ? selectedPath[selectedPath.Count - 1] : actor.tile;
		if (!IsAdjacent(from, tile))
			return false;

		if (Mathf.Abs(tile.height - from.height) > vertical)
			return false;

		if (!allowBacktracking && selectedPath.Contains(tile))
			return false;

		return CanPassThrough(tile, selectedPath.Count == maxSteps - 1);
	}

	void AddSelectableStack (List<Tile> result, Point p)
	{
		List<Tile> stack = board.GetSelectableTiles(p);
		for (int i = 0; i < stack.Count; ++i)
			AddIfSelectable(result, stack[i]);
	}

	void AddIfSelectable (List<Tile> result, Tile tile)
	{
		if (CanAddStep(tile))
			result.Add(tile);
	}

	bool IsAdjacent (Tile a, Tile b)
	{
		if (a == null || b == null)
			return false;

		Point delta = b.pos - a.pos;
		return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) == 1;
	}

	bool CanPassThrough (Tile tile, bool wouldBeFinalStep)
	{
		if (tile.content == null)
			return true;

		if (wouldBeFinalStep && requireEmptyEndpoint)
			return false;

		Alliance actorAlliance = actor.GetComponentInChildren<Alliance>();
		Alliance otherAlliance = tile.content.GetComponentInChildren<Alliance>();
		if (otherAlliance == null)
			return allowThroughNeutral;

		if (actorAlliance != null && actorAlliance.IsMatch(otherAlliance, Targets.Ally))
			return allowThroughAllies;
		if (actorAlliance != null && actorAlliance.IsMatch(otherAlliance, Targets.Foe))
			return allowThroughEnemies;

		return allowThroughNeutral;
	}
>>>>>>> Stashed changes
}
