using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainListItem : MonoBehaviour
{

    public TMP_Text indexTextObject;
    public Button editButton;
    public TrainController train;

    void Start()
    {
        editButton.onClick.AddListener(editTrain);
    }

    void editTrain()
    {
        OpenUpgradeUI();
    }
    
    private void OpenUpgradeUI()
    {
        UpgradeUIController.Instance.Open(train, 0); // TODO : Changer l'index du train
    }
}
