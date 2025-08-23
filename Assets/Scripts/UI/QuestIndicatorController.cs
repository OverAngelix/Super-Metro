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
       Canvas.ForceUpdateCanvases(); // force tous les layouts à se recalculer

        cursorImage.gameObject.SetActive(true);
        RectTransform cursorRect = cursorImage.rectTransform;
        RectTransform buttonRect = buttonToClick.GetComponent<RectTransform>();

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, buttonRect.position);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(cursorRect.parent as RectTransform, screenPoint, null, out localPoint);

        float width = buttonRect.rect.width;
        float height = buttonRect.rect.height;

        cursorRect.localPosition = new Vector3(localPoint.x + width / 2, localPoint.y - height / 2, cursorRect.localPosition.z);
    }

    public void HideCursor()
    {
        cursorImage.gameObject.SetActive(false);
    }
}
