using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Add to BattleController GameObject.
// Connects CutSceneState → VN DialogueSystem + CharacterManager + AudioManager.
// Characters defined in CharacterConfigSO are shown as 2D sprites during cutscenes.

public class TacticsCutSceneBridge : MonoBehaviour
{
    [Header("Optional cutscene music (leave blank to keep battle music)")]
    public string cutSceneMusicPath = "";

    System.Action onComplete;
    Coroutine runningCutScene;
    TacticsAudioBridge audioBridge;

    void Awake()
    {
        audioBridge = GetComponent<TacticsAudioBridge>();
    }

    // ── Public API called by CutSceneState ───────────────────────────────────

    public void StartCutScene(string conversationFile, System.Action onComplete)
    {
        this.onComplete = onComplete;
        if (runningCutScene != null) StopCoroutine(runningCutScene);
        runningCutScene = StartCoroutine(RunCutScene(conversationFile));
    }

    public void Advance()
    {
        DialogueSystem.instance?.OnUserPrompt_Next();
    }

    public void StopCutScene()
    {
        if (runningCutScene != null)
        {
            StopCoroutine(runningCutScene);
            runningCutScene = null;
        }
        CleanupCutScene(immediate: true);
    }

    // ── Coroutine ────────────────────────────────────────────────────────────

    IEnumerator RunCutScene(string conversationFile)
    {
        if (DialogueSystem.instance == null)
        {
            Debug.LogError("[TacticsCutSceneBridge] DialogueSystem not found in scene.");
            onComplete?.Invoke();
            yield break;
        }

        List<string> lines = FileManager.ReadTextAsset(conversationFile);
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning($"[TacticsCutSceneBridge] Empty or missing: {conversationFile}");
            onComplete?.Invoke();
            yield break;
        }

        // Start cutscene music if configured
        if (!string.IsNullOrEmpty(cutSceneMusicPath) && audioBridge != null)
            audioBridge.StartCutSceneMusic(cutSceneMusicPath);

        // Show VN canvas
        yield return DialogueSystem.instance.Show(immediate: true);

        // Run conversation — characters are shown/hidden by commands in the .txt file
        Conversation conversation = new Conversation(lines, file: conversationFile);
        yield return DialogueSystem.instance.Say(conversation);

        // Hide VN canvas and all characters
        CleanupCutScene(immediate: false);
        yield return new WaitForSeconds(0.5f); // allow fade out

        runningCutScene = null;
        onComplete?.Invoke();
    }

    void CleanupCutScene(bool immediate)
    {
        // Hide all characters that were shown during the cutscene
        if (CharacterManager.instance != null)
        {
            foreach (var character in CharacterManager.instance.allCharacters)
            {
                if (character.isVisible)
                    character.Hide();
            }
        }

        // Restore battle music if we changed it
        if (!string.IsNullOrEmpty(cutSceneMusicPath) && audioBridge != null)
            audioBridge.EndCutSceneMusic();

        DialogueSystem.instance?.Hide(immediate: immediate);
    }
}
