using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeUIController : MonoBehaviour
{
    public Button exitButton;
    public GameObject upgradeLineUI; // prefab pour chaque ligne
    public Transform upgradesContainer; // parent dans lequel on instancie les upgrades

    public static UpgradeUIController Instance;

    private TrainController currentTrain;
    private int trainIndex;
    private List<UpgradeLineUIController> upgradeLines = new List<UpgradeLineUIController>();
    void Awake()
    {
        Instance = this;
        exitButton.onClick.AddListener(Close);
        GetComponent<Canvas>().enabled = false; // fermé au départ
    }

    public void Open(TrainController train, int index)
    {
        currentTrain = train;
        trainIndex = index;

        GetComponent<Canvas>().enabled = true;
        RefreshUI();
    }

    public void Close()
    {
        GetComponent<Canvas>().enabled = false;
    }

    private void RefreshUI()
    {
        // Vider l'UI existante
        foreach (var line in upgradeLines)
            Destroy(line.gameObject);
        upgradeLines.Clear();

        // Générer les lignes dynamiquement
        Debug.Log(UpgradeManager.upgradesList);
        foreach (var kvp in UpgradeManager.upgradesList)
        {
            GameObject go = Instantiate(upgradeLineUI, upgradesContainer);
            var lineUI = go.GetComponent<UpgradeLineUIController>();
            lineUI.Init(kvp.Key, currentTrain);
            upgradeLines.Add(lineUI);
        }
    }

}
