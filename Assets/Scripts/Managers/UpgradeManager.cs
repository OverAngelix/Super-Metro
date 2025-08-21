using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static Dictionary<string, Upgrade> upgradesList = new Dictionary<string, Upgrade>
        {
            { "maxPassengers", new Upgrade("maxPassengers", 50f, t => t.maxPassengers += 1) },
            { "speed", new Upgrade("speed", 100f, t => t.speed += 1f) }
        };

    public static void Upgrade(string upgradeName, TrainController train)
    {
        if (!upgradesList.ContainsKey(upgradeName))
        {
            Debug.LogWarning($"Upgrade {upgradeName} does not exist.");
            return;
        }

        Upgrade upgrade = upgradesList[upgradeName];

        if (SuperGlobal.money < upgrade.price)
        {
            SuperGlobal.Log("Not enough money!");
            return;
        }

        // Appliquer l’upgrade
        upgrade.apply(train);

        // Mettre à jour l’argent et stats globales
        SuperGlobal.money -= upgrade.price;
        SuperGlobal.nbUpgrade++;
    }
}