using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Janelas Principais")]
    public UIWindow windowTemple;
    public UIWindow windowMap;

    [Header("Popups")]
    public UI_ExpeditionPopup popupExpedition;

    [Header("Narrativa")]
    public UI_Soliloquy soliloquyPanel;
    private bool _isDisplayingSoliloquy = false;

    // --- NOVO: GERENCIAMENTO DINÂMICO DE JANELAS (FASE 5.3) ---
    private List<GameObject> _activeWindows = new List<GameObject>();

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

    private void Update()
    {
        // Se o jogador apertar ESC, limpa a tela de pop-ups e volta para a base
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllPopupsAndWindows();
        }
    }

    // ==========================================
    // SISTEMA DE DRAG & DROP / FOCO (FASE 5.3)
    // ==========================================

    public void RegisterWindow(GameObject window)
    {
        if (!_activeWindows.Contains(window))
        {
            _activeWindows.Add(window);
            // Sempre que uma janela/popup abre, ela é jogada para a frente de todas as outras
            window.transform.SetAsLastSibling(); 
        }
    }

    public void UnregisterWindow(GameObject window)
    {
        if (_activeWindows.Contains(window))
        {
            _activeWindows.Remove(window);
        }
    }

    public void CloseAllPopupsAndWindows()
    {
        // Iteramos de trás para frente porque o SetActive(false) vai acionar o OnDisable() 
        // dos pop-ups, o que vai removê-los desta lista automaticamente.
        for (int i = _activeWindows.Count - 1; i >= 0; i--)
        {
            if (_activeWindows[i] != null)
            {
                _activeWindows[i].SetActive(false);
            }
        }

        // Tática de UX: Ao fechar todos os pop-ups, voltamos a exibir o Templo 
        // para o jogador não ficar encarando uma tela vazia sem janelas.
        OpenTemple();
    }

    // ==========================================
    // MÉTODOS DE NARRATIVA E JANELAS FIXAS
    // ==========================================

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

    public void OpenExpeditionPopup(string regionId)
    {
        if (popupExpedition != null)
        {
            popupExpedition.SetupAndShow(regionId);
        }
    }
}