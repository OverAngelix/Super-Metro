using UnityEngine;
using System.Collections.Generic;

public class StationController : MonoBehaviour
{
    public SuperGlobal.Station station;
    private GameObject trainPrefab;

    void Awake()
    {
        trainPrefab = Resources.Load<GameObject>("Prefabs/Train");
    }
    void Start()
    {
        if (station.index == 0)
        {
            RunTrain();
        }
    }

    // void Update()
    // {
    //     if (station.index == 1)
    //     {
    //     }
    // }

    private GameObject RunTrain()
    {
        GameObject obj = Instantiate(trainPrefab);
        obj.transform.position = station.obj.transform.position;
        obj.transform.rotation = Quaternion.Euler(270, 90, 0);

        TrainController train = UpdateTrainPath(obj);

        SuperGlobal.lines[station.lineNumber - 1].trainsList.Add(train);

        return obj;
    }

    public TrainController UpdateTrainPath(GameObject obj)
    {
        // Filtrer les stations de la même ligne et créer des Nodes
        List<Node> path = new List<Node>();
        foreach (var st in SuperGlobal.stations)
        {
            if (st.lineNumber == station.lineNumber)
                path.Add(new Node { name = st.name });
        }

        // // Pour que le train fasse un aller-retour infini
        // List<Node> fullPath = new List<Node>(path);
        // path.Reverse();
        // fullPath.AddRange(path);

        // Donner le chemin au train
        TrainController train = obj.GetComponent<TrainController>();
        train.SetPath(path);

        return train;
    }


}
