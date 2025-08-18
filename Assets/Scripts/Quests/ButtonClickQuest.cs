using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class ClickButtonQuest : BaseQuest
{
    private Button button;
    private bool clicked = false;
    private UnityAction action;

    public ClickButtonQuest(string name, RawImage img, TextMeshProUGUI txt,
                            Button btn,
                            Texture2D check, Texture2D uncheck)
        : base(name, img, txt, check, uncheck)
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
        valueText.text = questName;
        if (clicked)
        {
            completed = true;
            checkImage.texture = checkTexture;
        }
        else
        {
            checkImage.texture = uncheckTexture;
        }
    }
}
