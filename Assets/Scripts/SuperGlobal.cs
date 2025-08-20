using UnityEngine;
using System.Collections.Generic;

public class SuperGlobal : MonoBehaviour
{

    // FAIRE UI DES SPOTS
    // UI addStation responsive

    public static float money = 15000f;
    public static float ticketPrice = 1.8f;

    private static readonly bool isDebug = true;

    public static int nbUpgrade = 0;
    public static int nbStation = 4;

    public static List<float> peopleHappiness = new();

    public static float ComputeHappiness()
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

    public static List<Location> spots = new();
    public static List<Station> stations = new();
    public static List<TrainLine> trainLines = new()
    {
        new TrainLine // Première ligne (Rouge)
        {
            lineNumber = 1,
            maintenance = 250f,
            lineColor = Color.red,
            stations = new List<Station>(),
            trains = new List<TrainController>()
        },
        new TrainLine // Seconde ligne (Jaune)
        {
            lineNumber = 2,
            maintenance = 250f,
            lineColor = Color.yellow,
            stations = new List<Station>(),
            trains = new List<TrainController>()
        },
    };

    public static void SetStations()
    {
        LoadData();
        trainLines[0].stations = stations.FindAll(s => s.line == 1);
        trainLines[1].stations = stations.FindAll(s => s.line == 2);
    }

    public static Station GetStation(string targetName)
    {
        Station station = null;
        foreach (TrainLine trainline in trainLines)
        {
            station = trainline.stations.Find(st => st.name == targetName);
            if (station != null)
                break;
        }
        return station;
    }

    public static TrainLine GetTrainLineOfLineStation(int line)
    {
        foreach (TrainLine trainline in trainLines)
        {
            Station station = trainline.stations.Find(st => st.line == line);
            if (station != null)
            {
                return trainline;
            }
        }
        return null;
    }

    public static bool ExistStation(string targetName)
    {
        foreach (TrainLine trainline in trainLines)
        {
            Station station = trainline.stations.Find(st => st.name == targetName);
            if (station != null)
            {
                return true;
            }

        }
        return false;
    }

    public static void Log(string text)
    {
        if (isDebug)
        {
            Debug.Log(text);
        }
    }

    public static void LoadData()
    {
        // Charger les stations
        TextAsset stationsJsonFile = Resources.Load<TextAsset>("JSON/stations");
        if (stationsJsonFile != null)
        {
            StationList stationListWrapper = JsonUtility.FromJson<StationList>(stationsJsonFile.text);
            stations = stationListWrapper.stations;
        }

        // Charger les spots
        TextAsset spotsJsonFile = Resources.Load<TextAsset>("JSON/spots");
        if (spotsJsonFile != null)
        {
            LocationList spotListWrapper = JsonUtility.FromJson<LocationList>(spotsJsonFile.text);
            spots = spotListWrapper.spots;
        }
    }

}