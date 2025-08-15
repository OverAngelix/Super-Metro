using UnityEngine;
using System.Collections.Generic;

public class SpawnLocations : MonoBehaviour
{
    public GameObject locationPrefab;
    public GameObject stationPrefab;

    private float multiplier = 100f;
    private float refLat = 50.6710514f;
    private float refLon = 3.0242675f;

    void Awake()
    {
        // Instancier toutes les locations
        for (int i = 0; i < SuperGlobal.spots.Count; i++)
        {
            var spot = SuperGlobal.spots[i];
            GameObject obj = Instantiate(locationPrefab);
            obj.transform.position = new Vector3((spot.lat - refLat) * multiplier, 0, (spot.lon - refLon) * multiplier);
            spot.obj = obj;
        }

        // Instancier toutes les stations et les stocker
        List<GameObject> stationObjects = new List<GameObject>();
        for (int i = 0; i < SuperGlobal.stations.Count; i++)
        {
            var sta = SuperGlobal.stations[i];
            GameObject obj = Instantiate(stationPrefab);
            obj.transform.position = new Vector3((sta.lat - refLat) * multiplier, 0, (sta.lon - refLon) * multiplier);
            stationObjects.Add(obj);
            sta.obj = obj;
        }

        // Relier les stations
        ConnectStations connect = FindFirstObjectByType<ConnectStations>();
        connect.CreateLines(stationObjects);
    }
}
