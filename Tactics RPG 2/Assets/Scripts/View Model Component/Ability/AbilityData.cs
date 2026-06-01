using UnityEngine;

[CreateAssetMenu(menuName = "Tactics RPG/Ability Data")]
public class AbilityData : ScriptableObject
{
    public string abilityName;
    public string description;
    public int manaCost;
    public int staminaCost;
    public bool isMagic;
    public int masteryLevelRequired;
    public int observationRequirement = 100;
    public AbilityEvolution[] evolutions;
    public Sprite icon;
}