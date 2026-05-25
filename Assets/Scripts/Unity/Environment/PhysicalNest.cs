using UnityEngine;
using UnityEngine.UI;

public class PhysicalNest : MonoBehaviour
{
    [Header("UI Passiva")]
    [Tooltip("Imagem com Image Type ajustado para 'Filled' (Horizontal ou Radial)")]
    [SerializeField] private Image _progressBarFill;

    public string AssociatedCrowId { get; private set; }

    /// <summary>
    /// Tipagem forte na inicialização. Recebe a identidade de quem está gestando.
    /// </summary>
    public void Setup(string crowId)
    {
        AssociatedCrowId = crowId;
        UpdateProgress(0f); // Inicia vazio
    }

    /// <summary>
    /// Atualiza o preenchimento visual sem overhead de eventos de Slider.
    /// </summary>
    public void UpdateProgress(float progressPercentage)
    {
        if (_progressBarFill != null)
        {
            _progressBarFill.fillAmount = Mathf.Clamp01(progressPercentage);
        }
    }
}