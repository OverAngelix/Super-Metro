using System.Collections.Generic;
using UnityEngine;

public class TrainLine
{
    public int lineNumber;
    public float maintenance;
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

}