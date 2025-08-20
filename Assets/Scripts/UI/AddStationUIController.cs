using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddStationUIController : MonoBehaviour
{
    public GameObject panel;
    private float newLat;
    private float newLon;

    public TMP_InputField stationName;

    public Button previousLineButton;
    public TMP_Text lineText;
    public Button nextLineButton;
    private int currentLineIndex = 0;

    public Button validationButtonUI;
    public Button closeButtonUI;

    private bool isEditMode = false;
    public RawImage editionModeUI;

    void Awake()
    {
        nextLineButton.onClick.AddListener(ShowNextLine);
        previousLineButton.onClick.AddListener(ShowPreviousLine);
        validationButtonUI.onClick.AddListener(AddStation);
        closeButtonUI.onClick.AddListener(CloseUI);
        RefreshUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isEditMode = !isEditMode;
            if (isEditMode)
            {
                editionModeUI.enabled = true;
            }
            else
            {
                editionModeUI.enabled = false;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isEditMode && !SuperGlobal.isUIOpen)
            {
                HandleMapClick();
            }
        }
    }

    void HandleMapClick()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 unityClickPosition = hit.point;
            Vector2 latLon = GeoUtils.UnityPositionToLatLon(OSMTileManager.Instance.centerLat, OSMTileManager.Instance.centerLon, unityClickPosition, OSMTileManager.Instance.zoomLevel, OSMTileManager.Instance.tileSize);
            newLat = latLon.x;
            newLon = latLon.y;
            panel.SetActive(true);
            SuperGlobal.isUIOpen = true;
        }

    }

    private void AddStation()
    {
        TrainLine trainLine = SuperGlobal.trainLines[currentLineIndex];
        Station newStation = trainLine.AddStation(stationName.text, newLat, newLon);
        if (newStation != null)
        {
            OSMTileManager.Instance.AddStationOnMap(trainLine, newStation);
            SuperGlobal.money -= 500;
            SuperGlobal.nbStation += 1;
            panel.SetActive(false);
            SuperGlobal.isUIOpen = false;
            isEditMode = false;
            editionModeUI.enabled = false;
            stationName.text = "";
        }
    }

    private void CloseUI()
    {
        panel.SetActive(false);
        SuperGlobal.isUIOpen = false;
        stationName.text = "";
        newLat = 0f;
        newLon = 0f;
    }


    #region CHANGE LINE
    private void ShowNextLine()
    {
        if (currentLineIndex < SuperGlobal.trainLines.Count - 1)
        {
            currentLineIndex++;
            RefreshUI();
        }
    }

    private void ShowPreviousLine()
    {
        if (currentLineIndex > 0)
        {
            currentLineIndex--;
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        currentLineIndex = Mathf.Clamp(currentLineIndex, 0, SuperGlobal.trainLines.Count - 1);
        lineText.text = $"Line {SuperGlobal.trainLines[currentLineIndex].lineNumber}";
        previousLineButton.interactable = currentLineIndex > 0;
        nextLineButton.interactable = currentLineIndex < SuperGlobal.trainLines.Count - 1;
    }
    #endregion
}
