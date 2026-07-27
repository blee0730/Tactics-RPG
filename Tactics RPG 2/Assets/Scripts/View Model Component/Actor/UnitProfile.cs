using UnityEngine;

/// <summary>
/// Stores presentation-only data for a battle unit.
/// This keeps UI display data separate from combat stats and mechanics.
/// </summary>
public class UnitProfile : MonoBehaviour
{
    [Tooltip("Name shown in UI panels. If blank, the GameObject name is used.")]
    public string displayName;

    [Tooltip("Portrait/sprite shown in the status panel.")]
    public Sprite statusPortrait;

    [Tooltip("Optional link to the VN dialogue character config name. If blank, displayName/GameObject name is used.")]
    public string dialogueCharacterName;

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrEmpty(displayName))
                return displayName;
            return gameObject != null ? gameObject.name : string.Empty;
        }
    }

    public string DialogueCharacterName
    {
        get
        {
            if (!string.IsNullOrEmpty(dialogueCharacterName))
                return dialogueCharacterName;
            return DisplayName;
        }
    }
}
