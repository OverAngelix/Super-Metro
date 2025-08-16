using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrainController : MonoBehaviour
{
    public float speed = 20f;
    public int maxPassengers = 5; // capacité max du train
    public List<GameObject> passengers = new List<GameObject>();

    private List<Node> path;
    private int currentTargetIndex = 0;
    private bool forward = true; // sens du parcours

    public Node currentStation; // nouvelle propriété pour suivre la station actuelle

    public void SetPath(List<Node> newPath)
    {
        path = newPath;
        forward = true;

        StopAllCoroutines();

        // Positionner le train sur le premier node
        if (path.Count > 0)
        {
            string firstName = path[0].name;
            GameObject firstObj = SuperGlobal.spots.Find(s => s.name == firstName)?.obj
                                ?? SuperGlobal.stations.Find(st => st.name == firstName)?.obj;
            if (firstObj != null && currentTargetIndex == 0)
                transform.position = firstObj.transform.position;
        }

        // Démarrer le mouvement
        StartCoroutine(MoveAlongPath());
    }

    private IEnumerator MoveAlongPath()
    {
        while (true)
        {
            if (path == null || path.Count == 0) yield break;

            string targetName = path[currentTargetIndex].name;
            GameObject targetObj = SuperGlobal.spots.Find(s => s.name == targetName)?.obj
                                ?? SuperGlobal.stations.Find(st => st.name == targetName)?.obj;
            if (targetObj == null)
            {
                yield return null;
                continue;
            }

            // Déplacement constant
            while (Vector3.Distance(transform.position, targetObj.transform.position) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetObj.transform.position, Time.deltaTime * speed);

                // Étape 2 : Rotation vers la cible (nouvelle partie)
                Vector3 direction = targetObj.transform.position - transform.position;
                Quaternion toRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation * Quaternion.Euler(270, 90, 0), Time.deltaTime * speed);

                yield return null;
            }

            // Si on est sur une station, gérer les passagers
            SuperGlobal.Station station = SuperGlobal.stations.Find(st => st.name == targetName);
            if (station != null)
            {
                currentStation = path[currentTargetIndex]; // mise à jour de la station actuelle
                yield return StartCoroutine(HandlePassengersAtStation(station));
            }

            // Passer au node suivant
            if (forward)
            {
                currentTargetIndex++;
                if (currentTargetIndex >= path.Count)
                {
                    forward = false;
                    currentTargetIndex = path.Count - 2;
                }
            }
            else
            {
                currentTargetIndex--;
                if (currentTargetIndex < 0)
                {
                    forward = true;
                    currentTargetIndex = 1;
                }
            }
        }
    }

    private IEnumerator HandlePassengersAtStation(SuperGlobal.Station station)
    {
        // Tant qu'il y a de la place et des personnes qui attendent
        while (passengers.Count < maxPassengers && station.waitingPeople.Count > 0)
        {
            PersonController person = station.waitingPeople[0]; // maintenant c'est un PersonController
            station.waitingPeople.RemoveAt(0);

            passengers.Add(person.gameObject); // on ajoute le GameObject dans le train
            
            SuperGlobal.money += SuperGlobal.ticketPrice;

            // Déplacer la personne vers le train
            // On récupère la station où la personne veut descendre
            Node stationToGo = person.GetNextTarget();
            person.BoardTrain(this, stationToGo); // mise à jour pour inclure la station de descente

            yield return new WaitForSeconds(0.2f); // petit délai entre chaque embarquement
        }

        // Temps d'arrêt à la station
        yield return new WaitForSeconds(1f);
    }
}
