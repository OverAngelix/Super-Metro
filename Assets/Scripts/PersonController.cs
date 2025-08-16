using UnityEngine;
using System.Collections.Generic;

public class PersonController : MonoBehaviour
{
    public float speed = 1f;
    private List<Node> path;
    private int currentTargetIndex = 0;

    private bool onTrain = false;
    private TrainController currentTrain;

    private Node targetStationNode; // la station où la personne doit descendre

    // Variables pour le système de happiness
    private float travelTime = 0f;       // temps passé en mouvement
    private float waitTime = 0f;         // temps passé à attendre aux stations
    private float happiness = 1f;        // entre 0 et 1

    private bool isWaiting = false;      // savoir si la personne attend actuellement

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
        // Si la personne est dans le train, vérifier si elle doit descendre
        if (onTrain)
        {
            travelTime += Time.deltaTime; // temps en train considéré comme déplacement
            if (currentTrain != null && targetStationNode != null && currentTrain.currentStation?.name == targetStationNode.name)
            {
                LeaveTrain(targetStationNode);
            }
            return;
        }

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

            isWaiting = true;
            waitTime += Time.deltaTime; // incrémente le temps d'attente
            return; // stop mouvement tant qu'on attend le train
        }
        else
        {
            // On n'attend plus
            isWaiting = false;
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

        if (Vector3.Distance(transform.position, targetObj.transform.position) < 0.1f)
        {
            currentTargetIndex++;
            if (currentTargetIndex >= path.Count)
            {
                // Calculer la happiness finale
                happiness = CalculateHappiness(travelTime, waitTime);

                // Stocker dans SuperGlobal
                SuperGlobal.peopleHappiness.Add(happiness);

                Destroy(gameObject);
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
        float maxTime = 10f; // 1 minute pour exemple
        float score = 1f - Mathf.Clamp01(total / maxTime); // 1 si rapide, 0 si trop long
        return score;
    }
}
