using UnityEngine;

public class CustomCameraController : MonoBehaviour
{
    [Header("Vitesse")]
    public float moveSpeed = 10f;
    public float zoomSpeed = 20f;
    public float rotationSpeed = 50f;

    private Vector3 lastMousePos;

    void Update()
    {
        HandleMouseDrag();
        HandleZoom();
    }

    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            lastMousePos = Input.mousePosition;
        }

        Vector3 delta = Input.mousePosition - lastMousePos;

        // Clic gauche → translation X/Z
        if (Input.GetMouseButton(0))
        {
            Vector3 move = moveSpeed * Time.deltaTime * new Vector3(delta.x, 0, delta.y);
            transform.Translate(move, Space.World);

        }

        // Clic droit → rotation sur Z
        if (Input.GetMouseButton(1))
        {
            float roll = -delta.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, roll, Space.Self);
        }

        lastMousePos = Input.mousePosition;
    }

    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            // Calculer la nouvelle position Y
            float newY = transform.position.y - scroll * zoomSpeed * Time.deltaTime;

            newY = Mathf.Clamp(newY, 5f, 90f);
            
            // Appliquer seulement la différence sur Y
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
