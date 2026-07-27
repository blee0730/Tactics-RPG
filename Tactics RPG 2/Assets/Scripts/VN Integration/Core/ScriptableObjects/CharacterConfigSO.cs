using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Configuration Asset", menuName = "Dialogue System/Character Configuration Asset")]
public class CharacterConfigSO : ScriptableObject
{
    public CharacterConfigData[] characters;

    public CharacterConfigData GetConfig(string characterName, bool safe = true)
    {
        if (string.IsNullOrEmpty(characterName) || characters == null)
            return CharacterConfigData.Default;

        string lookup = characterName.ToLower();

        for(int i = 0; i < characters.Length; i++)
        {
            CharacterConfigData data = characters[i];
            if (data == null)
                continue;

            string dataName = data.name != null ? data.name.ToLower() : string.Empty;
            string dataAlias = data.alias != null ? data.alias.ToLower() : string.Empty;

            if(string.Equals(lookup, dataName) || (!string.IsNullOrEmpty(dataAlias) && string.Equals(lookup, dataAlias)))
                return safe ? data.Copy() : data;
        }

        return CharacterConfigData.Default;
    }
}
