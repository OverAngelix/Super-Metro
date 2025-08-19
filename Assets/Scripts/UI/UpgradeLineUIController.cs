using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeLineUIController : MonoBehaviour
{

    public Button button;
    public string upgradeName;
    public TMP_Text upgradeTextObject;
    public int lineNumber = 1; // temporaire
    public int trainIndex = 0; // temporaire
    void Start()
    {
        button.onClick.AddListener(upgrade);
        TMP_Text text = upgradeTextObject;
        switch (upgradeName)
        {
            case "speed":
                text.text = "Vitesse de la rame";
                break;
            case "maxPassengers":
                text.text = "Capacité maximale de la rame";
                break;
            default:
                break;
        }
    }

    void Update()
    {

    }

    public void upgrade()
    {
        float price = 0f;
        switch (upgradeName)
        {
            case "speed":
                price = 100f;
                if (SuperGlobal.money - price >= 0)
                {
                    SuperGlobal.trainlines[lineNumber - 1].trains[trainIndex].speed += 1f;
                    SuperGlobal.money -= price;
                    SuperGlobal.nbUpgrade += 1;

                }
                ;
                break;
            case "maxPassengers":
                price = 50f;
                if (SuperGlobal.money - price >= 0)
                {
                    SuperGlobal.trainlines[lineNumber - 1].trains[trainIndex].maxPassengers += 1;
                    SuperGlobal.money -= price;
                    SuperGlobal.nbUpgrade += 1;

                }
                ;
                break;
            default:
                break;
        }
    }
}
