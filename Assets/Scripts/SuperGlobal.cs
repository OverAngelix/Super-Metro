using UnityEngine;
using System.Collections.Generic;

public class SuperGlobal : MonoBehaviour
{
    public static float timeSpeed = 5f;
    public static float money = 1500f;
    public static float ticketPrice = 1.8f;
    public static float fees = 250f;

    public static int nbUpgrade = 0;
    public static int nbStation = 4;

    public static List<float> peopleHappiness = new List<float>();

    public static float computeHappiness()
    {
        if (peopleHappiness.Count == 0)
            return 0.5f;

        float sum = 0f;
        int count = 100;
        int start = Mathf.Max(0, peopleHappiness.Count - count);
        foreach (float h in peopleHappiness.GetRange(start, peopleHappiness.Count - start))
            sum += h;

        return sum / count;
    }


    public static bool isUIOpen = false;

    public static List<Line> lines = new List<Line>
    {
        new Line("Super ligne", 1)
    };

    public static List<Location> spots = new List<Location>
    {
     new Location("Parc de la Canteraine", 50.6234f, 3.0295f),
     new Location("Place du Général de Gaulle", 50.6336f, 3.0659f),
     new Location("Cathédrale Notre-Dame de la Treille", 50.6397f, 3.0632f),
     new Location("Parc Barbieux", 50.6881f, 3.1675f),
     new Location("Musée d'Art et d'Industrie André Diligent", 50.6944f, 3.1681f),
     new Location("LaM - Lille Métropole Musée d'art moderne", 50.6443f, 3.1289f),
     new Location("Villa Cavrois", 50.6886f, 3.1611f),
     new Location("Vélodrome de Roubaix", 50.6941f, 3.1744f),
     new Location("Port de Wambrechies", 50.6825f, 3.1806f),
     new Location("Lidl Wasquehal", 50.6782699f, 3.1309419f),
     new Location("Lidl Wavrin", 50.56581f, 2.9261697f),
     new Location("Kinepolis Lomme", 50.6526513f, 2.9800662f),
    };
    public static List<Station> stations = new List<Station>
    {
        new Station("Gare de Wavrin", 50.574449f, 2.9341066f, 1, 0),
        new Station("Portes des Postes", 50.618803f, 3.0475242f, 1, 1),
        new Station("Gare Lille Flandres", 50.638047f, 3.0700097f, 1, 2),
        new Station("Wasquehal Hôtel de Ville", 50.6697318f, 3.1264094f, 1, 3),
    };


    public static GameObject upgradeUI;

    public static void upgradeTrain(int lineNumber, int trainIndex)
    {
        upgradeUI = GameObject.Find("UpgradeUI");
        upgradeUI.GetComponent<Canvas>().enabled = true;
        UpgradeUIController upgradeUIController = upgradeUI.GetComponent<UpgradeUIController>();
        upgradeUIController.updateUI(lineNumber, trainIndex);
        Debug.Log("upgrade train");

    }


}