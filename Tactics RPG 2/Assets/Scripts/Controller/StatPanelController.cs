using UnityEngine;
using System.Collections;

public class StatPanelController : MonoBehaviour 
{
	#region Const
	const string ShowKey = "Show";
	const string HideKey = "Hide";
	#endregion

	#region Fields
	[SerializeField] StatPanel primaryPanel;
	[SerializeField] StatPanel secondaryPanel;
	[SerializeField] UnitStatusDetailPanelController detailPanel;
	[SerializeField] CameraRig cameraRigToLockDuringDetail;
	
	Tweener primaryTransition;
	bool detailCameraLockActive;
	Tweener secondaryTransition;
	#endregion

	#region Properties
	public bool IsDetailVisible
	{
		get { return detailPanel != null && detailPanel.IsShowing; }
	}
	#endregion

	#region MonoBehaviour
	void Awake ()
	{
		EnsureDetailPanel();
		EnsureCameraRigReference();
	}

	void Start ()
	{
		if (primaryPanel.panel.CurrentPosition == null)
			primaryPanel.panel.SetPosition(HideKey, false);
		if (secondaryPanel.panel.CurrentPosition == null)
			secondaryPanel.panel.SetPosition(HideKey, false);
	}
	#endregion

	#region Public
	public void ShowPrimary (GameObject obj)
	{
		primaryPanel.Display(obj);
		MovePanel(primaryPanel, ShowKey, ref primaryTransition);
	}

	public void HidePrimary ()
	{
		MovePanel(primaryPanel, HideKey, ref primaryTransition);
	}

	public void ShowSecondary (GameObject obj)
	{
		secondaryPanel.Display(obj);
		MovePanel(secondaryPanel, ShowKey, ref secondaryTransition);
	}

	public void HideSecondary ()
	{
		MovePanel(secondaryPanel, HideKey, ref secondaryTransition);
	}

	public void ShowDetail (GameObject obj)
	{
		EnsureDetailPanel();
		EnsureCameraRigReference();
		if (detailPanel != null)
		{
			detailPanel.Show(obj);
			SetDetailCameraLock(true);
		}
	}


	public void CycleDetailPage ()
	{
		EnsureDetailPanel();
		if (detailPanel != null && detailPanel.IsShowing)
			detailPanel.CycleSidePage();
	}

	public void HideDetail ()
	{
		if (detailPanel != null)
			detailPanel.Hide();
		SetDetailCameraLock(false);
	}

	void OnDisable ()
	{
		SetDetailCameraLock(false);
	}
	#endregion

	#region Private
	void EnsureDetailPanel ()
	{
		if (detailPanel != null)
			return;

		detailPanel = GetComponentInChildren<UnitStatusDetailPanelController>(true);
		if (detailPanel != null)
			return;

		GameObject obj = new GameObject("Unit Status Detail Controller");
		obj.transform.SetParent(transform, false);
		detailPanel = obj.AddComponent<UnitStatusDetailPanelController>();
	}

	void EnsureCameraRigReference ()
	{
		if (cameraRigToLockDuringDetail != null)
			return;

		BattleController battle = GetComponentInParent<BattleController>();
		if (battle != null)
			cameraRigToLockDuringDetail = battle.cameraRig;

		if (cameraRigToLockDuringDetail == null)
			cameraRigToLockDuringDetail = FindObjectOfType<CameraRig>();
	}

	void SetDetailCameraLock (bool locked)
	{
		if (detailCameraLockActive == locked)
			return;

		detailCameraLockActive = locked;
		EnsureCameraRigReference();
		if (cameraRigToLockDuringDetail != null)
			cameraRigToLockDuringDetail.SetMovementLocked(locked);
	}

	void MovePanel (StatPanel obj, string pos, ref Tweener t)
	{
		Panel.Position target = obj.panel[pos];
		if (obj.panel.CurrentPosition != target)
		{
			if (t != null)
				t.Stop();
			t = obj.panel.SetPosition(pos, true);
			t.duration = 0.5f;
			t.equation = EasingEquations.EaseOutQuad;
		}
	}
	#endregion
}
