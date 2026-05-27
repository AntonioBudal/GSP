using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Popups Físicos (Mesas)")]
    public UI_ExpeditionPopup popupExpedition;
    public UI_NurseryPopup popupNursery;
    public UI_TrainingPopup popupTraining;

    [Header("Narrativa e Fim de Jogo")]
    public UI_Soliloquy soliloquyPanel;
    public GameObject panelExtinction;

    // Pilha LIFO (Last In, First Out) para fechar janelas com o ESC na ordem certa
    private Stack<UIWindow> _activeModals = new Stack<UIWindow>();
    private bool _isDisplayingSoliloquy = false;

    // ==========================================
    // ROTEAMENTO DE ENTIDADES FÍSICAS
    // ==========================================

    [Header("Detalhes da Entidade")]
    public UIWindow popupCrowDetails; // Arraste a janela de ficha do corvo aqui no editor depois

    public void OpenCrowDetails(string crowId)
    {
        // Por enquanto, apenas registramos no log para a mecânica física funcionar
        Debug.Log($"[UIManager] O Padre está inspecionando o corvo: {crowId}");
        
        if (popupCrowDetails != null)
        {
            // Quando a sua janela de detalhes for recriada, ela receberá o ID aqui:
            // popupCrowDetails.GetComponent<SuaClasseDeDetalhes>().SetupAndShow(crowId);
            
            popupCrowDetails.Show();
            RegisterModal(popupCrowDetails);
        }
    }

    

    private void Update()
    {
        // Fecha o popup que estiver no topo da pilha
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTopModal();
        }
    }

    // ==========================================
    // ROTEAMENTO PARA AS MESAS FÍSICAS
    // ==========================================

    public void OpenTrainingPopup()
    {
        if (popupTraining != null)
        {
            popupTraining.SetupAndShow();
            RegisterModal(popupTraining.GetComponent<UIWindow>());
        }
    }

    public void OpenNurseryPopup()
    {
        if (popupNursery != null)
        {
            popupNursery.SetupAndShow();
            RegisterModal(popupNursery.GetComponent<UIWindow>());
        }
    }

    public void OpenExpeditionPopup(string regionId)
    {
        if (popupExpedition != null)
        {
            popupExpedition.SetupAndShow(regionId);
            RegisterModal(popupExpedition.GetComponent<UIWindow>());
        }
    }

    // ==========================================
    // CONTROLE DE PILHA (STACK)
    // ==========================================

    private void RegisterModal(UIWindow window)
    {
        if (window == null) return;
        
        // Garante que a janela renderize por cima de tudo
        window.transform.SetAsLastSibling(); 
        
        if (!_activeModals.Contains(window))
        {
            _activeModals.Push(window);
        }
    }

    public void CloseTopModal()
    {
        if (_activeModals.Count > 0)
        {
            UIWindow topModal = _activeModals.Pop();
            if (topModal != null) topModal.Hide();
        }
    }

    public void CloseAllModals()
    {
        while (_activeModals.Count > 0)
        {
            CloseTopModal();
        }
    }

    // ==========================================
    // NARRATIVA E SISTEMA
    // ==========================================

    public void TryDisplayNextSoliloquy()
    {
        if (_isDisplayingSoliloquy || SoliloquyDirector.Instance == null) return;

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

    public void ShowExtinctionScreen()
    {
        CloseAllModals();
        if (panelExtinction != null)
        {
            panelExtinction.SetActive(true);
            panelExtinction.transform.SetAsLastSibling();
        }
    }
}