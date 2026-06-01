// Assets/Scripts/Unity/UI/MainMenuController.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Corvus.Core.SaveSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("Painéis de Navegação")]
    [SerializeField] private GameObject _panelPrimaryNav;
    [SerializeField] private GameObject _panelSaveSelection;

    [Header("Botões Principais")]
    [SerializeField] private Button _btnContinue;
    [SerializeField] private Button _btnOpenArchives;
    [SerializeField] private Button _btnCloseArchives;
    [SerializeField] private Button _btnAbandon;

    [Header("Configuração de Múltiplos Slots")]
    [SerializeField] private RectTransform _slotsContainer;
    [SerializeField] private GameObject _slotPrefab;

    private Dictionary<int, SaveSlotMetadata> _loadedMetadata = new Dictionary<int, SaveSlotMetadata>();
    private readonly List<UI_SaveSlotController> _instantiatedSlots = new List<UI_SaveSlotController>();
    private int _mostRecentSlotId = -1;

    private async void Start()
    {
        SetupButtonListeners();
        _panelSaveSelection.SetActive(false);
        _panelPrimaryNav.SetActive(true);

        // Primeiramente, faz o scan do disco de forma assíncrona
        await RefreshSavesDataAsync();

        // Configura o botão de continuação rápida se houver algum save recente
        if (_mostRecentSlotId != -1)
        {
            _btnContinue.interactable = true;
            _btnContinue.onClick.RemoveAllListeners();
            _btnContinue.onClick.AddListener(() => LoadSlotGame(_mostRecentSlotId));
        }
        else
        {
            _btnContinue.interactable = false;
        }
    }

    private void SetupButtonListeners()
    {
        _btnOpenArchives.onClick.AddListener(() => ToggleSavePanel(true));
        _btnCloseArchives.onClick.AddListener(() => ToggleSavePanel(false));
        
        _btnAbandon.onClick.AddListener(() => {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
    }

    private async Task RefreshSavesDataAsync()
    {
        string basePath = Application.persistentDataPath;
        _loadedMetadata = await SaveScanner.ScanAllSlotsAsync(basePath);

        // Descobre qual é o save mais recente com base na data do relógio real
        System.DateTime newestDate = System.DateTime.MinValue;
        _mostRecentSlotId = -1;

        foreach (var kvp in _loadedMetadata)
        {
            if (kvp.Value.LastPlayedDate > newestDate)
            {
                newestDate = kvp.Value.LastPlayedDate;
                _mostRecentSlotId = kvp.Key;
            }
        }
    }

    private void ToggleSavePanel(bool open)
    {
        _panelPrimaryNav.SetActive(!open);
        _panelSaveSelection.SetActive(open);

        if (open)
        {
            BuildSlotsUI();
        }
    }

    private void BuildSlotsUI()
    {
        // Limpa o contentor para evitar duplicações
        foreach (var slot in _instantiatedSlots)
        {
            Destroy(slot.gameObject);
        }
        _instantiatedSlots.Clear();

        if (_slotPrefab == null || _slotsContainer == null) return;

        // Instancia rigidamente os 10 slots obrigatórios da arquitetura
        for (int i = 0; i < 10; i++)
        {
            GameObject slotInstance = Instantiate(_slotPrefab, _slotsContainer);
            var controller = slotInstance.GetComponent<UI_SaveSlotController>();

            if (controller != null)
            {
                SaveSlotMetadata meta = _loadedMetadata.ContainsKey(i) ? _loadedMetadata[i] : null;
                controller.Setup(i, meta, HandleSlotSelected, HandleSlotDeleted);
                _instantiatedSlots.Add(controller);
            }
        }
    }

    private void HandleSlotSelected(int slotId, bool isEmpty)
    {
        if (isEmpty)
        {
            // Inicia uma nova campanha limpa neste slot
            GameStateController.Instance.SelectedSlotID = slotId;
            
            // Garantia arquitetural: apaga qualquer resíduo caso o ficheiro info estivesse ausente mas o .sav presente
            var service = new SaveService(Application.persistentDataPath, slotId);
            service.DeleteSave();

            GameStateController.Instance.LoadMainScene();
        }
        else
        {
            LoadSlotGame(slotId);
        }
    }

    private async void HandleSlotDeleted(int slotId)
    {
        // I/O puro isolado do serviço
        var service = new SaveService(Application.persistentDataPath, slotId);
        service.DeleteSave();

        // Atualiza os dados internos e reconstrói a interface
        await RefreshSavesDataAsync();
        BuildSlotsUI();

        // Atualiza a validade do botão Continuar da navegação primária
        if (_mostRecentSlotId == slotId || _mostRecentSlotId == -1)
        {
            _btnContinue.interactable = _mostRecentSlotId != -1;
        }
    }

    private void LoadSlotGame(int slotId)
    {
        GameStateController.Instance.SelectedSlotID = slotId;
        GameStateController.Instance.LoadMainScene();
    }
}