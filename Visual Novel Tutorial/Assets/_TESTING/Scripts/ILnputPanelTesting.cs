#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ILnputPanelTesting : MonoBehaviour
{
    public InputPanel inputPanel;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Running());
    }

    IEnumerator Running()
    {
        Character Raelin = CharacterManager.instance.CreateCharacter("Raelin", revealAfterCreation: true);

        yield return Raelin.Say("Hi! What's your name?");

        inputPanel.Show("What is your name?");

        while(inputPanel.isWaitingOnUserInput)
            yield return null;

        string characterName = inputPanel.lastInput;

        yield return Raelin.Say($"It's very nice to meet you, {characterName}!");
    }
}
#endif