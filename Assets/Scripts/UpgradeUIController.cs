using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUIController : MonoBehaviour
{
    public Button exitButton;

    public GameObject upgradeUI;
    void Start()
    {
        exitButton = transform.Find("Panel/Vertical Box/ButtonExit").GetComponent<Button>();
        exitButton.onClick.AddListener(exit);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void exit()
    {
        upgradeUI = GameObject.Find("UpgradeUI");
        upgradeUI.GetComponent<Canvas>().enabled = false;
    }

    public void updateUI(int lineNumber, int trainIndex)
    {
        
    }
}
