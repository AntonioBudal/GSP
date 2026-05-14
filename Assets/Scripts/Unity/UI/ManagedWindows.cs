using UnityEngine;

public class ManagedWindow : MonoBehaviour
{
    private void OnEnable()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RegisterWindow(gameObject);
        }
    }

    private void OnDisable()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UnregisterWindow(gameObject);
        }
    }
    
    // Método auxiliar caso a janela tenha seu próprio botão individual de fechar
    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}