using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeLineUIController : MonoBehaviour
{
    public Button button;
    public TMP_Text upgradeTextObject;

    private string upgradeName;
    private TrainController train;

    // Appelée depuis l'UpgradeUIController pour initialiser la ligne
    public void Init(string upgradeName, TrainController train)
    {
        this.upgradeName = upgradeName;
        this.train = train;

        // Mettre à jour le texte en fonction du type d'upgrade
        switch (upgradeName)
        {
            case "speed":
                upgradeTextObject.text = $"Vitesse ({train.speed}) - Prix : {UpgradeManager.upgradesList[upgradeName].Price}";
                break;
            case "maxPassengers":
                upgradeTextObject.text = $"Capacité ({train.maxPassengers}) - Prix : {UpgradeManager.upgradesList[upgradeName].Price}";
                break;
            default:
                upgradeTextObject.text = upgradeName;
                break;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        UpgradeManager.Upgrade(upgradeName, train);
        // Mettre à jour le texte après upgrade
        Init(upgradeName, train);
    }
}
