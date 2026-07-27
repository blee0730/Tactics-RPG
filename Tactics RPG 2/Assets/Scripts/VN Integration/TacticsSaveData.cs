using System.Collections.Generic;
using UnityEngine;

// Saves and loads both Tactics battle state and VN dialogue state together.
//
// Usage:
//   TacticsSaveData.Save(slotNumber);
//   TacticsSaveData.Load(slotNumber);

[System.Serializable]
public class TacticsSaveData
{
    const string FILE_EXTENSION = ".tacs";
    const string SAVE_FOLDER    = "TacticsRPG/Saves/";

    public static TacticsSaveData Current { get; private set; } = new TacticsSaveData();

    // ── VN state ─────────────────────────────────────────────────────────────
    public VN_VariableData[]     variables;
    public VN_ConversationData[] activeConversations;

    // ── Tactics state ─────────────────────────────────────────────────────────
    public int      currentBattle     = 0;
    public bool     lastBattleWon     = false;
    public string[] unlockedAbilities = new string[0];
    public string   timestamp;

    // ── Save ──────────────────────────────────────────────────────────────────

    public static void Save(int slotNumber)
    {
        TacticsSaveData save  = new TacticsSaveData();
        save.timestamp        = System.DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
        save.currentBattle    = Current.currentBattle;
        save.lastBattleWon    = Current.lastBattleWon;
        save.unlockedAbilities = Current.unlockedAbilities;

        // Snapshot all VariableStore databases
        var varList = new List<VN_VariableData>();
        foreach (var db in VariableStore.databases.Values)
        {
            foreach (var pair in db.variables)
            {
                object val = pair.Value.Get();
                varList.Add(new VN_VariableData
                {
                    name  = $"{db.name}.{pair.Key}",
                    value = val != null ? val.ToString() : "",
                    type  = val != null ? val.GetType().ToString() : "System.String"
                });
            }
        }
        save.variables = varList.ToArray();

        // Snapshot the active conversation queue
        var convList = new List<VN_ConversationData>();
        if (DialogueSystem.instance != null)
        {
            Conversation[] queue =
                DialogueSystem.instance.conversationManager.GetConversationQueue();
            foreach (var conv in queue)
            {
                convList.Add(new VN_ConversationData
                {
                    conversation = conv.GetLines(),
                    progress     = conv.GetProgress()
                });
            }
        }
        save.activeConversations = convList.ToArray();

        // Write to disk via FileManager
        System.IO.Directory.CreateDirectory(
            System.IO.Path.GetDirectoryName(GetPath(slotNumber)));
        FileManager.Save(GetPath(slotNumber), JsonUtility.ToJson(save));

        Current = save;
        Debug.Log($"[TacticsSaveData] Saved to slot {slotNumber}.");
    }

    // ── Load ──────────────────────────────────────────────────────────────────

    public static void Load(int slotNumber)
    {
        TacticsSaveData save = FileManager.Load<TacticsSaveData>(GetPath(slotNumber));

        if (save == null)
        {
            Debug.LogWarning($"[TacticsSaveData] No save found in slot {slotNumber}.");
            return;
        }

        Current = save;

        // Restore VariableStore values
        if (save.variables != null)
        {
            foreach (var vd in save.variables)
            {
                switch (vd.type)
                {
                    case "System.Boolean":
                        if (bool.TryParse(vd.value, out bool b))
                            VariableStore.TrySetValue(vd.name, b);
                        break;
                    case "System.Int32":
                        if (int.TryParse(vd.value, out int i))
                            VariableStore.TrySetValue(vd.name, i);
                        break;
                    case "System.Single":
                        if (float.TryParse(vd.value, out float f))
                            VariableStore.TrySetValue(vd.name, f);
                        break;
                    default:
                        VariableStore.TrySetValue(vd.name, vd.value);
                        break;
                }
            }
        }

        // Re-queue saved conversations back into DialogueSystem
        if (save.activeConversations != null && DialogueSystem.instance != null)
        {
            foreach (var cd in save.activeConversations)
            {
                if (cd?.conversation != null && cd.conversation.Count > 0)
                {
                    var conv = new Conversation(cd.conversation, cd.progress);
                    DialogueSystem.instance.conversationManager.Enqueue(conv);
                }
            }
        }

        Debug.Log($"[TacticsSaveData] Loaded slot {slotNumber} — Battle {save.currentBattle}.");
    }

    static string GetPath(int slot) =>
        $"{Application.persistentDataPath}/{SAVE_FOLDER}slot{slot}{FILE_EXTENSION}";
}
