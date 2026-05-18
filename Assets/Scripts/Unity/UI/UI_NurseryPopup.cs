using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(UIWindow))]
public class UI_NurseryPopup : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Dropdown _parentADropdown;
    [SerializeField] private TMP_Dropdown _parentBDropdown;
    [SerializeField] private TMP_InputField _childNameInput;
    [SerializeField] private Button _btnBreed;
    [SerializeField] private Button _btnClose;
    [SerializeField] private TextMeshProUGUI _feedbackText;

    private UIWindow _window;
    private List<Crow> _availableParents = new List<Crow>();

    private void Awake()
    {
        _window = GetComponent<UIWindow>();
        _btnClose.onClick.AddListener(() => _window.Hide());
        _btnBreed.onClick.AddListener(OnBreedClicked);
    }

    public void SetupAndShow()
    {
        // 1. O Presenter não guarda estado, sempre puxa fresco
        RefreshDropdowns();
        _childNameInput.text = "";
        _feedbackText.text = "Selecione duas aves adultas e nomeie a linhagem.";
        _window.Show();
    }

    private void RefreshDropdowns()
    {
        _parentADropdown.ClearOptions();
        _parentBDropdown.ClearOptions();
        _availableParents.Clear();

        // Só podem acasalar aves que estão em casa e não fadigadas
        var allCrows = GameBootstrap.Instance.CrowRepo.GetAllCrows();
        foreach (var crow in allCrows)
        {
            if (crow.CurrentState == CrowState.Disponivel)
            {
                _availableParents.Add(crow);
            }
        }

        if (_availableParents.Count < 2)
        {
            var emptyOption = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("Aves insuficientes") };
            _parentADropdown.AddOptions(emptyOption);
            _parentBDropdown.AddOptions(emptyOption);
            _btnBreed.interactable = false;
        }
        else
        {
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
        string childName = string.IsNullOrWhiteSpace(_childNameInput.text) ? "Filhote_SemNome" : _childNameInput.text;

        // O Presenter apenas pede ao Domínio. Ele não sabe o que é a Genética.
        string message;
        bool success = GameBootstrap.Instance.Breeding.StartBreeding(parentA, parentB, childName, 3, out message);
        
        _feedbackText.text = message;

        if (success)
        {
            // Limpa o nome para evitar duplicatas acidentais e recarrega os dropdowns
            _childNameInput.text = "";
            RefreshDropdowns();
        }
    }
}