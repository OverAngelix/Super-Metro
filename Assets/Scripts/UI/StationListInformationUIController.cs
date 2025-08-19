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
        foreach (TrainLine trailLine in SuperGlobal.trainlines)
        {
            TicketUIController ticketUIController;
            if (!trailLine.ticketUIObject)
            {
                GameObject obj = Instantiate(ticketUIPrefab, canvasParent);
                ticketUIController = obj.GetComponent<TicketUIController>();
                ticketUIControllers.Add(ticketUIController);
                // Position offset depuis top right
                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1, 1); // top right
                rt.anchorMax = new Vector2(1, 1);
                rt.anchoredPosition = new Vector2(-200, -150); // offset

                // Stocker la référence
                trailLine.ticketUIObject = obj;

                upgradeButton = ticketUIController.upgradeButton;
                upgradeButton.onClick.AddListener(() => SuperGlobal.upgradeTrain(trailLine.lineNumber, 0));

            }

            else
            {
                ticketUIController = trailLine.ticketUIObject.GetComponent<TicketUIController>();
            }

            TMP_Text trainText = ticketUIController.trainText;
            trainText.text = "Train 1"; // Pour l'instant on prend que le premier train

            TMP_Text lineText = ticketUIController.lineText;
            lineText.text = "Ligne " + trailLine.lineNumber.ToString();

            TMP_Text passengersText = ticketUIController.passengersText;
            passengersText.text = "Passagers : " + trailLine.trains[0].passengers.Count.ToString() + " / " + trailLine.trains[0].maxPassengers.ToString(); // Pour l'instant on prend que le premier train
            
            TMP_Text speedText = ticketUIController.speedText;
            speedText.text = "Vitesse : " + trailLine.trains[0].speed.ToString() + " km / h"; // Pour l'instant on prend que le premier train
        }
    }

}
