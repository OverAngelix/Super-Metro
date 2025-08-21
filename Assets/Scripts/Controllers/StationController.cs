using UnityEngine;
using System.Collections.Generic;

public class StationController : MonoBehaviour
{
    public Station station;
    public GameObject trainPrefab;

    void Start()
    {
        if (station.index == 0)
        {
            RunTrain();
        }
    }

    private GameObject RunTrain()
    {
        GameObject obj = Instantiate(trainPrefab);
        obj.transform.position = station.obj.transform.position;
        obj.transform.rotation = Quaternion.Euler(270, 90, 0);

        TrainLine trainLine = SuperGlobal.GetTrainLineOfLineStation(station.line);
        TrainController train = UpdateTrainPath(obj, trainLine);
        if (trainLine.lineNumber == 2)
        {
            train.speed = 100f;
        }
        trainLine.trains.Add(train);

        return obj;
    }

    public TrainController UpdateTrainPath(GameObject obj, TrainLine trainLine)
    {
        // Filtrer les stations de la même ligne et créer des Nodes
        List<Node> path = new();
        foreach (var st in trainLine.stations)
        {
            if (st.line == station.line)
                path.Add(new Node { name = st.name });
        }
        // Donner le chemin au train
        TrainController train = obj.GetComponent<TrainController>();
        train.SetPath(path);
        return train;
    }

}
