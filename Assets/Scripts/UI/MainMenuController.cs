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
    public Button frenchButton;
    public Button englishButton;
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
        volumeValueText.text = "10";
        mainMusic.volume = 0.10f;
        frenchButton.onClick.AddListener(ChangeToFrench);
        englishButton.onClick.AddListener(ChangeToEnglish);
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

    public void ChangeToFrench()
    {
        I18nManager.Instance.SetLanguage("fr");
        englishButton.interactable = true;
        frenchButton.interactable = false;
    }

    public void ChangeToEnglish()
    {
        I18nManager.Instance.SetLanguage("en");
        englishButton.interactable = false;
        frenchButton.interactable = true;
    }

    public void Quit()
    {
        SuperGlobal.Log("Quitter le jeu");
        Application.Quit();
    }
}