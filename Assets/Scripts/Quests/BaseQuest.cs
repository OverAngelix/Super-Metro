using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

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
    public List<Dialog> startDialogs;
    public List<Dialog> completeDialogs;

    public BaseQuest(string name, RawImage img, TextMeshProUGUI txt,
                 Func<string> getVal, Func<bool> completedCondition,
                 Texture2D check, Texture2D uncheck,
                 List<Dialog> startDialogsValue = null, List<Dialog> completeDialogsValue = null)
    {
        questName = name;
        checkImage = img;
        valueText = txt;

        getCurrentValue = getVal;
        isCompleted = completedCondition;

        checkTexture = check;
        uncheckTexture = uncheck;

        startDialogs = startDialogsValue ?? new List<Dialog>();
        completeDialogs = completeDialogsValue ?? new List<Dialog>();

    }

    public virtual void UpdateQuest()
    {
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

        ShowDialogSequence(startDialogs);
    }


    // Méthode virtuelle pour ajouter des conditions d’apparition
    public virtual bool CanBeAccepted()
    {
        return true; // Par défaut, toujours acceptée
    }

    // Peut être overriden dans les classes filles
    protected virtual void OnCompleted()
    {
        ShowDialogSequence(completeDialogs);
    }
    private void ShowDialogSequence(List<Dialog> dialogs)
    {
        if (dialogs == null || dialogs.Count == 0)
            return;

        DialogUIController dialogUIController = DialogUIController.Instance;
        dialogUIController.title = questName;
        dialogUIController.StartDialogSequence(dialogs);
    }

}
