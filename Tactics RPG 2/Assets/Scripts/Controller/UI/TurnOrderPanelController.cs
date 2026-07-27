using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Triangle Strategy-style turn order bar.
///
/// This controller displays the same static SPD-based queue from TurnOrderController,
/// but presents it as a horizontal row of portrait icons at the top of the screen.
/// The player sees only the queue number and portrait, not SPD/CTR values.
/// </summary>
public class TurnOrderPanelController : MonoBehaviour
{
	[SerializeField] TurnOrderController turnOrderController;
	[SerializeField] RectTransform contentRoot;
	[SerializeField] TurnOrderEntry entryPrefab;
	[SerializeField] GameObject canvasRoot;
	[SerializeField] Text titleLabel;

	[Header("Hover Preview")]
	[SerializeField] BattleController battleController;
	[SerializeField] bool highlightHoveredUnitTurn = true;
	[SerializeField] bool ignoreCurrentActorAsNextTurn = true;
	[SerializeField] bool refreshHoverEveryFrame = true;

	[Header("Queue")]
	[SerializeField] int previewCount = 12;
	[SerializeField] bool includeCurrentActor = true;
	[SerializeField] bool hideWhenEmpty = false;

	[Header("Auto Build")]
	[SerializeField] bool autoBuildMissingUI = true;
	[SerializeField] bool autoAnchorToTop = true;
	[SerializeField] Vector2 barSize = new Vector2(760f, 86f);
	[SerializeField] Vector2 barOffset = new Vector2(0f, -12f);
	[SerializeField] Vector2 entrySize = new Vector2(58f, 68f);
	[SerializeField] float entrySpacing = 6f;

	List<TurnOrderEntry> entries = new List<TurnOrderEntry>();
	Unit hoveredUnit;

	void Awake ()
	{
		if (canvasRoot == null)
			canvasRoot = gameObject;

		if (autoBuildMissingUI)
			BuildMissingUI();
	}

	void OnEnable ()
	{
		this.AddObserver(OnTurnOrderChanged, TurnOrderController.TurnOrderChangedNotification);
		this.AddObserver(OnTurnOrderChanged, TurnOrderController.RoundBeganNotification);
		this.AddObserver(OnTurnOrderChanged, TurnOrderController.RoundEndedNotification);
		this.AddObserver(OnTurnOrderChanged, TurnOrderController.TurnBeganNotification);
		this.AddObserver(OnTurnOrderChanged, TurnOrderController.TurnCompletedNotification);
		this.AddObserver(OnStatsChanged, Stats.DidChangeNotification(StatTypes.HP));
		this.AddObserver(OnStatsChanged, Stats.DidChangeNotification(StatTypes.MHP));
		this.AddObserver(OnStatsChanged, Stats.DidChangeNotification(StatTypes.SPD));
		this.AddObserver(OnStatsChanged, Status.AddedNotification);
		this.AddObserver(OnStatsChanged, Status.RemovedNotification);
	}

	void OnDisable ()
	{
		this.RemoveObserver(OnTurnOrderChanged, TurnOrderController.TurnOrderChangedNotification);
		this.RemoveObserver(OnTurnOrderChanged, TurnOrderController.RoundBeganNotification);
		this.RemoveObserver(OnTurnOrderChanged, TurnOrderController.RoundEndedNotification);
		this.RemoveObserver(OnTurnOrderChanged, TurnOrderController.TurnBeganNotification);
		this.RemoveObserver(OnTurnOrderChanged, TurnOrderController.TurnCompletedNotification);
		this.RemoveObserver(OnStatsChanged, Stats.DidChangeNotification(StatTypes.HP));
		this.RemoveObserver(OnStatsChanged, Stats.DidChangeNotification(StatTypes.MHP));
		this.RemoveObserver(OnStatsChanged, Stats.DidChangeNotification(StatTypes.SPD));
		this.RemoveObserver(OnStatsChanged, Status.AddedNotification);
		this.RemoveObserver(OnStatsChanged, Status.RemovedNotification);
	}

	void Start ()
	{
		FindBattleControllerIfNeeded();
		hoveredUnit = GetHoveredUnit();
		Refresh();
	}

	void Update ()
	{
		if (!highlightHoveredUnitTurn || !refreshHoverEveryFrame)
			return;

		Unit currentHoveredUnit = GetHoveredUnit();
		if (currentHoveredUnit != hoveredUnit)
		{
			hoveredUnit = currentHoveredUnit;
			Refresh();
		}
	}

	public void Refresh ()
	{
		FindControllerIfNeeded();

		List<Unit> queue = turnOrderController != null
			? turnOrderController.GetTurnQueuePreview(previewCount, includeCurrentActor)
			: new List<Unit>();

		if (canvasRoot != null && hideWhenEmpty)
			canvasRoot.SetActive(queue.Count > 0);

		EnsureEntryCount(queue.Count);

		Unit activeUnit = turnOrderController != null ? turnOrderController.CurrentActor : null;
		if (highlightHoveredUnitTurn)
			hoveredUnit = GetHoveredUnit();
		int hoveredNextTurnIndex = GetHoveredNextTurnIndex(queue, activeUnit);

		for (int i = 0; i < entries.Count; ++i)
		{
			if (i < queue.Count)
			{
				bool isActive = queue[i] != null && queue[i] == activeUnit && includeCurrentActor && i == 0;
				bool isHoveredNextTurn = i == hoveredNextTurnIndex;
				entries[i].Display(queue[i], i + 1, isActive, isHoveredNextTurn);
			}
			else
			{
				entries[i].Clear();
			}
		}
	}

