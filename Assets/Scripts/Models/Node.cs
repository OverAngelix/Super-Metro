using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public string name;
    public float lat;
    public float lon;
    public bool isStation;
    public GameObject gameObject;
    public List<Edge> edges = new();

    public Node(string name, float lat, float lon, bool isStation, GameObject gameObject)
    {
        this.name = name;
        this.lat = lat;
        this.lon = lon;
        this.isStation = isStation;
        this.gameObject = gameObject;
    }
    
    public Node() { }

}
