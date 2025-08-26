using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainUIController : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject createTrainLineUIPrefab;
    public GameObject trainLineEditUIPrefab;

    [Header("UI")]
    public Button createTrainLineButton;
    public Button trainLineEditButton;
    public Button statisticsButton;
    private GameObject createTrainLineUIInstance;
    private CreateTrainLineUIController createTrainLineUIController;

    public TMP_Text happinessTextObject;

    public static MainUIController Instance;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        createTrainLineUIController = createTrainLineButton.GetComponent<CreateTrainLineUIController>();

        createTrainLineButton.onClick.AddListener(ShowOrHideCreateTrainLineUI);
        trainLineEditButton.onClick.AddListener(ShowOrHideTrainLineEditUI);
        statisticsButton.onClick.AddListener(ShowOrHideStatistics);
        createTrainLineUIInstance = Instantiate(createTrainLineUIPrefab, transform.parent);
        createTrainLineUIInstance.SetActive(false);

        SuperGlobal.Log(trainLineEditUIPrefab.ToString());
        trainLineEditUIPrefab.SetActive(false);

    }

    void Update()
    {
        happinessTextObject.text = $"{SuperGlobal.ComputeHappiness()*100} %";
    }

    private void ShowOrHideCreateTrainLineUI()
    {
        bool newState = !createTrainLineUIInstance.activeSelf;
        createTrainLineUIInstance.SetActive(newState);
    }

    private void ShowOrHideTrainLineEditUI()
    {
        bool newState = !trainLineEditUIPrefab.activeSelf;
        trainLineEditUIPrefab.SetActive(newState);
        trainLineEditUIPrefab.GetComponent<TrainLineEditUIController>().RefreshUI();
    }

    private void ShowOrHideStatistics()
    {
        GameObject statisticsUI = StatisticsUIController.Instance.gameObject;
        bool newState = !statisticsUI.activeSelf;
        statisticsUI.SetActive(newState);
        StatisticsUIController.Instance.RefreshUI(0);
    }

    void OnDestroy()
    {
        createTrainLineButton.onClick.RemoveListener(ShowOrHideCreateTrainLineUI);
        trainLineEditButton.onClick.RemoveListener(ShowOrHideTrainLineEditUI);
        statisticsButton.onClick.RemoveListener(ShowOrHideStatistics);
    }

}
