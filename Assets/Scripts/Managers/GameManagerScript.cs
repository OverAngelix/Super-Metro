using UnityEngine;
using TMPro;

public class GameManagerScript : MonoBehaviour
{
    public TextMeshProUGUI money;
    public float spawnInterval = 60f;
    private float timer = 0f;

    void Update()
    {
        money.text = SuperGlobal.money.ToString("F1");

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            foreach (TrainLine trainline in SuperGlobal.trainLines)
            {
                SuperGlobal.money -= trainline.maintenance;
            }
            timer = 0f;
        }
    }
}
