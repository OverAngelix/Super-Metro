using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateTrainLineUIController : MonoBehaviour
{
    [Header("UI")]
    public Button validateButton;
    public TMP_InputField inputField;
    public StationListInformationUIController stationListInformationUIController;

    private CreateTrainLineUIController createTrainLineUIController;
    void Start()
    {
        validateButton.onClick.AddListener(TryCreateTrainLine);
    }

    void OnDestroy()
    {
        validateButton.onClick.RemoveListener(TryCreateTrainLine);
    }

    void TryCreateTrainLine()
    {
        TrainLineManager.TryCreateTrainLine(inputField.text);
        stationListInformationUIController.RefreshUI();
        gameObject.SetActive(false);
    }
}