	void OnTurnOrderChanged (object sender, object args)
	{
		TurnOrderController controller = sender as TurnOrderController;
		if (controller != null)
			turnOrderController = controller;
		else
			FindControllerIfNeeded();

		Refresh();
	}

	void OnStatsChanged (object sender, object args)
	{
		Refresh();
	}

	void FindControllerIfNeeded ()
	{
		if (turnOrderController != null)
			return;
		turnOrderController = FindObjectOfType<TurnOrderController>();
	}

	void FindBattleControllerIfNeeded ()
	{
		if (battleController != null)
			return;
		battleController = FindObjectOfType<BattleController>();
	}

	Unit GetHoveredUnit ()
	{
		FindBattleControllerIfNeeded();
		if (battleController == null || battleController.currentTile == null)
			return null;

		GameObject content = battleController.currentTile.content;
		return content != null ? content.GetComponent<Unit>() : null;
	}

	int GetHoveredNextTurnIndex (List<Unit> queue, Unit activeUnit)
	{
		if (!highlightHoveredUnitTurn || hoveredUnit == null || queue == null)
			return -1;

		for (int i = 0; i < queue.Count; ++i)
		{
			if (queue[i] != hoveredUnit)
				continue;

			// If the player is hovering the unit whose turn is already open,
			// the useful preview is that unit's next future turn, not the current turn.
			if (ignoreCurrentActorAsNextTurn && hoveredUnit == activeUnit && includeCurrentActor && i == 0)
				continue;

			return i;
		}

		return -1;
	}

	void EnsureEntryCount (int count)
	{
		if (entryPrefab == null || contentRoot == null)
			return;

		while (entries.Count < count)
		{
			TurnOrderEntry entry = Instantiate(entryPrefab);
			entry.transform.SetParent(contentRoot, false);
			entry.gameObject.SetActive(true);
			entries.Add(entry);
		}
	}

	void BuildMissingUI ()
	{
		RectTransform rect = transform as RectTransform;
		if (rect == null)
			return;

		if (autoAnchorToTop)
		{
			rect.anchorMin = new Vector2(0.5f, 1f);
			rect.anchorMax = new Vector2(0.5f, 1f);
			rect.pivot = new Vector2(0.5f, 1f);
			rect.anchoredPosition = barOffset;
			rect.sizeDelta = barSize;
		}

		Image rootImage = GetComponent<Image>();
		if (rootImage == null)
			rootImage = gameObject.AddComponent<Image>();
		rootImage.color = new Color(0f, 0f, 0f, 0.42f);

		if (titleLabel != null)
			titleLabel.gameObject.SetActive(false);

		if (contentRoot == null)
		{
			GameObject content = new GameObject("Content", typeof(RectTransform));
			content.transform.SetParent(transform, false);
			contentRoot = content.GetComponent<RectTransform>();
		}

		ConfigureContentRoot(contentRoot);

		if (entryPrefab == null)
			entryPrefab = CreateRuntimeEntryPrefab();
	}

	void ConfigureContentRoot (RectTransform target)
	{
		if (target == null)
			return;

		target.anchorMin = new Vector2(0.5f, 0.5f);
		target.anchorMax = new Vector2(0.5f, 0.5f);
		target.pivot = new Vector2(0.5f, 0.5f);
		target.anchoredPosition = Vector2.zero;
		target.sizeDelta = new Vector2(barSize.x - 20f, entrySize.y);

		VerticalLayoutGroup oldVertical = target.GetComponent<VerticalLayoutGroup>();
		if (oldVertical != null)
			oldVertical.enabled = false;

		HorizontalLayoutGroup layout = target.GetComponent<HorizontalLayoutGroup>();
		if (layout == null)
			layout = target.gameObject.AddComponent<HorizontalLayoutGroup>();
		layout.childAlignment = TextAnchor.MiddleCenter;
		layout.childControlHeight = true;
		layout.childControlWidth = true;
		layout.childForceExpandHeight = false;
		layout.childForceExpandWidth = false;
		layout.spacing = entrySpacing;
		layout.padding = new RectOffset(8, 8, 0, 0);

		ContentSizeFitter fitter = target.GetComponent<ContentSizeFitter>();
		if (fitter == null)
			fitter = target.gameObject.AddComponent<ContentSizeFitter>();
		fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
		fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
	}

