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
	ATK, // Physical Attack
	DEF, // Physical Defense
<<<<<<< Updated upstream
	MAT, // Magic Attack
	MDF, // Magic Defense
	EVD, // Evade
	RES, // Status Resistance
=======
	MAG, // Magic Attack
	RES, // Magic Defense
	SKL, // Skill / Accuracy / Technique
	LCK, // Luck - crits and lucky avoids later
>>>>>>> Stashed changes
	SPD, // Speed
	MOV, // Move Range
	JMP, // Jump Height
	CTR, // Counter - for turn order
	FRT, // Fortitude - status resistance
	Count
}