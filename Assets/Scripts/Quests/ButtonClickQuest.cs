using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class ButtonClickQuest : BaseQuest
{
    private Button button;
    private bool clicked = false;
    private UnityAction action;

    public ButtonClickQuest(string name, RawImage img, TextMeshProUGUI txt,
                            Button btn,
                            Texture2D check, Texture2D uncheck, List<Dialog> startDialogs = null, List<Dialog> completeDialogs = null)
        : base(name, img, txt, null, null, check, uncheck, startDialogs, completeDialogs)
    {
        button = btn;

        // Création de l'action
        action = () =>
        {
            clicked = true;
            button.onClick.RemoveListener(action); // on retire le listener
        };

        button.onClick.AddListener(action);
    }

    public override void UpdateQuest()
    {
        UpdateQuestWithBool(clicked);
    }
}
