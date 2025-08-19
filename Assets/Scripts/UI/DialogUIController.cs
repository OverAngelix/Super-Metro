using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class DialogUIController : MonoBehaviour
{
    public RawImage imageObject;
    public Texture2D image;
    public TextMeshProUGUI titleObject;
    public TextMeshProUGUI textObject;
    public string text;
    public string title;
    public static DialogUIController Instance;
    void OnValidate()
    {
        UpdateContent();
    }

    public void UpdateContent()
    {
        imageObject.texture = image;
        titleObject.text = title;
        textObject.text = text;
    }
    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }
}
