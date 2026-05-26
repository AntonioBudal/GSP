using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [Header("Configuração Visual do Prompt")]
    [SerializeField] private GameObject _promptContainer;
    [SerializeField] private TextMeshPro _promptText;
    [SerializeField] private string _actionDescription = "Interagir";

    public string InteractionPrompt => _actionDescription;

    protected virtual void Start()
    {
        if (_promptContainer != null) 
            _promptContainer.SetActive(false);
    }

    public void ShowPrompt()
    {
        if (_promptContainer != null)
        {
            if (_promptText != null) 
                _promptText.text = $"[F] {_actionDescription}";
            
            _promptContainer.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (_promptContainer != null) 
            _promptContainer.SetActive(false);
    }

    // Cada objeto filho decidirá o que acontece ao sofrer o input do jogador
    public abstract void Interact();
}