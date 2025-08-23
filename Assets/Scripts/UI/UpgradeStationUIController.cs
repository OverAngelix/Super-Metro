using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class UpgradeStationUIController : MonoBehaviour
{
    [Header("Station liée")]
    [SerializeField] private Station station;

    [Header("Références UI")]
    [SerializeField] private TMP_Text nameTextObject;
    [SerializeField] private TMP_Text levelTextObject;
    [SerializeField] private TMP_Text peopleTextObject;
    [SerializeField] private TMP_Text capacityTextObject;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Canvas canvas;

    [Header("Paramètres Upgrade")]
    [SerializeField] private int upgradeCost = 150;
    [SerializeField] private int capacityIncrease = 50;

    public static UpgradeStationUIController Instance;
    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        closeButton.onClick.AddListener(Close);
        upgradeButton.onClick.AddListener(UpgradeStation);
        Close();
    }

    private void RefreshUI()
    {
        if (station == null) return;

        nameTextObject.text = station.name;
        levelTextObject.text = $"Niveau {station.level}";
        peopleTextObject.text = $"Personnes en attente : {station.waitingPeople.Count}";
        capacityTextObject.text = $"Capacité maximum : {station.capacity}";
    }

    public void Open(Station station)
    {
        if (canvas != null)
        {
            SetStation(station);
            canvas.enabled = true;
            RefreshUI();
        }
    }
    private void Close()
    {
        if (canvas != null)
            canvas.enabled = false;
    }

    // TODO : Enlever cette logique des prix ici et créeé des upgrades pour chaque type d'objet
    private void UpgradeStation()
    {
        if (SuperGlobal.money < upgradeCost) return;

        station.capacity += capacityIncrease;
        station.level++;
        SuperGlobal.money -= upgradeCost;
        SuperGlobal.nbUpgrade++;

        RefreshUI();
    }

    public void SetStation(Station station)
    {
        this.station = station;
    }
}
