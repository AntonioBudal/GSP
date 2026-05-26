using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MapNodeController : MonoBehaviour
{
    public string RegionID { get; private set; }
    
    [SerializeField] private Image _nodeImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _influenceText; // NOVO: Texto de Almas
    [SerializeField] private Button _interactButton;

    public void Setup(Region region)
    {
        RegionID = region.ID;
        _nameText.text = region.Name;
        UpdateVisualState(region.CurrentState);

        // Atualiza o texto de influência lendo a fonte da verdade
        UpdateInfluenceDisplay();

        _interactButton.onClick.RemoveAllListeners();
        _interactButton.onClick.AddListener(() => 
        {
            UIManager.Instance.OpenExpeditionPopup(RegionID);
        });
    }

    public void UpdateInfluenceDisplay()
    {
        // Pede a informação fresca ao Bootstrap
        if (GameBootstrap.Instance.Influence.TryGetInfluence(RegionID, out var runtime))
        {
            // Oculta fiéis se a região estiver sob Névoa Absoluta
            if (GameBootstrap.Instance.Map.GetRegion(RegionID)?.CurrentState == FogState.Oculto)
            {
                _influenceText.text = "??? Fiéis";
            }
            else
            {
                _influenceText.text = $"{runtime.Believers} Fiéis ({runtime.BelieverPercentage:P0})";
            }
        }
    }

    public void UpdateVisualState(FogState state)
    {
        switch (state)
        {
            case FogState.Oculto: 
                _nodeImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); 
                _interactButton.interactable = false;
                break;
            case FogState.Descoberto: 
                _nodeImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); 
                _interactButton.interactable = true;
                break;
            case FogState.Explorado: 
                _nodeImage.color = new Color(0.8f, 0.7f, 0.2f, 1f); 
                _interactButton.interactable = true;
                break;
        }
    }
}