	TurnOrderEntry CreateRuntimeEntryPrefab ()
	{
		GameObject root = new GameObject("Turn Portrait Entry Runtime Prefab", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(LayoutElement), typeof(Outline));
		root.transform.SetParent(transform, false);
		root.SetActive(false);

		RectTransform rect = root.GetComponent<RectTransform>();
		rect.sizeDelta = entrySize;

		LayoutElement layoutElement = root.GetComponent<LayoutElement>();
		layoutElement.preferredWidth = entrySize.x;
		layoutElement.preferredHeight = entrySize.y;
		layoutElement.minWidth = entrySize.x;
		layoutElement.minHeight = entrySize.y;

		Image background = root.GetComponent<Image>();
		background.color = new Color32(80, 80, 80, 230);

		CanvasGroup group = root.GetComponent<CanvasGroup>();
		Outline hoverOutline = root.GetComponent<Outline>();
		hoverOutline.effectColor = new Color32(255, 244, 150, 255);
		hoverOutline.effectDistance = new Vector2(4f, -4f);
		hoverOutline.enabled = false;

		GameObject portraitObj = new GameObject("Portrait", typeof(RectTransform), typeof(Image), typeof(Mask));
		portraitObj.transform.SetParent(root.transform, false);
		RectTransform portraitRect = portraitObj.GetComponent<RectTransform>();
		portraitRect.anchorMin = new Vector2(0f, 0f);
		portraitRect.anchorMax = new Vector2(1f, 1f);
		portraitRect.offsetMin = new Vector2(5f, 6f);
		portraitRect.offsetMax = new Vector2(-5f, -8f);
		Image portrait = portraitObj.GetComponent<Image>();
		portrait.color = Color.white;
		portrait.preserveAspect = true;
		Mask mask = portraitObj.GetComponent<Mask>();
		mask.showMaskGraphic = true;

		Text initials = CreateOverlayText("Initials", portraitObj.transform, "?", 24, TextAnchor.MiddleCenter, Color.white);
		RectTransform initialsRect = initials.GetComponent<RectTransform>();
		initialsRect.anchorMin = Vector2.zero;
		initialsRect.anchorMax = Vector2.one;
		initialsRect.offsetMin = Vector2.zero;
		initialsRect.offsetMax = Vector2.zero;

		GameObject badge = new GameObject("Order Badge", typeof(RectTransform), typeof(Image));
		badge.transform.SetParent(root.transform, false);
		RectTransform badgeRect = badge.GetComponent<RectTransform>();
		badgeRect.anchorMin = new Vector2(0f, 1f);
		badgeRect.anchorMax = new Vector2(0f, 1f);
		badgeRect.pivot = new Vector2(0f, 1f);
		badgeRect.anchoredPosition = new Vector2(-4f, 4f);
		badgeRect.sizeDelta = new Vector2(24f, 24f);
		Image badgeImage = badge.GetComponent<Image>();
		badgeImage.color = new Color32(245, 218, 137, 255);

		Text order = CreateOverlayText("Order", badge.transform, "1", 16, TextAnchor.MiddleCenter, new Color32(30, 25, 15, 255));
		RectTransform orderRect = order.GetComponent<RectTransform>();
		orderRect.anchorMin = Vector2.zero;
		orderRect.anchorMax = Vector2.one;
		orderRect.offsetMin = Vector2.zero;
		orderRect.offsetMax = Vector2.zero;

		Text status = CreateOverlayText("Status", root.transform, "", 10, TextAnchor.MiddleCenter, Color.white);
		RectTransform statusRect = status.GetComponent<RectTransform>();
		statusRect.anchorMin = new Vector2(0f, 0f);
		statusRect.anchorMax = new Vector2(1f, 0f);
		statusRect.pivot = new Vector2(0.5f, 0f);
		statusRect.anchoredPosition = Vector2.zero;
		statusRect.sizeDelta = new Vector2(0f, 14f);

		Text nextTurn = CreateOverlayText("Hover Next Turn", root.transform, "NEXT", 9, TextAnchor.MiddleCenter, new Color32(30, 25, 15, 255));
		RectTransform nextRect = nextTurn.GetComponent<RectTransform>();
		nextRect.anchorMin = new Vector2(0.5f, 1f);
		nextRect.anchorMax = new Vector2(0.5f, 1f);
		nextRect.pivot = new Vector2(0.5f, 1f);
		nextRect.anchoredPosition = new Vector2(14f, 5f);
		nextRect.sizeDelta = new Vector2(40f, 13f);
		nextTurn.gameObject.SetActive(false);

		TurnOrderEntry entry = root.AddComponent<TurnOrderEntry>();
		entry.AssignRuntimeReferences(background, portrait, order, initials, status, group, hoverOutline, nextTurn, entrySize);
		return entry;
	}

	Text CreateOverlayText (string objectName, Transform parent, string text, int fontSize, TextAnchor anchor, Color color)
	{
		GameObject obj = new GameObject(objectName, typeof(RectTransform), typeof(Text));
		obj.transform.SetParent(parent, false);
		Text label = obj.GetComponent<Text>();
		label.text = text;
		label.font = GetDefaultFont();
		label.fontSize = fontSize;
		label.alignment = anchor;
		label.color = color;
		label.horizontalOverflow = HorizontalWrapMode.Overflow;
		label.verticalOverflow = VerticalWrapMode.Truncate;
		return label;
	}

	Font GetDefaultFont ()
	{
		return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
	}
}
