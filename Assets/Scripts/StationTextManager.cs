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
        stationName.text = gameObject.name;
        stationNameUI.text = gameObject.name;
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
