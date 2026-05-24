using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Configuração de Varredura")]
    [SerializeField] private Transform _interactionPoint; // Ponto central da colisão (geralmente o peito do Padre)
    [SerializeField] private float _interactionRadius = 1.5f; // Distância do braço do Padre
    [SerializeField] private LayerMask _interactableLayer; // Filtro rígido para a Layer "Interativos"

    private InputAction _interactAction;
    private IInteractable _currentInteractable;

    private void Awake()
    {
        // Instanciação do botão de interação puramente via código
        _interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/f");
        _interactAction.AddBinding("<Gamepad>/buttonSouth"); // Botão 'A' no Xbox ou 'X' no PlayStation
    }

    private void OnEnable() => _interactAction.Enable();
    private void OnDisable() => _interactAction.Disable();

    private void Update()
    {
        CheckForInteractables();

        // Se houver um objeto válido próximo e o jogador pressionar F neste frame
        if (_currentInteractable != null && _interactAction.WasPressedThisFrame())
        {
            _currentInteractable.Interact();
        }
    }

    private void CheckForInteractables()
    {
        // Realiza uma varredura circular física na Layer específica
        Collider2D hit = Physics2D.OverlapCircle(_interactionPoint.position, _interactionRadius, _interactableLayer);

        if (hit != null)
        {
            // Tenta extrair o contrato do objeto encontrado
            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // Se encontramos um objeto NOVO, desliga o prompt do anterior (se houver) e liga o novo
                if (interactable != _currentInteractable)
                {
                    if (_currentInteractable != null) _currentInteractable.HidePrompt();
                    
                    _currentInteractable = interactable;
                    _currentInteractable.ShowPrompt();
                }
                return; // Mantém o objeto atualizado e encerra
            }
        }

        // Se a varredura falhou (afastou-se do objeto), limpa o estado
        if (_currentInteractable != null)
        {
            _currentInteractable.HidePrompt();
            _currentInteractable = null;
        }
    }

    // Desenha o círculo vermelho de debug no editor da Unity
    private void OnDrawGizmosSelected()
    {
        if (_interactionPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_interactionPoint.position, _interactionRadius);
    }
}