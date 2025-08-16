using UnityEngine;
using System.Collections.Generic;

public class PersonController : MonoBehaviour
{
    public float speed = 5f;
    private List<Node> path;
    private int currentTargetIndex = 0;

    private bool onTrain = false;
    private TrainController currentTrain;

    private Node targetStationNode; // la station où la personne doit descendre

    private void Start()
    {
        //Debug.Log("Spawn de perso");
    }
    
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
            // Debug.Log(currentTrain);
            // Debug.Log(targetStationNode);
            // Debug.Log(currentTrain.currentStation);
            //Debug.Log("current");
            //Debug.Log(currentTrain.currentStation?.name);
            //Debug.Log("target");
            //Debug.Log(targetStationNode.name);
            if (currentTrain != null && targetStationNode != null && currentTrain.currentStation?.name == targetStationNode.name)
            {
                LeaveTrain(targetStationNode);
            }
            return; // ne rien faire tant qu'on est dans le train
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
            {
                station.waitingPeople.Add(this);
                //GERER SURPLUS avec du mecontentement
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

        if (Vector3.Distance(transform.position, targetObj.transform.position) < 0.1f)
        {
            currentTargetIndex++;
            if (currentTargetIndex >= path.Count)
            {
                Destroy(gameObject);
            }
        }
    }

    // La personne monte dans le train en indiquant sa station de descente
    public void BoardTrain(TrainController train, Node stationNode)
    {
        //Debug.Log(stationNode);
        currentTrain = train;
        targetStationNode = stationNode;
        onTrain = true;
        //Debug.Log("person va à la station");
        //Debug.Log(stationNode.name);
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
        // Trouver l'index dans le chemin correspondant à la station où la personne vient de descendre
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i].name == stationNode.name)
            {
                currentTargetIndex = i + 1; // repartir après cette station
                break;
            }
        }
    }

}
