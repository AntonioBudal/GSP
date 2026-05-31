// Assets/Scripts/Core/Training/TrainingManager.cs
using System;
using System.Collections.Generic;
using Corvus.Core.SaveSystem;

// Estrutura para os Treinos de Especialização (Fase 7)

public class TrainingRuntime
{
    public string CrowId { get; }
   
    public CrowRole TargetRole { get; }
    public int DaysRemaining { get; set; }
    public int LifespanCost { get; }

    
    public TrainingRuntime(string crowId, CrowRole targetRole, int duration, int lifespanCost)
    {
        CrowId = crowId;
        TargetRole = targetRole;
        DaysRemaining = duration;
        LifespanCost = lifespanCost;
    }
}

// AQUI ESTÁ A CORREÇÃO: FatigueData agora é pública e independente!
public class FatigueData
{
    public string CrowId { get; set; } 
    public int DaysLeft { get; set; }
}

public class TrainingManager : IDisposable
{
    private readonly CrowStateController _stateController;
    private readonly GameClock _clock;
    private readonly ICrowRepository _crowRepository; 
    private readonly ProgressionManager _progressionManager; 

    public event Action<string> OnBaseTrainingExecuted;

    private readonly Dictionary<string, FatigueData> _runtimeFatigue;
    private readonly Dictionary<string, TrainingRuntime> _activeTrainings; 

    public TrainingManager(CrowStateController stateController, GameClock clock, ICrowRepository crowRepository, ProgressionManager progressionManager, 
                           List<TrainingSaveData> trainingSave = null, List<FatigueSaveData> fatigueSave = null)
    {
        _stateController = stateController ?? throw new ArgumentNullException(nameof(stateController));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _crowRepository = crowRepository ?? throw new ArgumentNullException(nameof(crowRepository));
        _progressionManager = progressionManager ?? throw new ArgumentNullException(nameof(progressionManager));
        
        _runtimeFatigue = new Dictionary<string, FatigueData>();
        _activeTrainings = new Dictionary<string, TrainingRuntime>();

        // Reconstruindo Fadiga
        if (fatigueSave != null)
        {
            foreach (var f in fatigueSave)
                _runtimeFatigue[f.CrowId] = new FatigueData { CrowId = f.CrowId, DaysLeft = f.DaysLeft };
        }

        // Reconstruindo Treinos Ativos
        if (trainingSave != null)
        {
            foreach (var t in trainingSave)
                _activeTrainings[t.CrowId] = new TrainingRuntime(t.CrowId, t.TargetRole, t.DaysRemaining, t.LifespanCost);
        }

        _clock.OnDayEnded += ProcessFatigueRecovery;
        _clock.OnDayEnded += ProcessSpecializationTicks;
    }

    // ==========================================
    // TREINOS BASE (Fase 2 - Otimizados)
    // ==========================================

    public bool TrainAltitudeFlight(Crow crow, out string message)
    {
        var transitionIn = _stateController.RequestTransition(crow, CrowState.EmTreino);
        if (!transitionIn.Success)

        {
            message = $"Falha no Treino: {transitionIn.Message}";
            return false;
        }

        crow.Speed += 1;

        _stateController.RequestTransition(crow, CrowState.Fadigado);
        
        _runtimeFatigue[crow.ID] = new FatigueData { CrowId = crow.ID, DaysLeft = 2 };

        message = $"Corvo [{crow.ID}] treinou Voo de Altitude. VEL +1. Fadigado por 2 dias.";
        OnBaseTrainingExecuted?.Invoke(crow.ID);
        return true;
    }

    public bool TrainBruteEndurance(Crow crow, out string message)
    {
        var transitionIn = _stateController.RequestTransition(crow, CrowState.EmTreino);
        if (!transitionIn.Success)
        {
            message = $"Falha no Treino: {transitionIn.Message}";
            return false;
        }

        crow.Resilience += 1;
        crow.Lifespan -= 3;

        if (crow.Lifespan <= 0)
        {
            crow.Lifespan = 0; 
            _stateController.RequestTransition(crow, CrowState.Morto);
            message = $"Treino Letal: Corvo [{crow.ID}] não suportou a Resistência Bruta e morreu. RES +1, Vida 0.";
            return true; 
        }

        _stateController.RequestTransition(crow, CrowState.Fadigado);
        _runtimeFatigue[crow.ID] = new FatigueData { CrowId = crow.ID, DaysLeft = 1 };

        message = $"Corvo [{crow.ID}] treinou Resistência Bruta. RES +1. Vida Útil -3. Fadigado por 1 dia.";
        return true;
    }

