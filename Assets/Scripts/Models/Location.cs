using UnityEngine;

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