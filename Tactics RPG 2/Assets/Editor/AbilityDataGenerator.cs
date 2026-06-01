#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

// ============================================================
// HOW TO USE:
// In Unity menu bar: Tools → Generate Ability Data Assets
//
// Scans every prefab under Assets/Resources/Abilities/,
// creates a matching AbilityData ScriptableObject under
// Assets/Resources/Ability Data/ (same subfolder structure),
// then wires the asset into the Ability component on each prefab.
//
// Safe to run multiple times — skips prefabs already wired.
// ============================================================

public class AbilityDataGenerator : EditorWindow
{
    const string PREFAB_ROOT = "Assets/Resources/Abilities";
    const string DATA_ROOT   = "Assets/Resources/Ability Data";

    [MenuItem("Tools/Generate Ability Data Assets")]
    static void Run()
    {
        int created = 0;
        int skipped = 0;
        int wired   = 0;
        int errors  = 0;

        // Ensure the root data folder exists before we do anything
        EnsureFolderExists(DATA_ROOT);

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PREFAB_ROOT });

        foreach (string guid in guids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) continue;

            Ability ability = prefab.GetComponent<Ability>();
            if (ability == null) continue;

            // Skip if already wired
            if (ability.data != null)
            {
                skipped++;
                continue;
            }

            // Build the matching .asset path
            // e.g. Assets/Resources/Abilities/Battle Tech/Haste.prefab
            //   -> Assets/Resources/Ability Data/Battle Tech/Haste.asset
            string relative  = prefabPath
                .Replace(PREFAB_ROOT + "/", "")
                .Replace(".prefab", ".asset");

            string dataPath   = DATA_ROOT + "/" + relative;
            string dataFolder = Path.GetDirectoryName(dataPath)
                                    .Replace('\\', '/');   // normalize on Windows

            // Ensure the category subfolder exists
            // (handles names with spaces like "Battle Tech", "White Magic", etc.)
            EnsureFolderExists(dataFolder);

            // Load existing asset or create a fresh one
            AbilityData data = AssetDatabase.LoadAssetAtPath<AbilityData>(dataPath);

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<AbilityData>();
                data.abilityName            = Path.GetFileNameWithoutExtension(prefabPath);
                data.description            = "";
                data.manaCost               = 0;
                data.staminaCost            = 0;
                data.isMagic                = false;
                data.masteryLevelRequired   = 0;
                data.observationRequirement = 100;

                // Verify the folder actually exists in the AssetDatabase before writing
                if (!AssetDatabase.IsValidFolder(dataFolder))
                {
                    Debug.LogError($"[AbilityDataGenerator] Folder still missing after " +
                                   $"EnsureFolderExists: {dataFolder}  — skipping {data.abilityName}");
                    errors++;
                    continue;
                }

                AssetDatabase.CreateAsset(data, dataPath);
                created++;
            }

            // Wire the asset into the prefab
            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                Ability prefabAbility = scope.prefabContentsRoot.GetComponent<Ability>();
                if (prefabAbility != null)
                {
                    prefabAbility.data = data;
                    wired++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string errorLine = errors > 0 ? $"\n⚠️  Errors:  {errors} (check Console)" : "";

        EditorUtility.DisplayDialog(
            "Ability Data Generator",
            $"Done!\n\n" +
            $"✅ Created: {created} new AbilityData assets\n" +
            $"🔗 Wired:   {wired} prefabs\n" +
            $"⏭  Skipped: {skipped} (already had data)" +
            errorLine + "\n\n" +
            $"Assets are in: {DATA_ROOT}\n" +
            $"Fill in descriptions, mana costs, etc. in the Inspector.",
            "OK"
        );

        Debug.Log($"[AbilityDataGenerator] Created {created}, wired {wired}, " +
                  $"skipped {skipped}, errors {errors}.");
    }

    // Creates every folder in the path one level at a time.
    // Calls AssetDatabase.Refresh() after each new folder so Unity
    // registers it before we try to create a child inside it.
    // Handles names with spaces (e.g. "Battle Tech", "White Magic").
    static void EnsureFolderExists(string folderPath)
    {
        folderPath = folderPath.Replace('\\', '/');

        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string[] parts  = folderPath.Split('/');
        string   current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
                // Refresh immediately so Unity sees the new folder before
                // we try to nest another folder or asset inside it.
                AssetDatabase.Refresh();
            }

            current = next;
        }
    }
}
#endif
