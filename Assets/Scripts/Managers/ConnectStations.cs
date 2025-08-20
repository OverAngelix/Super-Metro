using UnityEngine;
using System.Collections.Generic; // pour List


public class ConnectStations : MonoBehaviour
{
    private Transform linesContainer;
    public Material lineMaterial;


    public void Init()
    {
        if (linesContainer == null)
        {
            linesContainer = new GameObject("LinesContainer").transform;
            linesContainer.parent = this.transform;
        }
    }


    public void CreateLines(List<Station> stationObjects, Color colorLine)
    {
        Material colorMat = new Material(lineMaterial);
        colorMat.SetColor("_BaseColor", colorLine);
        for (int i = 0; i < stationObjects.Count - 1; i++)
        {
            CreateLine(stationObjects[i].obj.transform.position, stationObjects[i + 1].obj.transform.position, colorMat);
        }
    }

    void CreateLine(Vector3 start, Vector3 end, Material material)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.parent = linesContainer;
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = material;
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
