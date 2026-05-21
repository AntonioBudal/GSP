using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(UIWindow))]
public class UI_TrainingPopup : MonoBehaviour
{
    [Header("Alvo do Sacrifício")]
    [SerializeField] private TMP_Dropdown _crowDropdown;
    [SerializeField] private Button _btnClose;

    [Header("Ritos Rápidos (Imediatos)")]
    [SerializeField] private Button _btnAltitude;  // VEL +1 (Causa Fadiga)
    [SerializeField] private Button _btnEndurance; // RES +1 (Custa Vida Útil, Causa Fadiga)
    [SerializeField] private Button _btnSensory;   // FOC +1, RES -1 (Avança o Relógio)

    [Header("Especialização (Rito Longo)")]
    [SerializeField] private TMP_Dropdown _roleDropdown; // Seleção de Caminho
    [SerializeField] private Button _btnSpecialize; // Custa 10 anos de vida e 3 dias de tranca

    [Header("Registro (Feedback)")]
    [SerializeField] private TextMeshProUGUI _feedbackText;

    private UIWindow _window;
    private List<Crow> _availableCrows = new List<Crow>();
    private List<CrowRole> _availableRoles = new List<CrowRole> { CrowRole.Batedor, CrowRole.Mensageiro };

    private void Awake()
    {
        _window = GetComponent<UIWindow>();
        
        _btnClose.onClick.AddListener(() => _window.Hide());
        
        _btnAltitude.onClick.AddListener(OnAltitudeClicked);
        _btnEndurance.onClick.AddListener(OnEnduranceClicked);
        _btnSensory.onClick.AddListener(OnSensoryClicked);
        
        _btnSpecialize.onClick.AddListener(OnSpecializeClicked);

        SetupRoleDropdown();
    }

    public void SetupAndShow()
    {
        _feedbackText.text = "Selecione a cobaia. A carne é fraca, mas o bando prevalece.";
        RefreshDropdown();
        _window.Show();
    }

    private void SetupRoleDropdown()
    {
        _roleDropdown.ClearOptions();
        var options = _availableRoles.Select(r => new TMP_Dropdown.OptionData(r.ToString())).ToList();
        _roleDropdown.AddOptions(options);
    }

    private void RefreshDropdown()
    {
        _crowDropdown.ClearOptions();
        _availableCrows.Clear();

        var allCrows = GameBootstrap.Instance.CrowRepo.GetAllCrows();
        foreach (var crow in allCrows)
        {
            if (crow.CurrentState == CrowState.Disponivel)
            {
                _availableCrows.Add(crow);
            }
        }

        bool hasCrows = _availableCrows.Count > 0;
        
        _crowDropdown.interactable = hasCrows;
        _btnAltitude.interactable = hasCrows;
        _btnEndurance.interactable = hasCrows;
        _btnSensory.interactable = hasCrows;
        _btnSpecialize.interactable = hasCrows;
        _roleDropdown.interactable = hasCrows;

        if (hasCrows)
        {
            var options = _availableCrows.Select(c => 
                new TMP_Dropdown.OptionData($"{c.ID} | {c.Role} | V:{c.Lifespan} anos")
            ).ToList();
            _crowDropdown.AddOptions(options);
        }
        else
        {
            _crowDropdown.AddOptions(new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("Nenhuma ave apta") });
        }
    }

    // ==========================================
    // EXECUÇÕES DO DOMÍNIO
    // ==========================================

    private void OnAltitudeClicked()
    {
        if (_availableCrows.Count == 0) return;
        var target = _availableCrows[_crowDropdown.value];
        
        GameBootstrap.Instance.Training.TrainAltitudeFlight(target, out string msg);
        HandleResult(msg);
    }

    private void OnEnduranceClicked()
    {
        if (_availableCrows.Count == 0) return;
        var target = _availableCrows[_crowDropdown.value];
        
        GameBootstrap.Instance.Training.TrainBruteEndurance(target, out string msg);
        HandleResult(msg);
    }

    private void OnSensoryClicked()
    {
        if (_availableCrows.Count == 0) return;
        var target = _availableCrows[_crowDropdown.value];
        
        GameBootstrap.Instance.Training.TrainSensoryDeprivation(target, out string msg);
        HandleResult(msg);
    }

    private void OnSpecializeClicked()
    {
        if (_availableCrows.Count == 0) return;
        var target = _availableCrows[_crowDropdown.value];
        var targetRole = _availableRoles[_roleDropdown.value];

        // Especialização pede o ID no Domínio, diferente dos treinos base
        GameBootstrap.Instance.Training.StartSpecialization(target.ID, targetRole, out string msg);
        HandleResult(msg);
    }

    private void HandleResult(string message)
    {
        _feedbackText.text = message;
        // Sempre recarrega, pois a ave pode ter morrido, ficado fadigada ou entrado em tranca (EmTreino)
        RefreshDropdown(); 
    }
}