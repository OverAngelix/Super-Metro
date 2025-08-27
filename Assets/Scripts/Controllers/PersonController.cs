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
            GameObject firstObj = path[0].gameObject;
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
            if (currentHappiness <= 0f)
            {
                SendHappiness();
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

        GameObject targetObj = path[currentTargetIndex].gameObject;

        if (targetObj == null) return;

        // Vérifier si le node est une station

        Station station = SuperGlobal.GetStationByObj(targetObj);

        if (station != null)
        {
            // Arrivé à la station
            transform.position = targetObj.transform.position;

            if (!station.waitingPeople.Contains(this))
                station.waitingPeople.Add(this);

            waitTime += Time.deltaTime;

            // Vérifier happiness en temps réel
            float currentHappiness = CalculateHappiness(travelTime, waitTime);
            if (currentHappiness <= 0f)
            {
                SendHappiness();
            }

            return; // stop mouvement tant qu'on attend le train
        }

        // Mouvement vers le prochain node
        float currentSpeed = speed;

        if (currentTargetIndex > 0)
        {
            GameObject prevObj = path[currentTargetIndex - 1].gameObject;
            bool prevIsStation = SuperGlobal.ExistStationByObj(prevObj);
            bool targetIsStation = station != null;
            if (prevIsStation && targetIsStation) currentSpeed *= 4f;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetObj.transform.position, Time.deltaTime * currentSpeed);

        // Compter le temps de déplacement
        travelTime += Time.deltaTime;

        // Vérifier happiness en temps réel même en marchant
        float h = CalculateHappiness(travelTime, waitTime);
        if (h <= 0f)
        {
            SendHappiness();
            return;
        }

        if (Vector3.Distance(transform.position, targetObj.transform.position) < 0.1f)
        {
            currentTargetIndex++;
            if (currentTargetIndex >= path.Count)
            {
                FinishJourney();
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

        GameObject stationObj = stationNode.gameObject;
        if (stationObj != null)
            transform.position = stationObj.transform.position;

        GetComponent<Renderer>().enabled = true;

        onTrain = false;
        currentTrain.passengers.Remove(gameObject);
        currentTrain = null;

        // Reprendre le chemin à la station suivante
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i].gameObject == stationObj)
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

        float maxTime = Mathf.Max(20, 500f * distance / speed);

        float perfectScoreTimeThreshold = 1 / 4f; // c'est le ratio en dessous duquel on a 1 en terme de temps passé

        float score = total <= perfectScoreTimeThreshold * maxTime ? 1f : (total / ((perfectScoreTimeThreshold - 1) * maxTime)) - (1 / (perfectScoreTimeThreshold - 1)); // 1 si rapide, 0 si trop long

        SuperGlobal.Log($"Mon temps max est {maxTime}, ma distance {distance}, mon temps actuel est {total} et mon score est {score}");
        return score;
    }
    
    private void SendHappiness()
    {
        if (!happinessSent)
        {
            happiness = CalculateHappiness(travelTime, waitTime);
            SuperGlobal.peopleHappiness.Add(happiness);
            startSpot.peopleFromLocation.Add(this);
            endSpot.peopleToLocation.Add(this);
            happinessSent = true;
        }
    }

    private void FinishJourney()
    {
        // La personne termine son parcours → on s'assure que la happiness est envoyée
        SendHappiness();
        Destroy(gameObject);
    }
}
