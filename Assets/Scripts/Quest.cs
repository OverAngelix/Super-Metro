using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Quest
{
    public string questName;
    public RawImage checkImage;
    public TextMeshProUGUI valueText;

    // Fonction qui renvoie une string pour afficher l'état (facultatif)
    public Func<string> getCurrentValue;

    // Fonction qui détermine si la quête est complétée
    public Func<bool> isCompleted;
    public bool completed = false;

    // Textures check/uncheck
    private Texture2D checkTexture;
    private Texture2D uncheckTexture;

    public Quest(string name, RawImage img, TextMeshProUGUI txt,
                 Func<string> getVal, Func<bool> completed,
                 Texture2D check, Texture2D uncheck)
    {
        questName = name;
        checkImage = img;
        valueText = txt;
        getCurrentValue = getVal;
        isCompleted = completed;
        checkTexture = check;
        uncheckTexture = uncheck;
    }

   public void UpdateQuest()
{
    Debug.Log($"Updating quest {questName}");
    
    if (getCurrentValue != null)
    {
        var val = getCurrentValue();
        Debug.Log($"getCurrentValue returned: {val}");
        valueText.text = val; // questName n'est pas encore utilisée pour l'instant
    }
    else
        valueText.text = questName;

    if (isCompleted != null)
    {
        var completedStatus = isCompleted();
        Debug.Log($"isCompleted returned: {completedStatus}");
        checkImage.texture = completedStatus ? checkTexture : uncheckTexture;
    }
    else
    {
        checkImage.texture = uncheckTexture;
    }
}


    public void ForceComplete()
    {
        completed = true;
        Debug.Log($"{questName} complétée !");
    }

}
