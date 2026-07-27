using UnityEngine;

// Handles VN system input during cutscenes using Unity's built-in Input class.
// No New Input System package required.
//
// Active keybindings (change the KeyCode values below to match your preferences):
//   Next dialogue      — Space, Return, or left mouse click
//   History back       — Left Bracket [ or Page Up
//   History forward    — Right Bracket ] or Page Down
//   Toggle history log — Tab or H

public class PlayerInputManager : MonoBehaviour
{
    [Header("Next Dialogue")]
    public KeyCode nextKey1    = KeyCode.Space;
    public KeyCode nextKey2    = KeyCode.Return;

    [Header("History Navigation")]
    public KeyCode historyBackKey    = KeyCode.LeftBracket;
    public KeyCode historyForwardKey = KeyCode.RightBracket;
    public KeyCode historyLogKey     = KeyCode.Tab;

    // Set to false to temporarily disable input (e.g. during battle state transitions)
    [HideInInspector] public bool inputEnabled = true;

    void Update()
    {
        if (!inputEnabled) return;

        // Next dialogue line
        if (Input.GetKeyDown(nextKey1)
            || Input.GetKeyDown(nextKey2)
            || Input.GetMouseButtonDown(0))
        {
            OnNext();
        }

        // History navigation
        if (Input.GetKeyDown(historyBackKey))
            OnHistoryBack();

        if (Input.GetKeyDown(historyForwardKey))
            OnHistoryForward();

        if (Input.GetKeyDown(historyLogKey))
            OnHistoryToggleLog();
    }

    // ── Actions ──────────────────────────────────────────────────────────────

    void OnNext()
    {
        if (DialogueSystem.instance != null)
            DialogueSystem.instance.OnUserPrompt_Next();
    }

    void OnHistoryBack()
    {
        if (HistoryManager.instance != null)
            HistoryManager.instance.GoBack();
    }

    void OnHistoryForward()
    {
        if (HistoryManager.instance != null)
            HistoryManager.instance.GoForward();
    }

    void OnHistoryToggleLog()
    {
        if (HistoryManager.instance == null) return;

        var logs = HistoryManager.instance.logManager;
        if (!logs.isOpen)
            logs.Open();
        else
            logs.Close();
    }
}
