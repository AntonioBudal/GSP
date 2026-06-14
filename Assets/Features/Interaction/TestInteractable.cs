using UnityEngine;

// Exige que o GameObject tenha um colisor 2D para funcionar
[RequireComponent(typeof(Collider2D))]
public class TestInteractable : MonoBehaviour, IInteractable
{
    [Header("Configuração de Teste")]
    public PopupType popupToOpen;

    public void OnInteract()
    {
        // Quando o InteractionManager detectar o clique neste objeto, ele chama este método.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenPopup(popupToOpen);
        }
        else
        {
            Debug.LogWarning("[TestInteractable] UIManager não encontrado na cena!");
        }
    }
}