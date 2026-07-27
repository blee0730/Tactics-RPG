using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour 
{
	#region Fields / Properties
	[SerializeField] GameObject dirtPrefab;
	[SerializeField] GameObject grassPrefab;
	[SerializeField] GameObject stonePrefab;
	[SerializeField] GameObject woodPrefab;
	[SerializeField] GameObject waterPrefab;
	[SerializeField] GameObject skyPrefab;
	[SerializeField] GameObject icePrefab;

	// Compatibility dictionary: the normal/default surface at each X/Z board point.
	// Old systems that expect one tile per Point should still use this.
	public Dictionary<Point, Tile> topTiles = new Dictionary<Point, Tile>();

	// Non-selectable visual/support blocks.
	public Dictionary<Vector3, Tile> fillerTiles = new Dictionary<Vector3, Tile>();

	// Compatibility dictionary: one representative splitTop per X/Z board point.
	// Use selectableTiles for the real multi-layer stack, because more than one
	// splitTop can exist at the same X/Z position.
	public Dictionary<Point, Tile> splitTops = new Dictionary<Point, Tile>();

	// Runtime selectable surfaces at each X/Z board point. This includes topTile
	// and splitTop tiles, sorted from lowest height to highest height.
	public Dictionary<Point, List<Tile>> selectableTiles = new Dictionary<Point, List<Tile>>();

	public Point min { get { return _min; }}
	public Point max { get { return _max; }}
	Point _min;
	Point _max;
	Point[] dirs = new Point[4]
	{
		new Point(0, 1),
		new Point(0, -1),
		new Point(1, 0),
		new Point(-1, 0)
	};
	Color selectedTileColor = new Color(0, 1, 1, 1);
	List<Color> cachedColors = new List<Color>();

	#endregion

	#region Public
	public void Load (LevelData data)
	{
		ClearTileIndexes();

		_min = new Point(int.MaxValue, int.MaxValue);
		_max = new Point(int.MinValue, int.MinValue);
		
		for (int i = 0; i < data.tiles.Count; ++i)
		{
			Tile t = Create(data.tileTypes[i]);
			if (t == null)
				continue;

			t.Load(data.tiles[i]);

			bool isTopTile = data.topTiles != null && i < data.topTiles.Count && data.topTiles[i];
			bool isSplitTop = data.splitTops != null && i < data.splitTops.Count && data.splitTops[i];
			t.topTile = isTopTile;
			t.splitTop = isSplitTop;

			RegisterTile(t);
			
			_min.x = Mathf.Min(_min.x, t.pos.x);
			_min.y = Mathf.Min(_min.y, t.pos.y);
			_max.x = Mathf.Max(_max.x, t.pos.x);
			_max.y = Mathf.Max(_max.y, t.pos.y);
		}
	}

	// Returns the default tile for a board position. This preserves the old API:
	// old code that only knows about one tile per Point still receives the topTile.
	// If no explicit topTile exists, the highest selectable splitTop is returned.
	public Tile GetTile (Point p)
	{
		if (topTiles.ContainsKey(p))
			return topTiles[p];

		List<Tile> stack = GetSelectableTiles(p);
		return stack.Count > 0 ? stack[stack.Count - 1] : null;
	}

	public List<Tile> GetSelectableTiles (Point p)
	{
		if (selectableTiles.ContainsKey(p))
			return new List<Tile>(selectableTiles[p]);
		return new List<Tile>();
	}

	public List<Tile> GetAllSelectableTiles ()
	{
		List<Tile> retValue = new List<Tile>();
		foreach (List<Tile> stack in selectableTiles.Values)
			retValue.AddRange(stack);
		return retValue;
	}

	public int GetLayerIndex (Tile tile)
	{
		if (tile == null)
			return -1;

		List<Tile> stack = GetSelectableTiles(tile.pos);
		return stack.IndexOf(tile);
	}

	public Tile GetClosestSelectableTile (Point p, float height)
	{
		List<Tile> stack = GetSelectableTiles(p);
		if (stack.Count == 0)
			return null;

		Tile best = stack[0];
		float bestDifference = Mathf.Abs(best.height - height);
		for (int i = 1; i < stack.Count; ++i)
		{
			float difference = Mathf.Abs(stack[i].height - height);
			if (difference < bestDifference)
			{
				best = stack[i];
				bestDifference = difference;
			}
		}

		return best;
	}

	public Tile GetTileAtLayer (Point p, int layerIndex)
	{
		List<Tile> stack = GetSelectableTiles(p);
		if (stack.Count == 0)
			return null;

		while (layerIndex < 0)
			layerIndex += stack.Count;
		layerIndex %= stack.Count;
		return stack[layerIndex];
	}

	public Tile ReplaceTopTile(Point p, Tile.TileType tileType, float height, bool transferContent)
	{
		Tile oldTile = GetTile(p);
		GameObject oldContent = oldTile != null ? oldTile.content : null;
		if (oldTile != null)
		{
			oldTile.topTile = false;
		}

		Tile newTile = Create(tileType);
		if (newTile == null)
		{
			RebuildTileIndexes();
			return oldTile;
		}

		newTile.topTile = true;
		newTile.splitTop = false;
		newTile.Load(p, height);

		if (transferContent && oldContent != null)
		{
			Unit unit = oldContent.GetComponent<Unit>();
			if (unit != null)
			{
				unit.Place(newTile);
				unit.Match();
			}
			else
			{
				if (oldTile != null && oldTile.content == oldContent)
					oldTile.content = null;
				newTile.content = oldContent;
				oldContent.transform.localPosition = newTile.center;
			}
		}

		RebuildTileIndexes();
		return newTile;
	}

	public void SetTileHeight(Tile tile, float height)
	{
		if (tile == null)
			return;

		tile.Load(tile.pos, height);
		RebuildTileIndexes();

		if (tile.content != null)
		{
			Unit unit = tile.content.GetComponent<Unit>();
			if (unit != null)
				unit.Match();
		}
	}

	public List<Tile> Search (Tile start, Func<Tile, Tile, bool> addTile)
	{
		List<Tile> retValue = new List<Tile>();
		if (start == null)
			return retValue;

		retValue.Add(start);

		ClearSearch();
		Queue<Tile> checkNext = new Queue<Tile>();
		Queue<Tile> checkNow = new Queue<Tile>();

		start.distance = 0;
		start.layerChanges = 0;
		start.lastLayerChangeStep = 0;
		checkNow.Enqueue(start);

		while (checkNow.Count > 0)
		{
			Tile t = checkNow.Dequeue();

			// Multilayer traversal is edge-based now. Do NOT connect upper and
			// lower selectable tiles in the same X/Z column directly. A walking
			// path must change layers by jumping/dropping to an adjacent tile,
			// then it can continue moving along that lower/upper layer.
			//
			// Example bridge/river route:
			// bridge(P) -> lower adjacent river(Q) -> lower river under bridge(P)
			for (int i = 0; i < 4; ++i)
			{
				List<Tile> nextTiles = GetSelectableTiles(t.pos + dirs[i]);
				for (int j = 0; j < nextTiles.Count; ++j)
					TryAddSearchTile(t, nextTiles[j], addTile, checkNext, retValue);
			}

			if (checkNow.Count == 0)
				SwapReference(ref checkNow, ref checkNext);
		}

		return retValue;
	}


	// Returns true when an adjacent move would change height by clipping
	// through a covered splitTop surface instead of using an open edge.
	//
	// Important distinction:
	// - splitTop means "this is a lower/alternate layer under another selectable layer."
	// - topTile, even at a lower height, is treated as an exposed surface and must
	//   remain selectable/reachable as normal terrain.
	//
	// This avoids the previous flip-flop where lower topTile terrain was treated
	// like a covered bridge-underpass and became impossible to reach.
	//
	// Stable multilayer rule:
	// 1. Same-column layer swaps are never generated by Board.Search.
	// 2. Same-height movement is allowed, even on covered splitTop tiles, because
	//    the unit is already on that layer and is moving horizontally along it.
	// 3. Dropping DOWN directly onto a covered splitTop is blocked; enter the lower
	//    layer through an exposed adjacent lower tile first.
	// 4. Jumping UP directly from a covered splitTop is blocked; move to an exposed
	//    lower edge first, then jump.
	// 5. Lower topTile surfaces are not considered covered just because they are
	//    lower than nearby/stacked surfaces.
	public bool BlocksLayerTransitionThroughStack (Tile from, Tile to)
	{
		return !CanTraverseAdjacentLayerEdge(from, to);
	}

	public bool CanTraverseAdjacentLayerEdge (Tile from, Tile to)
	{
		if (from == null || to == null || from == to)
			return false;

		// A same-column upper/lower switch is always a fall-through-floor move.
		if (from.pos == to.pos)
			return false;

		int dx = Mathf.Abs(from.pos.x - to.pos.x);
		int dy = Mathf.Abs(from.pos.y - to.pos.y);
		if (dx + dy != 1)
			return false;

		// Moving along the current layer does not clip, even when that layer is
		// underneath a bridge/ceiling. This is what lets units walk under a bridge
		// after they have entered the lower layer through an exposed edge.
		if (Mathf.Approximately(from.height, to.height))
			return true;

		// Drop from upper to lower: only covered splitTop landings are blocked.
		// Lower topTile terrain is exposed normal terrain and should remain reachable.
		if (to.height < from.height)
			return !IsCoveredSplitTop(to);

		// Jump from lower to upper: only covered splitTop takeoff tiles are blocked.
		// If the lower tile is a normal topTile, it is an exposed edge and can jump.
		return !IsCoveredSplitTop(from);
	}

	public void SelectTiles (List<Tile> tiles)
	{
		if (tiles == null)
			return;

		for (int i = 0; i <= tiles.Count - 1; i++)
		{
			if (tiles[i] == null)
				continue;

			cachedColors.Add(tiles[i].GetComponent<Renderer>().material.GetColor("_Color"));
			tiles[i].GetComponent<Renderer>().material.SetColor("_Color", selectedTileColor);
		}
	}

	public void DeSelectTiles (List<Tile> tiles)
	{
		if (tiles == null)
		{
			cachedColors.Clear();
			return;
		}

		int colorIndex = 0;
		for (int i = 0; i <= tiles.Count - 1; i++)
		{
			if (tiles[i] == null)
				continue;

			if (colorIndex < cachedColors.Count)
				tiles[i].GetComponent<Renderer>().material.SetColor("_Color", cachedColors[colorIndex]);
			colorIndex++;
		}
		cachedColors.Clear();
	}
	#endregion

	#region Private
	void ClearTileIndexes ()
	{
		topTiles.Clear();
		fillerTiles.Clear();
		splitTops.Clear();
		selectableTiles.Clear();
	}

	void RebuildTileIndexes ()
	{
		ClearTileIndexes();
		for (int i = 0; i < transform.childCount; ++i)
		{
			Tile tile = transform.GetChild(i).GetComponent<Tile>();
			if (tile != null)
				RegisterTile(tile);
		}
	}

	void RegisterTile (Tile tile)
	{
		if (tile == null)
			return;

		if (tile.topTile)
			AddTopTile(tile);
		else if (tile.splitTop)
			AddSplitTopTile(tile);
		else
			fillerTiles[tile.transform.localPosition] = tile;
	}

	void AddTopTile (Tile tile)
	{
		if (!topTiles.ContainsKey(tile.pos) || topTiles[tile.pos].height < tile.height)
			topTiles[tile.pos] = tile;
		AddSelectableTile(tile);
	}

	void AddSplitTopTile (Tile tile)
	{
		if (!splitTops.ContainsKey(tile.pos) || splitTops[tile.pos].height < tile.height)
			splitTops[tile.pos] = tile;
		AddSelectableTile(tile);
	}

	void AddSelectableTile (Tile tile)
	{
		if (!selectableTiles.ContainsKey(tile.pos))
			selectableTiles.Add(tile.pos, new List<Tile>());

		List<Tile> stack = selectableTiles[tile.pos];
		if (!stack.Contains(tile))
			stack.Add(tile);

		stack.Sort((a, b) => a.height.CompareTo(b.height));
	}

	void TryAddSearchTile (Tile from, Tile next, Func<Tile, Tile, bool> addTile, Queue<Tile> checkNext, List<Tile> retValue)
	{
		if (next == null || next == from)
			return;

		int originalFromDistance = from.distance;
		if (!addTile(from, next))
		{
			from.distance = originalFromDistance;
			return;
		}

		int proposedDistance = from.distance + 1;
		bool changesLayer = IsLayerChange(from, next);
		int proposedLayerChanges = from.layerChanges + (changesLayer ? 1 : 0);
		int proposedLastLayerChangeStep = changesLayer ? proposedDistance : from.lastLayerChangeStep;
		bool firstVisit = next.distance == int.MaxValue;
		bool shorterPath = proposedDistance < next.distance;
		bool betterEqualPath = proposedDistance == next.distance && ShouldReplaceEqualDistancePath(next, from, proposedLayerChanges, proposedLastLayerChangeStep, changesLayer);

		from.distance = originalFromDistance;

		if (!firstVisit && !shorterPath && !betterEqualPath)
			return;

		next.distance = proposedDistance;
		next.prev = from;
		next.layerChanges = proposedLayerChanges;
		next.lastLayerChangeStep = proposedLastLayerChangeStep;

		if (firstVisit || shorterPath || betterEqualPath)
		{
			checkNext.Enqueue(next);
			if (!retValue.Contains(next))
				retValue.Add(next);
		}
	}

	bool ShouldReplaceEqualDistancePath (Tile next, Tile proposedPrev, int proposedLayerChanges, int proposedLastLayerChangeStep, bool proposedFinalStepChangesLayer)
	{
		if (next == null || proposedPrev == null)
			return false;

		if (next.prev == null)
			return true;

		if (proposedLayerChanges < next.layerChanges)
			return true;
		if (proposedLayerChanges > next.layerChanges)
			return false;

		// If two routes are the same length and have the same number of
		// layer changes, prefer the one that changes layer earlier and then
		// walks along the selected layer. This fixes stacked rows:
		// lower -> upper -> upper beats lower -> lower -> upper.
		if (proposedLastLayerChangeStep < next.lastLayerChangeStep)
			return true;
		if (proposedLastLayerChangeStep > next.lastLayerChangeStep)
			return false;

		bool currentFinalStepChangesLayer = IsLayerChange(next.prev, next);
		if (!proposedFinalStepChangesLayer && currentFinalStepChangesLayer)
			return true;

		return false;
	}

	bool IsLayerChange (Tile from, Tile to)
	{
		return from != null && to != null && !Mathf.Approximately(from.height, to.height);
	}


	bool IsCoveredSplitTop (Tile tile)
	{
		if (tile == null || !tile.splitTop)
			return false;

		List<Tile> stack = GetSelectableTiles(tile.pos);
		for (int i = 0; i < stack.Count; ++i)
		{
			Tile other = stack[i];
			if (other == null || other == tile)
				continue;

			if (other.height > tile.height && !Mathf.Approximately(other.height, tile.height))
				return true;
		}
		return false;
	}

	void ClearSearch ()
	{
		foreach (List<Tile> stack in selectableTiles.Values)
		{
			for (int i = 0; i < stack.Count; ++i)
			{
				stack[i].prev = null;
				stack[i].distance = int.MaxValue;
				stack[i].layerChanges = int.MaxValue;
				stack[i].lastLayerChangeStep = int.MaxValue;
			}
		}
	}

	void SwapReference (ref Queue<Tile> a, ref Queue<Tile> b)
	{
		Queue<Tile> temp = a;
		a = b;
		b = temp;
	}

	Tile Create (Tile.TileType tileType)
	{
		switch(tileType)
		{
			case Tile.TileType.dirt:
				GameObject instance = Instantiate(dirtPrefab) as GameObject;
				instance.transform.parent = transform;
				return instance.GetComponent<Tile>();
			case Tile.TileType.grass:
				instance = Instantiate(grassPrefab) as GameObject;
				instance.transform.parent = transform;
				return instance.GetComponent<Tile>();
			case Tile.TileType.stone:
				instance = Instantiate(stonePrefab) as GameObject;
				instance.transform.parent = transform;
				return instance.GetComponent<Tile>();
			case Tile.TileType.wood:
				instance = Instantiate(woodPrefab) as GameObject;
				instance.transform.parent = transform;
				return instance.GetComponent<Tile>();
			case Tile.TileType.water:
				instance = Instantiate(waterPrefab) as GameObject;
				instance.transform.parent = transform;
				return instance.GetComponent<Tile>();
			case Tile.TileType.sky:
				instance = Instantiate(skyPrefab) as GameObject;
				instance.transform.parent = transform;
				return instance.GetComponent<Tile>();
			case Tile.TileType.ice:
				if (icePrefab == null)
				{
					Debug.LogWarning("Board icePrefab is not assigned. Falling back to waterPrefab for ice tile creation.");
					instance = Instantiate(waterPrefab) as GameObject;
				}
				else
				{
					instance = Instantiate(icePrefab) as GameObject;
				}
				instance.transform.parent = transform;
				Tile iceTile = instance.GetComponent<Tile>();
				if (iceTile != null)
					iceTile.tileType = Tile.TileType.ice;
				return iceTile;
		}
		return null;
	}

	#endregion
}
