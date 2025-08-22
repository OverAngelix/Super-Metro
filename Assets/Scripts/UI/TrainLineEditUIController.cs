using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainLineEditUIController : MonoBehaviour
{
    [Header("Data")]

    public int currentTrainLineIndex = 0;
    public TrainLine trainLine;

    [Header("UI References")]
    public Transform stationListContainer;
    public Transform trainListContainer;
    public StationsListItem stationListItemPrefab;
    public TrainListItem trainListItemPrefab;
    public TMP_Text trainLineNameText;
    private List<StationsListItem> stationItems = new();
    private List<TrainListItem> trainItems = new();

    public void RefreshUI()
    {
        RefreshStations();
        RefreshTrains();
    }
    public void RefreshStations()
    {
        // Clear ancienne liste
        foreach (var item in stationItems)
        {
            Destroy(item.gameObject);
        }
        stationItems.Clear();
        if (trainLine == null)
        {
            trainLine = SuperGlobal.trainLines[currentTrainLineIndex];
        }
        // Nom de la ligne
        if (trainLineNameText != null)
            // trainLineNameText.text = trainLine.name;

            // Création des items pour chaque station
            for (int i = 0; i < trainLine.stations.Count; i++)
            {
                Station station = trainLine.stations[i];

                StationsListItem item = Instantiate(stationListItemPrefab, stationListContainer);
                item.station = station;
                item.indexTextObject.text = station.index.ToString();
                item.nameTextObject.text = station.name;

                // On connecte l’action d’édition
                item.editButton.onClick.RemoveAllListeners();
                item.editButton.onClick.AddListener(() => OnEditStation(item.station));

                stationItems.Add(item);
            }
    }

    public void RefreshTrains()
    {
        // Clear ancienne liste
        foreach (var item in trainItems)
        {
            Destroy(item.gameObject);
        }
        trainItems.Clear();
        if (trainLine == null)
        {
            trainLine = SuperGlobal.trainLines[currentTrainLineIndex];
        }
        // Nom de la ligne
        if (trainLineNameText != null)
            // trainLineNameText.text = trainLine.name;

            // Création des items pour chaque train
            for (int i = 0; i < trainLine.trains.Count; i++)
            {
                TrainController train = trainLine.trains[i];

                TrainListItem item = Instantiate(trainListItemPrefab, trainListContainer);
                item.train = train;
                item.indexTextObject.text = i.ToString();

                // On connecte l’action d’édition
                item.editButton.onClick.RemoveAllListeners();
                item.editButton.onClick.AddListener(() => OnEditTrain(item.train));

                trainItems.Add(item);
            }
    }

    void OnEditStation(Station station)
    {
        Debug.Log("Edit station : " + station.name);
    }
    
     void OnEditTrain(TrainController train)
    {
        Debug.Log("Edit train");
    }
}
