using UnityEngine;
using System.Collections;

public class CameraRig : MonoBehaviour 
{
	public float speed = 3f;
	public Transform follow;
	Transform _transform;
<<<<<<< Updated upstream
=======
	public Transform pitch;

	[Header("Runtime Locks")]
	[Tooltip("When true, the camera rig ignores rotation, zoom, and follow movement. Used by blocking UI such as the full unit status panel.")]
	public bool movementLocked;
>>>>>>> Stashed changes
	
	void Awake ()
	{
		_transform = transform;
	}
	
	void Update ()
	{
<<<<<<< Updated upstream
=======
		if (movementLocked)
			return;

		currentX += Input.mouseScrollDelta.y * sensitivity * Time.deltaTime;
		currentY += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
		currentX = Mathf.Clamp(currentX, XMin, XMax);
		_transform.rotation = Quaternion.Euler(0, currentY, 0);
		pitch.localRotation = Quaternion.Euler(currentX, 0, 0);

>>>>>>> Stashed changes
		if (follow)
			_transform.position = Vector3.Lerp(_transform.position, follow.position, speed * Time.deltaTime);
	}
	public void SetMovementLocked (bool locked)
	{
		movementLocked = locked;
	}

}