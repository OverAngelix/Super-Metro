using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StationListInformationUIController : MonoBehaviour
{
    public static StationListInformationUIController Instance;

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
        foreach (TrainLine trainLine in SuperGlobal.trainLines)
        {
            TicketUIController ticketUIController;
            if (!trainLine.ticketUIObject)
            {
                GameObject obj = Instantiate(ticketUIPrefab, canvasParent);
                ticketUIController = obj.GetComponent<TicketUIController>();
                ticketUIController.trainLine = trainLine;
                ticketUIControllers.Add(ticketUIController);
                // Position offset depuis top right
                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1, 1); // top right
                rt.anchorMax = new Vector2(1, 1);
                rt.anchoredPosition = new Vector2(-200, -150); // offset

                // Stocker la référence
                trainLine.ticketUIObject = obj;

                // upgradeButton = ticketUIController.upgradeButton;
                // upgradeButton.onClick.AddListener(() => SuperGlobal.upgradeTrain(trainLine.lineNumber, 0));

            }

            else
            {
                ticketUIController = trainLine.ticketUIObject.GetComponent<TicketUIController>();
            }

            // TMP_Text trainText = ticketUIController.trainText;
            // trainText.text = "Train 1"; // Pour l'instant on prend que le premier train

            // TMP_Text lineText = ticketUIController.lineText;
            // lineText.text = "Ligne " + trainLine.lineNumber.ToString();

            // TMP_Text passengersText = ticketUIController.passengersText;
            // passengersText.text = "Passagers : " + trainLine.trains[0].passengers.Count.ToString() + " / " + trainLine.trains[0].maxPassengers.ToString(); // Pour l'instant on prend que le premier train
            
            // TMP_Text speedText = ticketUIController.speedText;
            // speedText.text = "Vitesse : " + trainLine.trains[0].speed.ToString() + " km / h"; // Pour l'instant on prend que le premier train
        }
    }

}
