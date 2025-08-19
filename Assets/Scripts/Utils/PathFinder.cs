using System.Collections.Generic;
using UnityEngine;

public static class Dijkstra
{
    // Calcule la distance entre deux coordonnées lat/lon
    public static float Distance(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371f; // rayon de la Terre en km
        float dLat = Mathf.Deg2Rad * (lat2 - lat1);
        float dLon = Mathf.Deg2Rad * (lon2 - lon1);
        float a = Mathf.Sin(dLat/2) * Mathf.Sin(dLat/2) +
                  Mathf.Cos(Mathf.Deg2Rad * lat1) * Mathf.Cos(Mathf.Deg2Rad * lat2) *
                  Mathf.Sin(dLon/2) * Mathf.Sin(dLon/2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1-a));
        return R * c; // distance en km
    }

    public static List<Node> FindShortestPath(Node start, Node end, List<Node> allNodes)
    {
        var distances = new Dictionary<Node, float>();
        var previous = new Dictionary<Node, Node>();
        var unvisited = new List<Node>(allNodes);

        foreach(var node in allNodes)
            distances[node] = float.MaxValue;

        distances[start] = 0;

        while(unvisited.Count > 0)
        {
            // prendre le noeud avec la distance minimale
            unvisited.Sort((a,b) => distances[a].CompareTo(distances[b]));
            Node current = unvisited[0];
            unvisited.RemoveAt(0);

            if(current == end)
                break;

            foreach(var edge in current.edges)
            {
                float alt = distances[current] + edge.cost;
                if(alt < distances[edge.to])
                {
                    distances[edge.to] = alt;
                    previous[edge.to] = current;
                }
            }
        }

        // reconstruire le chemin
        var path = new List<Node>();
        Node temp = end;
        while(temp != null)
        {
            path.Insert(0, temp);
            previous.TryGetValue(temp, out temp);
        }
        return path;
    }
}
