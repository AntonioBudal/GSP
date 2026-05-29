using UnityEngine;
using UnityEngine.EventSystems;

public class UI_MapCameraController : MonoBehaviour, IDragHandler, IScrollHandler
{
    [Header("Arquitetura")]
    [Tooltip("O objeto pai que contém todos os gráficos (Fundo, Linhas, Nós).")]
    [SerializeField] private RectTransform _content;
    [Tooltip("O buraco da fechadura (A máscara dentro da moldura decorativa).")]
    [SerializeField] private RectTransform _viewport;
    [Tooltip("Necessário para manter a mesma velocidade de arrasto em 1080p ou 4K.")]
    [SerializeField] private Canvas _parentCanvas; 

    [Header("Zoom Tático")]
    [SerializeField] private float _zoomSpeed = 0.15f;
    [SerializeField] private float _minZoom = 1f;
    [SerializeField] private float _maxZoom = 4f;

    [Header("Pixel Art Quality")]
    [Tooltip("Arredonda a posição para inteiros para evitar que a Pixel Art fique embaçada (sub-pixel blurring).")]
    [SerializeField] private bool _snapToPixels = true;

    private Vector2 _targetPosition;
    private float _targetZoom = 1f;

    private void Awake()
    {
        if (_content != null)
        {
            _targetPosition = _content.anchoredPosition;
            _targetZoom = _content.localScale.x;
        }

        if (_parentCanvas == null)
            _parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (_content == null || _parentCanvas == null) return;

        float scroll = eventData.scrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        // 1. Registra onde o mouse está no espaço LOCAL do mapa ANTES do zoom
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _content, 
            eventData.position, 
            eventData.pressEventCamera, 
            out Vector2 mouseLocalPos
        );

        // 2. Calcula o novo zoom
        float oldZoom = _targetZoom;
        _targetZoom += scroll * _zoomSpeed;
        _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);

        // 3. Aplica o Zoom
        _content.localScale = new Vector3(_targetZoom, _targetZoom, 1f);

        // 4. A Magia: Compensa a posição do Pivot para que o mapa cresça em direção ao cursor
        Vector2 shift = mouseLocalPos * (_targetZoom - oldZoom);
        _targetPosition -= shift;

        ApplyTransform();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_content == null || _parentCanvas == null) return;

        // O delta bruto lido do hardware é normalizado pela escala atual da tela
        _targetPosition += eventData.delta / _parentCanvas.scaleFactor;
        
        ApplyTransform();
    }

    private void ApplyTransform()
    {
        ClampPosition();

        // Evita saltos visuais e borrões na Pixel Art
        if (_snapToPixels)
        {
            _content.anchoredPosition = new Vector2(
                Mathf.Round(_targetPosition.x),
                Mathf.Round(_targetPosition.y)
            );
        }
        else
        {
            _content.anchoredPosition = _targetPosition;
        }
    }

    private void ClampPosition()
    {
        if (_viewport == null || _content == null) return;

        // Tamanho real do mapa com o zoom aplicado
        float currentWidth = _content.rect.width * _targetZoom;
        float currentHeight = _content.rect.height * _targetZoom;

        // Tamanho da janela visual (Limitador)
        float viewWidth = _viewport.rect.width;
        float viewHeight = _viewport.rect.height;

        float minX = 0f, maxX = 0f;
        float minY = 0f, maxY = 0f;

        // O mapa só bate nas bordas se for maior que a janela de visualização
        if (currentWidth > viewWidth)
        {
            maxX = (currentWidth - viewWidth) / 2f;
            minX = -maxX;
        }

        if (currentHeight > viewHeight)
        {
            maxY = (currentHeight - viewHeight) / 2f;
            minY = -maxY;
        }

        _targetPosition.x = Mathf.Clamp(_targetPosition.x, minX, maxX);
        _targetPosition.y = Mathf.Clamp(_targetPosition.y, minY, maxY);
    }
}