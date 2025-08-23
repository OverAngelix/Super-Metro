using UnityEngine;
using UnityEngine.UI;

public class QuestIndicatorController : MonoBehaviour
{

    public RawImage cursorImage;

    public static QuestIndicatorController Instance;
    void Awake()
    {
        Instance = this;
    }

    public void ShowCursorOnButton(Button buttonToClick)
    {
        cursorImage.gameObject.SetActive(true);
        RectTransform cursorRect = cursorImage.rectTransform;
        RectTransform buttonRect = buttonToClick.GetComponent<RectTransform>();

        Vector2 buttonCenter = buttonRect.rect.center;
        float width = buttonRect.rect.width;
        float height = buttonRect.rect.height;
        Vector3 worldCenter = buttonRect.TransformPoint(buttonCenter);
        Vector3 localPos = cursorRect.parent.InverseTransformPoint(worldCenter);

        cursorRect.localPosition = new Vector3(localPos.x + width / 2, localPos.y - height / 2, localPos.z);
    }

    public void HideCursor()
    {
        Debug.Log("hide");
        cursorImage.gameObject.SetActive(false);
    }
}
