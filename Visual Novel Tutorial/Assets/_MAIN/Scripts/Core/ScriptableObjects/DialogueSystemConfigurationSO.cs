using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue System Configuration", menuName = "Dialogue System/Dialogue Configuration Asset")]
public class DialogueSystemConfigurationSO : ScriptableObject
{
    public CharacterConfigSO characterConfigurationAsset;

    public Color defaultTextColor = Color.white;
    public TMP_FontAsset defaultFont;

    public float dialogueFontScale = 1f;
    public float defaultDialogueFontSize = 18;
    public float defaultNameFontSize = 22;
}