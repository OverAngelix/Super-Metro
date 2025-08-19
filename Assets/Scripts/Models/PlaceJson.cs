[System.Serializable]
public class PlaceJson
{
    public string name;
    public float lat;
    public float lon;
    public int zoom;
}

[System.Serializable]
public class PlacesJson
{
    public PlaceJson[] lieux;
}