using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConditionQuest : BaseQuest
{
    public ConditionQuest(string name, RawImage img, TextMeshProUGUI txt,
                          Func<string> getVal, Func<bool> completed,
                          Texture2D check, Texture2D uncheck, string startDialog = null, string completeDialog = null)
        : base(name, img, txt, getVal, completed, check, uncheck, startDialog, completeDialog)
    {
    }
}
