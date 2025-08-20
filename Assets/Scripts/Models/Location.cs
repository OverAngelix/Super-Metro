using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Location
{
    public string name;
    public float lat;
    public float lon;
    public GameObject obj;

    public Location(string name, float lat, float lon)
    {
        this.name = name;
        this.lat = lat;
        this.lon = lon;
    }
}

[Serializable]
public class LocationList
{
    public List<Location> spots;
}