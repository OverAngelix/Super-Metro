using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public abstract class BaseQuest
{
    // Nom de la quête
    public string questName;

    // UI
    public RawImage checkImage;
    public TextMeshProUGUI valueText;

    // Textures
    protected Texture2D checkTexture;
    protected Texture2D uncheckTexture;

    // État
    protected bool completed = false;
    private bool started = false;

    // Fonctions de progression
    protected Func<string> getCurrentValue;
    protected Func<bool> isCompleted;

    // Dialogues optionnels
    public string startDialog;
    public string completeDialog;

    public BaseQuest(string name, RawImage img, TextMeshProUGUI txt,
                 Func<string> getVal, Func<bool> completedCondition,
                 Texture2D check, Texture2D uncheck,
                 string startDialogValue = null, string completeDialogValue = null)
    {
        questName = name;
        checkImage = img;
        valueText = txt;

        getCurrentValue = getVal;
        isCompleted = completedCondition;

        checkTexture = check;
        uncheckTexture = uncheck;

        startDialog = startDialogValue;
        completeDialog = completeDialogValue;

    }

    public virtual void UpdateQuest() {
        UpdateQuestWithBool(completed);
    }

    public void UpdateQuestWithBool(bool currentCondition)
    {
        if (!started)
        {
            TryStartQuest();
            return;
        }

        valueText.text = getCurrentValue?.Invoke() ?? questName;

        if (!completed && currentCondition)
        {
            completed = true;
            checkImage.texture = checkTexture;
            Debug.Log($"Quête terminée : {questName}");
            OnCompleted();
        }
        else if (!completed)
        {
            checkImage.texture = uncheckTexture;
        }
    }


    public virtual void ForceComplete()
    {
        completed = true;
        checkImage.texture = checkTexture;
        Debug.Log($"{questName} complétée (forcée) !");
        OnCompleted();
    }

    // Appelé automatiquement dans UpdateQuest si la quête n’a pas commencé
    public virtual void TryStartQuest()
    {
        if (started || !CanBeAccepted())
            return;

        started = true;

        if (!string.IsNullOrEmpty(startDialog))
        {
            DialogUIController dialogUIController = DialogUIController.Instance;
            dialogUIController.gameObject.SetActive(true);
            dialogUIController.text = startDialog;
            dialogUIController.title = questName;
            dialogUIController.UpdateContent();
            Debug.Log($"[Dialogue début] {questName} : {startDialog}");
        }
    }

    // Méthode virtuelle pour ajouter des conditions d’apparition
    public virtual bool CanBeAccepted()
    {
        return true; // Par défaut, toujours acceptée
    }

    // Peut être overriden dans les classes filles
    protected virtual void OnCompleted()
    {   
        if (!string.IsNullOrEmpty(completeDialog))
        {
            DialogUIController dialogUIController = DialogUIController.Instance;
            dialogUIController.gameObject.SetActive(true);
            dialogUIController.text = completeDialog;
            dialogUIController.title = questName;
            dialogUIController.UpdateContent();
            Debug.Log($"[Dialogue fin] {questName} : {completeDialog}");
        }
    }
}
