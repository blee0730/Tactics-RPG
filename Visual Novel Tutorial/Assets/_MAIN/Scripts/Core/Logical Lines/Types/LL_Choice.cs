using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static LogicalLineUtils.Encapsulation;

public class LL_Choice : ILogicalLine
{
    public string keyword => "choice";
    private const char CHOICE_IDENTIFIER = '-';

    public IEnumerator Execute(DIALOGUE_LINE line)
    {
        var currentConversation = DialogueSystem.instance.conversationManager.conversation;
        var progress = DialogueSystem.instance.conversationManager.conversationProgress;
        EncapsulatedData data = RipEncapsulationData(currentConversation, progress, ripHeaderAndEncapsulators: true, parentStartingIndex: currentConversation.fileStartIndex);
        List<Choice> choices = GetChoicesFromData(data);

        string title = line.dialogueData.rawData;
        ChoicePanel panel = ChoicePanel.instance;
        string[] choiceTitles = choices.Select(c => c.title).ToArray();

        panel.Show(title, choiceTitles);

        while(panel.isWaitingOnUserChoice)
            yield return null;

        Choice selectedChoice = choices[panel.lastDescision.answerIndex];

        Conversation newConversation = new Conversation(selectedChoice.resultLines, file: currentConversation.file, fileStartIndex: selectedChoice.startIndex, fileEndIndex: selectedChoice.endIndex);
        DialogueSystem.instance.conversationManager.conversation.SetProgress(data.endingIndex - currentConversation.fileStartIndex);
        DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);

        AutoReader autoReader = DialogueSystem.instance.autoReader;
        if(autoReader != null && autoReader.isOn && autoReader.skip)
        {
            if(VN_Configuration.activeConfig != null && !VN_Configuration.activeConfig.continueSkippingAfterChoice)
                autoReader.Disable();
        }
    }

    public bool Matches(DIALOGUE_LINE line)
    {
        return(line.hasSpeaker && line.speakerData.name.ToLower() == keyword);
    }

    private List<Choice> GetChoicesFromData(EncapsulatedData data)
    {
        List<Choice> choices = new List<Choice>();
        int encapsulationDepth = 0;
        bool isFirstChoice = true;

        Choice choice = new Choice
        {
            title = string.Empty,
            resultLines = new List<string>(),
        };

        int choiceIndex = 0, i = 0;
        for(i = 1; i < data.lines.Count; i++)
        {
            var line = data.lines[i];
            if(IsChoiceStart(line) && encapsulationDepth == 1)
            {
                if(!isFirstChoice)
                {
                    choice.startIndex = data.startingIndex + (choiceIndex + 1);
                    choice.endIndex = data.startingIndex + (i - 1);
                    choices.Add(choice);
                    choice = new Choice
                    {
                        title = string.Empty,
                        resultLines = new List<string>(),
                    };
                }

                choiceIndex = i;
                choice.title = line.Trim().Substring(1);
                isFirstChoice = false;
                continue;
            }

            AddLineToResults(line, ref choice, ref encapsulationDepth);
        }

        if(!choices.Contains(choice))
        {
            choice.startIndex = data.startingIndex + (choiceIndex + 1);
            choice.endIndex = data.startingIndex + (i - 2);
            choices.Add(choice);
        }

        return choices;
    }

    private void AddLineToResults(string line, ref Choice choice, ref int encapsulationDepth)
    {
        line.Trim();

        if(IsEncapsulationStart(line))
        {
            if(encapsulationDepth > 0)
                choice.resultLines.Add(line);

            encapsulationDepth++;
            return;
        }

        if(IsEncapsulationEnd(line))
        {
            encapsulationDepth--;

            if(encapsulationDepth > 0)
                choice.resultLines.Add(line);

            return;
        }

        choice.resultLines.Add(line);
    }
    private bool IsChoiceStart(string line) => line.Trim().StartsWith(CHOICE_IDENTIFIER);

    private struct Choice
    {
        public string title;
        public List<string> resultLines;
        public int startIndex;
        public int endIndex;
    }
}
