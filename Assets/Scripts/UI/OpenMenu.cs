using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OpenMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuPrefab;
    [SerializeField] private Button menuButton;

    public Button frenchButton;
    public Button englishButton;
    public Button closeMenuButton;


    private void Start()
    {
        menuButton.onClick.AddListener(OpenCanvas);
        closeMenuButton.onClick.AddListener(CloseCanvas);
        frenchButton.onClick.AddListener(ChangeToFrench);
        englishButton.onClick.AddListener(ChangeToEnglish);
        if (menuPrefab != null)
        {
            menuPrefab.SetActive(false);
        }
    }

    private void OpenCanvas()
    {
        if (menuPrefab != null)
            menuPrefab.SetActive(true);
    }

    private void CloseCanvas()
    {
        if (menuPrefab != null)
            menuPrefab.SetActive(false);
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
}