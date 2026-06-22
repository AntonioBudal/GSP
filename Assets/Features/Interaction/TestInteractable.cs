using UnityEngine;
using System.Linq; // Necessário para filtrar a lista

[RequireComponent(typeof(Collider2D))]
public class TestInteractable : MonoBehaviour, IInteractable
{
    [Header("Configuração de Teste")]
    public PopupType popupToOpen;

    public void OnInteract()
    {
        if (UIManager.Instance != null)
        {
            // 1. Busca APENAS os corvos disponíveis na fonte de dados
            var availableRavens = SaveManager.Instance.CurrentSave.ravens
                .Where(r => r.state == RavenState.Available)
                .ToList();

            // 2. Envia a lista como "Payload" (o segundo parâmetro) para o UIManager
            UIManager.Instance.OpenPopup(popupToOpen, availableRavens);
        }
        else
        {
            Debug.LogWarning("[TestInteractable] UIManager não encontrado na cena!");
        }
    }
}