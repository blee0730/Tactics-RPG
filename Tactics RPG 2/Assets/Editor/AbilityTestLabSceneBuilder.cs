#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class AbilityTestLabSceneBuilder
{
    const string SourceScenePath = "Assets/Scenes/Battle.unity";
    const string TargetScenePath = "Assets/Scenes/Ability Test Lab.unity";
    const string DefaultLevelPath = "Assets/Resources/Levels/level 1.asset";

    [MenuItem("Tools/Tactics RPG/Testing/Create Ability Test Lab Scene")]
    public static void CreateAbilityTestLabScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Ability Test Lab", "Exit Play Mode before creating the test scene.", "OK");
            return;
        }

        if (!File.Exists(SourceScenePath))
        {
            EditorUtility.DisplayDialog("Ability Test Lab", "Could not find source scene: " + SourceScenePath, "OK");
            return;
        }

        if (File.Exists(TargetScenePath))
        {
            bool replace = EditorUtility.DisplayDialog(
                "Ability Test Lab",
                "Ability Test Lab.unity already exists. Replace it with a fresh copy of Battle.unity?",
                "Replace",
                "Cancel");

            if (!replace)
                return;

            AssetDatabase.DeleteAsset(TargetScenePath);
        }

        string targetFolder = Path.GetDirectoryName(TargetScenePath);
        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        if (!AssetDatabase.CopyAsset(SourceScenePath, TargetScenePath))
        {
            EditorUtility.DisplayDialog("Ability Test Lab", "Failed to copy Battle.unity into Ability Test Lab.unity.", "OK");
            return;
        }

        AssetDatabase.Refresh();

        Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
        BattleController battleController = Object.FindObjectOfType<BattleController>();
        if (battleController == null)
        {
            EditorUtility.DisplayDialog("Ability Test Lab", "Copied the scene, but could not find a BattleController in it.", "OK");
            return;
        }

        AbilityTestLabMode labMode = battleController.GetComponent<AbilityTestLabMode>();
        if (labMode == null)
            labMode = battleController.gameObject.AddComponent<AbilityTestLabMode>();

        labMode.unitSlots = AbilityTestLabMode.DefaultSlots();
        labMode.includeEveryCatalogRecipeAbility = true;
        labMode.includeUncataloguedAbilityPrefabs = true;
        labMode.hideOriginalCatalogsOnTester = true;
        labMode.useAnalyzeTestingLayout = true;
        labMode.keepReinCatalogLimited = true;
        labMode.ensureAnalyzeTeacher = true;
        labMode.analyzeTeacherRecipeName = "Lucy";
        labMode.analyzeTeacherDisplayLabel = "Analyze Teacher - All Abilities";
        labMode.clearExistingUnits = true;
        labMode.addNeverEndingVictoryCondition = true;
        labMode.addAutoStatusController = true;
        labMode.selectFirstNonSkippingHero = true;

        LevelData defaultLevel = AssetDatabase.LoadAssetAtPath<LevelData>(DefaultLevelPath);
        if (defaultLevel != null)
            battleController.levelData = defaultLevel;

        EditorUtility.SetDirty(battleController);
        EditorUtility.SetDirty(labMode);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = battleController.gameObject;
        EditorGUIUtility.PingObject(battleController.gameObject);

        EditorUtility.DisplayDialog(
            "Ability Test Lab",
            "Created Assets/Scenes/Ability Test Lab.unity. Open it and press Play to test abilities, Analyze learning, status ticks, turn order, and hover highlighting.",
            "OK");
    }
}
#endif
