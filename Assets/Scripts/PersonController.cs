using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PersonController : MonoBehaviour
{
    public float speed = 1f;
    private List<Node> path;
    private int currentTargetIndex = 0;

    private bool onTrain = false;
    private TrainController currentTrain;
    private bool happinessSent = false;

    private Node targetStationNode; // la station où la personne doit descendre

    // Variables pour le système de happiness
    private float travelTime = 0f;       // temps passé en mouvement
    private float waitTime = 0f;         // temps passé à attendre aux stations
    private float happiness = 1f;        // entre 0 et 1

    private void Start() { }

    public void SetPath(List<Node> newPath)
    {
        path = newPath;
        currentTargetIndex = 0;

        if (path.Count > 0)
        {
            string firstName = path[0].name;
            GameObject firstObj = SuperGlobal.spots.Find(s => s.name == firstName)?.obj
                                ?? SuperGlobal.stations.Find(st => st.name == firstName)?.obj;
            if (firstObj != null)
                transform.position = firstObj.transform.position;
        }
    }

    public Node GetNextTarget()
    {
        if (path != null && currentTargetIndex < path.Count - 1)
            return path[currentTargetIndex + 1];
        return null;
    }

    void Update()
    {
        // Si la personne est dans le train
        if (onTrain)
        {
            travelTime += Time.deltaTime; 

            // 🔹 Vérifier happiness même en train
            float currentHappiness = CalculateHappiness(travelTime, waitTime);
            if (currentHappiness <= 0f && !happinessSent)
            {
                SuperGlobal.peopleHappiness.Add(0f);
                happinessSent = true;
            }

            // Vérifier si elle doit descendre
            if (currentTrain != null && targetStationNode != null && currentTrain.currentStation?.name == targetStationNode.name)
            {
                LeaveTrain(targetStationNode);
            }
            return;
        }

        // Pas dans le train : marcher ou attendre
        if (path == null || currentTargetIndex >= path.Count) return;

        string targetName = path[currentTargetIndex].name;
        GameObject targetObj = SuperGlobal.spots.Find(s => s.name == targetName)?.obj
                            ?? SuperGlobal.stations.Find(st => st.name == targetName)?.obj;

        if (targetObj == null) return;

        // Vérifier si le node est une station
        SuperGlobal.Station station = SuperGlobal.stations.Find(st => st.name == targetName);
        if (station != null)
        {
            // Arrivé à la station
            transform.position = targetObj.transform.position;

            if (!station.waitingPeople.Contains(this))
                station.waitingPeople.Add(this);

            waitTime += Time.deltaTime;

            // 🔹 Vérifier happiness en temps réel
            float currentHappiness = CalculateHappiness(travelTime, waitTime);
            if (currentHappiness <= 0f && !happinessSent)
            {
                SuperGlobal.peopleHappiness.Add(0f);
                happinessSent = true;
            }

            return; // stop mouvement tant qu'on attend le train
        }

        // Mouvement vers le prochain node
        float currentSpeed = speed;

        if (currentTargetIndex > 0)
        {
            string prevName = path[currentTargetIndex - 1].name;
            bool prevIsStation = SuperGlobal.stations.Exists(st => st.name == prevName);
            bool targetIsStation = station != null;
            if (prevIsStation && targetIsStation) currentSpeed *= 4f;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetObj.transform.position, Time.deltaTime * currentSpeed);

        // Compter le temps de déplacement
        travelTime += Time.deltaTime;

        // 🔹 Vérifier happiness en temps réel même en marchant
        float h = CalculateHappiness(travelTime, waitTime);
        if (h <= 0f)
        {
            SuperGlobal.peopleHappiness.Add(0f);
            Destroy(gameObject);
            return;
        }

        if (Vector3.Distance(transform.position, targetObj.transform.position) < 0.1f)
        {
            currentTargetIndex++;
            if (currentTargetIndex >= path.Count && !happinessSent)
            {
                happiness = CalculateHappiness(travelTime, waitTime);
                SuperGlobal.peopleHappiness.Add(happiness);
                happinessSent = true;
            }
        }
    }

    public void BoardTrain(TrainController train, Node stationNode)
    {
        currentTrain = train;
        targetStationNode = stationNode;
        onTrain = true;
        GetComponent<Renderer>().enabled = false;
    }

    public void LeaveTrain(Node stationNode)
    {
        if (stationNode == null) return;

        GameObject stationObj = SuperGlobal.stations.Find(st => st.name == stationNode.name)?.obj;
        if (stationObj != null)
            transform.position = stationObj.transform.position;

        GetComponent<Renderer>().enabled = true;

        onTrain = false;
        currentTrain.passengers.Remove(gameObject);
        currentTrain = null;

        // Reprendre le chemin à la station suivante
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i].name == stationNode.name)
            {
                currentTargetIndex = i + 1;
                break;
            }
        }
    }

    private float CalculateHappiness(float travel, float wait)
    {
        // Exemple simple : moins le temps total est élevé, plus la note est haute
        float total = travel + wait;

        // Ajuster maxTime selon ce que tu juges "long"
        Node startNode = path[0];
        Node lastNode = path[path.Count - 1];
        Vector2 start = new Vector2(startNode.lat, startNode.lon);
        Vector2 last = new Vector2(lastNode.lat, lastNode.lon);
        float distance = Vector2.Distance(start, last);

        float maxTime = Mathf.Max(10, 500f * distance / speed);
        Debug.Log("TIME : " + maxTime);
        ; // 1 minute pour exemple
        float score = 1f - Mathf.Clamp01(total / maxTime); // 1 si rapide, 0 si trop long
        Debug.Log("SCORE : " + score);
        return score;
    }
}
