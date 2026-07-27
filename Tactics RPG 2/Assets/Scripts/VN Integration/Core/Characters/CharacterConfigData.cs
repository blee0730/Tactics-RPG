using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Sprite expressions are stored as a List<CharacterSpritePair> instead of
// SerializedDictionary — no third-party plugin needed.
// In the Inspector this appears as an expandable list with "Key" and "Sprite" fields.
// Access sprites by name via GetSprite(expressionName).
[System.Serializable]
public class CharacterSpritePair
{
    [Tooltip("Expression name (e.g. 'neutral', 'angry', 'surprised')")]
    public string key;
    public Sprite sprite;
}

[System.Serializable]
public class CharacterConfigData
{
    public string name;
    public string alias;
    public Character.CharacterType characterType;

    public Color nameColor;
    public Color dialogueColor;

    public TMP_FontAsset nameFont;
    public TMP_FontAsset dialogueFont;

    public float nameFontSize;
    public float dialogueFontSize;

    [Tooltip("Expression sprites. Key = expression name (e.g. 'neutral'), Sprite = the image.")]
    public List<CharacterSpritePair> sprites = new List<CharacterSpritePair>();

    // ── Sprite lookup ─────────────────────────────────────────────────────────

    // Returns the sprite for the given expression name, or null if not found.
    public Sprite GetSprite(string expressionKey)
    {
        if (string.IsNullOrEmpty(expressionKey) || sprites == null)
            return null;

        string lower = expressionKey.ToLower();
        foreach (var pair in sprites)
        {
            if (pair == null || string.IsNullOrEmpty(pair.key))
                continue;

            if (pair.key.ToLower() == lower)
                return pair.sprite;
        }
        return null;
    }

    public bool HasSprite(string expressionKey) => GetSprite(expressionKey) != null;

    // ── Safe values ───────────────────────────────────────────────────────────

    public Color SafeNameColor => EnsureVisibleColor(nameColor);
    public Color SafeDialogueColor => EnsureVisibleColor(dialogueColor);
    public TMP_FontAsset SafeNameFont => nameFont != null ? nameFont : defaultFont;
    public TMP_FontAsset SafeDialogueFont => dialogueFont != null ? dialogueFont : defaultFont;
    public float SafeNameFontSize => nameFontSize > 0 ? nameFontSize : DialogueSystem.instance.config.defaultNameFontSize;
    public float SafeDialogueFontSize => dialogueFontSize > 0 ? dialogueFontSize : DialogueSystem.instance.config.defaultDialogueFontSize;

    static Color EnsureVisibleColor(Color color)
    {
        // Unity's color picker often leaves alpha at 0 when adding fresh config
        // entries. Alpha 0 makes valid speaker dialogue invisible, which is almost
        // never intended for character text. Treat it as fully visible.
        if (color.a <= 0f)
            color.a = 1f;
        return color;
    }

    // ── Copy / Default ────────────────────────────────────────────────────────

    public CharacterConfigData Copy()
    {
        CharacterConfigData result = new CharacterConfigData();

        result.name           = name ?? string.Empty;
        result.alias          = alias ?? string.Empty;
        result.characterType  = characterType;
        result.nameFont       = SafeNameFont;
        result.dialogueFont   = SafeDialogueFont;
        result.nameColor      = SafeNameColor;
        result.dialogueColor  = SafeDialogueColor;
        result.dialogueFontSize = SafeDialogueFontSize;
        result.nameFontSize     = SafeNameFontSize;

        // Copy sprite list by reference — sprites are assets, no need to deep-copy
        result.sprites = sprites != null ? new List<CharacterSpritePair>(sprites) : new List<CharacterSpritePair>();

        return result;
    }

    private static Color           defaultColor => DialogueSystem.instance.config.defaultTextColor;
    private static TMP_FontAsset   defaultFont  => DialogueSystem.instance.config.defaultFont;

    public static CharacterConfigData Default
    {
        get
        {
            CharacterConfigData result = new CharacterConfigData();

            result.name           = "";
            result.alias          = "";
            result.characterType  = Character.CharacterType.Text;
            result.nameFont       = defaultFont;
            result.dialogueFont   = defaultFont;
            result.nameColor      = defaultColor;
            result.dialogueColor  = defaultColor;
            result.dialogueFontSize = DialogueSystem.instance.config.defaultDialogueFontSize;
            result.nameFontSize     = DialogueSystem.instance.config.defaultNameFontSize;

            return result;
        }
    }
}
