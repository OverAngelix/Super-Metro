using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System;
public class ButtonClickQuest : BaseQuest
{
    private Button button;
    private bool clicked = false;
    private UnityAction action;

    public ButtonClickQuest(
        string name,
        Button btn,
        Texture2D check,
        Texture2D uncheck,
        List<Dialog> startDialogs = null,
        List<Dialog> completeDialogs = null,
        Func<bool> activationCondition = null
        )
        : base(name, null, null, check, uncheck, startDialogs, completeDialogs, activationCondition)
    {
        button = btn;

        action = () =>
        {
            clicked = true;
            button.onClick.RemoveListener(action);
        };

        button.onClick.AddListener(action);
    }

    public override void UpdateQuest()
    {
        UpdateQuestWithBool(clicked);
    }
}
