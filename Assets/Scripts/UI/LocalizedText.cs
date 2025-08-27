using UnityEngine;
using TMPro; // Si vous utilisez TextMeshPro

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    public string key;
    private TMP_Text textComponent;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        I18nManager.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    public void UpdateText()
    {
        if (textComponent != null && I18nManager.Instance != null)
        {
            textComponent.text = I18nManager.Instance.GetTranslation(key);
        }
    }

    void OnDestroy()
    {
        I18nManager.OnLanguageChanged -= UpdateText;
    }
}