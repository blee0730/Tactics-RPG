using System.Collections.Generic;
using UnityEngine;

public class UnlockedAbilityCollection :
    MonoBehaviour
{
    public List<string> unlocked =
        new List<string>();

    public bool Contains(string name)
    {
        return unlocked.Contains(name);
    }

    public void Add(string name)
    {
        if (!Contains(name))
            unlocked.Add(name);
    }
}