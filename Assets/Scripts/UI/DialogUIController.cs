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
    private Queue<(List<Dialog> dialogs, string dialogTitle)> questDialogQueue 
        = new Queue<(List<Dialog>, string)>();

    private bool isDialogOpen = false;
    private bool isSequenceRunning = false;

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

    public void UpdateContent()
    {
        if (imageObject != null) imageObject.texture = image;
        if (titleObject != null) titleObject.text = title;
        if (textObject != null) textObject.text = text;
    }

    // Ajoute une séquence de dialogues à la file d'attente globale
    public void EnqueueDialogSequence(List<Dialog> dialogs, string dialogTitle = null)
    {
        if (dialogs == null || dialogs.Count == 0) return;

        questDialogQueue.Enqueue((dialogs, dialogTitle));

        if (!isSequenceRunning)
            RunNextSequence();
    }

    // Lance la prochaine séquence dans la file
    private void RunNextSequence()
    {
        if (questDialogQueue.Count == 0)
        {
            isSequenceRunning = false;
            return;
        }

        isSequenceRunning = true;
        var next = questDialogQueue.Dequeue();

        dialogQueue.Clear();
        foreach (var d in next.dialogs)
            dialogQueue.Enqueue(d);

        title = next.dialogTitle ?? "";

        ShowNextDialog();
    }

    private void ShowNextDialog()
    {
        if (dialogQueue.Count == 0)
        {
            HideDialog();
            RunNextSequence(); // Passe à la prochaine séquence si elle existe
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
