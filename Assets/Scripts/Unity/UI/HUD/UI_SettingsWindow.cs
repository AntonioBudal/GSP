// Assets/Scripts/Unity/UI/UI_SettingsWindow.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Corvus.Core.Settings;

[RequireComponent(typeof(CanvasGroup))]
public class UI_SettingsWindow : UIWindow
{
    [Header("Navegação (Tabs)")]
    [SerializeField] private Button _btnTabVideo;
    [SerializeField] private Button _btnTabAudio;
    [SerializeField] private GameObject _pageVideo;
    [SerializeField] private GameObject _pageAudio;
    [SerializeField] private TextMeshProUGUI _txtTitle;

    [Header("Vídeo Inputs")]
    [SerializeField] private TMP_Dropdown _dropResolution;
    [SerializeField] private TMP_Dropdown _dropScreenMode;
    [SerializeField] private Toggle _tglVSync;

    [Header("Áudio Inputs")]
    [SerializeField] private Slider _sldMaster;
    [SerializeField] private Slider _sldMusic;
    [SerializeField] private Slider _sldSFX;

    [Header("Rodapé")]
    [SerializeField] private Button _btnApply;
    [SerializeField] private Button _btnDefaults;
    [SerializeField] private Button _btnBack;

    [Header("Aviso de Descarte (Unsaved Modal)")]
    [SerializeField] private GameObject _subPanelUnsaved;
    [SerializeField] private Button _btnConfirmDiscard;
    [SerializeField] private Button _btnCancelClose;

    private SettingsManager _manager;
    private bool _isPopulatingUI = false; // Bloqueia loops infinitos de eventos
    private List<Resolution> _filteredResolutions;

    private void Awake()
    {
        // Ligação dos botões de Aba
        _btnTabVideo.onClick.AddListener(() => SwitchTab(true));
        _btnTabAudio.onClick.AddListener(() => SwitchTab(false));

        // Ligação dos Inputs de Vídeo
        _dropResolution.onValueChanged.AddListener(_ => OnUIValueChanged());
        _dropScreenMode.onValueChanged.AddListener(_ => OnUIValueChanged());
        _tglVSync.onValueChanged.AddListener(_ => OnUIValueChanged());

        // Ligação dos Inputs de Áudio
        _sldMaster.onValueChanged.AddListener(_ => OnUIValueChanged());
        _sldMusic.onValueChanged.AddListener(_ => OnUIValueChanged());
        _sldSFX.onValueChanged.AddListener(_ => OnUIValueChanged());

        // Ligação do Rodapé
        _btnApply.onClick.AddListener(OnApplyClicked);
        _btnDefaults.onClick.AddListener(OnDefaultsClicked);
        _btnBack.onClick.AddListener(() => UIManager.Instance.CloseTopModal());

        // Ligação do Modal de Confirmação
        _btnConfirmDiscard.onClick.AddListener(ForceDiscardAndClose);
        _btnCancelClose.onClick.AddListener(() => _subPanelUnsaved.SetActive(false));
    }

    public void SetupAndShow()
    {
        _manager = GameStateController.Instance.Settings;
        if (_manager == null) return;

        _subPanelUnsaved.SetActive(false);
        PopulateResolutionsDropdown();
        
        // Espelha o estado real do jogo para a tela
        UpdateUIFromProfile(_manager.AppliedProfile);
        
        SwitchTab(true); // Começa na aba de Vídeo
        RefreshApplyButtonState();
        
        Show();
    }

    // ==========================================
    // LOGICA VISUAL E POPULAÇÃO DE DADOS
    // ==========================================

    private void SwitchTab(bool toVideo)
    {
        _pageVideo.SetActive(toVideo);
        _pageAudio.SetActive(!toVideo);
    }

    private void PopulateResolutionsDropdown()
    {
        _dropResolution.ClearOptions();
        _filteredResolutions = new List<Resolution>();
        var options = new List<string>();

        // Filtra resoluções para evitar aberrações e duplicadas cegas
        foreach (var res in Screen.resolutions)
        {
            // Ignora resoluções minúsculas
            if (res.width < 800) continue; 
            
            _filteredResolutions.Add(res);
            
            // Formatando o texto limpo para a UI (ex: 1920x1080 @ 60Hz)
            options.Add($"{res.width}x{res.height} @ {Mathf.RoundToInt((float)res.refreshRateRatio.value)}Hz");
        }

        _dropResolution.AddOptions(options);
    }

    private void UpdateUIFromProfile(SettingsProfile profile)
    {
        _isPopulatingUI = true;

        // VÍDEO
        int resIndex = _filteredResolutions.FindIndex(r => r.width == profile.Video.ResolutionWidth && r.height == profile.Video.ResolutionHeight);
        if (resIndex >= 0) _dropResolution.value = resIndex;
        
        _dropScreenMode.value = (int)profile.Video.ScreenMode; // Enum C# para Int do Dropdown
        _tglVSync.isOn = profile.Video.VSync == 1;

        // ÁUDIO
        _sldMaster.value = profile.Audio.MasterVolume;
        _sldMusic.value = profile.Audio.MusicVolume;
        _sldSFX.value = profile.Audio.SfxVolume;

        _isPopulatingUI = false;
        RefreshApplyButtonState();
    }

    // ==========================================
    // FLUXO DE DADOS E DIRTY FLAG
    // ==========================================

    private void OnUIValueChanged()
    {
        if (_isPopulatingUI || _manager == null) return;

        // Constrói um rascunho com o que está na tela
        var pending = new SettingsProfile();
        
        if (_dropResolution.value >= 0 && _dropResolution.value < _filteredResolutions.Count)
        {
            var res = _filteredResolutions[_dropResolution.value];
            pending.Video.ResolutionWidth = res.width;
            pending.Video.ResolutionHeight = res.height;
            pending.Video.RefreshRate = Mathf.RoundToInt((float)res.refreshRateRatio.value);
        }

        pending.Video.ScreenMode = (AppScreenMode)_dropScreenMode.value;
        pending.Video.VSync = _tglVSync.isOn ? 1 : 0;

        pending.Audio.MasterVolume = _sldMaster.value;
        pending.Audio.MusicVolume = _sldMusic.value;
        pending.Audio.SfxVolume = _sldSFX.value;

        // Envia para o Manager validar e sujar (IsDirty = true)
        _manager.UpdatePendingProfile(pending);
        RefreshApplyButtonState();
    }

    private void RefreshApplyButtonState()
    {
        _btnApply.interactable = _manager.IsDirty;
        _txtTitle.text = _manager.IsDirty ? "O Códice da Ordem *" : "O Códice da Ordem";
    }

    // ==========================================
    // AÇÕES DO RODAPÉ
    // ==========================================

    private async void OnApplyClicked()
    {
        await _manager.SaveAsync();
        RefreshApplyButtonState();
    }

    private void OnDefaultsClicked()
    {
        _manager.ResetToDefaults();
        UpdateUIFromProfile(_manager.PendingProfile);
    }

    // ==========================================
    // INTERCEPÇÃO DO FECHO (TRY CLOSE)
    // ==========================================

    public override bool TryClose()
    {
        if (_manager.IsDirty)
        {
            // Bloqueia o fechamento e abre o aviso
            _subPanelUnsaved.SetActive(true);
            return false; 
        }

        return true; // Sem mudanças, pode fechar
    }

    private void ForceDiscardAndClose()
    {
        _manager.DiscardPendingChanges();
        _subPanelUnsaved.SetActive(false);
        
        // Força a remoção usando a API nativa já que agora está limpo
        UIManager.Instance.CloseTopModal(); 
    }
}