using System; 
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class ConditionQuest : BaseQuest
{
    private Func<string> getCurrentValue;
    private Func<bool> isCompleted;

    public ConditionQuest(string name, RawImage img, TextMeshProUGUI txt,
                          Func<string> getVal, Func<bool> completed,
                          Texture2D check, Texture2D uncheck)
        : base(name, img, txt, check, uncheck)
    {
        getCurrentValue = getVal;
        isCompleted = completed;
    }

    public override void UpdateQuest()
    {
        valueText.text = getCurrentValue?.Invoke() ?? questName;
        if (isCompleted != null)
        {
            completed = isCompleted();
            checkImage.texture = completed ? checkTexture : uncheckTexture;
        }
    }
}
