using UnityEngine;
using UnityEditor;
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
	public void GrowDirt ()
	{
		GrowSingle(pos, Tile.TileType.dirt);
	}

	public void GrowGrass ()
	{
		GrowSingle(pos, Tile.TileType.grass);
	}

	public void GrowStone ()
	{
		GrowSingle(pos, Tile.TileType.stone);
	}

	public void GrowWood ()
	{
		GrowSingle(pos, Tile.TileType.wood);
	}

	public void GrowWater ()
	{
		GrowSingle(pos, Tile.TileType.water);
	}

	public void GrowSky ()
	{
		GrowSingle(pos, Tile.TileType.sky);
	}

	public void Raise()
	{
		if(topTiles.ContainsKey(pos))
		{
			Tile t = topTiles[pos];
			if(fillerTiles.ContainsKey(new Vector3(pos.x, t.height - 0.25f, pos.y)))
				fillerTiles[new Vector3(pos.x, t.height - 0.25f, pos.y)].splitTop = true;
			t.splitTop = true;
			t.Grow();
		}
	}
	
	public void Shrink ()
	{
		ShrinkSingle(pos);
	}

	public void UpdateMarker ()
	{
		Tile t = topTiles.ContainsKey(pos) ? topTiles[pos] : null;
		marker.localPosition = t != null ? t.center : new Vector3(pos.x, 0, pos.y);
	}

	public void Clear ()
	{
		for (int i = transform.childCount - 1; i >= 0; --i)
			DestroyImmediate(transform.GetChild(i).gameObject);
		topTiles.Clear();
		fillerTiles.Clear();
	}

	public void Save ()
	{
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

		string fileName = string.Format("Assets/Resources/Levels/{1}.asset", filePath, name);
		AssetDatabase.CreateAsset(board, fileName);
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
		
		Tile t = topTiles[p];
		topTiles.Remove(p);
		DestroyImmediate(t.gameObject);

		if(fillerTiles.ContainsKey(new Vector3(p.x, t.height - 0.25f, p.y)))
		{
			topTiles.Add(p, fillerTiles[new Vector3(p.x, t.height - 0.25f, p.y)]);
			fillerTiles[new Vector3(p.x, t.height - 0.25f, p.y)].GetComponent<Tile>().topTile = true;
			fillerTiles.Remove(new Vector3(p.x, t.height - 0.25f, p.y));
		}

		else
		{
			foreach(Tile tile in fillerTiles.Values)
			{
				if(tile.pos == p && tile.splitTop)
				{
					tile.topTile = true;
					tile.splitTop = false;
					t = tile;
				}
			}

			topTiles.Add(p, t);
			fillerTiles.Remove(new Vector3(t.pos.x, t.height, t.pos.y));
		}
	}

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
	#endregion
}
