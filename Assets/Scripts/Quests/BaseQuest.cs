using UnityEngine.UI;
using TMPro;
using UnityEngine;

public abstract class BaseQuest
{
    public string questName;
    public RawImage checkImage;
    public TextMeshProUGUI valueText;
    protected Texture2D checkTexture;
    protected Texture2D uncheckTexture;
    protected bool completed;

    public BaseQuest(string name, RawImage img, TextMeshProUGUI txt,
                     Texture2D check, Texture2D uncheck)
    {
        questName = name;
        checkImage = img;
        valueText = txt;
        checkTexture = check;
        uncheckTexture = uncheck;
    }

    public abstract void UpdateQuest();

    public virtual void ForceComplete()
    {
        completed = true;
        checkImage.texture = checkTexture;
        Debug.Log($"{questName} complétée !");
    }
}
