using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler
{
    [Tooltip("Arraste a janela raiz aqui (a que vai se mover). Se vazio, ele pega o pai automático.")]
    [SerializeField] private RectTransform _windowRoot;

    private RectTransform _canvasRect;
    private Vector2 _pointerOffset;

    private void Awake()
    {
        // Se esqueceu de linkar, tenta achar a janela pai
        if (_windowRoot == null) _windowRoot = transform.parent.GetComponent<RectTransform>();
        
        // Acha o Canvas raiz para fazer o cálculo correto independente da resolução
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null) _canvasRect = canvas.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_windowRoot != null)
        {
            // O SEGREDO: Se o cartão estiver preso em um Layout Group (como a lista de corvos)
            if (_windowRoot.parent.GetComponent<UnityEngine.UI.LayoutGroup>() != null)
            {
                // Libertamos o cartão! Mudamos o "pai" dele para ser o pai do Layout Group (ex: a Window_Temple).
                // O "true" no final garante que o cartão não pule na tela ao trocar de pai.
                _windowRoot.SetParent(_windowRoot.parent.parent, true);
            }

            // Agora que ele está livre, jogamos ele para a frente da tela.
            _windowRoot.SetAsLastSibling();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_canvasRect == null) return;

        // Calcula a diferença entre o clique exato do mouse e o centro real do painel
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, 
            eventData.position, 
            eventData.pressEventCamera, 
            out Vector2 localPointerPosition
        );
        
        _pointerOffset = (Vector2)_windowRoot.localPosition - localPointerPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_canvasRect == null || _windowRoot == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, 
            eventData.position, 
            eventData.pressEventCamera, 
            out Vector2 localPointerPosition))
        {
            // Move a janela
            _windowRoot.localPosition = localPointerPosition + _pointerOffset;
            ClampToScreen();
        }
    }

    // Evita que o jogador arraste a janela para fora do monitor e perca ela
    private void ClampToScreen()
    {
        Vector3[] canvasCorners = new Vector3[4];
        _canvasRect.GetWorldCorners(canvasCorners);

        Vector3[] windowCorners = new Vector3[4];
        _windowRoot.GetWorldCorners(windowCorners);

        float minX = canvasCorners[0].x;
        float maxX = canvasCorners[2].x;
        float minY = canvasCorners[0].y;
        float maxY = canvasCorners[2].y;

        Vector3 clampedPosition = _windowRoot.position;

        if (windowCorners[0].x < minX) clampedPosition.x += minX - windowCorners[0].x;
        if (windowCorners[2].x > maxX) clampedPosition.x -= windowCorners[2].x - maxX;
        
        if (windowCorners[0].y < minY) clampedPosition.y += minY - windowCorners[0].y;
        if (windowCorners[2].y > maxY) clampedPosition.y -= windowCorners[2].y - maxY;

        _windowRoot.position = clampedPosition;
    }
}