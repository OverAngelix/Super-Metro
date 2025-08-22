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
    public StationsListItem stationListItemPrefab; 
    public TMP_Text trainLineNameText;
    private List<StationsListItem> stationItems = new();

    public void RefreshUI()
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

    void OnEditStation(Station station)
    {
        Debug.Log("Edit station : " + station.name);

        // Ici tu peux ouvrir une UI d’édition pour modifier le nom, l’index etc.
        // Par exemple un pop-up avec un TMP_InputField et un bouton Save.
    }
}
