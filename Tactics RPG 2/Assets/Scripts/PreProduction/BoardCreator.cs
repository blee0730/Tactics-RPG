using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class BoardCreator : MonoBehaviour 
{
	#region Fields / Properties
	[SerializeField] GameObject dirtPrefab;
	[SerializeField] GameObject grassPrefab;
	[SerializeField] GameObject stonePrefab;
	[SerializeField] GameObject woodPrefab;
	[SerializeField] GameObject waterPrefab;
	[SerializeField] GameObject skyPrefab;
	[SerializeField] GameObject tileSelectionIndicatorPrefab;
	[SerializeField] int height = 8;
	[SerializeField] Point pos;
	[SerializeField] LevelData levelData;
	Dictionary<Vector3, Tile> fillerTiles = new Dictionary<Vector3, Tile>();
	Dictionary<Point, Tile> topTiles = new Dictionary<Point, Tile>();

	public Point CurrentPosition { get { return pos; } }
	public int MaxHeight { get { return height; } set { height = Mathf.Max(0, value); } }

	Transform marker
	{
		get
		{
			if (_marker == null)
			{
				GameObject instance = Instantiate(tileSelectionIndicatorPrefab) as GameObject;
				_marker = instance.transform;
			}
			return _marker;
		}
	}
	Transform _marker;
	#endregion

	#region Public
	public void Move (Point direction)
	{
		SetPosition(pos + direction);
	}

	public void SetPosition (Point p)
	{
		pos = p;
		UpdateMarker();
	}

	public void Grow (Tile.TileType tileType)
	{
		GrowSingle(pos, tileType);
		UpdateMarker();
	}

	public void GrowDirt ()
	{
		Grow(Tile.TileType.dirt);
	}

	public void GrowGrass ()
	{
		Grow(Tile.TileType.grass);
	}

	public void GrowStone ()
	{
		Grow(Tile.TileType.stone);
	}

	public void GrowWood ()
	{
		Grow(Tile.TileType.wood);
	}

	public void GrowWater ()
	{
		Grow(Tile.TileType.water);
	}

	public void GrowSky ()
	{
		Grow(Tile.TileType.sky);
	}

	public void Raise()
	{
		if(topTiles.ContainsKey(pos))
		{
			Tile t = topTiles[pos];
			Vector3 fillerKey = new Vector3(pos.x, t.height - 0.25f, pos.y);
			if(fillerTiles.ContainsKey(fillerKey))
				fillerTiles[fillerKey].splitTop = true;
			t.splitTop = true;
			t.Grow();
		}
		UpdateMarker();
	}
	
	public void Shrink ()
	{
		ShrinkSingle(pos);
		UpdateMarker();
	}

	public bool HasTopTile (Point p)
	{
		return topTiles.ContainsKey(p);
	}

	public Tile GetTopTile (Point p)
	{
		return topTiles.ContainsKey(p) ? topTiles[p] : null;
	}

	public void UpdateMarker ()
	{
		Tile t = topTiles.ContainsKey(pos) ? topTiles[pos] : null;
		marker.localPosition = t != null ? t.center : new Vector3(pos.x, 0.25f, pos.y);
	}

	public void Clear ()
	{
		for (int i = transform.childCount - 1; i >= 0; --i)
			DestroyImmediate(transform.GetChild(i).gameObject);
		topTiles.Clear();
		fillerTiles.Clear();
		UpdateMarker();
	}

	public void Save ()
	{
#if UNITY_EDITOR
		string filePath = Application.dataPath + "/Resources/Levels";
		if (!Directory.Exists(filePath))
			CreateSaveDirectory ();
		
		LevelData board = ScriptableObject.CreateInstance<LevelData>();
		board.tiles = new List<Vector3>(topTiles.Count + fillerTiles.Count);
		board.tileTypes = new List<Tile.TileType>(topTiles.Count + fillerTiles.Count);
		board.topTiles = new List<bool>(topTiles.Count + fillerTiles.Count);
		board.splitTops = new List<bool>(topTiles.Count + fillerTiles.Count);
		foreach (Tile t in topTiles.Values)
		{
			board.tiles.Add(t.transform.localPosition);
			board.tileTypes.Add(t.tileType);
			board.topTiles.Add(t.topTile);
			board.splitTops.Add(t.splitTop);
		}
		foreach (Tile t in fillerTiles.Values)
		{
			board.tiles.Add(t.transform.localPosition);
			board.tileTypes.Add(t.tileType);
			board.topTiles.Add(t.topTile);
			board.splitTops.Add(t.splitTop);
		}

		string fileName = string.Format("Assets/Resources/Levels/{0}.asset", name);
		AssetDatabase.CreateAsset(board, AssetDatabase.GenerateUniqueAssetPath(fileName));
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
#else
		Debug.LogWarning("BoardCreator.Save can only create LevelData assets inside the Unity Editor.");
#endif
	}

	public void Load ()
	{
		Clear();
		if (levelData == null)
			return;
		
		for(int i = 0; i < levelData.tiles.Count; i++)
		{
			Tile t = Create(levelData.tileTypes[i]);
			t.Load(levelData.tiles[i]);
			if(levelData.topTiles[i])
			{
				topTiles.Add(t.pos, t);
				t.topTile = true;
			}
			else
				fillerTiles.Add(t.transform.localPosition, t);

			if(levelData.splitTops[i])
				t.splitTop = true;
		}
		UpdateMarker();
	}
	#endregion

	#region Private
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
	
	Tile GetOrCreate (Point p, Tile.TileType tileType)
	{
		if (topTiles.ContainsKey(p))
		{
			topTiles[p].GetComponent<Tile>().topTile = false;
			fillerTiles.Add(topTiles[p].transform.localPosition, topTiles[p]);
			Tile tile = Create(tileType);
			tile.topTile = true;
			tile.Load(p, topTiles[p].height);
			topTiles.Remove(p);
			topTiles.Add(p, tile);
			return tile;
		}
		
		Tile t = Create(tileType);
		t.topTile = true;
		t.Load(p, 0);
		topTiles.Add(p, t);
		
		return t;
	}
	
	void GrowSingle (Point p, Tile.TileType tileType)
	{
		Tile t = GetOrCreate(p, tileType);
		if (t.height < height)
			t.Grow();
	}

	void ShrinkSingle (Point p)
	{
		if (!topTiles.ContainsKey(p))
			return;
		
		Tile removedTop = topTiles[p];
		topTiles.Remove(p);
		DestroyImmediate(removedTop.gameObject);

		Vector3 directBelowKey = new Vector3(p.x, removedTop.height - 0.25f, p.y);
		if(fillerTiles.ContainsKey(directBelowKey))
		{
			Tile replacement = fillerTiles[directBelowKey];
			replacement.topTile = true;
			topTiles.Add(p, replacement);
			fillerTiles.Remove(directBelowKey);
			return;
		}

		Tile splitReplacement = null;
		Vector3 splitReplacementKey = Vector3.zero;
		foreach(KeyValuePair<Vector3, Tile> pair in fillerTiles)
		{
			Tile tile = pair.Value;
			if(tile.pos == p && tile.splitTop)
			{
				splitReplacement = tile;
				splitReplacementKey = pair.Key;
				break;
			}
		}

		if (splitReplacement != null)
		{
			splitReplacement.topTile = true;
			splitReplacement.splitTop = false;
			topTiles.Add(p, splitReplacement);
			fillerTiles.Remove(splitReplacementKey);
		}
	}

#if UNITY_EDITOR
	void CreateSaveDirectory ()
	{
		string filePath = Application.dataPath + "/Resources";
		if (!Directory.Exists(filePath))
			AssetDatabase.CreateFolder("Assets", "Resources");
		filePath += "/Levels";
		if (!Directory.Exists(filePath))
			AssetDatabase.CreateFolder("Assets/Resources", "Levels");
		AssetDatabase.Refresh();
	}
#endif
	#endregion
}
