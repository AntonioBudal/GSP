// Assets/Scripts/Expedition/ExpeditionManager.cs
using System;
using System.Collections.Generic;

public class ExpeditionManager : IDisposable
{
    private readonly CrowStateController _stateController;
    private readonly GameClock _clock;
    private readonly ExplorationEngine _explorationEngine;
    private readonly MapManager _mapManager;
    private readonly MapRevealService _mapRevealService;
    private readonly ICrowRepository _crowRepository;

    private readonly Dictionary<string, ExpeditionRuntime> _activeExpeditions;

    public event Action<ExpeditionProgressEvent> OnExpeditionTick;

    public ExpeditionManager(CrowStateController stateController, GameClock clock, 
                             ExplorationEngine explorationEngine, MapManager mapManager, 
                             MapRevealService mapRevealService, ICrowRepository crowRepository)
    {
        _stateController = stateController;
        _clock = clock;
        _explorationEngine = explorationEngine;
        _mapManager = mapManager;
        _mapRevealService = mapRevealService;
        _crowRepository = crowRepository;
        _activeExpeditions = new Dictionary<string, ExpeditionRuntime>();

        _clock.OnDayProcessing += ProcessActiveExpeditions;
    }

    public bool SendToExpedition(string crowId, MissionType mission, string targetRegionId, out string message)
    {
        Crow crow = _crowRepository.GetCrow(crowId);
        if (crow == null)
        {
            message = "Erro: Ave não encontrada no repositório.";
            return false;
        }

        if (_activeExpeditions.ContainsKey(crowId))
        {
            message = $"Bloqueio: Ave [{crowId}] já em missão.";
            return false;
        }

        Region target = _mapManager.GetRegion(targetRegionId);
        if (target == null || (mission == MissionType.Reconhecimento && target.CurrentState != FogState.Descoberto))
        {
            message = "Bloqueio: Destino inválido ou névoa incompatível.";
            return false;
        }

        var transition = _stateController.RequestTransition(crow, CrowState.EmExpedicao);
        if (!transition.Success)
        {
            message = transition.Message;
            return false;
        }

        _activeExpeditions[crowId] = new ExpeditionRuntime(crowId, mission, targetRegionId);
        message = $"Expedição autorizada. Destino: [{target.Name}].";
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

            runtime.DaysElapsed++;

            if (runtime.Mission == MissionType.Reconhecimento)
            {
                // Motor 100% desacoplado de domínio
                var result = _explorationEngine.EvaluateDailyRecon(
                    crow.GetStat(CrowStat.Speed), 
                    crow.GetStat(CrowStat.Focus), 
                    region.BaseDifficulty
                );

                runtime.Progress += result.ProgressGain;
                OnExpeditionTick?.Invoke(new ExpeditionProgressEvent(currentDay, crow.ID, runtime.DaysElapsed));

                // Processamento de Dano Imediato
                if (result.Injury == 2)
                {
                    _stateController.RequestTransition(crow, CrowState.Morto);
                    resolvedIds.Add(crow.ID);
                    continue; 
                }

                // Condição de Vitória Parametrizada
                int requiredProgress = 3; // Isso poderá vir da dificuldade da Região futuramente
                if (runtime.Progress >= requiredProgress)
                {
                    // A ave sobreviveu. O estado dela volta ao normal (ou Fadigado se Injury == 1)
                    var nextState = result.Injury == 1 ? CrowState.Fadigado : CrowState.Disponivel;
                    _stateController.RequestTransition(crow, nextState);
                    
                    // Delega a mutação do mapa para o serviço especializado
                    _mapRevealService.ApplyExplorationSuccess(region.ID, result.RevealPower);
                    
                    resolvedIds.Add(crow.ID);
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