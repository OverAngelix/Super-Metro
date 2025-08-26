using UnityEngine;
using System.Linq;

public class StatisticsMovementsUIController : MonoBehaviour
{
    public static StatisticsMovementsUIController Instance;
    public GameObject statisticsMovementsLineUI;
    public Transform content;

    void Update()
    {
        RefreshUI();
    }
    public void RefreshUI()
    {
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Location spot in SuperGlobal.spots)
        {
            GameObject obj = Instantiate(statisticsMovementsLineUI, content);
            StatisticsMovementsLineUIController controller = obj.GetComponent<StatisticsMovementsLineUIController>();
            controller.nameValue = spot.name;
            controller.fromHappinessValue = spot.peopleFromLocation.Any()
                                            ? spot.peopleFromLocation.Average(p => p.happiness)
                                            : 0f;
            controller.toHappinessValue = spot.peopleToLocation.Any()
                                            ? spot.peopleToLocation.Average(p => p.happiness)
                                            : 0f;
            controller.RefreshUI();
        }
    }
}
