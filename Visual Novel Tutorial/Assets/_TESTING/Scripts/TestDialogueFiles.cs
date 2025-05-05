#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDialogueFiles : MonoBehaviour
{
    [SerializeField] private TextAsset fileToRead = null;
    // Start is called before the first frame update
    void Start()
    {
        StartConversation();
    }

    void StartConversation()
    {
        List<string> lines = FileManager.ReadTextAsset(fileToRead);
        
        DialogueSystem.instance.Say(lines);
    }

    public void LoadFile(string filePath)
    {
        List<string> lines = new List<string>();
        TextAsset file = Resources.Load<TextAsset>(filePath);

        try
        {
            lines = FileManager.ReadTextAsset(file);
        }
        catch
        {
            Debug.LogError($"Dialogue file at path 'Resources/{filePath}' does not exist!");
            return;
        }

        DialogueSystem.instance.Say(lines, filePath);
    }
}
#endif