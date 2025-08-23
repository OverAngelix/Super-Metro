using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StationTextManager : MonoBehaviour
{
    public TextMeshProUGUI stationName;
    public TextMeshProUGUI stationNameUI;
    private Station station;

    void Start()
    {
        StationController stationControllerscript = GetComponent<StationController>();
        station = stationControllerscript.station;
        stationName.text = station.name;
        stationNameUI.text = station.name;
    }

    private void OnMouseDown()
    {
        UpgradeStationUIController.Instance.Open(station);
    }
}
