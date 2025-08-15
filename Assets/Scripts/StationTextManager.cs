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

    private SuperGlobal.Station station;


    void Start()
    {
        StationController stationControllerscript = GetComponent<StationController>();
        station = stationControllerscript.station;
        stationName.text = station.name;
        stationNameUI.text = station.name;
        closeButtonUI.onClick.AddListener(closeUI);
        upgradeButtonUI.onClick.AddListener(upgradeStation);
    }

    void Update()
    {
        niveauUI.text = "Niveau " + station.level;
        personnesEnAttenteUI.text = "Personne en attente : " + station.waitingPeople.Count;
        CapaciteMaxUI.text = "Capacité maximum : " + station.capacity;
    }

    private void OnMouseDown()
    {
        CanvasUI.enabled = true;
    }
    
    #region ACTIONS BUTTONS
    void closeUI()
    {
        CanvasUI.enabled = false;
    }

    void upgradeStation()
    {
        if (SuperGlobal.money - 150 >= 0)
        {
            station.capacity += 50;
            station.level += 1;
            SuperGlobal.money -= 150;
        }
    }
    #endregion
}
