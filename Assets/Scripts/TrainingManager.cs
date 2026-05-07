using System;
using System.Collections.Generic;

public class TrainingManager : IDisposable
{
    private CrowStateController _stateController;
    private GameClock _clock;

    // Correção 2 e 5: Objeto de runtime usando o ID (string) como chave segura
    private class FatigueData
    {
        public Crow Target { get; set; }
        public int DaysLeft { get; set; }
    }
    
    private Dictionary<string, FatigueData> _runtimeFatigue;

    public TrainingManager(CrowStateController stateController, GameClock clock)
    {
        _stateController = stateController;
        _clock = clock;
        _runtimeFatigue = new Dictionary<string, FatigueData>();

        // Assinatura do evento
        _clock.OnDayEnded += ProcessFatigueRecovery;
    }

    // --- MINI FASE 2.2 ---

    public bool TrainAltitudeFlight(Crow crow, out string message)
    {
        // Correção 1: Usando a API rigorosa de TransitionResult
        var transitionIn = _stateController.RequestTransition(crow, CrowState.EmTreino);
        if (!transitionIn.Success)
        {
            message = $"Falha no Treino: {transitionIn.Message}";
            return false;
        }

        crow.Speed += 1;

        var transitionOut = _stateController.RequestTransition(crow, CrowState.Fadigado);
        _runtimeFatigue[crow.ID] = new FatigueData { Target = crow, DaysLeft = 2 };

        message = $"Corvo [{crow.ID}] treinou Voo de Altitude. VEL +1. Fadigado por 2 dias.";
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

        // Correção 4: Trava de Vida Mínima (Death Check)
        if (crow.Lifespan <= 0)
        {
            crow.Lifespan = 0; // Previne valores negativos na UI depois
            _stateController.RequestTransition(crow, CrowState.Morto);
            message = $"Treino Letal: Corvo [{crow.ID}] não suportou a Resistência Bruta e morreu. RES +1, Vida 0.";
            return true; 
        }

        _stateController.RequestTransition(crow, CrowState.Fadigado);
        _runtimeFatigue[crow.ID] = new FatigueData { Target = crow, DaysLeft = 1 };

        message = $"Corvo [{crow.ID}] treinou Resistência Bruta. RES +1. Vida Útil -3. Fadigado por 1 dia.";
        return true;
    }

    // --- MINI FASE 2.3 ---

    public bool TrainSensoryDeprivation(Crow crow, out string message)
    {
        // Trava: Resiliência mínima
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

        // Retorna imediatamente para Disponível, sem fadiga pessoal
        _stateController.RequestTransition(crow, CrowState.Disponivel);

        // Consome 1 dia do Relógio Central
        _clock.AdvanceTime(1);

        message = $"Corvo [{crow.ID}] completou Privação Sensorial. FOC +1, RES -1 permanente. O Relógio Central avançou 1 dia.";
        return true;
    }

    // --- PROCESSAMENTO E LIMPEZA ---

    private void ProcessFatigueRecovery(int currentDay)
    {
        List<string> recoveredIDs = new List<string>();

        foreach (var kvp in _runtimeFatigue)
        {
            kvp.Value.DaysLeft -= 1;

            if (kvp.Value.DaysLeft <= 0)
            {
                recoveredIDs.Add(kvp.Key);
            }
        }

        foreach (var id in recoveredIDs)
        {
            Crow recoveredCrow = _runtimeFatigue[id].Target;
            _runtimeFatigue.Remove(id);
            _stateController.RequestTransition(recoveredCrow, CrowState.Disponivel);
        }
    }

    // Correção 3: Limpeza de assinatura
    public void Dispose()
    {
        if (_clock != null)
        {
            _clock.OnDayEnded -= ProcessFatigueRecovery;
        }
    }
}