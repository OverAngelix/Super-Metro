using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("UI Prefab")]
    public GameObject questLineUIPrefab; // ton QuestLineUI
    public Transform questsParent;        // parent où les lignes vont être créées

    [Header("Textures")]
    public Texture2D checkTexture;
    public Texture2D uncheckTexture;

    public List<BaseQuest> quests = new List<BaseQuest>();

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return null; // attendre 1 frame, c'est une "arnaque" pour que les tickets soient générés pour la quete du menu d'amélioration, il faudra faire autrement quand on aura le tuto
        InitializeQuests();
    }

    void InitializeQuests()
    {

        /////////Exemple d'une quête "appuyez sur le menu d'amélioration"//////////////////////////
        
        StationListInformationUIController stationListInformationUIController = StationListInformationUIController.Instance;
        Button upgradeBtn = stationListInformationUIController.ticketUIControllers[0].upgradeButton;
        AddButtonClickQuest(
            "Cliquez sur le menu d'amélioration",
            upgradeBtn
        );

        /////////////////////////////


        AddConditionQuest(
            "Bonheur",
            () => "Bonheur : " + (SuperGlobal.computeHappiness() * 100).ToString("F1") + " / 80",
            () => (SuperGlobal.computeHappiness() * 100) >= 80 && SuperGlobal.peopleHappiness.Count > 100
        );

        // AddQuest(
        //     "Argent",
        //     () => "Argent : " + SuperGlobal.money.ToString("F1") + " / 5000",
        //     () => SuperGlobal.money >= 5000
        // );

        int nbUpgradesObjectif = 10;
        AddConditionQuest(
            "Améliorations",
            () => "Améliorations : " + SuperGlobal.nbUpgrade + " / " + nbUpgradesObjectif,
            () => SuperGlobal.nbUpgrade >= nbUpgradesObjectif
        );

        // int nbStationsObjectif = 6;
        // AddQuest(
        //     "Stations",
        //     () => "Nouvelles Stations : " + SuperGlobal.nbStation + " / " + nbStationsObjectif,
        //     () => SuperGlobal.nbStation >= nbStationsObjectif
        // );

        // Tu peux ajouter ici les 4 autres quêtes dynamiquement de la même façon
    }

    // DEPRECATED
    // public void AddQuest(string name, Func<string> getCurrentValue, Func<bool> isCompleted)
    // {
    //     // Instancie la ligne UI
    //     GameObject go = Instantiate(questLineUIPrefab, questsParent);
    //     QuestLineUIController qlUIController = go.GetComponent<QuestLineUIController>();

    //     // Crée la quête
    //     Quest newQuest = new BaseQuest(
    //         name,
    //         qlUIController.checkImage,
    //         qlUIController.valueText,
    //         getCurrentValue,
    //         isCompleted,
    //         checkTexture,
    //         uncheckTexture
    //     );

    //     quests.Add(newQuest);
    // }

    public void AddConditionQuest(string name, Func<string> getCurrentValue, Func<bool> isCompleted)
    {
        GameObject go = Instantiate(questLineUIPrefab, questsParent);
        QuestLineUIController ql = go.GetComponent<QuestLineUIController>();

        BaseQuest quest = new ConditionQuest(name, ql.checkImage, ql.valueText,
                                            getCurrentValue, isCompleted,
                                            checkTexture, uncheckTexture);
        quests.Add(quest);
    }

    public void AddButtonClickQuest(string name, Button button)
    {
        GameObject go = Instantiate(questLineUIPrefab, questsParent);
        QuestLineUIController ql = go.GetComponent<QuestLineUIController>();

        BaseQuest quest = new ClickButtonQuest(name, ql.checkImage, ql.valueText,
                                            button,
                                            checkTexture, uncheckTexture);
        quests.Add(quest);
    }


    private void Update()
    {
        foreach (var quest in quests)
        {
            quest.UpdateQuest();
        }
    }
}
