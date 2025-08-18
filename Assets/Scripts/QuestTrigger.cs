using UnityEngine;


public class QuestTrigger : MonoBehaviour
{
    public Quest linkedQuest;

    public void OnButtonClicked()
    {
        if (linkedQuest != null)
        {
            linkedQuest.ForceComplete();
        }
    }
}
