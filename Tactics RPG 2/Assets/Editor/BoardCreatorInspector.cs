using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(BoardCreator))]
public class BoardCreatorInspector : Editor 
{
	public BoardCreator current
	{
		get
		{
			return (BoardCreator)target;
		}
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();
		
		if (GUILayout.Button("Clear"))
			current.Clear();
		if (GUILayout.Button("Grow Dirt"))
			current.GrowDirt();
		if (GUILayout.Button("Grow Grass"))
			current.GrowGrass();
		if (GUILayout.Button("Grow Stone"))
			current.GrowStone();
		if (GUILayout.Button("Grow Wood"))
			current.GrowWood();
		if (GUILayout.Button("Grow Water"))
			current.GrowWater();
		if (GUILayout.Button("Grow Sky"))
			current.GrowSky();
		if (GUILayout.Button("Raise"))
			current.Raise();
		if (GUILayout.Button("Shrink"))
			current.Shrink();
		if (GUILayout.Button("Save"))
			current.Save();
		if (GUILayout.Button("Load"))
			current.Load();
		
		if (GUI.changed)
			current.UpdateMarker ();
	}
}
