// Assets/Scripts/Unity/UI_MapNodeController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MapNodeController : MonoBehaviour
{
    public string RegionID { get; private set; }
    
    [SerializeField] private Image _nodeImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Button _interactButton;

    public void Setup(Region region)
    {
        RegionID = region.ID;
        _nameText.text = region.Name;
        UpdateVisualState(region.CurrentState);

        // Limpa ouvintes antigos e adiciona o evento de clique (Fase 2.4)
        _interactButton.onClick.RemoveAllListeners();
        _interactButton.onClick.AddListener(() => 
        {
            UIManager.Instance.OpenExpeditionPopup(RegionID);
        });

        
    }

    public void UpdateVisualState(FogState state)
    {
        switch (state)
        {
            case FogState.Oculto: 
                _nodeImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Quase preto
                _interactButton.interactable = false;
                break;
            case FogState.Descoberto: 
                _nodeImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Cinza
                _interactButton.interactable = true;
                break;
            case FogState.Explorado: 
                _nodeImage.color = new Color(0.8f, 0.7f, 0.2f, 1f); // Dourado
                _interactButton.interactable = true;
                break;
        }
    }
}