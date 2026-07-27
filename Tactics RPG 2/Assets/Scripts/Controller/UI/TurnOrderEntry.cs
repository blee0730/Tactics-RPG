using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One portrait icon in the top turn-order bar.
/// Shows only order number + portrait + optional short status marker.
/// </summary>
public class TurnOrderEntry : MonoBehaviour
{
	[SerializeField] Image background;
	[SerializeField] Image portrait;
	[SerializeField] Text orderLabel;
	[SerializeField] Text initialsLabel;
	[SerializeField] Text statusLabel;
	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] Outline hoverOutline;
	[SerializeField] Text hoverLabel;

	[SerializeField] Color activeColor = new Color32(249, 210, 118, 255);
	[SerializeField] Color allyColor = new Color32(42, 92, 146, 245);
	[SerializeField] Color enemyColor = new Color32(139, 52, 52, 245);
	[SerializeField] Color neutralColor = new Color32(80, 80, 80, 245);
	[SerializeField] Color disabledColor = new Color32(55, 55, 55, 210);
	[SerializeField] Color hoverOutlineColor = new Color32(255, 244, 150, 255);
	[SerializeField] float activeScale = 1.14f;
	[SerializeField] float hoverScale = 1.08f;

	Vector3 normalScale = Vector3.one;
	Vector2 normalSize;

	public Unit Unit { get; private set; }

	public void Display (Unit unit, int order, bool isActive)
	{
		Display(unit, order, isActive, false);
	}

	public void Display (Unit unit, int order, bool isActive, bool isHoveredNextTurn)
	{
		Unit = unit;
		if (unit == null)
		{
			Clear();
			return;
		}

		gameObject.SetActive(true);

		UnitProfile profile = unit.GetComponent<UnitProfile>();
		Alliance alliance = unit.GetComponent<Alliance>();
		bool skipped = IsSkipped(unit);

		if (orderLabel != null)
			orderLabel.text = order.ToString();

		Sprite sprite = profile != null ? profile.statusPortrait : null;
		if (portrait != null)
		{
			portrait.sprite = sprite;
			portrait.enabled = sprite != null;
			portrait.preserveAspect = true;
		}

		if (initialsLabel != null)
		{
			initialsLabel.text = sprite == null ? GetInitials(profile != null ? profile.DisplayName : unit.name) : string.Empty;
			initialsLabel.gameObject.SetActive(sprite == null);
		}

		if (statusLabel != null)
		{
			statusLabel.text = GetStatusLabel(unit);
			statusLabel.gameObject.SetActive(!string.IsNullOrEmpty(statusLabel.text));
		}

		if (hoverOutline != null)
		{
			hoverOutline.effectColor = hoverOutlineColor;
			hoverOutline.enabled = isHoveredNextTurn;
		}

		if (hoverLabel != null)
		{
			hoverLabel.text = isHoveredNextTurn ? "NEXT" : string.Empty;
			hoverLabel.gameObject.SetActive(isHoveredNextTurn);
		}

		if (background != null)
		{
			if (skipped)
				background.color = disabledColor;
			else
				background.color = isActive ? activeColor : GetAllianceColor(alliance);
		}

		if (canvasGroup != null)
			canvasGroup.alpha = skipped ? 0.48f : 1f;

		float scale = 1f;
		if (isActive)
			scale = activeScale;
		else if (isHoveredNextTurn)
			scale = hoverScale;
		transform.localScale = normalScale * scale;
	}

	public void Clear ()
	{
		Unit = null;
		if (portrait != null)
		{
			portrait.sprite = null;
			portrait.enabled = false;
		}
		if (orderLabel != null) orderLabel.text = string.Empty;
		if (initialsLabel != null)
		{
			initialsLabel.text = string.Empty;
			initialsLabel.gameObject.SetActive(false);
		}
		if (statusLabel != null)
		{
			statusLabel.text = string.Empty;
			statusLabel.gameObject.SetActive(false);
		}
		if (hoverOutline != null)
			hoverOutline.enabled = false;
		if (hoverLabel != null)
		{
			hoverLabel.text = string.Empty;
			hoverLabel.gameObject.SetActive(false);
		}
		transform.localScale = normalScale;
		gameObject.SetActive(false);
	}

	public void AssignRuntimeReferences (Image backgroundImage, Image portraitImage, Text orderText, Text initialsText, Text statusText, CanvasGroup group, Vector2 entrySize)
	{
		AssignRuntimeReferences(backgroundImage, portraitImage, orderText, initialsText, statusText, group, null, null, entrySize);
	}

	public void AssignRuntimeReferences (Image backgroundImage, Image portraitImage, Text orderText, Text initialsText, Text statusText, CanvasGroup group, Outline outline, Text nextTurnLabel, Vector2 entrySize)
	{
		background = backgroundImage;
		portrait = portraitImage;
		orderLabel = orderText;
		initialsLabel = initialsText;
		statusLabel = statusText;
		canvasGroup = group;
		hoverOutline = outline;
		hoverLabel = nextTurnLabel;
		normalSize = entrySize;
		normalScale = Vector3.one;
	}

	Color GetAllianceColor (Alliance alliance)
	{
		if (alliance == null)
			return neutralColor;
		if (alliance.type == Alliances.Hero)
			return allyColor;
		if (alliance.type == Alliances.Enemy)
			return enemyColor;
		return neutralColor;
	}

	string GetStatusLabel (Unit unit)
	{
		if (unit.GetComponentInChildren<KnockOutStatusEffect>() != null)
			return "KO";
		if (unit.GetComponentInChildren<StopStatusEffect>() != null)
			return "STOP";
		if (unit.cantMove && unit.cantAct)
			return "LOCK";
		if (unit.cantMove)
			return "MOVE";
		if (unit.cantAct)
			return "ACT";
		return string.Empty;
	}

	bool IsSkipped (Unit unit)
	{
		return unit.GetComponentInChildren<KnockOutStatusEffect>() != null || unit.GetComponentInChildren<StopStatusEffect>() != null;
	}

	string GetInitials (string source)
	{
		if (string.IsNullOrEmpty(source))
			return "?";
		return source.Substring(0, 1).ToUpper();
	}
}
