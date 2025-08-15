using UnityEngine;
using System.Collections.Generic;

public class SuperGlobal : MonoBehaviour
{
    public static float timeSpeed = 5f;
    public static float money = 100f;
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

    public class Station : Location
    {
        public int lineNumber;
        public int index;
        public List<PersonController> waitingPeople = new List<PersonController>();


        public Station(string name, float lat, float lon, int lineNumber, int index)
            : base(name, lat, lon)
        {
            this.lineNumber = lineNumber;
            this.index = index;

        }
    }


public static List<Location> spots = new List<Location>
    {
     new Location("Parc de la Canteraine", 50.6234f, 3.0295f),
     new Location("Place du Général de Gaulle", 50.6336f, 3.0659f),
     new Location("Cathédrale Notre-Dame de la Treille", 50.6397f, 3.0632f),
     new Location("Parc Barbieux", 50.6881f, 3.1675f),
     new Location("Musée d'Art et d'Industrie André Diligent", 50.6944f, 3.1681f),
     new Location("LaM - Lille Métropole Musée d'art moderne", 50.6443f, 3.1289f),
     new Location("Villa Cavrois", 50.6886f, 3.1611f),
     new Location("Vélodrome de Roubaix", 50.6941f, 3.1744f),
     new Location("Port de Wambrechies", 50.6825f, 3.1806f),
     new Location("Lidl Wasquehal", 50.6782699f, 3.1309419f),
     new Location("Lidl Wavrin", 50.56581f, 2.9261697f),
     new Location("Kinepolis Lomme", 50.6526513f, 2.9800662f),
    };
    public static List<Station> stations = new List<Station>
    {
        new Station("Gare de Wavrin", 50.574449f, 2.9341066f, 1, 0),
        new Station("Portes des Postes", 50.618803f, 3.0475242f, 1, 1),
        new Station("Gare Lille Flandres", 50.638047f, 3.0700097f, 1, 2),
        new Station("Wasquehal Hôtel de Ville", 50.6697318f, 3.1264094f, 1, 3),
    };
}
