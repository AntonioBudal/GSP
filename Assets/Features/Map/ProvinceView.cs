using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class ProvinceView : MonoBehaviour, IInteractable
{
    [Header("Identidade")]
    public string provinceId; 

    [Header("Feedback Visual")]
    [SerializeField] private Color hiddenColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color frontierColor = new Color(0.8f, 0.6f, 0.1f, 1f);
    [SerializeField] private Color revealedColor = new Color(1f, 1f, 1f, 1f);

    private SpriteRenderer spriteRenderer;
    private ProvinceStatus currentStatus;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        RefreshVisualState();
    }

    public void RefreshVisualState()
    {
        if (MapManager.Instance == null) return;

        // Consulta direta e encapsulada, sem tocar no SaveManager
        currentStatus = MapManager.Instance.GetProvinceStatus(provinceId);

        switch (currentStatus)
        {
            case ProvinceStatus.Hidden: 
                spriteRenderer.color = hiddenColor; 
                break;
            case ProvinceStatus.Frontier: 
                spriteRenderer.color = frontierColor; 
                break;
            case ProvinceStatus.Revealed: 
                spriteRenderer.color = revealedColor; 
                break;
        }
    }

    public void OnInteract()
    {
        switch (currentStatus)
        {
            case ProvinceStatus.Hidden:
                Debug.Log($"[ProvinceView] {provinceId} está inexplorada.");
                break;
            
            case ProvinceStatus.Frontier:
                // Abre o popup de expedição injetando o ID da província como payload
                UIManager.Instance.OpenPopup(PopupType.Expedition, provinceId);
                break;
            
            case ProvinceStatus.Revealed:
                Debug.Log($"[ProvinceView] {provinceId} já é território conhecido.");
                break;
        }
    }
}