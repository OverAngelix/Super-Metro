using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public abstract class BaseQuest
{
    public string questName;

    public RawImage checkImage;
    public TextMeshProUGUI valueText;

    protected Texture2D checkTexture;
    protected Texture2D uncheckTexture;

    public bool completed = false;
    private bool started = false;

    protected Func<string> getCurrentValue;
    protected Func<bool> isCompleted;

    public List<Dialog> startDialogs;
    public List<Dialog> completeDialogs;
    public Func<bool> canActivate; 
    public bool IsActive { get; private set; } = false;
    public GameObject questLineUI;
    public BaseQuest(
        string name,
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
        GameObject go = GameObject.Instantiate(QuestManager.Instance.questLineUIPrefab, QuestManager.Instance.questsParentTransform);
        QuestLineUIController ql = go.GetComponent<QuestLineUIController>();
        checkImage = ql.checkImage;
        valueText = ql.valueText;

        questLineUI = go;
        UpdateQuest();
    }

    protected virtual void OnActivated()
    {
        SuperGlobal.Log($"Quête {questName} activée !");
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
            SuperGlobal.Log($"Quête terminée : {questName}");
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
        SuperGlobal.Log($"{questName} complétée (forcée) !");
        OnCompleted();
    }

    public virtual void TryStartQuest()
    {
        if (started || !CanBeAccepted())
            return;

        started = true;

        ShowDialogSequence(startDialogs);
    }


    public virtual bool CanBeAccepted()
    {
        return true;
    }

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
