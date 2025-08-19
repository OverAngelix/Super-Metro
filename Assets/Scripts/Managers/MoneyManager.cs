using UnityEngine;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    RawImage moneyImage;
    void Start()
    {
        moneyImage = GetComponent<RawImage>();
        moneyImage.color = new Color(00, 255, 00);

    }

    // Update is called once per frame
    void Update()
    {
        if (SuperGlobal.money <= 0)
        {
        moneyImage.color = new Color(255, 10, 00);
        }
        else
        {
        moneyImage.color = new Color(00, 255, 00);
        }
    }
}
