using UnityEngine;
using TMPro;

public class StatisticsUIController : MonoBehaviour
{
    public static StatisticsUIController Instance;
    public GameObject content;
    public TMP_Dropdown dropdown;

    void Awake()
    {
        Instance = this;
        GetComponent<Canvas>().enabled = true;
        gameObject.SetActive(false);
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }

    private void OnDropdownValueChanged(int index)
    {
        RefreshUI(index);
    }

    public void RefreshUI(int index)
    {
        switch (index)
        {
            case 0:
                Debug.Log("Option 0 sélectionnée → fais un truc");
                break;

            case 1:
                Debug.Log("Option 1 sélectionnée → fais autre chose");
                break;

            case 2:
                Debug.Log("Option 2 sélectionnée → encore une autre action");
                break;
        }
    }
}
