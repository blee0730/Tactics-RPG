using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(BoardCreator))]
public class BoardCreatorInspector : Editor 
{
	static Tile.TileType brush = Tile.TileType.grass;

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

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Fast Map Builder", EditorStyles.boldLabel);
		brush = (Tile.TileType)EditorGUILayout.EnumPopup("Brush", brush);

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("↑", GUILayout.Width(40)))
			Move(new Point(0, 1));
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("←", GUILayout.Width(40)))
			Move(new Point(-1, 0));
		if (GUILayout.Button("Paint", GUILayout.Width(60)))
			Paint();
		if (GUILayout.Button("→", GUILayout.Width(40)))
			Move(new Point(1, 0));
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("↓", GUILayout.Width(40)))
			Move(new Point(0, -1));
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Raise"))
			DoChange(current.Raise);
		if (GUILayout.Button("Shrink"))
			DoChange(current.Shrink);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.HelpBox(
			"Scene View hotkeys while this object is selected:\n" +
			"WASD/Arrow Keys = move cursor\n" +
			"Space/Enter = paint/grow selected brush\n" +
			"Q/E = cycle brush, 1-6 = Dirt/Grass/Stone/Wood/Water/Sky\n" +
			"R/PageUp/= = raise, F/PageDown/- = shrink\n" +
			"Ctrl+S = save, Ctrl+L = load",
			MessageType.Info);
		
		EditorGUILayout.Space();
		if (GUILayout.Button("Clear"))
			DoChange(current.Clear);
		if (GUILayout.Button("Grow Dirt"))
			DoChange(current.GrowDirt);
		if (GUILayout.Button("Grow Grass"))
			DoChange(current.GrowGrass);
		if (GUILayout.Button("Grow Stone"))
			DoChange(current.GrowStone);
		if (GUILayout.Button("Grow Wood"))
			DoChange(current.GrowWood);
		if (GUILayout.Button("Grow Water"))
			DoChange(current.GrowWater);
		if (GUILayout.Button("Grow Sky"))
			DoChange(current.GrowSky);
		if (GUILayout.Button("Raise"))
			DoChange(current.Raise);
		if (GUILayout.Button("Shrink"))
			DoChange(current.Shrink);
		if (GUILayout.Button("Save"))
			DoChange(current.Save);
		if (GUILayout.Button("Load"))
			DoChange(current.Load);
		
		if (GUI.changed)
			current.UpdateMarker ();
	}

	void OnSceneGUI ()
	{
		Event e = Event.current;
		if (e.type != EventType.KeyDown)
			return;

		if (e.control || e.command)
		{
			if (e.keyCode == KeyCode.S)
			{
				DoChange(current.Save);
				e.Use();
			}
			else if (e.keyCode == KeyCode.L)
			{
				DoChange(current.Load);
				e.Use();
			}
			return;
		}

		switch (e.keyCode)
		{
		case KeyCode.UpArrow:
		case KeyCode.W:
			Move(new Point(0, 1));
			e.Use();
			break;
		case KeyCode.DownArrow:
		case KeyCode.S:
			Move(new Point(0, -1));
			e.Use();
			break;
		case KeyCode.LeftArrow:
		case KeyCode.A:
			Move(new Point(-1, 0));
			e.Use();
			break;
		case KeyCode.RightArrow:
		case KeyCode.D:
			Move(new Point(1, 0));
			e.Use();
			break;
		case KeyCode.Space:
		case KeyCode.Return:
			Paint();
			e.Use();
			break;
		case KeyCode.R:
		case KeyCode.PageUp:
		case KeyCode.Equals:
			DoChange(current.Raise);
			e.Use();
			break;
		case KeyCode.F:
		case KeyCode.PageDown:
		case KeyCode.Minus:
			DoChange(current.Shrink);
			e.Use();
			break;
		case KeyCode.Q:
			CycleBrush(-1);
			e.Use();
			break;
		case KeyCode.E:
			CycleBrush(1);
			e.Use();
			break;
		case KeyCode.Alpha1:
			brush = Tile.TileType.dirt;
			e.Use();
			break;
		case KeyCode.Alpha2:
			brush = Tile.TileType.grass;
			e.Use();
			break;
		case KeyCode.Alpha3:
			brush = Tile.TileType.stone;
			e.Use();
			break;
		case KeyCode.Alpha4:
			brush = Tile.TileType.wood;
			e.Use();
			break;
		case KeyCode.Alpha5:
			brush = Tile.TileType.water;
			e.Use();
			break;
		case KeyCode.Alpha6:
			brush = Tile.TileType.sky;
			e.Use();
			break;
		}

		SceneView.RepaintAll();
	}

	void Move (Point direction)
	{
		DoChange(delegate { current.Move(direction); });
	}

	void Paint ()
	{
		DoChange(delegate { current.Grow(brush); });
	}

	void CycleBrush (int direction)
	{
		int count = System.Enum.GetValues(typeof(Tile.TileType)).Length;
		int next = ((int)brush + direction) % count;
		if (next < 0)
			next += count;
		brush = (Tile.TileType)next;
		Repaint();
	}

	void DoChange (System.Action action)
	{
		Undo.RegisterFullObjectHierarchyUndo(current.gameObject, "Board Creator Change");
		action();
		EditorUtility.SetDirty(current);
	}
}
