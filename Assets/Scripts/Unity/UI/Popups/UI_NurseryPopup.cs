using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(UIWindow))]
public class UI_NurseryPopup : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private Button _btnClose; // Btn_Close

    [Header("Body")]
    [SerializeField] private TMP_Dropdown _parentADropdown; 
    [SerializeField] private TMP_Dropdown _parentBDropdown; 
    [SerializeField] private TMP_InputField _lineageNameInput; // Input_LineageName

    [Header("Footer / Status (Bottom_StatusBar)")]
    [SerializeField] private Button _btnBreed; // Btn_Breed
    [SerializeField] private TextMeshProUGUI _feedbackText; // Footer_Report

    private UIWindow _window;
    private List<Crow> _availableParents = new List<Crow>();

    private void Awake()
    {
        _window = GetComponent<UIWindow>();
        
        // Atribuição imutável dos eventos de botões
        _btnClose.onClick.AddListener(() => UIManager.Instance.CloseTopModal());
        _btnBreed.onClick.AddListener(OnBreedClicked);
    }

    public void SetupAndShow()
    {
        // 1. O Presenter não guarda estado, sempre puxa fresco
        _lineageNameInput.text = string.Empty;
        
        // Texto litúrgico padrão
        _feedbackText.text = "O registro aguarda os genitores para o sacrifício genético.";
        
        RefreshDropdowns();
        _window.Show();
    }

    private void RefreshDropdowns()
    {
        _parentADropdown.ClearOptions();
        _parentBDropdown.ClearOptions();
        _availableParents.Clear();

        // Consulta State-less: Lê direto da fonte da verdade
        var allCrows = GameBootstrap.Instance.CrowRepo.GetAllCrows();
        
        foreach (var crow in allCrows)
        {
            // Apenas aves no estado base podem realizar o ritual
            if (crow.CurrentState == CrowState.Disponivel)
            {
                _availableParents.Add(crow);
            }
        }

        // Trava visual (Disabled State) se não houver o mínimo para reprodução
        if (_availableParents.Count < 2)
        {
            var emptyOption = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("Aves insuficientes") };
            _parentADropdown.AddOptions(emptyOption);
            _parentBDropdown.AddOptions(emptyOption);
            
            _btnBreed.interactable = false;
        }
        else
        {
            // Preenche os dados formatados conforme design (Ex: "Morsgar (V:7 F:5 R:8)")
            var options = _availableParents.Select(c => 
                new TMP_Dropdown.OptionData($"{c.ID} (V:{c.Speed} F:{c.Focus} R:{c.Resilience})")
            ).ToList();
            
            _parentADropdown.AddOptions(options);
            _parentBDropdown.AddOptions(options);
            
            _btnBreed.interactable = true;
        }
    }

    private void OnBreedClicked()
    {
        if (_availableParents.Count < 2) return;

        var parentA = _availableParents[_parentADropdown.value];
        var parentB = _availableParents[_parentBDropdown.value];
        
        // Aplica o Fallback caso o jogador não nomeie a linhagem
        string childName = string.IsNullOrWhiteSpace(_lineageNameInput.text) 
            ? "Filhote_SemNome" 
            : _lineageNameInput.text;

        // O Presenter apenas transmite a ordem. O Domínio julga, executa e devolve a sentença.
        string message;
        bool success = GameBootstrap.Instance.Breeding.StartBreeding(parentA, parentB, childName, 3, out message);
        
        // Atualiza o Footer_Report com a decisão do Manager
        _feedbackText.text = message;

        if (success)
        {
            // Limpa o documento para evitar duplicatas acidentais e recarrega o censo
            _lineageNameInput.text = string.Empty;
            RefreshDropdowns();
        }
    }
}