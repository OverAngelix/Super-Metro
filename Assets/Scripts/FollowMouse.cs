using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    RectTransform rectTransform;
    Canvas canvas;

    void Start()
    {
        Cursor.visible = false;
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out mousePos);

        rectTransform.anchoredPosition = mousePos;
    }
}
