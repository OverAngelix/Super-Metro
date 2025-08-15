using UnityEngine;
using System.Collections.Generic; // pour List


public class ConnectStations : MonoBehaviour
{
    public Material lineMaterial; // assigner un matériel rouge dans l'éditeur

    public void CreateLines(List<GameObject> stationObjects)
    {
        for (int i = 0; i < stationObjects.Count - 1; i++)
        {
            CreateLine(stationObjects[i].transform.position, stationObjects[i + 1].transform.position);
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("Line");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startColor = Color.red;
        lr.endColor = Color.red;
        lr.startWidth = 0.5f;
        lr.endWidth = 0.5f;
        lr.positionCount = 2;
        lr.SetPosition(0, start + Vector3.up * 0.5f);
        lr.SetPosition(1, end + Vector3.up * 0.5f);
    }
}
