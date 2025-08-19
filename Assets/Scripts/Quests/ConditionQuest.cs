using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ConditionQuest : BaseQuest
{
    public ConditionQuest(
        string name,
        RawImage img,
        TextMeshProUGUI txt,
        Func<string> getVal,
        Func<bool> completed,
        Texture2D check,
        Texture2D uncheck,
        List<Dialog> startDialogs = null,
        List<Dialog> completeDialogs = null,
        Func<bool> activationCondition = null
        )
        : base(name, img, txt, getVal, completed, check, uncheck, startDialogs, completeDialogs, activationCondition)
    {
    }

    public override void UpdateQuest()
    {
        // Ici on évalue la condition réelle de complétion
        bool currentCondition = isCompleted?.Invoke() ?? false;
        UpdateQuestWithBool(currentCondition);
    }

}
