using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Telas Principais (Mutuamente Exclusivas)")]
    [Tooltip("Arraste aqui as janelas de fundo: Temple, Map, Bercario, etc.")]
    [SerializeField] private UIWindow[] _mainViews; 

    // Mantemos uma referência apenas para a janela de "Fallback" (quando o jogador aperta ESC)
    [SerializeField] private UIWindow _defaultHomeWindow; 

    [Header("Popups")]
    public UI_ExpeditionPopup popupExpedition;
    public UI_NurseryPopup popupNursery; // Se o Berçário for um popup modal em vez de tela cheia

    [Header("Treinamento")]
    public UI_TrainingPopup popupTraining;

    public void OpenTrainingPopup()
    {
    if (popupTraining != null) popupTraining.SetupAndShow();
    }

    [Header("Narrativa")]
    public UI_Soliloquy soliloquyPanel;
    private bool _isDisplayingSoliloquy = false;

    // Gerenciamento Dinâmico de popups e janelas arrastáveis
    private List<GameObject> _activeWindows = new List<GameObject>();

    [Header("Fim dos Tempos")]
    public GameObject panelExtinction;

    public void ShowExtinctionScreen()
    {
        CloseAllPopupsAndWindows();
        if (panelExtinction != null)
        {
            panelExtinction.SetActive(true);
            panelExtinction.transform.SetAsLastSibling(); // Garante que fica por cima do HUD
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Força o estado inicial limpando todas e abrindo apenas a Home (Templo)
        OpenMainView(_defaultHomeWindow);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllPopupsAndWindows();
        }
    }

    // ==========================================
    // SISTEMA ESCALÁVEL DE JANELAS (NOVO)
    // ==========================================

    /// <summary>
    /// Método único e genérico. Ao chamar este método por um Botão da UI, 
    /// passe a própria UIWindow que você quer abrir como parâmetro.
    /// </summary>
    public void OpenMainView(UIWindow targetView)
    {
        if (targetView == null) return;

        // Varre a lista de telas principais: liga a alvo, desliga o resto.
        foreach (var view in _mainViews)
        {
            if (view == null) continue;

            if (view == targetView)
                view.Show();
            else
                view.Hide();
        }
    }

    // ==========================================
    // SISTEMA DE DRAG & DROP E POPUPS
    // ==========================================

    public void RegisterWindow(GameObject window)
    {
        if (!_activeWindows.Contains(window))
        {
            _activeWindows.Add(window);
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
        for (int i = _activeWindows.Count - 1; i >= 0; i--)
        {
            if (_activeWindows[i] != null)
            {
                _activeWindows[i].SetActive(false);
            }
        }

        // Retorna o foco para o Templo ao limpar a tela
        OpenMainView(_defaultHomeWindow);
    }

    // ==========================================
    // MÉTODOS DE NARRATIVA E CASOS ESPECÍFICOS
    // ==========================================

    public void TryDisplayNextSoliloquy()
    {
        if (_isDisplayingSoliloquy) return;
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
        TryDisplayNextSoliloquy();
    }

    // Popups continuam precisando de métodos específicos caso precisem receber parâmetros (como o ID da região)
    public void OpenExpeditionPopup(string regionId)
    {
        if (popupExpedition != null) popupExpedition.SetupAndShow(regionId);
    }

    public void OpenCrowDetails(string crowId)
    {
        Debug.Log($"[UI] Solicitada abertura da ficha do corvo: {crowId}");
        // Quando criarmos a janela de detalhes, a chamada será:
        // if (popupCrowDetails != null) popupCrowDetails.SetupAndShow(crowId);
    }
}