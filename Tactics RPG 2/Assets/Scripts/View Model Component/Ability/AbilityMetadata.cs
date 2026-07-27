using UnityEngine;
using System.Collections;

public class AbilityMetadata : MonoBehaviour
{
	public AbilitySourceTypes sourceType = AbilitySourceTypes.None;
	public AbilityDamageTypes damageType = AbilityDamageTypes.None;

	// Explicit designer-controlled flags. These avoid inferring behavior from
	// placeholder components such as MagicalAbilityPower or AbilityMagicCost.
	public bool blockedBySilence = false;
	public bool ignoresInvulnerability = false;
	public bool canTargetUntargetable = false;
}
