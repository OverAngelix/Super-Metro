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

    void OnDestroy()
    {
        createTrainLineButton.onClick.RemoveListener(ShowOrHideCreateTrainLineUI);
        trainLineEditButton.onClick.RemoveListener(ShowOrHideTrainLineEditUI);
    }

}
