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
    public bool completed = false;
    private bool started = false;

    // Fonctions de progression
    protected Func<string> getCurrentValue;
    protected Func<bool> isCompleted;

    // Dialogues optionnels
    public List<Dialog> startDialogs;
    public List<Dialog> completeDialogs;
    public Func<bool> canActivate; 
    public bool IsActive { get; private set; } = false;
    public GameObject questLineUI;
    public BaseQuest(
        string name,
        RawImage img,
        TextMeshProUGUI txt,
        Func<string> getVal,
        Func<bool> completedCondition,
        Texture2D check,
        Texture2D uncheck,
        List<Dialog> startDialogsValue = null,
        List<Dialog> completeDialogsValue = null,
        Func<bool> activationCondition = null
        )
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
        canActivate = activationCondition ?? (() => true); 
    }

    public override string ToString()
    {
        string value = getCurrentValue != null ? getCurrentValue() : "N/A";
        return $"[BaseQuest] Name: {questName}, Completed: {isCompleted}, Value: {value}";
    }

    public virtual void UpdateQuest()
    {
        UpdateQuestWithBool(completed);
    }
    
    public void TryActivate()
    {
        if (!IsActive && canActivate())
        {
            IsActive = true;
            CreateUI();
            OnActivated();
        }
    }

    private void CreateUI()
    {
        if (questLineUI != null) return;

        GameObject go = GameObject.Instantiate(QuestManager.Instance.questLineUIPrefab, QuestManager.Instance.questsParent);
        QuestLineUIController ql = go.GetComponent<QuestLineUIController>();
        checkImage = ql.checkImage;
        valueText = ql.valueText;

        questLineUI = go;
        UpdateQuest();
    }

    protected virtual void OnActivated()
    {
        Debug.Log($"Quête {questName} activée !");
    }

    public void UpdateQuestWithBool(bool currentCondition)
    {
        if (!IsActive) return;
        if (!started)
        {
            TryStartQuest();
            return;
        }

        if (valueText == null || checkTexture == null) return;

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
        dialogUIController.EnqueueDialogSequence(dialogs, questName);
    }


}
