using UnityEngine;
using UnityEngine.Events;

public class EnvironmentStation : BaseInteractable
{
    [Header("Gatilho de Interface")]
    [Tooltip("Arraste o método do UIManager que esta mesa deve invocar no clique.")]
    [SerializeField] private UnityEvent _onInteract;

    public override void Interact()
    {
        _onInteract?.Invoke();
    }
}