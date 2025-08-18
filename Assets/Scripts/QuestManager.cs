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

    public List<Quest> quests = new List<Quest>();

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
        AddQuest(
            "Cliquez sur le menu d'amélioration",
            () => "Cliquez sur le menu d'amélioration", // getCurrentValue
            () => false                                // isCompleted (sera géré via trigger)
        );
        StationListInformationUIController stationListInformationUIController = StationListInformationUIController.Instance;
        Button upgradeBtn = stationListInformationUIController.ticketUIControllers[0].upgradeButton;
        // Crée un UnityAction local pour pouvoir l’enlever après usage
        UnityEngine.Events.UnityAction action = null;
        action = () =>
        {
            Debug.Log("coucou");
            upgradeBtn.onClick.RemoveListener(action); // retire le listener après un clic
        };

        // Ajoute le listener au bouton
        upgradeBtn.onClick.AddListener(action);

        /////////////////////////////


        // AddQuest(
        //     "Bonheur",
        //     () => "Bonheur : " + (SuperGlobal.computeHappiness() * 100).ToString("F1") + " / 80",
        //     () => (SuperGlobal.computeHappiness() * 100) >= 80 && SuperGlobal.peopleHappiness.Count > 100
        // );

        // AddQuest(
        //     "Argent",
        //     () => "Argent : " + SuperGlobal.money.ToString("F1") + " / 5000",
        //     () => SuperGlobal.money >= 5000
        // );

        // int nbUpgradesObjectif = 10;
        // AddQuest(
        //     "Améliorations",
        //     () => "Améliorations : " + SuperGlobal.nbUpgrade + " / " + nbUpgradesObjectif,
        //     () => SuperGlobal.nbUpgrade >= nbUpgradesObjectif
        // );

        // int nbStationsObjectif = 6;
        // AddQuest(
        //     "Stations",
        //     () => "Nouvelles Stations : " + SuperGlobal.nbStation + " / " + nbStationsObjectif,
        //     () => SuperGlobal.nbStation >= nbStationsObjectif
        // );

        // Tu peux ajouter ici les 4 autres quêtes dynamiquement de la même façon
    }

    public void AddQuest(string name, Func<string> getCurrentValue, Func<bool> isCompleted)
    {
        // Instancie la ligne UI
        GameObject go = Instantiate(questLineUIPrefab, questsParent);
        QuestLineUIController qlUIController = go.GetComponent<QuestLineUIController>();

        // Crée la quête
        Quest newQuest = new Quest(
            name,
            qlUIController.checkImage,
            qlUIController.valueText,
            getCurrentValue,
            isCompleted,
            checkTexture,
            uncheckTexture
        );

        quests.Add(newQuest);
    }

    private void Update()
    {
        foreach (var quest in quests)
        {
            quest.UpdateQuest();
        }
    }
}
