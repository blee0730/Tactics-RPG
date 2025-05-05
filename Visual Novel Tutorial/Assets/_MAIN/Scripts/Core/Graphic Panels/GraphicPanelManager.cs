using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicPanelManager : MonoBehaviour
{
    public static GraphicPanelManager instance {get; private set;}

    [field:SerializeField] public GraphicPanel[] allPanels {get; private set;}

    private void Awake()
    {
        instance = this;
    }

    public GraphicPanel GetPanel(string name)
    {
        foreach(var panel in allPanels)
        {
            if(panel.panelName.ToLower() == name)
                return panel;
        }

        return null;
    }
}
