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

    private IEnumerator Start()
    {
        yield return null; // pour skip la premier frame de l'update (le temps que tout soit initialisé)
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
            MainUIController mainUIController = MainUIController.Instance;
            Button trainLineEditButton = mainUIController.trainLineEditButton;
            Debug.Log("aze0");
            AddButtonClickQuest(
                "Ouvre le menu de configuration de lignes",
                trainLineEditButton,
                new List<Dialog>() {
                    new Dialog("", "Bonjour, bienvenue dans Super Métro, tu t'apprètes à vivre une super aventure."),
                    new Dialog("", "Ici, notre but, c'est de remplacer toutes les voitures par les métros, et de faciliter la vie des gens."),
                    new Dialog("", "Nous possédons déjà une ligne de métro, achète donc une nouvelle rame afin de la rendre active. Clique ici pour ouvrir le menu de configuration des lignes.")
                },
                new List<Dialog>() {
                    new Dialog("", "Bravo.") }
            );
            Debug.Log("aze1");
            TrainLineEditUIController trainLineEditController = TrainLineEditUIController.Instance;
            Debug.Log("aze " + trainLineEditController.ToString());
            Button addTrainButton = trainLineEditController.addTrainButton;
            AddButtonClickQuest(
                "Achète une nouvelle rame",
                addTrainButton,
                new List<Dialog>() {
                    new Dialog("", "Parfait, maintenant cliques sur le bouton pour acheter un nouveau train.")
                },
                new List<Dialog>() {
                    new Dialog("", "Regarde, tu as un train qui fonctionne, c'est super !.")
                },
                () => GetQuestByName("Ouvre le menu de configuration de lignes").completed == true
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

            // int nbUpgradesObjectif = 10;
            // AddConditionQuest(
            //     "Améliorations",
            //     () => "Améliorations : " + SuperGlobal.nbUpgrade + " / " + nbUpgradesObjectif,
            //     () => SuperGlobal.nbUpgrade >= nbUpgradesObjectif,
            //     new List<Dialog>() { new Dialog("", "Test debut quete") },
            //     new List<Dialog>() { new Dialog("", "Test fin quete") },
            //     () => GetQuestByName("Cliquez sur le menu d'amélioration").completed == true
            // );

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
