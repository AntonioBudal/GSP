using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private Camera mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Fazemos cache da câmera principal para não usar Camera.main no Update (o que custa performance)
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Detecta o clique com o botão esquerdo do mouse
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        // Converte a posição do mouse na tela para uma coordenada no mundo 2D
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Verifica se há algum Collider2D exatamente naquele ponto
        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPosition);

        if (hitCollider != null)
        {
            // Tenta pegar o componente que implementa a nossa interface
            IInteractable interactable = hitCollider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                Debug.Log($"[InteractionManager] Clicou em um objeto interativo: {hitCollider.gameObject.name}");
                interactable.OnInteract();
            }
        }
    }
}