using UnityEngine;
using System.Collections.Generic;

public class SpawnPersons : MonoBehaviour
{
    public GameObject personPrefab;
    public float spawnInterval = 60f;
    private float timer = 0f;

    // private void Start() {
        // SpawnPerson();
    // }
    void Update()
    {
        timer += Time.deltaTime * SuperGlobal.timeSpeed;
        if (timer >= spawnInterval)
        {
            SpawnPerson();
            timer = 0f;
        }
    }

    private void SpawnPerson()
    {
        if (SuperGlobal.spots.Count < 2) return;

        // Choisir un point de départ aléatoire
        int startIndex = Random.Range(0, SuperGlobal.spots.Count);
        var startSpot = SuperGlobal.spots[startIndex];

        // Choisir une destination différente
        int endIndex;
        do
        {
            endIndex = Random.Range(0, SuperGlobal.spots.Count);
        } while (endIndex == startIndex);
        var endSpot = SuperGlobal.spots[endIndex];

        // Construire le graphe des nodes
        List<Node> allNodes = new List<Node>();

        // Ajouter tous les spots comme nodes
        foreach (var spot in SuperGlobal.spots)
            allNodes.Add(new Node(spot.name, spot.lat, spot.lon, false));

        // Ajouter toutes les stations comme nodes
        foreach (var station in SuperGlobal.stations)
            allNodes.Add(new Node(station.name, station.lat, station.lon, true));

        // Créer les edges entre tous les nodes
        foreach (var node in allNodes)
        {
            foreach (var other in allNodes)
            {
                if (node == other) continue;

                float dist = Dijkstra.Distance(node.lat, node.lon, other.lat, other.lon);
                float speedMultiplier = (node.isStation && other.isStation) ? 0.25f : 1f; // métro ×4 plus rapide
                node.edges.Add(new Edge(other, dist * speedMultiplier));
            }
        }

        Node startNode = allNodes.Find(n => n.name == startSpot.name);
        Node endNode = allNodes.Find(n => n.name == endSpot.name);

        // Calculer le chemin le plus rapide
        List<Node> path = Dijkstra.FindShortestPath(startNode, endNode, allNodes);

        // Instancier le personnage au départ
        GameObject newPerson = Instantiate(personPrefab, startSpot.obj.transform.position, Quaternion.identity);
        PersonController controller = newPerson.GetComponent<PersonController>();

        // Passer le chemin au contrôleur
        controller.SetPath(path);
        //Debug.Log($"Personne spawnée de {startSpot.name} à {endSpot.name}");
    }
}
