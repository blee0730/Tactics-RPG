using UnityEngine;
using System;

/// <summary>
/// Play-mode keyboard/gamepad controls for the preproduction BoardCreator.
/// It can listen to the existing gameplay InputController events, and it can also
/// poll the same Unity Input Manager axes directly when the Board Creator scene
/// does not have an InputController object yet.
/// </summary>
public class BoardCreatorInputController : MonoBehaviour
{
	[SerializeField] BoardCreator boardCreator;
	[SerializeField] Tile.TileType brush = Tile.TileType.grass;

	[Header("Existing Gameplay Input")]
	[SerializeField] bool listenToGameplayInputController = true;
	[SerializeField] bool pollAxesWhenNoGameplayInputController = true;
	[SerializeField] CameraRig cameraRig;

	[Header("Direct Axis Names")]
	[SerializeField] string horizontalAxis = "Horizontal";
	[SerializeField] string verticalAxis = "Vertical";
	[SerializeField] string paintButton = "Fire1";
	[SerializeField] string shrinkButton = "Fire2";
	[SerializeField] string raiseButton = "Fire3";

	MapBuilderRepeater horizontal;
	MapBuilderRepeater vertical;
	InputController gameplayInputController;

	void Awake ()
	{
		if (boardCreator == null)
			boardCreator = GetComponent<BoardCreator>();
		if (boardCreator == null)
			boardCreator = GetComponentInParent<BoardCreator>();
		if (boardCreator == null)
			boardCreator = FindObjectOfType<BoardCreator>();

		horizontal = new MapBuilderRepeater(horizontalAxis);
		vertical = new MapBuilderRepeater(verticalAxis);
	}

	void OnEnable ()
	{
		gameplayInputController = FindObjectOfType<InputController>();
		if (listenToGameplayInputController)
		{
			InputController.moveEvent += OnGameplayMove;
			InputController.fireEvent += OnGameplayFire;
		}
	}

	void OnDisable ()
	{
		InputController.moveEvent -= OnGameplayMove;
		InputController.fireEvent -= OnGameplayFire;
	}

	void Update ()
	{
		if (boardCreator == null)
			return;

		HandleEditorStyleHotkeys();

		if (ShouldPollAxesDirectly())
			PollDirectAxes();
	}

	bool ShouldPollAxesDirectly ()
	{
		if (!pollAxesWhenNoGameplayInputController)
			return false;
		if (!listenToGameplayInputController)
			return true;
		return gameplayInputController == null;
	}

	void PollDirectAxes ()
	{
		int x = horizontal.Update();
		int y = vertical.Update();
		if (x != 0 || y != 0)
			MoveCursor(CameraRelativeDirection(new Point(x, y)));

		if (Input.GetButtonUp(paintButton))
			Paint();
		if (Input.GetButtonUp(shrinkButton))
			Shrink();
		if (Input.GetButtonUp(raiseButton))
			Raise();
	}

	void HandleEditorStyleHotkeys ()
	{
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetKeyDown(KeyCode.S))
				boardCreator.Save();
			if (Input.GetKeyDown(KeyCode.L))
				boardCreator.Load();
			return;
		}

		if (Input.GetKeyDown(KeyCode.Alpha1)) SetBrush(Tile.TileType.dirt);
		if (Input.GetKeyDown(KeyCode.Alpha2)) SetBrush(Tile.TileType.grass);
		if (Input.GetKeyDown(KeyCode.Alpha3)) SetBrush(Tile.TileType.stone);
		if (Input.GetKeyDown(KeyCode.Alpha4)) SetBrush(Tile.TileType.wood);
		if (Input.GetKeyDown(KeyCode.Alpha5)) SetBrush(Tile.TileType.water);
		if (Input.GetKeyDown(KeyCode.Alpha6)) SetBrush(Tile.TileType.sky);

		if (Input.GetKeyDown(KeyCode.Q)) CycleBrush(-1);
		if (Input.GetKeyDown(KeyCode.E)) CycleBrush(1);

		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
			Paint();
		if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.Equals))
			Raise();
		if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.Minus))
			Shrink();
	}

	void OnGameplayMove (object sender, InfoEventArgs<Point> e)
	{
		if (boardCreator == null || !listenToGameplayInputController)
			return;
		MoveCursor(e.info);
	}

	void OnGameplayFire (object sender, InfoEventArgs<int> e)
	{
		if (boardCreator == null || !listenToGameplayInputController)
			return;

		switch (e.info)
		{
		case 0:
			Paint();
			break;
		case 1:
			Shrink();
			break;
		case 2:
			Raise();
			break;
		}
	}

	Point CameraRelativeDirection (Point raw)
	{
		if (cameraRig == null)
			return raw;

		float y = cameraRig.transform.rotation.eulerAngles.y;
		if (y >= 270 && y <= 360)
			return raw;
		if (y >= 0 && y < 90)
			return new Point(raw.y, -raw.x);
		if (y >= 90 && y < 180)
			return new Point(-raw.x, -raw.y);
		return new Point(-raw.y, raw.x);
	}

	public void MoveCursor (Point direction)
	{
		boardCreator.Move(direction);
	}

	public void Paint ()
	{
		boardCreator.Grow(brush);
	}

	public void Raise ()
	{
		boardCreator.Raise();
	}

	public void Shrink ()
	{
		boardCreator.Shrink();
	}

	public void SetBrush (Tile.TileType nextBrush)
	{
		brush = nextBrush;
	}

	public void CycleBrush (int direction)
	{
		int count = Enum.GetValues(typeof(Tile.TileType)).Length;
		int next = ((int)brush + direction) % count;
		if (next < 0)
			next += count;
		brush = (Tile.TileType)next;
	}
}

class MapBuilderRepeater
{
	const float threshold = 0.5f;
	const float rate = 0.08f;
	float next;
	bool hold;
	string axis;

	public MapBuilderRepeater (string axisName)
	{
		axis = axisName;
	}

	public int Update ()
	{
		int retValue = 0;
		int value = Mathf.RoundToInt(Input.GetAxisRaw(axis));

		if (value != 0)
		{
			if (Time.time > next)
			{
				retValue = value;
				next = Time.time + (hold ? rate : threshold);
				hold = true;
			}
		}
		else
		{
			hold = false;
			next = 0;
		}

		return retValue;
	}
}
