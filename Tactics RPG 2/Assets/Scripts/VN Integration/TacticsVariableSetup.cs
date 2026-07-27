using UnityEngine;

// Add this component to the BattleController GameObject (or a persistent
// game manager object). Call Setup() once at the start of the game.
//
// After this runs, your VN dialogue files can use variables like:
//   $battle.win  — true/false, did player win the current battle?
//   $player.name — the player character's name
//
// You can add more variables here as your story grows.
// Variables persist between scenes via VariableStore.databases (static).

public class TacticsVariableSetup : MonoBehaviour
{
    [Header("Player Variables")]
    [SerializeField] string playerName = "Rein";

    void Awake()
    {
        Setup();
    }

    public void Setup()
    {
        // Player identity
        VariableStore.CreateVariable("player.name", playerName);

        // Battle state flags — set these via SetBattleResult() during gameplay
        VariableStore.CreateVariable("battle.win",    false);
        VariableStore.CreateVariable("battle.number", 0);

        // Story progression flags — set these as narrative milestones are reached
        VariableStore.CreateVariable("story.met_rosemary",  false);
        VariableStore.CreateVariable("story.met_lucy",      false);
        VariableStore.CreateVariable("story.chapter",       1);

        Debug.Log("[TacticsVariableSetup] Variables registered with VariableStore.");
    }

    // Call this from EndBattleState (or wherever you determine win/loss)
    // so VN outro conversations can branch on $battle.win
    public static void SetBattleResult(bool playerWon, int battleNumber)
    {
        VariableStore.TrySetValue("battle.win",    playerWon);
        VariableStore.TrySetValue("battle.number", battleNumber);
    }

    // Example: mark a story event as reached from anywhere in code
    public static void SetStoryFlag(string flagName, bool value)
    {
        if (!VariableStore.TrySetValue("story." + flagName, value))
            VariableStore.CreateVariable("story." + flagName, value);
    }
}
