using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrainController : MonoBehaviour
{
    public float speed = 20f;
    public int maxPassengers = 5;
    public float timeToEnter = 0.2f;
    public float timeToWaitAtStation = 2f;
    public List<GameObject> passengers = new();

    private List<Node> path;
    private int currentTargetIndex = 0;
    private bool forward = true; // sens du parcours

    public Node currentStation;

    public void SetPath(List<Node> newPath)
    {
        path = newPath;

        StopAllCoroutines();

        // Positionner le train sur le premier node
        if (path.Count > 0)
        {
            GameObject firstObj = path[0].gameObject;
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
            GameObject targetObj = path[currentTargetIndex].gameObject;
            if (targetObj == null)
            {
                yield return null;
                continue;
            }

            // Déplacement constant
            while (Vector3.Distance(transform.position, targetObj.transform.position) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetObj.transform.position, Time.deltaTime * speed);
                // Rotation vers la cible
                Vector3 direction = targetObj.transform.position - transform.position;
                if (direction != Vector3.zero)
                {
                    Quaternion toRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, toRotation * Quaternion.Euler(270, 90, 0), Time.deltaTime * speed);
                }
                yield return null;
            }

            // Si on est sur une station, gérer les passagers
            Station station = SuperGlobal.GetStationByObj(targetObj);

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

    private IEnumerator HandlePassengersAtStation(Station station)
    {
        float endTime = Time.time + timeToWaitAtStation;

        while (passengers.Count < maxPassengers && station.waitingPeople.Count > 0 && Time.time < endTime)
        {
            PersonController person = station.waitingPeople[0];
            station.waitingPeople.RemoveAt(0);

            passengers.Add(person.gameObject);
            SuperGlobal.money += SuperGlobal.ticketPrice;

            Node stationToGo = person.GetNextTarget();
            person.BoardTrain(this, stationToGo);
            StationListInformationUIController.Instance.RefreshUI();

            if (Time.time + timeToEnter < endTime)
                yield return new WaitForSeconds(timeToEnter);
            else
                break;
        }

        float remaining = endTime - Time.time;
        if (remaining > 0)
            yield return new WaitForSeconds(remaining);
    }

    public void Upgrade(string upgradeName)
    {
        UpgradeManager.Upgrade(upgradeName, this);
    }

}
