using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StationTextManager : MonoBehaviour
{
    public TextMeshProUGUI stationName;
    public TextMeshProUGUI stationNameUI;
    public TextMeshProUGUI niveauUI;
    public TextMeshProUGUI personnesEnAttenteUI;
    public TextMeshProUGUI CapaciteMaxUI;

    public Canvas CanvasUI;

    public Button closeButtonUI;
    public Button upgradeButtonUI;

    void Start()
    {
        StationController stationControllerscript = GetComponent<StationController>();
        SuperGlobal.Station station = stationControllerscript.station;
        stationName.text = station.name;
        stationNameUI.text = station.name;
        niveauUI.text = "Niveau " + "2";
        personnesEnAttenteUI.text = "Personne en attente : " + "50";
        CapaciteMaxUI.text = "Capacité maximum : " + "100";
        closeButtonUI.onClick.AddListener(closeUI);
    }

    private void OnMouseDown()
    {
        CanvasUI.enabled = true;
    }

    void closeUI()
    {
        CanvasUI.enabled = false;
    }

}
