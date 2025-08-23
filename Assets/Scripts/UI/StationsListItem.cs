using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationsListItem : MonoBehaviour
{

    public TMP_Text indexTextObject;
    public TMP_Text nameTextObject;
    public Button editButton;
    public Station station;

    void Start()
    {
        editButton.onClick.AddListener(OpenEditStationUI);
        indexTextObject.text = station.index.ToString();
        nameTextObject.text = station.name;
    }

    private void OpenEditStationUI()
    {
        UpgradeStationUIController.Instance.Open(station);
    }
}
