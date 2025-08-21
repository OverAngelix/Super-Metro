using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("UI Prefab")]
    public GameObject questLineUIPrefab;
    public Transform questsParentTransform;

    [Header("Textures")]
    public Texture2D checkTexture;
    public Texture2D uncheckTexture;
    public bool skipTutorial;
    public List<BaseQuest> quests = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeQuests();
    }

    public BaseQuest GetQuestByName(string name)
    {
        return quests.Find(q => q.questName == name);
    }

    void InitializeQuests()
    {

        /////////Exemple d'une quête "appuyez sur le menu d'amélioration"//////////////////////////

        if (!skipTutorial)
        {
            StationListInformationUIController stationListInformationUIController = StationListInformationUIController.Instance;
            Button upgradeBtn = stationListInformationUIController.ticketUIControllers[0].upgradeButton;
            AddButtonClickQuest(
                "Cliquez sur le menu d'amélioration",
                upgradeBtn,
                new List<Dialog>() { new Dialog("", "Bonjour, il faut que tu améliores"), new Dialog("", "Pour ça, clique sur le menu dans le ticket rouge en haut à gauche.") },
                new List<Dialog>() { new Dialog("", "Bravo tu as amélioré") }
            );

            /////////////////////////////


            // AddConditionQuest(
            //     "Bonheur",
            //     () => "Bonheur : " + (SuperGlobal.ComputeHappiness() * 100).ToString("F1") + " / 80",
            //     () => (SuperGlobal.ComputeHappiness() * 100) >= 80 && SuperGlobal.peopleHappiness.Count > 100
            // );

            // AddConditionQuest(
            //     "Argent",
            //     () => "Argent : " + SuperGlobal.money.ToString("F1") + " / 5000",
            //     () => SuperGlobal.money >= 5000
            // );

            int nbUpgradesObjectif = 10;
            AddConditionQuest(
                "Améliorations",
                () => "Améliorations : " + SuperGlobal.nbUpgrade + " / " + nbUpgradesObjectif,
                () => SuperGlobal.nbUpgrade >= nbUpgradesObjectif,
                new List<Dialog>() { new Dialog("", "Test debut quete") },
                new List<Dialog>() { new Dialog("", "Test fin quete") },
                () => GetQuestByName("Cliquez sur le menu d'amélioration").completed == true
            );

            // int nbStationsObjectif = 6;
            // AddQuest(
            //     "Stations",
            //     () => "Nouvelles Stations : " + SuperGlobal.nbStation + " / " + nbStationsObjectif,
            //     () => SuperGlobal.nbStation >= nbStationsObjectif
            // );

            // Tu peux ajouter ici les 4 autres quêtes dynamiquement de la même façon
        }
    }

    public void AddConditionQuest(
        string name,
        Func<string> getCurrentValue,
        Func<bool> isCompleted,
        List<Dialog> startDialogs = null,
        List<Dialog> completeDialogs = null,
        Func<bool> activationCondition = null
        )
    {
        BaseQuest quest = new ConditionQuest(name,
                                            getCurrentValue, isCompleted,
                                            checkTexture, uncheckTexture, startDialogs, completeDialogs, activationCondition);
        quests.Add(quest);
    }

    public void AddButtonClickQuest(
        string name, Button button,
        List<Dialog> startDialogs = null,
        List<Dialog> completeDialogs = null,
        Func<bool> activationCondition = null
        )
    {
        BaseQuest quest = new ButtonClickQuest(name,
                                            button,
                                            checkTexture, uncheckTexture, startDialogs, completeDialogs, activationCondition);
        quests.Add(quest);
    }

    private void Update()
    {
        foreach (var quest in quests)
        {
            quest.TryActivate();
            quest.UpdateQuest();
        }
    }
}
