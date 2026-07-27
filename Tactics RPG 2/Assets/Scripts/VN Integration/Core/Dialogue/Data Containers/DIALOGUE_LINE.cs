using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DIALOGUE_LINE
{
    public string rawData {get; private set;} = string.Empty;
    public DL_SPEAKER_DATA speakerData;
    public DL_DIALOGUE_DATA dialogueData;
    public DL_COMMAND_DATA commandData;

    public bool hasSpeaker => speakerData != null;//speakerData != string.Empty;
    public bool hasDialogue => dialogueData != null;
    public bool hasCommands => commandData != null;

    public DIALOGUE_LINE(string rawLine, string speakerData, string dialogueData, string commandData)
    {
        rawData = rawLine;
        this.speakerData = (string.IsNullOrWhiteSpace(speakerData) ? null : new DL_SPEAKER_DATA(speakerData));
        this.dialogueData = (string.IsNullOrWhiteSpace(dialogueData) ? null : new DL_DIALOGUE_DATA(dialogueData));
        this.commandData = (string.IsNullOrWhiteSpace(commandData) ? null : new DL_COMMAND_DATA(commandData));
    }
}
