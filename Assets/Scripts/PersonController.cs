using UnityEngine;
using System.Collections.Generic;

public class PersonController : MonoBehaviour
{
    public float speed = 5f;
    private List<Node> path;
    private int currentTargetIndex = 0;

    // On ne met plus startPoint ni targetPoint
    // public GameObject startPoint;
    // public GameObject targetPoint;

    public void SetPath(List<Node> newPath)
    {
        path = newPath;
        currentTargetIndex = 0;

        // Positionner le personnage sur le premier node
        if (path.Count > 0)
        {
            string firstName = path[0].name;
            GameObject firstObj = SuperGlobal.spots.Find(s => s.name == firstName)?.obj 
                                ?? SuperGlobal.stations.Find(st => st.name == firstName)?.obj;
            if (firstObj != null)
                transform.position = firstObj.transform.position;
        }
    }

    void Update()
    {
        if (path == null || currentTargetIndex >= path.Count) return;

        string targetName = path[currentTargetIndex].name;
        GameObject targetObj = SuperGlobal.spots.Find(s => s.name == targetName)?.obj
                            ?? SuperGlobal.stations.Find(st => st.name == targetName)?.obj;

        if (targetObj == null) return;

        // Calculer la vitesse selon le segment
        float currentSpeed = speed;
        if (currentTargetIndex > 0)
        {
            string prevName = path[currentTargetIndex - 1].name;

            bool prevIsStation = SuperGlobal.stations.Exists(st => st.name == prevName);
            bool targetIsStation = SuperGlobal.stations.Exists(st => st.name == targetName);

            // Si les deux points sont des stations → métro → 4 fois plus vite
            if (prevIsStation && targetIsStation)
            {
                currentSpeed *= 4f;
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, targetObj.transform.position, Time.deltaTime * currentSpeed);

        // Vérifier si on est arrivé au node courant
        if (Vector3.Distance(transform.position, targetObj.transform.position) < 0.1f)
        {
            currentTargetIndex++;
            if (currentTargetIndex >= path.Count)
            {
                Destroy(gameObject);
            }
        }
    }


}
