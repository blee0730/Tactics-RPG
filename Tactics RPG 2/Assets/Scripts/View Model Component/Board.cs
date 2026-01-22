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
	public Dictionary<Point, Tile> topTiles = new Dictionary<Point, Tile>();
	public Dictionary<Vector3, Tile> fillerTiles = new Dictionary<Vector3, Tile>();
	public Dictionary<Point, Tile> splitTops = new Dictionary<Point, Tile>();
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
		_min = new Point(int.MaxValue, int.MaxValue);
		_max = new Point(int.MinValue, int.MinValue);
		
		for (int i = 0; i < data.tiles.Count; ++i)
		{
			Tile t = Create(data.tileTypes[i]);
			t.Load(data.tiles[i]);
			if(data.topTiles[i])
				topTiles.Add(t.pos, t);
			else if(data.splitTops[i])
				splitTops.Add(t.pos, t);
			else
				fillerTiles.Add(t.transform.localPosition, t);
			
			_min.x = Mathf.Min(_min.x, t.pos.x);
			_min.y = Mathf.Min(_min.y, t.pos.y);
			_max.x = Mathf.Max(_max.x, t.pos.x);
			_max.y = Mathf.Max(_max.y, t.pos.y);
		}
	}

	public Tile GetTile (Point p)
	{
		return topTiles.ContainsKey(p) ? topTiles[p] : null;
	}

	public List<Tile> Search (Tile start, Func<Tile, Tile, bool> addTile)
	{
		List<Tile> retValue = new List<Tile>();
		retValue.Add(start);

		ClearSearch();
		Queue<Tile> checkNext = new Queue<Tile>();
		Queue<Tile> checkNow = new Queue<Tile>();

		start.distance = 0;
		checkNow.Enqueue(start);

		while (checkNow.Count > 0)
		{
			Tile t = checkNow.Dequeue();
			for (int i = 0; i < 4; ++i)
			{
				Tile next = GetTile(t.pos + dirs[i]);
				if (next == null || next.distance <= t.distance + 1)
					continue;

				if (addTile(t, next))
				{
					next.distance = t.distance + 1;
					next.prev = t;
					checkNext.Enqueue(next);
					retValue.Add(next);
				}
			}

			if (checkNow.Count == 0)
				SwapReference(ref checkNow, ref checkNext);
		}

		return retValue;
	}

	public void SelectTiles (List<Tile> tiles)
	{
		for (int i = 0; i <= tiles.Count - 1; i++)
		{
			cachedColors.Add(tiles[i].GetComponent<Renderer>().material.GetColor("_Color"));
			tiles[i].GetComponent<Renderer>().material.SetColor("_Color", selectedTileColor);
		}
	}

	public void DeSelectTiles (List<Tile> tiles)
	{
		for (int i = 0; i <= tiles.Count - 1; i++)
			tiles[i].GetComponent<Renderer>().material.SetColor("_Color", cachedColors[i]);
		cachedColors.Clear();
	}
	#endregion

	#region Private
	void ClearSearch ()
	{
		foreach (Tile t in topTiles.Values)
		{
			t.prev = null;
			t.distance = int.MaxValue;
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
		}
		return null;
	}

	#endregion
}