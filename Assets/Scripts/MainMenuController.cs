using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement; // Pour charger une scène
using UnityEngine.UI;
public class MainMenuController : MonoBehaviour
{
    public Button startButton;
    public Button optionsButton;
    public Button quitButton;
    public Button closeMenuOptionsButton;

    public GameObject MainMenu;
    public GameObject OptionsMenu;

    public Scrollbar volumeBar;
    public TextMeshProUGUI volumeValueText;
    public AudioSource mainMusic;
    private void Start()
    {
        startButton.onClick.AddListener(StartTutorial);
        optionsButton.onClick.AddListener(Options);
        quitButton.onClick.AddListener(Quit);
        closeMenuOptionsButton.onClick.AddListener(CloseMenuOptions);
        volumeBar.onValueChanged.AddListener(ChangeVolume);
        volumeValueText.text = "100";

    }
    public void StartTutorial()
    {
        SceneManager.LoadScene("MapManagement");
    }

    public void Options()
    {
        MainMenu.SetActive(false);
        OptionsMenu.SetActive(true);
    }

    public void CloseMenuOptions()
    {
        MainMenu.SetActive(true);
        OptionsMenu.SetActive(false);
    }

    public void ChangeVolume(float value)
    {
        mainMusic.volume = value;
        volumeValueText.text = Mathf.Round(value * 100).ToString();
    }

    public void Quit()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
    }
}