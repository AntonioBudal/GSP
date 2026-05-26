// Assets/Scripts/Progression/ProgressionManager.cs
using System;
using System.Collections.Generic;
using Corvus.Core.SaveSystem;

public class ProgressionManager : IDisposable
{
    private readonly GameClock _clock;
    private readonly MapManager _mapManager;

    // O cofre de conquistas já destrancadas
    private readonly HashSet<MilestoneID> _unlockedMilestones;

    // Agregadores de Estado (Memória passiva)
    private int _totalDaysElapsed;
    private int _totalRegionsExplored;

    // Evento puro para a interface visual disparar pop-ups ou conquistas
    public event Action<MilestoneID, string> OnMilestoneReached;

    // Injetamos os orquestradores que queremos observar
    public ProgressionManager(GameClock clock, MapManager mapManager, ProgressionSaveData saveData = null)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _mapManager = mapManager ?? throw new ArgumentNullException(nameof(mapManager));
        _unlockedMilestones = new HashSet<MilestoneID>();

        if (saveData != null)
        {
            _totalDaysElapsed = saveData.TotalDaysElapsed;
            _totalRegionsExplored = saveData.TotalRegionsExplored;
            foreach (var milestone in saveData.UnlockedMilestones)
            {
                _unlockedMilestones.Add(milestone);
            }
        }
        else
        {
            _totalDaysElapsed = 0;
            _totalRegionsExplored = 0;
        }

        _clock.OnDayEnded += HandleDayEnded;
        _mapManager.OnRegionStateChanged += HandleRegionStateChanged;
    }

    // Método para o SaveManager coletar os dados
    public ProgressionSaveData GetSaveSnapshot()
    {
        return new ProgressionSaveData
        {
            TotalDaysElapsed = this._totalDaysElapsed,
            TotalRegionsExplored = this._totalRegionsExplored,
            UnlockedMilestones = new List<MilestoneID>(this._unlockedMilestones)
        };
    }

    /// <summary>
    /// Consulta rápida para a Fase 8.2 (Toggles). Outros sistemas usarão isso para bloquear ações.
    /// </summary>
    public bool IsUnlocked(MilestoneID milestone)
    {
        return _unlockedMilestones.Contains(milestone);
    }

    private void UnlockMilestone(MilestoneID id, string title)
    {
        // O HashSet.Add retorna false se o item já existia. Isso previne spam de eventos.
        if (_unlockedMilestones.Add(id)) 
        {
            OnMilestoneReached?.Invoke(id, title);
        }
    }

    // ==========================================
    // HANDLERS (A regra de negócio do avanço)
    // ==========================================

    private void HandleDayEnded(int currentDay)
    {
        _totalDaysElapsed++;

        if (_totalDaysElapsed >= 7 && !IsUnlocked(MilestoneID.SobrevivenciaInicial))
        {
            UnlockMilestone(MilestoneID.SobrevivenciaInicial, "Marco Atingido: Uma semana se passou no silêncio.");
        }
    }

    private void HandleRegionStateChanged(MapStateResult result)
    {
        // Só nos importamos se a transição foi um sucesso e se o destino final foi "Explorado"
        if (result.Success && result.NewState == FogState.Explorado)
        {
            _totalRegionsExplored++;

            if (_totalRegionsExplored >= 3 && !IsUnlocked(MilestoneID.FronteiraAberta))
            {
                UnlockMilestone(MilestoneID.FronteiraAberta, "Marco Atingido: A Fronteira se Abre.");
            }
        }
    }

    public bool IsFeatureUnlocked(FeatureID feature)
    {
        switch (feature)
        {
            case FeatureID.Bercario:
                // O Berçário só abre após a primeira semana de sobrevivência
                return IsUnlocked(MilestoneID.SobrevivenciaInicial);
                
            case FeatureID.Evangelizacao:
                // A Palavra só é espalhada após conhecer 3 regiões
                return IsUnlocked(MilestoneID.FronteiraAberta);
                
            case FeatureID.Especializacao:
                // Especialização exige que o Berçário já esteja rodando (mesmo requisito)
                return IsUnlocked(MilestoneID.SobrevivenciaInicial);
                
            default:
                return true; // Features base (Reconhecimento, Treinos Base) vêm abertas
        }
    }

    public void Dispose()
    {
        // Limpeza estrita para evitar memory leaks caso o manager seja destruído
        if (_clock != null) _clock.OnDayEnded -= HandleDayEnded;
        if (_mapManager != null) _mapManager.OnRegionStateChanged -= HandleRegionStateChanged;
    }
}