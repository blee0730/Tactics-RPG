#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoicePanelTesting : MonoBehaviour
{
    ChoicePanel panel;
    // Start is called before the first frame update
    void Start()
    {
        panel = ChoicePanel.instance;
        StartCoroutine(Running());
    }

    IEnumerator Running()
    {
        string[] choices = new string[]
        {
            "Witness? Is that camera on?",
            "Oh, nah!",
            "I didn't see nothin'!",
            "Matta' Fact- I'm blind in my left eye and 43% blind in my right eye."
        };

        panel.Show("Did You Witness Anything Strange", choices);

        while(panel.isWaitingOnUserChoice)
            yield return null;

        var decision = panel.lastDescision;

        Debug.Log($"Made choice {decision.answerIndex} '{decision.choices[decision.answerIndex]}'");
    }
}
#endif