using System;
using System.Collections.Generic;

[Serializable]
public class Station : Location
{
    public int line;
    public int index;
    public int level;
    public int capacity;
    public List<PersonController> waitingPeople = new();

    public Station(string name, float lat, float lon, int line, int index)
        : base(name, lat, lon)
    {
        this.line = line;
        this.index = index;
        this.level = 1;
        this.capacity = 50;
    }
}

[Serializable]
public class StationList
{
    public List<Station> stations;
}