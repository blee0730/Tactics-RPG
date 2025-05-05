#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GraphicLayerTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Running());
    }

    IEnumerator Running()
    {
        GraphicPanel panel = GraphicPanelManager.instance.GetPanel("background");
        GraphicLayer layer0 = panel.GetLayer(0, true);
        GraphicLayer layer1 = panel.GetLayer(1, true);

        layer0.SetVideo("Graphics/BG Videos/Nebula");
        layer1.SetTexture("Graphics/BG Images/Spaceshipinterior");

        yield return new WaitForSeconds(2);

        GraphicPanel cinematic = GraphicPanelManager.instance.GetPanel("cinematic");
        GraphicLayer cinLayer = cinematic.GetLayer(0, true);

        cinLayer.SetTexture("Graphics/Gallery/pup");

        yield return DialogueSystem.instance.Say("Narrator", "We truly don't deserve dogs");

        cinLayer.Clear();

        yield return new WaitForSeconds(1);

        panel.Clear();
    }
}
#endif