using UnityEngine;
using System;
using System.Collections;

class Repeater
{
	const float threshold = 0.5f;
	const float rate = 0.25f;
	float _next;
	bool _hold;
	string _axis;
	
	public Repeater (string axisName)
	{
		_axis = axisName;
	}
	
	public int Update ()
	{
		int retValue = 0;
		int value = Mathf.RoundToInt( Input.GetAxisRaw(_axis) );
		
		if (value != 0)
		{
			if (Time.time > _next)
			{
				retValue = value;
				_next = Time.time + (_hold ? rate : threshold);
				_hold = true;
			}
		}
		else
		{
			_hold = false;
			_next = 0;
		}
		
		return retValue;
	}
}

public class InputController : MonoBehaviour 
{
	public static event EventHandler<InfoEventArgs<Point>> moveEvent;
	public static event EventHandler<InfoEventArgs<int>> fireEvent;
	public static event EventHandler<InfoEventArgs<int>> layerCycleEvent;
	Repeater _hor = new Repeater("Horizontal");
	Repeater _ver = new Repeater("Vertical");
	string[] _buttons = new string[] {"Fire1", "Fire2", "Fire3"};
	public CameraRig cameraRig;

	// Unity's default old Input Manager maps Fire2 to Alt and Fire3 to Shift.
	// These are also the layer-cycle keys, so their release can accidentally
	// arrive as a cancel/back Fire event. Consume only the matching key release
	// after it was used for layer cycling.
	bool _consumeAltReleaseAsFire2;
	bool _consumeShiftReleaseAsFire3;

	void Update () 
	{
		int x = _hor.Update();
		int y = _ver.Update();
		if (x != 0 || y != 0)
		{
			RaiseMoveEvent(new Point(x, y));
		}

		bool shiftDown = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
		bool altDown = Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);
		bool shiftUp = Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift);
		bool altUp = Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt);

		if (layerCycleEvent != null)
		{
			// Shift cycles to the next/higher selectable splitTop/topTile layer when possible.
			// Alt cycles to the previous/lower layer. On a two-layer stack, either key works like a toggle.
			if (shiftDown)
			{
				_consumeShiftReleaseAsFire3 = true;
				layerCycleEvent(this, new InfoEventArgs<int>(1));
			}
			else if (altDown)
			{
				_consumeAltReleaseAsFire2 = true;
				layerCycleEvent(this, new InfoEventArgs<int>(-1));
			}
		}

		for (int i = 0; i < 3; ++i)
		{
			if (Input.GetButtonUp(_buttons[i]))
			{
				if (ShouldConsumeFireButtonUp(i, altUp, shiftUp))
					continue;

				if (fireEvent != null)
					fireEvent(this, new InfoEventArgs<int>(i));
			}
		}

		if (altUp)
			_consumeAltReleaseAsFire2 = false;
		if (shiftUp)
			_consumeShiftReleaseAsFire3 = false;
	}

	bool ShouldConsumeFireButtonUp (int buttonIndex, bool altUp, bool shiftUp)
	{
		// Fire2 is button index 1 in this controller. In Unity's default input
		// manager that is also Alt, which we use for lower-layer cycling.
		if (buttonIndex == 1 && _consumeAltReleaseAsFire2 && altUp)
			return true;

		// Fire3 is button index 2 in this controller. In Unity's default input
		// manager that is also Shift, which we use for upper-layer cycling.
		if (buttonIndex == 2 && _consumeShiftReleaseAsFire3 && shiftUp)
			return true;

		return false;
	}

	void RaiseMoveEvent (Point raw)
	{
		if (moveEvent == null)
			return;

		moveEvent(this, new InfoEventArgs<Point>(CameraRelativeDirection(raw)));
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
}
