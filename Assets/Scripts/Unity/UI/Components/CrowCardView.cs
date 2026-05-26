using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CrowCardView : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _roleText;
    [SerializeField] private TextMeshProUGUI _stateText;
    
    // O texto agora pode ficar em cima da barra de vida!
    [SerializeField] private TextMeshProUGUI _lifespanText;

    [Header("Stat Bars (Image Type: Filled)")]
    [SerializeField] private Image _speedFill;
    [SerializeField] private Image _focusFill;
    [SerializeField] private Image _resilienceFill;
    [SerializeField] private float _maxStatValue = 10f; 

    [Header("Lifespan Bar")]
    [SerializeField] private Image _lifespanFill;
    [Tooltip("O tempo máximo de vida médio para calcular a barra (ex: 50 anos)")]
    [SerializeField] private float _maxLifespanValue = 50f;
    [SerializeField] private Color _healthyLifeColor = new Color(0.8f, 0.1f, 0.1f); // Vermelho escuro/sangue
    [SerializeField] private Color _dangerLifeColor = new Color(1f, 0f, 0f); // Vermelho vivo/alerta

    [Header("State Feedback")]
    [SerializeField] private Image _stateBackground;

    public void Setup(string crowName, CrowRole role, int speed, int focus, int resilience, int lifespan, CrowState state)
    {
        _nameText.text = crowName;
        _roleText.text = $"Papel: {role}";
        _stateText.text = state.ToString();

        // Texto com o valor exato (moeda de troca)
        _lifespanText.text = $"{lifespan} Anos";

        // Preenchimento das barras de atributos
        if (_speedFill != null) _speedFill.fillAmount = (float)speed / _maxStatValue;
        if (_focusFill != null) _focusFill.fillAmount = (float)focus / _maxStatValue;
        if (_resilienceFill != null) _resilienceFill.fillAmount = (float)resilience / _maxStatValue;

        // Preenchimento da barra de Vida Útil
        if (_lifespanFill != null)
        {
            float lifePercentage = Mathf.Clamp01((float)lifespan / _maxLifespanValue);
            _lifespanFill.fillAmount = lifePercentage;

            // Feedback Visual de Perigo: Se a vida for menor que 10 anos, a barra pisca/muda para alerta
            if (lifespan <= 10)
            {
                _lifespanFill.color = _dangerLifeColor;
            }
            else
            {
                _lifespanFill.color = _healthyLifeColor;
            }
        }

        // Feedback Visual Rápido via Cores de Fundo (Estado atual do Corvo)
        switch (state)
        {
            case CrowState.Disponivel: _stateBackground.color = new Color(0.2f, 0.8f, 0.2f, 0.5f); break; // Verde
            case CrowState.Fadigado: _stateBackground.color = new Color(0.8f, 0.8f, 0.2f, 0.5f); break; // Amarelo
            case CrowState.EmExpedicao: _stateBackground.color = new Color(0.2f, 0.5f, 0.8f, 0.5f); break; // Azul
            case CrowState.EmTreino: _stateBackground.color = new Color(0.6f, 0.2f, 0.8f, 0.5f); break; // Roxo
            case CrowState.Morto: _stateBackground.color = new Color(0.8f, 0.2f, 0.2f, 0.5f); break; // Vermelho
            default: _stateBackground.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); break; // Cinza
        }
    }
}