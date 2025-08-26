using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StatisticsMovementsLineUIController : MonoBehaviour
{
    public TMP_Text name;
    public TMP_Text fromHappiness;
    public TMP_Text toHappiness;

    public string nameValue;
    public float fromHappinessValue;
    public float toHappinessValue;

    private void Start()
    {
        RefreshUI();
    }
    public void RefreshUI()
    {
        name.text = nameValue;
        fromHappiness.text = fromHappinessValue.ToString();
        toHappiness.text = toHappinessValue.ToString();
    }
}

