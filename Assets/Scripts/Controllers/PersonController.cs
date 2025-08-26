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
    public float happiness = 1f;

    public Location startSpot;
    public Location endSpot;
    private Node targetStationNode; // la station où la personne doit descendre

    // Variables pour le système de happiness
    private float travelTime = 0f;       // temps passé en mouvement
    private float waitTime = 0f;         // temps passé à attendre aux stations

    public void SetPath(List<Node> newPath)
    {
        path = newPath;
        currentTargetIndex = 0;

        if (path.Count > 0)
        {
            string firstNodeName = path[0].name;
            GameObject firstObj = SuperGlobal.spots.Find(s => s.name == firstNodeName)?.obj
                                ?? SuperGlobal.GetStation(firstNodeName)?.obj;
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
        if (onTrain)
        {
            travelTime += Time.deltaTime;

            // Vérifier happiness même en train
            float currentHappiness = CalculateHappiness(travelTime, waitTime);
            if (currentHappiness <= 0f && !happinessSent)
            {
                happiness = 0f;
                SuperGlobal.peopleHappiness.Add(happiness);
                startSpot.peopleFromLocation.Add(this);
                endSpot.peopleToLocation.Add(this);
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

        string targetNodeName = path[currentTargetIndex].name;
        GameObject targetObj = SuperGlobal.spots.Find(s => s.name == targetNodeName)?.obj
                            ?? SuperGlobal.GetStation(targetNodeName)?.obj;

        if (targetObj == null) return;

        // Vérifier si le node est une station

        Station station = SuperGlobal.GetStation(targetNodeName);

        if (station != null)
        {
            // Arrivé à la station
            transform.position = targetObj.transform.position;

            if (!station.waitingPeople.Contains(this))
                station.waitingPeople.Add(this);

            waitTime += Time.deltaTime;

            // Vérifier happiness en temps réel
            float currentHappiness = CalculateHappiness(travelTime, waitTime);
            if (currentHappiness <= 0f && !happinessSent)
            {
                happiness = 0f;
                SuperGlobal.peopleHappiness.Add(happiness);
                startSpot.peopleFromLocation.Add(this);
                endSpot.peopleToLocation.Add(this);
                happinessSent = true;
            }

            return; // stop mouvement tant qu'on attend le train
        }

        // Mouvement vers le prochain node
        float currentSpeed = speed;

        if (currentTargetIndex > 0)
        {
            string prevName = path[currentTargetIndex - 1].name;
            bool prevIsStation = SuperGlobal.ExistStation(prevName);
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
                startSpot.peopleFromLocation.Add(this);
                endSpot.peopleToLocation.Add(this);
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

        GameObject stationObj = SuperGlobal.GetStation(stationNode.name)?.obj;
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
        // Moins le temps total est élevé, plus la note est haute
        float total = travel + wait;

        // Ajuster maxTime selon ce qui est "long"
        Node startNode = path[0];
        Node lastNode = path[path.Count - 1];
        Vector2 start = new Vector2(startNode.lat, startNode.lon);
        Vector2 last = new Vector2(lastNode.lat, lastNode.lon);
        float distance = Vector2.Distance(start, last);

        float maxTime = Mathf.Max(10, 500f * distance / speed);
        float score = 1f - Mathf.Clamp01(total / maxTime); // 1 si rapide, 0 si trop long
        return score;
    }
}
