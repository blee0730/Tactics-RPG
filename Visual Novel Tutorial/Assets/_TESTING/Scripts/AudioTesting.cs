#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Running());
    }

    Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

    IEnumerator Running()
    {
        Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
        Raelin.Show();

        yield return DialogueSystem.instance.Say("Narrator", "Can we see your ship?");

        GraphicPanelManager.instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/5");
        AudioManager.instance.PlayTrack("Audio/Music/Calm", startingVolume: 0.7f);

    }
}
#endif