using UnityEngine;
using System.Collections;

public enum StatTypes
{
	LVL, // Level
	EXP, // Experience
	HP,  // Hit Points
	MHP, // Max Hit Points
	MP,  // Magic Points
	MMP, // Max Magic Points
	STR, // Physical Attack
	DEF, // Physical Defense
	MAG, // Magic Attack
	RES, // Magic Defense
	SKL, // Skill / Accuracy / Technique
	LCK, // Luck - crits and lucky avoids later
	SPD, // Speed
	MOV, // Move Range
	JMP, // Jump Height
	CTR, // Counter - for turn order
	FRT, // Fortitude - status resistance
	Count
}