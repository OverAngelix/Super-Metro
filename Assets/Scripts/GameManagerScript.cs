using UnityEngine;
using TMPro;

public class GameManagerScript : MonoBehaviour
{
    public TextMeshProUGUI money;

    public float spawnInterval = 20f;
    private float timer = 0f;

    // Update is called once per frame
    void Update()
    {
        money.text = SuperGlobal.money.ToString("F1") + "€";

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SuperGlobal.money -= SuperGlobal.fees;
            timer = 0f;
        }
    }
}
