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
        
    }
}
