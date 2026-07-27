using UnityEngine;

/// <summary>
/// Never ends the battle. Used only by the ability test lab so dummy HP/status tests
/// do not immediately kick the player out of the scene.
/// </summary>
public class AbilityTestLabVictoryCondition : BaseVictoryCondition
{
    protected override void CheckForGameOver()
    {
        // Intentionally empty. The lab is a sandbox.
    }
}
