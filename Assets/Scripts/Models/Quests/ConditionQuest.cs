using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ConditionQuest : BaseQuest
{
    public ConditionQuest(
        string name,
        Func<string> getVal,
        Func<bool> completed,
        Texture2D check,
        Texture2D uncheck,
        List<Dialog> startDialogs = null,
        List<Dialog> completeDialogs = null,
        Func<bool> activationCondition = null
        )
        : base(name, getVal, completed, check, uncheck, startDialogs, completeDialogs, activationCondition)
    {
    }

    public override void UpdateQuest()
    {
        bool currentCondition = isCompleted?.Invoke() ?? false;
        UpdateQuestWithBool(currentCondition);
    }

}
