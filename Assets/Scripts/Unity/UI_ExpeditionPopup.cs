// Assets/Scripts/Unity/UI_ExpeditionPopup.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(UIWindow))]
public class UI_ExpeditionPopup : MonoBehaviour
{
    [Header("Componentes UI")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TMP_Dropdown _crowDropdown;
    [SerializeField] private Button _btnRecon;
    [SerializeField] private Button _btnEvangelize;
    [SerializeField] private Button _btnClose;
    [SerializeField] private TextMeshProUGUI _feedbackText;

    private UIWindow _window;
    private string _currentRegionId;
    private List<Crow> _availableCrows = new List<Crow>();

    private void Awake()
    {
        _window = GetComponent<UIWindow>();
        
        _btnRecon.onClick.AddListener(OnReconClicked);
        _btnEvangelize.onClick.AddListener(OnEvangelizeClicked);
        _btnClose.onClick.AddListener(() => _window.Hide());
    }

    public void SetupAndShow(string regionId)
    {
        _currentRegionId = regionId;
        var region = GameBootstrap.Instance.Map.GetRegion(regionId);
        
        _titleText.text = $"Alvo: {region?.Name ?? regionId}";
        _feedbackText.text = "Selecione uma ave e a missão.";

        RefreshDropdown();
        _window.Show();
    }

    private void RefreshDropdown()
    {
        _crowDropdown.ClearOptions();
        _availableCrows.Clear();

        // Consulta State-less: Lê direto da fonte da verdade
        var allCrows = GameBootstrap.Instance.CrowRepo.GetAllCrows();
        
        foreach (var crow in allCrows)
        {
            // Filtra apenas os que estão em casa e prontos
            if (crow.CurrentState == CrowState.Disponivel)
            {
                _availableCrows.Add(crow);
            }
        }

        if (_availableCrows.Count == 0)
        {
            _crowDropdown.options.Add(new TMP_Dropdown.OptionData("Nenhuma ave disponível"));
            _crowDropdown.interactable = false;
            _btnRecon.interactable = false;
            _btnEvangelize.interactable = false;
        }
        else
        {
            var options = _availableCrows.Select(c => 
                new TMP_Dropdown.OptionData($"{c.ID} (VEL: {c.Speed} FOC: {c.Focus})")
            ).ToList();
            
            _crowDropdown.AddOptions(options);
            _crowDropdown.interactable = true;
            _btnRecon.interactable = true;
            _btnEvangelize.interactable = true;
        }
        
        _crowDropdown.RefreshShownValue();
    }

    private void OnReconClicked()
    {
        if (_availableCrows.Count == 0) return;
        var selectedCrow = _availableCrows[_crowDropdown.value];
        
        // Chamando o método real do seu ExpeditionManager, passando o Enum correto
        string message;
        bool success = GameBootstrap.Instance.Expeditions.SendToExpedition(selectedCrow.ID, MissionType.Reconhecimento, _currentRegionId, out message);
        
        _feedbackText.text = message;
        if (success) RefreshDropdown(); // Atualiza a lista, pois o corvo sumiu (foi viajar)
    }

    private void OnEvangelizeClicked()
    {
        if (_availableCrows.Count == 0) return;
        var selectedCrow = _availableCrows[_crowDropdown.value];
        
        // Chamando o método real, mas agora com o Enum de Evangelização
        string message;
        bool success = GameBootstrap.Instance.Expeditions.SendToExpedition(selectedCrow.ID, MissionType.Evangelizacao, _currentRegionId, out message);
        
        _feedbackText.text = message;
        if (success) RefreshDropdown();
    }
}