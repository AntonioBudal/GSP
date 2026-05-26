using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Configuração de Varredura")]
    [SerializeField] private Transform _interactionPoint; 
    [SerializeField] private float _interactionRadius = 1.5f; 
    [SerializeField] private LayerMask _interactableLayer; 

    private InputAction _interactAction;
    private IInteractable _currentInteractable;

    private void Awake()
    {
        if (_interactionPoint == null)
        {
            Debug.LogError($"[{nameof(PlayerInteractor)}] Ponto de interação nulo.", this);
            enabled = false;
            return;
        }

        _interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/f");
        _interactAction.AddBinding("<Gamepad>/buttonSouth"); 
    }

    private void OnEnable() => _interactAction.Enable();
    private void OnDisable() => _interactAction.Disable();

    private void Update()
    {
        CheckForInteractables();

        if (_currentInteractable != null && _interactAction.WasPressedThisFrame())
        {
            _currentInteractable.Interact();
        }
    }

    private void CheckForInteractables()
    {
        Collider2D hit = Physics2D.OverlapCircle(_interactionPoint.position, _interactionRadius, _interactableLayer);

        if (hit != null)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (interactable != _currentInteractable)
                {
                    if (_currentInteractable != null) _currentInteractable.HidePrompt();
                    
                    _currentInteractable = interactable;
                    _currentInteractable.ShowPrompt();
                }
                return; 
            }
        }

        if (_currentInteractable != null)
        {
            _currentInteractable.HidePrompt();
            _currentInteractable = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_interactionPoint == null) return;
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(_interactionPoint.position, _interactionRadius);
    }
}