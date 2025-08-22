using UnityEngine;
using System.Collections.Generic;

public class TrainLineManager : MonoBehaviour
{
    public static void TryCreateTrainLine(string name)
    {
        int cost = 1000;

        if (SuperGlobal.money >= cost)
        {
            SuperGlobal.money -= cost;
            TrainLine newLine = new TrainLine {
            lineNumber = SuperGlobal.trainLines.Count + 1,
            maintenance = 250f,
            lineColor = Color.red,
            stations = new List<Station>(),
            trains = new List<TrainController>()
            };
            SuperGlobal.trainLines.Add(newLine);

        }
        else
        {
            SuperGlobal.Log("Pas assez d'argent pour créer une ligne !");
        }
    }
}