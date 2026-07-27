using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// CutSceneState now uses TacticsCutSceneBridge to play cutscenes
// through the VN DialogueSystem instead of the old ConversationController.
//
// Conversation files live at: Assets/Resources/Conversations/
// Format: plain .txt files using the VN dialogue syntax:
//   Rein "So this is the battlefield..."
//   Rosemary "Stay focused. They'll be on us soon."
//
// See the VN tutorial's dialogue format guide for the full syntax.

public class CutSceneState : BattleState
{
    TacticsCutSceneBridge bridge;

    protected override void Awake()
    {
        base.Awake();
        bridge = owner.GetComponentInChildren<TacticsCutSceneBridge>();

        if (bridge == null)
            Debug.LogError("[CutSceneState] TacticsCutSceneBridge not found on BattleController. " +
                           "Add it as a component.");
    }

    public override void Enter()
    {
        base.Enter();

        string conversationFile;

        if (IsBattleOver())
        {
            conversationFile = DidPlayerWin()
                ? "Conversations/OutroSceneWin"
                : "Conversations/OutroSceneLose";
        }
        else
        {
            conversationFile = "Conversations/IntroScene";
        }

        if (bridge != null)
            bridge.StartCutScene(conversationFile, OnCutSceneComplete);
        else
            OnCutSceneComplete(); // fallback — skip cutscene entirely
    }

    public override void Exit()
    {
        base.Exit();
        if (bridge != null)
            bridge.StopCutScene();
    }

    protected override void AddListeners()
    {
        base.AddListeners();
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();
    }

    // Confirm button press — advance dialogue in VN system
    protected override void OnFire(object sender, InfoEventArgs<int> e)
    {
        base.OnFire(sender, e);
        if (bridge != null)
            bridge.Advance();
    }

    void OnCutSceneComplete()
    {
        if (IsBattleOver())
            owner.ChangeState<EndBattleState>();
        else
            owner.ChangeState<SelectUnitState>();
    }
}
