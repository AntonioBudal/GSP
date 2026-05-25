using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class EnvironmentStation : MonoBehaviour, IInteractable
{
    public enum StationType { Training, Nursery }

    [Header("Configuração")]
    [SerializeField] private StationType _stationType;
    [SerializeField] private string _promptName = "Mesa de Tortura";

    [Header("Visuais")]
    [SerializeField] private GameObject _floatingPrompt;
    [SerializeField] private TextMeshPro _promptText;

    public string InteractionPrompt => _promptName;

    private void Start()
    {
        if (_floatingPrompt != null) _floatingPrompt.SetActive(false);
    }

    public void ShowPrompt()
    {
        if (_floatingPrompt != null)
        {
            if (_promptText != null) _promptText.text = $"[F] {_promptName}";
            _floatingPrompt.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (_floatingPrompt != null) _floatingPrompt.SetActive(false);
    }

    public void Interact()
    {
        // Redireciona para o painel correto no UIManager baseado no tipo da estação
        switch (_stationType)
        {
            case StationType.Training:
                UIManager.Instance.OpenTrainingPopup();
                break;

            case StationType.Nursery:
                // Como não criei o método específico OpenNurseryPopup antes, chamamos o objeto direto.
                // Se o seu UIManager tiver um OpenNurseryPopup(), use-o.
                if (UIManager.Instance.popupNursery != null)
                    UIManager.Instance.popupNursery.gameObject.SetActive(true);
                break;
        }
    }
}