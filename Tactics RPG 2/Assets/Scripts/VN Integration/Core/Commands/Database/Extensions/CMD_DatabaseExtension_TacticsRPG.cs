using System;

// Registers all Tactics RPG custom commands with the VN dialogue system.
//
// HOW IT WORKS — nothing to set up in the Inspector:
// CommandManager.Awake() uses reflection to automatically find every subclass
// of CMD_DatabaseExtension in the project and call its Extend() method.
// This class is found and registered automatically the moment the scene loads.
//
// Available commands in dialogue files:
//   @unlock_support Rein Rosemary C
//   @unlock_support Rein Rosemary B "Conversations/Support/Rein_Rosemary_B"
//   @set_battle_result true 1
//   @set_story_flag met_rosemary true

public class CMD_DatabaseExtension_TacticsRPG : CMD_DatabaseExtension
{
    new public static void Extend(CommandDatabase database)
    {
        database.AddCommand("unlock_support",    new Action<string[]>(CMD_UnlockSupport));
        database.AddCommand("set_battle_result", new Action<string[]>(CMD_SetBattleResult));
        database.AddCommand("set_story_flag",    new Action<string[]>(CMD_SetStoryFlag));
    }

    // @unlock_support <char1> <char2> <rank> [conversationFile]
    // Example: @unlock_support Rein Rosemary C
    static void CMD_UnlockSupport(string[] args)
    {
        if (args.Length < 3)
        {
            UnityEngine.Debug.LogWarning("[TacticsRPG] unlock_support: needs <char1> <char2> <rank>");
            return;
        }

        if (!Enum.TryParse(args[2].ToUpper(), out SupportRank rank))
        {
            UnityEngine.Debug.LogWarning($"[TacticsRPG] unlock_support: invalid rank '{args[2]}'. Use C/B/A/S.");
            return;
        }

        string file = args.Length > 3 ? args[3] : "";
        SupportConversationLog.Unlock(args[0], args[1], rank, file);
        SupportConversationLog.Save();
    }

    // @set_battle_result <true|false> <battleNumber>
    // Example: @set_battle_result true 1
    static void CMD_SetBattleResult(string[] args)
    {
        if (args.Length < 2) return;
        bool.TryParse(args[0], out bool won);
        int.TryParse(args[1],  out int num);
        TacticsVariableSetup.SetBattleResult(won, num);
    }

    // @set_story_flag <flagName> <true|false>
    // Example: @set_story_flag met_rosemary true
    static void CMD_SetStoryFlag(string[] args)
    {
        if (args.Length < 2) return;
        bool.TryParse(args[1], out bool value);
        TacticsVariableSetup.SetStoryFlag(args[0], value);
    }
}
