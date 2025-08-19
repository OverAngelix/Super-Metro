using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DialogUIController : MonoBehaviour
{
    public RawImage imageObject;
    public Texture2D image;
    public TextMeshProUGUI titleObject;
    public TextMeshProUGUI textObject;

    public string text;
    public string title;

    public Image overlay;
    private CanvasGroup canvasGroup;

    private Queue<Dialog> dialogQueue = new Queue<Dialog>();
    private bool isDialogOpen = false;

    public static DialogUIController Instance;

    void Awake()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        HideDialog();
    }

    void Update()
    {
        if (isDialogOpen && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            ShowNextDialog();
        }
    }

    void OnValidate()
    {
        UpdateContent();
    }

    // Met à jour le contenu avec les variables actuelles
    public void UpdateContent()
    {
        if (imageObject != null) imageObject.texture = image;
        if (titleObject != null) titleObject.text = title;
        if (textObject != null) textObject.text = text;
    }

    // Lance une séquence de dialogues
    public void StartDialogSequence(List<Dialog> dialogs, string dialogTitle = null)
    {
        if (dialogs == null || dialogs.Count == 0) return;

        dialogQueue.Clear();
        foreach (var d in dialogs)
            dialogQueue.Enqueue(d);

        if (!string.IsNullOrEmpty(dialogTitle))
            title = dialogTitle;

        ShowNextDialog();
    }

    // Passe au dialogue suivant dans la queue
    private void ShowNextDialog()
    {
        if (dialogQueue.Count == 0)
        {
            HideDialog();
            return;
        }

        Dialog current = dialogQueue.Dequeue();
        text = current.text;

        UpdateContent();
        ShowDialog();
    }

    public void ShowDialog()
    {
        gameObject.SetActive(true);
        isDialogOpen = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (overlay != null) overlay.enabled = true;
    }

    public void HideDialog()
    {
        isDialogOpen = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (overlay != null) overlay.enabled = false;

        gameObject.SetActive(false);
    }
}