using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CMD_DatabaseExtension
{
    public static void Extend(CommandDatabase database) { }

    public static CommandParameters ConvertDataToParameters(string[] data, int startingIndex = 0) => new CommandParameters(data, startingIndex);
}
