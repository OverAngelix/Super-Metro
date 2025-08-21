using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeLineUIController : MonoBehaviour
{
    public Button button;
    public TMP_Text upgradeTextObject;

    private string upgradeName;
    private TrainController train;
    public StationListInformationUIController stationListInformationUIController;

    // Appelée depuis l'UpgradeUIController pour initialiser la ligne
    public void Init(string upgradeName, TrainController train)
    {
        this.upgradeName = upgradeName;
        this.train = train;

        // Mettre à jour le texte en fonction du type d'upgrade
        upgradeTextObject.text = upgradeName switch
        {
            "speed" => $"Vitesse ({train.speed}) - Prix : {UpgradeManager.upgradesList[upgradeName].price}",
            "maxPassengers" => $"Capacité ({train.maxPassengers}) - Prix : {UpgradeManager.upgradesList[upgradeName].price}",
            _ => upgradeName,
        };
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        train.Upgrade(upgradeName);
        // Mettre à jour le texte après upgrade
        Init(upgradeName, train);
        // Mettre à jour les ticketUI sur le côté
        stationListInformationUIController.RefreshUI();
    }
}
