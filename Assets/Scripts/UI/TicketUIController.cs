using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TicketUIController : MonoBehaviour
{
    public Button upgradeButton;
    public TMP_Text trainText;
    public TMP_Text lineText;
    public TMP_Text passengersText;
    public TMP_Text speedText;

    public Button nextTrainButton;
    public Button previousTrainButton;
    public TrainLine trainLine;

    private int currentTrainIndex = 0;

    private void Awake()
    {
        nextTrainButton.onClick.AddListener(ShowNextTrain);
        previousTrainButton.onClick.AddListener(ShowPreviousTrain);
        upgradeButton.onClick.AddListener(OpenUpgradeUI);
    }

    private void Start()
    {
        RefreshUI();
    }

    private void OnDestroy()
    {
        nextTrainButton.onClick.RemoveListener(ShowNextTrain);
        previousTrainButton.onClick.RemoveListener(ShowPreviousTrain);
        upgradeButton.onClick.RemoveListener(OpenUpgradeUI);
    }

    public void RefreshUI()
    {
        if (trainLine == null || trainLine.trains == null || trainLine.trains.Count == 0)
        {
            trainText.text = "Aucun train";
            lineText.text = string.Empty;
            passengersText.text = string.Empty;
            speedText.text = string.Empty;

            nextTrainButton.interactable = false;
            previousTrainButton.interactable = false;
            upgradeButton.interactable = false;
            return;
        }

        currentTrainIndex = Mathf.Clamp(currentTrainIndex, 0, trainLine.trains.Count - 1);

        TrainController currentTrain = trainLine.trains[currentTrainIndex];

        trainText.text = $"Train {currentTrainIndex}";
        lineText.text = $"Ligne {trainLine.lineNumber}";
        passengersText.text = $"Passagers : {currentTrain.passengers.Count} / {currentTrain.maxPassengers}";
        speedText.text = $"Vitesse : {currentTrain.speed} km/h";

        previousTrainButton.interactable = currentTrainIndex > 0;
        nextTrainButton.interactable = currentTrainIndex < trainLine.trains.Count - 1;
        upgradeButton.interactable = true;
    }

    private void ShowNextTrain()
    {
        if (currentTrainIndex < trainLine.trains.Count - 1)
        {
            currentTrainIndex++;
            RefreshUI();
        }
    }

    private void ShowPreviousTrain()
    {
        if (currentTrainIndex > 0)
        {
            currentTrainIndex--;
            RefreshUI();
        }
    }

     private void OpenUpgradeUI()
    {
        UpgradeUIController.Instance.Open(trainLine.trains[currentTrainIndex], currentTrainIndex);
    }
}
