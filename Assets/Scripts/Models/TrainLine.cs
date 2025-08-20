using System.Collections.Generic;
using UnityEngine;

public class TrainLine
{
    public int lineNumber;
    public float maintenance;
    public float stationPrice;
    public Color lineColor;
    public List<Station> stations;
    public List<TrainController> trains;
    public GameObject ticketUIObject;

    public int NumberStations()
    {
        return stations.Count;
    }

    public int NumberTrains()
    {
        return trains.Count;
    }

    public Station AddStation(string name, float lat, float lon)
    {
        if (SuperGlobal.money - stationPrice >= 0 && !string.IsNullOrEmpty(name))
        {
            Station newStation = new(name, lat, lon, lineNumber, NumberStations());
            stations.Add(newStation);
            return newStation;
        }
        else if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Le nom de la station ne peut pas être vide !");
        }
        else
        {
            SuperGlobal.Log("No such money ! ");
        }
        return null;

    }

}