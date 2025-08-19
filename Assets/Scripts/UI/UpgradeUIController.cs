using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUIController : MonoBehaviour
{
    public Button exitButton;

    public GameObject upgradeLineUI;
    public static UpgradeUIController Instance;
    void Start()
    {
        exitButton.onClick.AddListener(exit);
    }

    // Update is called once per frame

    void Awake()
    {
        Instance = this;
    }
    void Update()
    {

    }

    void exit()
    {
        upgradeLineUI.GetComponent<Canvas>().enabled = false;
    }

    public void updateUI(int lineNumber, int trainIndex)
    {
        
    }
}
