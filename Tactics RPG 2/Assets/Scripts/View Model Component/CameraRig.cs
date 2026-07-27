using UnityEngine;
using System.Collections;

public class CameraRig : MonoBehaviour 
{
	public float speed = 3f;
	public Transform follow;
	Transform _transform;
	public Transform pitch;

	[Header("Runtime Locks")]
	[Tooltip("When true, the camera rig ignores rotation, zoom, and follow movement. Used by blocking UI such as the full unit status panel.")]
	public bool movementLocked;
	
	void Awake ()
	{
		_transform = transform;
	}
	
	void Update ()
	{
		if (follow)
			_transform.position = Vector3.Lerp(_transform.position, follow.position, speed * Time.deltaTime);
	}
	public void SetMovementLocked (bool locked)
	{
		movementLocked = locked;
	}

}