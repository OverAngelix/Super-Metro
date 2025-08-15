using UnityEngine;

public class PersonController : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;
    public float speed = 5.0f;    
    
    private void Start() {
        transform.position = pointA.transform.position;
    }
    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, pointB.transform.position, Time.deltaTime * speed);
    }
}