    public bool TrainSensoryDeprivation(Crow crow, out string message)
    {
        if (crow.Resilience <= 0)
        {
            message = $"Falha: Corvo [{crow.ID}] possui Resiliência 0 e enlouqueceria na Privação Sensorial.";
            return false;
        }

        var transitionIn = _stateController.RequestTransition(crow, CrowState.EmTreino);
        if (!transitionIn.Success)
        {
            message = $"Falha no Treino: {transitionIn.Message}";
            return false;
        }

        crow.Focus += 1;
        crow.Resilience -= 1;

        _stateController.RequestTransition(crow, CrowState.Disponivel);
        _clock.AdvanceTime(1);

        message = $"Corvo [{crow.ID}] completou Privação Sensorial. FOC +1, RES -1 permanente. O Relógio avançou 1 dia.";
        return true;
    }

    // ==========================================
    // ESPECIALIZAÇÃO (Fase 7.1)
    // ==========================================

    public bool StartSpecialization(string crowId, CrowRole targetRole, out string message)
    {
        Crow crow = _crowRepository.GetCrow(crowId);
        if (crow == null) { message = "Erro: Ave não encontrada."; return false; }

        if (crow.Role != CrowRole.Geral)
        {
            message = $"Bloqueio: O corvo já possui um caminho ([{crow.Role}]).";
            return false;
        }

        if (!_progressionManager.IsFeatureUnlocked(FeatureID.Especializacao))
        {
            message = "Falha: A Especialização exige instalações avançadas não disponíveis.";
            return false;
        }

        int lifespanCost = 10;
        int trainingDays = 3;

        if (crow.Lifespan <= lifespanCost)
        {
            message = $"Bloqueio Letal: A ave possui apenas {crow.Lifespan} anos. Este treino a mataria.";
            return false;
        }

        var transition = _stateController.RequestTransition(crow, CrowState.EmTreino);
        if (!transition.Success) { message = transition.Message; return false; }

        _activeTrainings[crowId] = new TrainingRuntime(crowId, targetRole, trainingDays, lifespanCost);
        message = $"Especialização iniciada. Em {trainingDays} dias, [{crow.ID}] será um [{targetRole}].";
        return true;
    }

    // ==========================================
    // PROCESSAMENTO E LIMPEZA
    // ==========================================

    private void ProcessFatigueRecovery(int currentDay)
    {
        List<string> recoveredIDs = new List<string>();

        foreach (var kvp in _runtimeFatigue)
        {
            kvp.Value.DaysLeft -= 1;
            if (kvp.Value.DaysLeft <= 0) recoveredIDs.Add(kvp.Key);
        }

        foreach (var id in recoveredIDs)
        {
            Crow recoveredCrow = _crowRepository.GetCrow(id);
            if (recoveredCrow != null && recoveredCrow.CurrentState == CrowState.Fadigado)
            {
                _stateController.RequestTransition(recoveredCrow, CrowState.Disponivel);
            }
            _runtimeFatigue.Remove(id);
        }
    }

    private void ProcessSpecializationTicks(int currentDay)
    {
        List<string> completedIds = new List<string>();

        foreach (var kvp in _activeTrainings)
        {
            kvp.Value.DaysRemaining--;
            if (kvp.Value.DaysRemaining <= 0) completedIds.Add(kvp.Key);
        }

        foreach (var id in completedIds)
        {
            TrainingRuntime runtime = _activeTrainings[id];
            Crow crow = _crowRepository.GetCrow(id);

            if (crow != null)
            {
                crow.Lifespan -= runtime.LifespanCost;
                crow.Role = runtime.TargetRole;
                _stateController.RequestTransition(crow, CrowState.Fadigado);
            }

            _activeTrainings.Remove(id);
        }
    }

    public void Dispose()
    {
        if (_clock != null)
        {
            _clock.OnDayEnded -= ProcessFatigueRecovery;
            _clock.OnDayEnded -= ProcessSpecializationTicks;
        }
    }

    // Métodos para o SaveManager fotografar o estado:
    public IEnumerable<TrainingRuntime> GetAllActiveTrainings() => _activeTrainings.Values;
    public IEnumerable<FatigueData> GetAllFatigueData() => _runtimeFatigue.Values;
}