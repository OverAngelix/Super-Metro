using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public RawImage happinessCheck;
    public TextMeshProUGUI happinessValue;
    public RawImage moneyCheck;
    public TextMeshProUGUI moneyValue;
    public RawImage upgradeCheck;
    public TextMeshProUGUI upgradeValue;
    public int nbUpgradesObjectif = 10;
    public RawImage stationCheck;
    public TextMeshProUGUI stationValue;
    public int nbStationsObjectif = 6;


    public Texture2D checkTexture;
    public Texture2D uncheckTexture;
    private void Start()
    {
        happinessCheck.texture = uncheckTexture;
        moneyCheck.texture = uncheckTexture;
        upgradeCheck.texture = uncheckTexture;
        stationCheck.texture = uncheckTexture;
        happinessValue.text = "Bonheur : " + (SuperGlobal.computeHappiness() * 100).ToString("F1") + " / 80";
        moneyValue.text = "Argent : " + SuperGlobal.money + " / 5000";
        upgradeValue.text = "Ameliorations : " + SuperGlobal.nbUpgrade + " / " + nbUpgradesObjectif;
        stationValue.text = "Nouvelles Stations : " + SuperGlobal.nbStation + " / " + nbStationsObjectif;


    }

    // Update is called once per frame
    void Update()
    {
        happinessCheck.texture = uncheckTexture;
        moneyCheck.texture = uncheckTexture;
        upgradeCheck.texture = uncheckTexture;
        stationCheck.texture = uncheckTexture;
        happinessValue.text = "Bonheur : " + (SuperGlobal.computeHappiness() * 100).ToString("F1") + " / 80";
        moneyValue.text = "Argent : " + SuperGlobal.money.ToString("F1") + " / 5000";
        upgradeValue.text = "Ameliorations : " + SuperGlobal.nbUpgrade + " / " + nbUpgradesObjectif;
        stationValue.text = "Nouvelles Stations : " + SuperGlobal.nbStation + " / " + nbStationsObjectif;


        if ((SuperGlobal.computeHappiness() * 100) == 80 && SuperGlobal.peopleHappiness.Count > 100)
        {
            happinessCheck.texture = checkTexture;
        }

        if (SuperGlobal.money >= 5000)
        {
            moneyCheck.texture = checkTexture;
        }

        if (SuperGlobal.nbUpgrade >= nbUpgradesObjectif)
        {
            upgradeCheck.texture = checkTexture;
        }

        if (SuperGlobal.nbStation >= nbStationsObjectif)
        {
            stationCheck.texture = checkTexture;
        }

        if ((SuperGlobal.computeHappiness() * 100) == 80 && SuperGlobal.peopleHappiness.Count > 100 && SuperGlobal.money >= 5000 && SuperGlobal.nbUpgrade >= nbUpgradesObjectif && SuperGlobal.nbStation >= nbStationsObjectif)
        {
            Debug.Log("BRAVO TU AS REUSSI ! ");
        }
    }
}
