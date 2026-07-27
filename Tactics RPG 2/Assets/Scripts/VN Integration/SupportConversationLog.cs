using System.Collections.Generic;
using UnityEngine;

// Replaces the VN Gallery system with a Support Conversation tracker.
// Records which support conversations have been seen between which characters,
// at which support rank (C/B/A/S), and whether they are unlocked to replay.
//
// Integrates with GalleryConfig's unlock/save system under the hood —
// just uses character-pair keys instead of image names.
//
// Usage from code:
//   SupportConversationLog.Unlock("Rein", "Rosemary", SupportRank.C);
//   bool seen = SupportConversationLog.IsUnlocked("Rein", "Rosemary", SupportRank.C);
//
// Usage from dialogue files:
//   @unlock_support Rein Rosemary C

public enum SupportRank { C, B, A, S }

[System.Serializable]
public class SupportEntry
{
    public string character1;
    public string character2;
    public SupportRank rank;
    public string conversationFile; // path under Resources/Conversations/Support/
    public string timestamp;
}

public static class SupportConversationLog
{
    const string SAVE_KEY_PREFIX = "support_";
    static List<SupportEntry> entries = new List<SupportEntry>();
    static bool loaded = false;

    static string MakeKey(string char1, string char2, SupportRank rank)
    {
        // Always sort alphabetically so "Rein+Rosemary" == "Rosemary+Rein"
        string a = char1.ToLower();
        string b = char2.ToLower();
        if (string.Compare(a, b) > 0) { var tmp = a; a = b; b = tmp; }
        return $"{SAVE_KEY_PREFIX}{a}_{b}_{rank}";
    }

    public static void Unlock(string char1, string char2, SupportRank rank,
                               string conversationFile = "")
    {
        string key = MakeKey(char1, char2, rank);

        // Reuse GalleryConfig's unlock persistence
        GalleryConfig.UnlockImage(key);

        // Also keep an in-memory entry for richer data
        if (!entries.Exists(e =>
            MakeKey(e.character1, e.character2, e.rank) == key))
        {
            entries.Add(new SupportEntry
            {
                character1       = char1,
                character2       = char2,
                rank             = rank,
                conversationFile = conversationFile,
                timestamp        = System.DateTime.Now.ToString("yy-MM-dd HH:mm:ss")
            });
        }

        Debug.Log($"[SupportLog] Unlocked: {char1} & {char2} — Rank {rank}");
    }

    public static bool IsUnlocked(string char1, string char2, SupportRank rank)
    {
        return GalleryConfig.ImageIsUnlocked(MakeKey(char1, char2, rank));
    }

    // Returns all unlocked support entries — for building the support log UI
    public static List<SupportEntry> GetAllUnlocked()
    {
        Load();
        return entries;
    }

    // Returns unlocked entries for a specific character
    public static List<SupportEntry> GetUnlockedFor(string characterName)
    {
        Load();
        string lower = characterName.ToLower();
        return entries.FindAll(e =>
            e.character1.ToLower() == lower ||
            e.character2.ToLower() == lower);
    }

    static void Load()
    {
        if (loaded) return;
        GalleryConfig.Load();
        loaded = true;
    }

    public static void Save() => GalleryConfig.Save();
}
