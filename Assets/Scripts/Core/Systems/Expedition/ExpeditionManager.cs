// Assets/Scripts/Expedition/ExpeditionManager.cs
using System;
using System.Collections.Generic;
using Corvus.Core.SaveSystem;

public class ExpeditionManager : IDisposable
{
    private readonly CrowStateController _stateController;

    private readonly ProgressionManager _progressionManager;
    private readonly GameClock _clock;
    
    // Motores Específicos
    private readonly ExplorationEngine _explorationEngine;
    private readonly EvangelizationEngine _evangelizationEngine;
    
    // Serviços e Orquestradores
    private readonly MapManager _mapManager;
    private readonly MapRevealService _mapRevealService;
    private readonly InfluenceManager _influenceManager;
    private readonly ICrowRepository _crowRepository;

    private readonly Dictionary<string, ExpeditionRuntime> _activeExpeditions;

    public event Action<ExpeditionProgressEvent> OnExpeditionTick;

    public ExpeditionManager(CrowStateController stateController, GameClock clock, 
                         ExplorationEngine explorationEngine, EvangelizationEngine evangelizationEngine,
                         MapManager mapManager, MapRevealService mapRevealService, 
                         InfluenceManager influenceManager, ICrowRepository crowRepository,
                         ProgressionManager progressionManager, 
                         List<ExpeditionSaveData> saveDatas = null) 
    {
        _stateController = stateController;
        _clock = clock;
        _explorationEngine = explorationEngine;
        _evangelizationEngine = evangelizationEngine;
        _mapManager = mapManager;
        _mapRevealService = mapRevealService;
        _influenceManager = influenceManager;
        _crowRepository = crowRepository;
        _progressionManager = progressionManager;
        
        _activeExpeditions = new Dictionary<string, ExpeditionRuntime>();

        // Reconstruindo as Expedições
        if (saveDatas != null)
        {
            foreach (var data in saveDatas)
            {
                var runtime = new ExpeditionRuntime(data.CrowId, data.Mission, data.TargetRegionId);
                runtime.DaysElapsed = data.DaysElapsed;
                runtime.Progress = data.Progress;
                _activeExpeditions[data.CrowId] = runtime;
            }
        }

        _clock.OnDayProcessing += ProcessActiveExpeditions;
    }

    // Método para o SaveManager fotografar o estado:
    public IEnumerable<ExpeditionRuntime> GetAllActiveExpeditions() => _activeExpeditions.Values;

    public bool SendToExpedition(string crowId, MissionType mission, string targetRegionId, out string message)
    {
        Crow crow = _crowRepository.GetCrow(crowId);
        if (crow == null) { message = "Erro: Ave não encontrada no repositório."; return false; }
        if (_activeExpeditions.ContainsKey(crowId)) { message = $"Bloqueio: Ave [{crowId}] já em missão."; return false; }

        Region target = _mapManager.GetRegion(targetRegionId);
        if (target == null) { message = "Erro: Destino inválido."; return false; }

        // Validação de Névoa por Tipo de Missão
        if (mission == MissionType.Reconhecimento && target.CurrentState != FogState.Descoberto)
        {
            message = "Bloqueio: Reconhecimento exige uma região [Descoberta].";
            return false;
        }
        if (mission == MissionType.Evangelizacao && target.CurrentState != FogState.Explorado)
        {
            message = "Bloqueio: Evangelização exige uma região [Explorada]."; 
            return false;
        }

        if (mission == MissionType.Evangelizacao && !_progressionManager.IsFeatureUnlocked(FeatureID.Evangelizacao))
        {
            message = "Falha: A Evangelização só é permitida após a Fronteira ser Aberta (3 regiões).";
            return false;
        }

        var transition = _stateController.RequestTransition(crow, CrowState.EmExpedicao);
        if (!transition.Success) { message = transition.Message; return false; }

        _activeExpeditions[crowId] = new ExpeditionRuntime(crowId, mission, targetRegionId);
        message = $"Expedição autorizada. Destino: [{target.Name}] | Missão: [{mission}].";
        return true;
    }

    private void ProcessActiveExpeditions(int currentDay)
    {
        List<string> resolvedIds = new List<string>();

        foreach (var kvp in _activeExpeditions)
        {
            ExpeditionRuntime runtime = kvp.Value;
            Crow crow = _crowRepository.GetCrow(runtime.CrowId);
            Region region = _mapManager.GetRegion(runtime.TargetRegionId);

            if (crow == null || region == null) continue;

            runtime.DaysElapsed++;

            // ==========================================
            // FLUXO DE RECONHECIMENTO (Fase 5)
            // ==========================================
            if (runtime.Mission == MissionType.Reconhecimento)
            {
                // Injetando crow.Role para aplicar bônus de Batedor
                var result = _explorationEngine.EvaluateDailyRecon(
                    crow.GetStat(CrowStat.Speed), 
                    crow.GetStat(CrowStat.Focus), 
                    region.BaseDifficulty, 
                    crow.Role);

                runtime.Progress += result.ProgressGain;
                OnExpeditionTick?.Invoke(new ExpeditionProgressEvent(currentDay, crow.ID, runtime.DaysElapsed));

                if (result.Injury == 2)
                {
                    _stateController.RequestTransition(crow, CrowState.Morto);
                    resolvedIds.Add(crow.ID);
                    continue; 
                }

                if (runtime.Progress >= 3)
                {
                    var nextState = result.Injury == 1 ? CrowState.Fadigado : CrowState.Disponivel;
                    _stateController.RequestTransition(crow, nextState);
                    _mapRevealService.ApplyExplorationSuccess(region.ID, result.RevealPower);
                    resolvedIds.Add(crow.ID);
                }
            }
            // ==========================================
            // FLUXO DE EVANGELIZAÇÃO (Fase 6)
            // ==========================================
            else if (runtime.Mission == MissionType.Evangelizacao)
            {
                if (_influenceManager.TryGetInfluence(region.ID, out var influence))
                {
                    // Injetando crow.Role para aplicar bônus de Mensageiro
                    var result = _evangelizationEngine.EvaluateDailyPreaching(
                        crow.GetStat(CrowStat.Focus), 
                        crow.GetStat(CrowStat.Resilience), 
                        influence.Demographics.BaseResistance, 
                        crow.Role);

                    runtime.Progress += result.ProgressGain;
                    OnExpeditionTick?.Invoke(new ExpeditionProgressEvent(currentDay, crow.ID, runtime.DaysElapsed));
                    
                    if (result.ConvertedAmount > 0)
                    {
                        _influenceManager.TryConvertBelievers(region.ID, result.ConvertedAmount);
                    }

                    if (result.Injury == 2)
                    {
                        _stateController.RequestTransition(crow, CrowState.Morto);
                        resolvedIds.Add(crow.ID);
                        continue;
                    }

                    int requiredPreachingDays = 5;
                    if (runtime.Progress >= requiredPreachingDays)
                    {
                        var nextState = result.Injury == 1 ? CrowState.Fadigado : CrowState.Disponivel;
                        _stateController.RequestTransition(crow, nextState);
                        resolvedIds.Add(crow.ID);
                    }
                }
            }
        }

        foreach (var id in resolvedIds) _activeExpeditions.Remove(id);
    }

    public void Dispose()
    {
        if (_clock != null) _clock.OnDayProcessing -= ProcessActiveExpeditions;
    }
}