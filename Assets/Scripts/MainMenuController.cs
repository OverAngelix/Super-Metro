using UnityEngine;
using UnityEngine.SceneManagement; // Pour charger une scène
using UnityEngine.UI;
public class MainMenuController : MonoBehaviour
{
    public Button startButton;
    public Button optionsButton;
    public Button quitButton;

    private void Start()
    {
        startButton.onClick.AddListener(startTutorial);
        optionsButton.onClick.AddListener(options);
        quitButton.onClick.AddListener(quit);
    }
    public void startTutorial()
    {
        SceneManager.LoadScene("StationManagement");
    }

    public void options()
    {
        Debug.Log("Ouvre le menu Options");
    }

    public void quit()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
    }
}