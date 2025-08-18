using System.Collections.Generic;
using UnityEngine;

public class Line
{
    public string name;
    public int lineNumber;
    public List<TrainController> trainsList = new List<TrainController>();
    public GameObject ticketUIObject;
    
    public Line(string name, int lineNumber)
    {
        this.lineNumber = lineNumber;
        this.name = name;
    }
}