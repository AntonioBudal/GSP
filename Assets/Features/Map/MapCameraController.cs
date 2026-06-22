using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MapCameraController : MonoBehaviour
{
    [Header("Configurações de Zoom")]
    [SerializeField] private float zoomSpeed = 4f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 10f;

    [Header("Configurações de Pan (Arrasto)")]
    [SerializeField] private Vector2 mapLimitsX = new Vector2(-15f, 15f);
    [SerializeField] private Vector2 mapLimitsY = new Vector2(-10f, 10f);

    private Camera cam;
    private Vector3 dragOrigin;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    private void LateUpdate()
    {
        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            dragOrigin = GetMouseWorldPosition();
        }

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            Vector3 difference = dragOrigin - GetMouseWorldPosition();
            Vector3 newPosition = cam.transform.position + difference;

            newPosition.x = Mathf.Clamp(newPosition.x, mapLimitsX.x, mapLimitsX.y);
            newPosition.y = Mathf.Clamp(newPosition.y, mapLimitsY.x, mapLimitsY.y);
            
            cam.transform.position = newPosition;
        }
    }

    // Calcula a posição no mundo garantindo que a profundidade (Z)
    // é exatamente a distância da câmera até o plano 2D (Z=0)
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(cam.transform.position.z); 
        return cam.ScreenToWorldPoint(mousePos);
    }
}