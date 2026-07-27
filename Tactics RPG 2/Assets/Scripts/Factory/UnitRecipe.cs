using UnityEngine;
using System.Collections;

public class UnitRecipe : ScriptableObject 
{
	[Header("Unit Presentation")]
	[Tooltip("Optional name shown in UI panels. If blank, the recipe asset name is used.")]
	public string displayName;
	[Tooltip("Optional portrait/sprite shown in the status panel.")]
	public Sprite statusPortrait;
	[Tooltip("Optional VN dialogue character config name. If blank, displayName or recipe name is used.")]
	public string dialogueCharacterName;

	[Header("Battle Setup")]
	public string model;
	public string job;
	public string attack;
	public string abilityCatalog;
	public string strategy;
	public Locomotions locomotion;
	public Alliances alliance;
}
