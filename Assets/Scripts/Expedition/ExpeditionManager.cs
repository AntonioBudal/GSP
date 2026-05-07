using System;
using System.Collections.Generic;




// Interface para injetar a tabela de eventos futuramente
public interface IExpeditionEventProvider
{
    ExpeditionEvent GetEventForDay(int day);
}

public class ExpeditionManager : IDisposable
{
    private CrowStateController _stateController;
    private GameClock _clock;
    private ExpeditionEventEngine _engine;
    private IExpeditionEventProvider _eventProvider;

    private Dictionary<string, ExpeditionRuntime> _activeExpeditions;

    // A variável do jogador (Progresso) exigida na 3.3
    public int PlayerProgress { get; private set; }

    public event Action<ExpeditionProgressEvent> OnExpeditionDayProcessed;

    public ExpeditionManager(CrowStateController stateController, GameClock clock, 
                             ExpeditionEventEngine engine, IExpeditionEventProvider eventProvider)
    {
        _stateController = stateController;
        _clock = clock;
        _engine = engine;
        _eventProvider = eventProvider;
        _activeExpeditions = new Dictionary<string, ExpeditionRuntime>();

        _clock.OnDayProcessing += ProcessActiveExpeditions;
    }

    public bool SendToExpedition(Crow crow, out string message)
    {
        if (_activeExpeditions.ContainsKey(crow.ID))
        {
            message = $"Rejeitado: Corvo [{crow.ID}] já está em expedição.";
            return false;
        }

        var transition = _stateController.RequestTransition(crow, CrowState.EmExpedicao);
        if (!transition.Success)
        {
            message = transition.Message;
            return false;
        }

        _activeExpeditions[crow.ID] = new ExpeditionRuntime(crow);
        message = $"Corvo [{crow.ID}] enviado.";
        return true;
    }

    // AQUI ACONTECE A MÁGICA DA FASE 3.3
    private void ProcessActiveExpeditions(int currentDay)
    {
        // 1. Busca o desafio do dia no provedor externo
        ExpeditionEvent dailyEvent = _eventProvider.GetEventForDay(currentDay);
        List<string> resolvedCrows = new List<string>();

        foreach (var kvp in _activeExpeditions)
        {
            string crowId = kvp.Key;
            ExpeditionRuntime runtime = kvp.Value;
            runtime.DaysElapsed++;

            // 2. O Motor puramente matemático atua
            EventResolution resolution = _engine.Evaluate(runtime.Target, dailyEvent);

            // 3. O Manager interpreta e aplica consequência ao runtime
            if (resolution.Passed) runtime.Successes++;
            else runtime.Failures++;

            OnExpeditionDayProcessed?.Invoke(new ExpeditionProgressEvent(currentDay, crowId, runtime.DaysElapsed));

            // 4. Critérios da Fase 3.3: Acúmulo de falhas letais ou sucessos
            if (runtime.Failures >= 2) // Exemplo: 2 falhas na mesma expedição = Morte
            {
                ResolveExpedition(crowId, CrowState.Morto, out string logDeath);
                // Logar logDeath para a UI
                resolvedCrows.Add(crowId);
            }
            else if (runtime.Successes >= 3) // Exemplo: 3 sucessos = Retorna com Glória
            {
                PlayerProgress += 10; // Incrementa a conta do jogador
                ResolveExpedition(crowId, CrowState.Disponivel, out string logSuccess);
                // Logar logSuccess para a UI
                resolvedCrows.Add(crowId);
            }
        }

        // Limpa os corvos que terminaram a expedição do loop de processamento
        foreach (var id in resolvedCrows)
        {
            _activeExpeditions.Remove(id);
        }
    }

    public bool ResolveExpedition(string crowId, CrowState outcomeState, out string message)
    {
        if (!_activeExpeditions.TryGetValue(crowId, out var runtime))
        {
            message = "Expedição não encontrada.";
            return false;
        }

        var transition = _stateController.RequestTransition(runtime.Target, outcomeState);
        if (!transition.Success)
        {
            message = transition.Message;
            return false;
        }

        message = $"Concluído. Corvo [{crowId}] -> [{outcomeState}]. Progresso atual do Jogador: {PlayerProgress}";
        return true;
    }

    public void Dispose()
    {
        if (_clock != null)
            _clock.OnDayProcessing -= ProcessActiveExpeditions;
    }
}