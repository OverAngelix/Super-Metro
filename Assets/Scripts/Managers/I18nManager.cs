using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class I18nManager : MonoBehaviour
{
    public static I18nManager Instance;
    public static event Action OnLanguageChanged;

    public string currentLanguage = "en";

    private Dictionary<string, Dictionary<string, string>> localizedStrings = new Dictionary<string, Dictionary<string, string>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadTranslations("Menu.csv");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadTranslations(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(fileName));

        if (textAsset == null)
        {
            Debug.LogError("Fichier CSV de traduction non trouvé : " + fileName);
            return;
        }

        string[] lines = textAsset.text.Split('\n');
        string[] languages = lines[0].Split(','); // First line is language key (fr, en for us)

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            string key = values[0];

            for (int j = 1; j < languages.Length; j++)
            {
                string language = languages[j].Trim();
                string translation = values[j].Trim();

                if (!localizedStrings.ContainsKey(language))
                {
                    localizedStrings[language] = new Dictionary<string, string>();
                }
                localizedStrings[language][key] = translation;
            }
        }
    }

    public string GetTranslation(string key)
    {
        if (localizedStrings.ContainsKey(currentLanguage) && localizedStrings[currentLanguage].ContainsKey(key))
        {
            return localizedStrings[currentLanguage][key];
        }

        Debug.LogWarning("Traduction manquante pour la clé '" + key + "' dans la langue '" + currentLanguage + "'");
        return key;
    }

    public void SetLanguage(string newLanguage)
    {
        currentLanguage = newLanguage;
        OnLanguageChanged?.Invoke();
    }
}