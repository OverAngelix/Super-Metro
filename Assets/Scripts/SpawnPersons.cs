using UnityEngine;

public class SpawnPersons : MonoBehaviour
{
    public GameObject person;

    public Transform spawnPoint;
    public float spawnInterval = 60f;

    public GameObject pointA;
    public GameObject pointB;
    private float timer = 0f;
    void Update()
    {
        timer += Time.deltaTime * SuperGlobal.timeSpeed;
        Debug.Log(SuperGlobal.timeSpeed);
        if (timer >= spawnInterval)
        {
            SpawnObject();
            timer = 0f;
        }
    }

    private void SpawnObject()
    {
        GameObject newPerson = Instantiate(person, spawnPoint.position, Quaternion.identity);
        PersonController personController = newPerson.GetComponent<PersonController>();
        personController.pointA = pointA;
        personController.pointB = pointB;
    }
}
