using UnityEngine;
using System.Collections.Generic;

public class StationListInformationUIController : MonoBehaviour
{
    public static StationListInformationUIController Instance;

    public GameObject ticketUIPrefab;
    public Transform canvasParent;
    public List<TicketUIController> ticketUIControllers = new List<TicketUIController>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (TrainLine trainLine in SuperGlobal.trainLines)
        {
            TicketUIController ticketUIController;

            if (!trainLine.ticketUIObject)
            {
                GameObject obj = Instantiate(ticketUIPrefab, canvasParent);
                ticketUIController = obj.GetComponent<TicketUIController>();

                ticketUIController.trainLine = trainLine;
                trainLine.ticketUIObject = obj;

                ticketUIControllers.Add(ticketUIController);

                // Position offset depuis top right
                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1, 1); // top right
                rt.anchorMax = new Vector2(1, 1);
                rt.anchoredPosition = new Vector2(-200, -150);
            }
            else
            {
                ticketUIController = trainLine.ticketUIObject.GetComponent<TicketUIController>();
            }

            // Chaque ticket se met à jour lui-même
            ticketUIController.RefreshUI();
        }
    }
}
