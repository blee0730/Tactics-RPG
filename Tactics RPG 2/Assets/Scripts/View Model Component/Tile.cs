using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour 
{
	#region Fields / Properties
	public Point pos;
	public float height;
	public TileType tileType;
	public bool topTile;
	public bool splitTop;
	public Vector3 center { get { return new Vector3(pos.x, height, pos.y); }}
	public GameObject content;
	[HideInInspector] public Tile prev;
	[HideInInspector] public int distance;
	#endregion

	#region Public
	public void Grow ()
	{
		height += 0.25f;
		Match();
	}
	
	public void Shrink ()
	{
		height -= 0.25f;
		Match ();
	}

	public void Load (Point p, float h)
	{
		pos = p;
		height = h;
		Match();
	}
	
	public void Load (Vector3 v)
	{
		Load (new Point((int)v.x, (int)v.z), v.y);
	}

	public enum TileType
	{
		dirt,
		grass,
		stone,
		wood,
		water,
		sky
	}
	#endregion

	#region Private
	void Match ()
	{
		transform.localPosition = new Vector3( pos.x, height, pos.y );
	}
	#endregion
}
