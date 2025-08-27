using UnityEngine;
using TMPro;

public class LocationTextManager : MonoBehaviour
{
    public TextMeshProUGUI locationNameUI;
    private Location location;

    void Update()
    {
        LocationController locationControllerscript = GetComponent<LocationController>();
        location = locationControllerscript.location;
        locationNameUI.text = location.name;
    }
}
