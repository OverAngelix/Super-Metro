using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SuperGlobal : MonoBehaviour
{

    // FAIRE UI DES SPOTS
    // UI addStation responsive

    public static float money = 15000f;
    public static float ticketPrice = 1.8f;

    private static readonly bool isDebug = false;

    public static int nbUpgrade = 0;
    public static int nbStation = 4;

    public static int happinessCount = 100;
    public static List<float> peopleHappiness = Enumerable.Repeat(0.5f, happinessCount).ToList();

    public static float ComputeHappiness()
    {
        // if (peopleHappiness.Count == 0)
        // return 0.5f;

        int start = Mathf.Max(0, peopleHappiness.Count - happinessCount);

        var range = peopleHappiness.GetRange(start, peopleHappiness.Count - start);

        // float sum = 0f;
        // foreach (float h in range)
        //     sum += h;

        // float average = sum / range.Count;
        // return Mathf.Round(average * 100f) / 100f;
        return range.Any()
        ? peopleHappiness.Average()
        : 0f;
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
        // new TrainLine // Seconde ligne (Jaune)
        // {
        //     lineNumber = 2,
        //     maintenance = 250f,
        //     lineColor = Color.yellow,
        //     stations = new List<Station>(),
        //     trains = new List<TrainController>()
        // },
        // new TrainLine // Troisieme ligne (verte)
        // {
        //     lineNumber = 3,
        //     maintenance = 250f,
        //     lineColor = Color.green,
        //     stations = new List<Station>(),
        //     trains = new List<TrainController>()
        // },
    };

    public static void SetStations()
    {
        LoadData();
        foreach (TrainLine trainLine in trainLines) {
            trainLine.stations = stations.FindAll(s => s.line == trainLine.lineNumber);
        }
    }

    public static Station GetStation(string targetName)
    {
        Station station = null;
        foreach (TrainLine trainline in trainLines)
        {
            station = trainline.stations.Find(st => st.name == targetName && st.line == trainline.lineNumber);
            if (station != null)
                break;
        }
        return station;
    }

    public static Station GetStationByObj(GameObject obj)
    {
        Station station = null;
        foreach (TrainLine trainline in trainLines)
        {
            station = trainline.stations.Find(st => st.obj == obj);
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

    public static bool ExistStationByObj(GameObject obj)
    {
        foreach (TrainLine trainline in trainLines)
        {
            Station station = trainline.stations.Find(st => st.obj == obj);
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