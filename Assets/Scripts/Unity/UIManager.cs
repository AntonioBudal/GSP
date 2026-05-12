using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Janelas Principais")]
    public UIWindow windowTemple;
    public UIWindow windowMap;

    [Header("Popups")]
    public UI_ExpeditionPopup popupExpedition;

    // Adicione esta variável junto com as outras (abaixo de popupExpedition)
    [Header("Narrativa")]
    public UI_Soliloquy soliloquyPanel;
    private bool _isDisplayingSoliloquy = false;

    // --- MÉTODOS DE NARRATIVA ---

    public void TryDisplayNextSoliloquy()
    {
        // Se já tem um texto na tela, ignora. A fila vai segurar a próxima fala.
        if (_isDisplayingSoliloquy) return;
        
        // Proteção caso o SoliloquyDirector ainda não tenha sido instanciado
        if (SoliloquyDirector.Instance == null) return;

        var nextThought = SoliloquyDirector.Instance.GetNextThought();
        
        if (nextThought != null && soliloquyPanel != null)
        {
            _isDisplayingSoliloquy = true;
            soliloquyPanel.ShowText(nextThought.Text);
        }
    }

    public void OnSoliloquyFinished()
    {
        _isDisplayingSoliloquy = false;
        
        // Tenta puxar o próximo da fila imediatamente após o atual sumir
        TryDisplayNextSoliloquy();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Estado inicial da interface
        if (windowMap != null) windowMap.HideImmediate();
        if (windowTemple != null) windowTemple.ShowImmediate();
    }

    public void OpenTemple()
    {
        if (windowMap != null) windowMap.Hide();
        if (windowTemple != null) windowTemple.Show();
    }

    public void OpenMap()
    {
        if (windowTemple != null) windowTemple.Hide();
        if (windowMap != null) windowMap.Show();
    }

    // O método que o C# não estava achando agora está aqui, devidamente isolado
    public void OpenExpeditionPopup(string regionId)
    {
        if (popupExpedition != null)
        {
            popupExpedition.SetupAndShow(regionId);
        }
    }
}