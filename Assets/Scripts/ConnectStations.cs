using UnityEngine;
using System.Collections.Generic; // pour List


public class ConnectStations : MonoBehaviour
{
    public Material lineMaterial;
    private Transform linesContainer;


    public void Init()
    {
        if (linesContainer == null)
        {
            linesContainer = new GameObject("LinesContainer").transform;
            linesContainer.parent = this.transform;
        }
    }


    public void CreateLines(List<Station> stationObjects)
    {
        // ClearLines();

        for (int i = 0; i < stationObjects.Count - 1; i++)
        {
            CreateLine(stationObjects[i].obj.transform.position, stationObjects[i + 1].obj.transform.position);
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.parent = linesContainer;

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
    private void ClearLines()
    {
        foreach (Transform child in linesContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
