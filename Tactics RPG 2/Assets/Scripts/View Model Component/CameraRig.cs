using UnityEngine;
using System.Collections;

public class CameraRig : MonoBehaviour 
{
	private const float XMin = 0.0f;
	private const float XMax = 90.0f;
	public float speed = 3f;
	public Transform follow;
	Transform _transform;
	public Transform pitch;
<<<<<<< Updated upstream

	[Header("Runtime Locks")]
	[Tooltip("When true, the camera rig ignores rotation, zoom, and follow movement. Used by blocking UI such as the full unit status panel.")]
	public bool movementLocked;
=======
>>>>>>> Stashed changes
	
	private float currentX = 35.264f;
	private float currentY = 0.0f;
	public float sensitivity = 1000.0f;
	void Awake ()
	{
		_transform = transform;
	}

	void Update()
	{
		currentX += Input.mouseScrollDelta.y * sensitivity * Time.deltaTime;
		currentY += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
		currentX = Mathf.Clamp(currentX, XMin, XMax);
		_transform.rotation = Quaternion.Euler(0, currentY, 0);
		pitch.localRotation = Quaternion.Euler(currentX, 0, 0);

		if (follow)
			_transform.position = Vector3.Lerp(_transform.position, follow.position, speed * Time.deltaTime);
	}
	public void SetMovementLocked (bool locked)
	{
		movementLocked = locked;
	}

}