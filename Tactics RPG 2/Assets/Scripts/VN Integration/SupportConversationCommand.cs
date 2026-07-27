using UnityEngine;

// Registers the @unlock_support command with the VN CommandManager.
// Place this on the same GameObject as CommandManager (usually DialogueSystem root).
//
// Usage in dialogue files:
//   @unlock_support Rein Rosemary C
//   @unlock_support Rein Rosemary B "Conversations/Support/Rein_Rosemary_B"
//
// This records the support in SupportConversationLog and saves to disk.

public class SupportConversationCommand : MonoBehaviour
{
    void Start()
    {
        //CommandManager.instance.AddCommand("unlock_support", new System.Action<string[]>(CMD_UnlockSupport));
    }

    void CMD_UnlockSupport(string[] args)
    {
        if (args.Length < 3)
        {
            Debug.LogWarning("[SupportCommand] Usage: unlock_support <char1> <char2> <rank> [conversationFile]");
            return;
        }

        string char1 = args[0];
        string char2 = args[1];
        string rankStr = args[2].ToUpper();
        string file  = args.Length > 3 ? args[3] : "";

        if (!System.Enum.TryParse(rankStr, out SupportRank rank))
        {
            Debug.LogWarning($"[SupportCommand] Invalid rank '{rankStr}'. Use C/B/A/S.");
            return;
        }

        SupportConversationLog.Unlock(char1, char2, rank, file);
        SupportConversationLog.Save();
    }
}
