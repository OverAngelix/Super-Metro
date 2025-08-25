using UnityEngine;
using UnityEngine.UI;

public class QuestIndicatorController : MonoBehaviour
{

    public RawImage cursorImage;
    public Button forceButtonToClick;

    public static QuestIndicatorController Instance;
    void Awake()
    {
        Instance = this;
    }

    public void ShowCursorOnButton(Button buttonToClick)
    {
        if (forceButtonToClick) buttonToClick = forceButtonToClick;
        Canvas.ForceUpdateCanvases(); // force tous les layouts à se recalculer

        cursorImage.gameObject.SetActive(true);
        RectTransform cursorRect = cursorImage.rectTransform;
        RectTransform buttonRect = buttonToClick.GetComponent<RectTransform>();

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, buttonRect.position);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(cursorRect.parent as RectTransform, screenPoint, cursorImage.GetComponentInParent<Canvas>().worldCamera, out localPoint);

        float width = cursorRect.rect.width;
        float height = cursorRect.rect.height;

        // width = 0;
        // height = 0;

        cursorRect.anchoredPosition = new Vector3(localPoint.x + width / 2 , localPoint.y - height / 2.5f, cursorRect.localPosition.z);

        // cursorRect.localPosition = new Vector3(localPoint.x, localPoint.y, cursorRect.localPosition.z);
    
    }

    public void HideCursor()
    {
        cursorImage.gameObject.SetActive(false);
    }
}
