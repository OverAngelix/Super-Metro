using UnityEngine;

public class Edge
{
    public Node to;
    public float cost;
    public Edge(Node to, float cost)
    {
        this.to = to;
        this.cost = cost;
    }
}