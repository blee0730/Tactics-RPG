using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AbilityMenuPanelController : MonoBehaviour 
{
	#region Constants
	const string ShowKey = "Show";
	const string HideKey = "Hide";
	const string EntryPoolKey = "AbilityMenuPanel.Entry";
	const int MenuCount = 7;
	#endregion

	#region Fields / Properties
	[SerializeField] GameObject entryPrefab;
	[SerializeField] Text titleLabel;
	[SerializeField] Panel panel;
	[SerializeField] GameObject canvas;

	[Header("Scrolling")]
	[SerializeField] bool autoBuildScrollView = true;
	[SerializeField] RectTransform contentRoot;
	[SerializeField] ScrollRect scrollRect;
	[SerializeField] Scrollbar verticalScrollbar;
	[SerializeField] int maxVisibleOptions = 7;
	[SerializeField] float entryHeight = 44f;
	[SerializeField] float entrySpacing = 6f;
	[SerializeField] float scrollbarWidth = 12f;

	[Header("Dynamic Readable Text Layout")]
	[SerializeField] bool useDynamicPanelSize = true;
	[SerializeField] float minDynamicPanelWidth = 285f;
	[SerializeField] float maxDynamicPanelWidth = 420f;
	[SerializeField] float dynamicPanelPadding = 78f;
	[SerializeField] float averageCharacterWidth = 7.4f;
	[SerializeField] float compactEntryHeight = 34f;
	[SerializeField] float wrappedEntryHeight = 52f;
	[SerializeField] bool wrapLongOptionText = true;
	[SerializeField] int compactOptionFontSize = 16;
	[SerializeField] int wrappedOptionFontSize = 15;
	[SerializeField] int maxWrappedLines = 2;

	// Only these entries are instantiated. The controller virtualizes the long list
	// by rewriting these visible rows as the selection scrolls. This avoids the
	// large one-frame hitch caused by creating/rebuilding dozens or hundreds of UI rows.
	List<AbilityMenuEntry> menuEntries = new List<AbilityMenuEntry>(MenuCount);
	List<string> optionTitles = new List<string>();
	List<bool> optionLocks = new List<bool>();
	LayoutElement scrollViewLayoutElement;
	float originalPanelWidth = -1f;
	float originalPanelHeight = -1f;
	int currentOptionFontSize = 16;
	int currentWrappedLineCount = 1;
	int scrollTopIndex;
	bool suppressScrollbarCallback;
	public int selection { get; private set; }
	#endregion

	#region MonoBehaviour
	void Awake ()
	{
		selection = -1;
		CacheOriginalPanelSize();
		BuildScrollViewIfNeeded();
		ApplyMenuReadableLayout();
		WireScrollbarCallback();

		int prewarmCount = Mathf.Max(1, maxVisibleOptions);
		GameObjectPoolController.AddEntry(EntryPoolKey, entryPrefab, prewarmCount, prewarmCount + 2);
		GameObjectPoolController.SetMaxCount(EntryPoolKey, prewarmCount + 2);
	}

	void Start ()
	{
		panel.SetPosition(HideKey, false);
		canvas.SetActive(false);
	}
	#endregion

	#region Public
	public void Show (string title, List<string> options)
	{
		BuildScrollViewIfNeeded();
		WireScrollbarCallback();
		maxVisibleOptions = Mathf.Max(1, maxVisibleOptions);

		canvas.SetActive(true);
		ClearRuntimeData();
		titleLabel.text = title;

		if (options != null)
		{
			for (int i = 0; i < options.Count; ++i)
			{
				optionTitles.Add(options[i]);
				optionLocks.Add(false);
			}
		}

		ApplyDynamicReadableLayout(title);
		EnsureVisibleEntryPool();
		UpdateScrollViewSize();
		SetScrollTopIndex(0, false);

		if (optionTitles.Count > 0)
			SetSelection(FindNextUnlockedIndex(0, 1));

		TogglePos(ShowKey);
	}

	public void Hide ()
	{
		Tweener t = TogglePos(HideKey);
		t.completedEvent += delegate(object sender, System.EventArgs e)
		{
			if (panel.CurrentPosition == panel[HideKey])
			{
				Clear();
				canvas.SetActive(false);
			}
		};
	}

	public void SetLocked (int index, bool value)
	{
		if (index < 0 || index >= optionLocks.Count)
			return;

		optionLocks[index] = value;

		if (value && selection == index)
		{
			int next = FindNextUnlockedIndex(index + 1, 1);
			if (next < 0)
				next = FindNextUnlockedIndex(index - 1, -1);

			// If every option is locked, keep the selection on the requested index so
			// the menu still has a stable highlighted row, but CanPerform checks will
			// prevent confirmation.
			if (next >= 0)
				SetSelection(next);
			else
			{
				selection = index;
				EnsureSelectionVisible();
				UpdateVisibleEntries();
			}
		}
		else
		{
			UpdateVisibleEntries();
		}
	}

	public void Next ()
	{
		if (optionTitles.Count == 0)
			return;

		int start = selection >= 0 ? selection + 1 : 0;
		int next = FindNextUnlockedIndex(start, 1);
		if (next >= 0)
			SetSelection(next);
	}

	public void Previous ()
	{
		if (optionTitles.Count == 0)
			return;

		int start = selection >= 0 ? selection - 1 : optionTitles.Count - 1;
		int previous = FindNextUnlockedIndex(start, -1);
		if (previous >= 0)
			SetSelection(previous);
	}
	#endregion

	#region Private
	AbilityMenuEntry Dequeue ()
	{
		Poolable p = GameObjectPoolController.Dequeue(EntryPoolKey);
		AbilityMenuEntry entry = p.GetComponent<AbilityMenuEntry>();
		Transform parent = contentRoot != null ? contentRoot : panel.transform;
		entry.transform.SetParent(parent, false);
		entry.transform.localScale = Vector3.one;
		entry.gameObject.SetActive(true);

		ApplyEntryHeight(entry);

		ConfigureEntryText(entry);
		entry.Reset();
		return entry;
	}

	void Enqueue (AbilityMenuEntry entry)
	{
		if (entry == null)
			return;

		Poolable p = entry.GetComponent<Poolable>();
		GameObjectPoolController.Enqueue(p);
	}

	void Clear ()
	{
		for (int i = menuEntries.Count - 1; i >= 0; --i)
			Enqueue(menuEntries[i]);
		menuEntries.Clear();
		ClearRuntimeData();
	}

	void ClearRuntimeData ()
	{
		optionTitles.Clear();
		optionLocks.Clear();
		selection = -1;
		scrollTopIndex = 0;
	}

	void EnsureVisibleEntryPool ()
	{
		int visibleCount = Mathf.Min(maxVisibleOptions, optionTitles.Count);
		visibleCount = Mathf.Max(0, visibleCount);

		while (menuEntries.Count < visibleCount)
			menuEntries.Add(Dequeue());

		while (menuEntries.Count > visibleCount)
		{
			int last = menuEntries.Count - 1;
			Enqueue(menuEntries[last]);
			menuEntries.RemoveAt(last);
		}

		UpdateVisibleEntries();
	}

	bool SetSelection (int value)
	{
		if (value < 0 || value >= optionTitles.Count)
			return false;
		if (optionLocks[value])
			return false;
		
		selection = value;
		EnsureSelectionVisible();
		UpdateVisibleEntries();
		return true;
	}

	int FindNextUnlockedIndex (int start, int direction)
	{
		if (optionTitles.Count == 0)
			return -1;

		direction = direction >= 0 ? 1 : -1;
		for (int i = 0; i < optionTitles.Count; ++i)
		{
			int index = start + (i * direction);
			while (index < 0)
				index += optionTitles.Count;
			index %= optionTitles.Count;

			if (!optionLocks[index])
				return index;
		}

		return -1;
	}

	Tweener TogglePos (string pos)
	{
		Tweener t = panel.SetPosition(pos, true);
		t.duration = 0.5f;
		t.equation = EasingEquations.EaseOutQuad;
		return t;
	}

	void BuildScrollViewIfNeeded ()
	{
		if (!autoBuildScrollView || panel == null || contentRoot != null)
			return;

		RectTransform panelRect = panel.GetComponent<RectTransform>();
		if (panelRect == null)
			return;

		GameObject scrollViewObject = new GameObject("Ability Menu Scroll View", typeof(RectTransform), typeof(ScrollRect), typeof(LayoutElement));
		scrollViewObject.layer = panel.gameObject.layer;
		RectTransform scrollViewRect = scrollViewObject.GetComponent<RectTransform>();
		scrollViewRect.SetParent(panel.transform, false);
		scrollViewRect.anchorMin = new Vector2(0f, 0f);
		scrollViewRect.anchorMax = new Vector2(1f, 0f);
		scrollViewRect.pivot = new Vector2(0.5f, 1f);
		scrollViewRect.sizeDelta = new Vector2(0f, GetVisibleHeight(maxVisibleOptions));

		scrollViewLayoutElement = scrollViewObject.GetComponent<LayoutElement>();
		scrollViewLayoutElement.minHeight = GetVisibleHeight(maxVisibleOptions);
		scrollViewLayoutElement.preferredHeight = GetVisibleHeight(maxVisibleOptions);
		scrollViewLayoutElement.flexibleHeight = 0f;

		GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
		viewportObject.layer = panel.gameObject.layer;
		RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
		viewportRect.SetParent(scrollViewRect, false);
		viewportRect.anchorMin = new Vector2(0f, 0f);
		viewportRect.anchorMax = new Vector2(1f, 1f);
		viewportRect.offsetMin = Vector2.zero;
		viewportRect.offsetMax = new Vector2(-scrollbarWidth - 4f, 0f);
		Image viewportImage = viewportObject.GetComponent<Image>();
		viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
		Mask mask = viewportObject.GetComponent<Mask>();
		mask.showMaskGraphic = false;

		GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
		contentObject.layer = panel.gameObject.layer;
		contentRoot = contentObject.GetComponent<RectTransform>();
		contentRoot.SetParent(viewportRect, false);
		contentRoot.anchorMin = new Vector2(0f, 1f);
		contentRoot.anchorMax = new Vector2(1f, 1f);
		contentRoot.pivot = new Vector2(0.5f, 1f);
		contentRoot.anchoredPosition = Vector2.zero;
		contentRoot.sizeDelta = new Vector2(0f, GetVisibleHeight(maxVisibleOptions));

		VerticalLayoutGroup contentLayout = contentObject.GetComponent<VerticalLayoutGroup>();
		contentLayout.padding = new RectOffset(0, 0, 0, 0);
		contentLayout.spacing = entrySpacing;
		contentLayout.childAlignment = TextAnchor.UpperLeft;
		contentLayout.childForceExpandWidth = true;
		contentLayout.childForceExpandHeight = false;

		verticalScrollbar = CreateScrollbar(scrollViewRect);

		scrollRect = scrollViewObject.GetComponent<ScrollRect>();
		scrollRect.content = contentRoot;
		scrollRect.viewport = viewportRect;
		scrollRect.horizontal = false;
		scrollRect.vertical = true;
		scrollRect.movementType = ScrollRect.MovementType.Clamped;
		scrollRect.inertia = false;
		scrollRect.scrollSensitivity = entryHeight;
		scrollRect.verticalScrollbar = verticalScrollbar;
		scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
		scrollRect.verticalScrollbarSpacing = 4f;
	}

	void CacheOriginalPanelSize ()
	{
		if (panel == null)
			return;

		RectTransform panelRect = panel.GetComponent<RectTransform>();
		if (panelRect == null)
			return;

		if (originalPanelWidth < 0f)
			originalPanelWidth = panelRect.sizeDelta.x > 0f ? panelRect.sizeDelta.x : minDynamicPanelWidth;
		if (originalPanelHeight < 0f)
			originalPanelHeight = panelRect.sizeDelta.y;
	}

	void ApplyMenuReadableLayout ()
	{
		ApplyDynamicReadableLayout(titleLabel != null ? titleLabel.text : string.Empty);
	}

	void ApplyDynamicReadableLayout (string title)
	{
		CacheOriginalPanelSize();

		if (!useDynamicPanelSize || panel == null)
		{
			currentOptionFontSize = wrapLongOptionText ? wrappedOptionFontSize : compactOptionFontSize;
			currentWrappedLineCount = 1;
			entryHeight = compactEntryHeight;
			return;
		}

		RectTransform panelRect = panel.GetComponent<RectTransform>();
		if (panelRect == null)
			return;

		int longest = Mathf.Max(GetDisplayLength(title), GetLongestOptionLength());
		float baseWidth = originalPanelWidth > 0f ? originalPanelWidth : minDynamicPanelWidth;
		float wantedWidth = longest * averageCharacterWidth + dynamicPanelPadding;
		float width = Mathf.Clamp(Mathf.Max(baseWidth, wantedWidth), minDynamicPanelWidth, maxDynamicPanelWidth);

		float textWidth = Mathf.Max(90f, width - dynamicPanelPadding);
		int charsPerLine = Mathf.Max(12, Mathf.FloorToInt(textWidth / Mathf.Max(1f, averageCharacterWidth)));
		currentWrappedLineCount = Mathf.Clamp(Mathf.CeilToInt((float)Mathf.Max(1, longest) / (float)charsPerLine), 1, Mathf.Max(1, maxWrappedLines));
		bool shouldWrap = wrapLongOptionText && currentWrappedLineCount > 1;

		currentOptionFontSize = shouldWrap ? wrappedOptionFontSize : compactOptionFontSize;
		entryHeight = shouldWrap ? wrappedEntryHeight : compactEntryHeight;

		Vector2 size = panelRect.sizeDelta;
		size.x = width;
		if (originalPanelHeight > 0f)
			size.y = originalPanelHeight;
		panelRect.sizeDelta = size;

		if (scrollRect != null)
			scrollRect.scrollSensitivity = entryHeight;

		for (int i = 0; i < menuEntries.Count; ++i)
			ApplyEntryHeight(menuEntries[i]);
	}

	int GetLongestOptionLength ()
	{
		int longest = 0;
		for (int i = 0; i < optionTitles.Count; ++i)
			longest = Mathf.Max(longest, GetDisplayLength(optionTitles[i]));
		return longest;
	}

	int GetDisplayLength (string text)
	{
		if (string.IsNullOrEmpty(text))
			return 0;

		// Treat line breaks as separate wrapped lines instead of one extremely long line.
		string[] lines = text.Split('\n');
		int longest = 0;
		for (int i = 0; i < lines.Length; ++i)
			longest = Mathf.Max(longest, lines[i] != null ? lines[i].Length : 0);
		return longest;
	}

	void ConfigureEntryText (AbilityMenuEntry entry)
	{
		if (entry == null)
			return;

		entry.ConfigureTextLayout(wrapLongOptionText && currentWrappedLineCount > 1, currentOptionFontSize);
	}

	void ApplyEntryHeight (AbilityMenuEntry entry)
	{
		if (entry == null)
			return;

		LayoutElement layout = entry.GetComponent<LayoutElement>();
		if (layout == null)
			layout = entry.gameObject.AddComponent<LayoutElement>();
		layout.minHeight = entryHeight;
		layout.preferredHeight = entryHeight;
		layout.flexibleHeight = 0f;
	}

	void WireScrollbarCallback ()
	{
		if (verticalScrollbar == null)
			return;

		verticalScrollbar.onValueChanged.RemoveListener(OnScrollbarValueChanged);
		verticalScrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
	}

	Scrollbar CreateScrollbar (RectTransform parent)
	{
		GameObject scrollbarObject = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
		scrollbarObject.layer = panel.gameObject.layer;
		RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
		scrollbarRect.SetParent(parent, false);
		scrollbarRect.anchorMin = new Vector2(1f, 0f);
		scrollbarRect.anchorMax = new Vector2(1f, 1f);
		scrollbarRect.pivot = new Vector2(1f, 0.5f);
		scrollbarRect.sizeDelta = new Vector2(scrollbarWidth, 0f);
		scrollbarRect.anchoredPosition = Vector2.zero;

		Image scrollbarBackground = scrollbarObject.GetComponent<Image>();
		scrollbarBackground.color = new Color(0f, 0f, 0f, 0.35f);

		GameObject slidingAreaObject = new GameObject("Sliding Area", typeof(RectTransform));
		slidingAreaObject.layer = panel.gameObject.layer;
		RectTransform slidingAreaRect = slidingAreaObject.GetComponent<RectTransform>();
		slidingAreaRect.SetParent(scrollbarRect, false);
		slidingAreaRect.anchorMin = Vector2.zero;
		slidingAreaRect.anchorMax = Vector2.one;
		slidingAreaRect.offsetMin = Vector2.zero;
		slidingAreaRect.offsetMax = Vector2.zero;

		GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
		handleObject.layer = panel.gameObject.layer;
		RectTransform handleRect = handleObject.GetComponent<RectTransform>();
		handleRect.SetParent(slidingAreaRect, false);
		handleRect.anchorMin = Vector2.zero;
		handleRect.anchorMax = Vector2.one;
		handleRect.offsetMin = Vector2.zero;
		handleRect.offsetMax = Vector2.zero;

		Image handleImage = handleObject.GetComponent<Image>();
		handleImage.color = new Color(0.95f, 0.82f, 0.45f, 0.85f);

		Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
		scrollbar.handleRect = handleRect;
		scrollbar.direction = Scrollbar.Direction.BottomToTop;
		scrollbar.targetGraphic = handleImage;
		return scrollbar;
	}

	void UpdateScrollViewSize ()
	{
		int visibleCount = Mathf.Min(maxVisibleOptions, Mathf.Max(1, optionTitles.Count));
		float height = GetVisibleHeight(visibleCount);

		if (scrollViewLayoutElement != null)
		{
			scrollViewLayoutElement.minHeight = height;
			scrollViewLayoutElement.preferredHeight = height;
		}

		if (contentRoot != null)
			contentRoot.sizeDelta = new Vector2(contentRoot.sizeDelta.x, height);

		UpdateScrollbarVisual();

		for (int i = 0; i < menuEntries.Count; ++i)
			ApplyEntryHeight(menuEntries[i]);
	}

	float GetVisibleHeight (int visibleCount)
	{
		visibleCount = Mathf.Max(1, visibleCount);
		return visibleCount * entryHeight + Mathf.Max(0, visibleCount - 1) * entrySpacing;
	}

	int MaxScrollTopIndex ()
	{
		return Mathf.Max(0, optionTitles.Count - maxVisibleOptions);
	}

	void EnsureSelectionVisible ()
	{
		if (optionTitles.Count <= maxVisibleOptions)
		{
			SetScrollTopIndex(0, false);
			return;
		}

		if (selection < scrollTopIndex)
			SetScrollTopIndex(selection, true);
		else if (selection >= scrollTopIndex + maxVisibleOptions)
			SetScrollTopIndex(selection - maxVisibleOptions + 1, true);
	}

	void SetScrollTopIndex (int value, bool rebuildVisibleEntries)
	{
		scrollTopIndex = Mathf.Clamp(value, 0, MaxScrollTopIndex());

		if (contentRoot != null)
			contentRoot.anchoredPosition = Vector2.zero;

		UpdateScrollbarVisual();
		UpdateVisibleEntries();
	}

	void UpdateVisibleEntries ()
	{
		for (int i = 0; i < menuEntries.Count; ++i)
		{
			int optionIndex = scrollTopIndex + i;
			AbilityMenuEntry entry = menuEntries[i];

			if (optionIndex < 0 || optionIndex >= optionTitles.Count)
			{
				entry.gameObject.SetActive(false);
				continue;
			}

			entry.gameObject.SetActive(true);
			ConfigureEntryText(entry);
			entry.Reset();
			entry.Title = optionTitles[optionIndex];
			entry.IsLocked = optionLocks[optionIndex];
			entry.IsSelected = optionIndex == selection;
		}
	}

	void UpdateScrollbarVisual ()
	{
		if (verticalScrollbar == null)
			return;

		bool needsScrollbar = optionTitles.Count > maxVisibleOptions;
		verticalScrollbar.gameObject.SetActive(needsScrollbar);
		if (!needsScrollbar)
			return;

		suppressScrollbarCallback = true;
		verticalScrollbar.size = Mathf.Clamp01((float)maxVisibleOptions / (float)Mathf.Max(1, optionTitles.Count));
		int max = MaxScrollTopIndex();
		verticalScrollbar.value = max > 0 ? 1f - ((float)scrollTopIndex / (float)max) : 1f;
		suppressScrollbarCallback = false;
	}

	void OnScrollbarValueChanged (float value)
	{
		if (suppressScrollbarCallback || optionTitles.Count <= maxVisibleOptions)
			return;

		int max = MaxScrollTopIndex();
		int top = Mathf.RoundToInt((1f - value) * max);
		SetScrollTopIndex(top, true);
	}
	#endregion
}
