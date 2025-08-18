using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StationListInformationUIController : MonoBehaviour
{
    public static StationListInformationUIController Instance { get; private set; }

    public GameObject ticketUIPrefab;
    public Transform canvasParent;
    private Button upgradeButton;
    public List<TicketUIController> ticketUIControllers = new List<TicketUIController>();

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        
    }

    void Update()
    {
        foreach (Line line in SuperGlobal.lines)
        {
            if (!line.ticketUIObject)
            {
                GameObject obj = Instantiate(ticketUIPrefab, canvasParent);
                ticketUIControllers.Add(obj.GetComponent<TicketUIController>());
                // Position offset depuis top right
                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1, 1); // top right
                rt.anchorMax = new Vector2(1, 1);
                rt.anchoredPosition = new Vector2(-200, -150); // offset

                // Stocker la référence
                line.ticketUIObject = obj;

                upgradeButton = line.ticketUIObject.transform.Find("Content/Upgrade").GetComponent<Button>();
                upgradeButton.onClick.AddListener(() => SuperGlobal.upgradeTrain(line.lineNumber, 0));

            }

            TMP_Text trainText = line.ticketUIObject.transform.Find("Content/Head/Train").GetComponent<TMP_Text>();
            trainText.text = "Train 1"; // Pour l'instant on prend que le premier train

            TMP_Text lineText = line.ticketUIObject.transform.Find("Content/Head/Line").GetComponent<TMP_Text>();
            lineText.text = "Ligne " + line.lineNumber.ToString();

            TMP_Text passengersText = line.ticketUIObject.transform.Find("Content/Description/Passengers").GetComponent<TMP_Text>();
            passengersText.text = "Passagers : " + line.trainsList[0].passengers.Count.ToString() + " / " + line.trainsList[0].maxPassengers.ToString(); // Pour l'instant on prend que le premier train
            
            TMP_Text speedText = line.ticketUIObject.transform.Find("Content/Description/Speed").GetComponent<TMP_Text>();
            speedText.text = "Vitesse : " + line.trainsList[0].speed.ToString() + " km / h"; // Pour l'instant on prend que le premier train
        }
    }

}
