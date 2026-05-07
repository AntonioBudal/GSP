using System;
using System.Collections.Generic;

public class BreedingManager : IDisposable
{
    private readonly CrowStateController _stateController;
    private readonly GameClock _clock;
    private readonly GeneticsEngine _geneticsEngine;
    private readonly Func<string, Crow> _crowResolver; // Resolve ID para a Entidade sem criar um Repository acoplado

    private readonly Dictionary<string, BreedingRuntime> _activeGestaions;

    // Saída pura via evento
    public event Action<Crow> OnChildBorn;

    public BreedingManager(CrowStateController stateController, GameClock clock, 
                           GeneticsEngine geneticsEngine, Func<string, Crow> crowResolver)
    {
        _stateController = stateController ?? throw new ArgumentNullException(nameof(stateController));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _geneticsEngine = geneticsEngine ?? throw new ArgumentNullException(nameof(geneticsEngine));
        _crowResolver = crowResolver ?? throw new ArgumentNullException(nameof(crowResolver));
        
        _activeGestaions = new Dictionary<string, BreedingRuntime>();
        _clock.OnDayEnded += ProcessGestationTicks;
    }

    public bool StartBreeding(Crow parentA, Crow parentB, string targetChildId, int gestationDays, out string message)
    {
        if (parentA.ID == parentB.ID)
        {
            message = "Bloqueio: Auto-cruzamento inválido.";
            return false;
        }

        // Checagem de duplicação exigida
        if (_activeGestaions.ContainsKey(targetChildId))
        {
            message = $"Bloqueio: Já existe uma gestação em andamento para o ID [{targetChildId}].";
            return false;
        }

        var transitionA = _stateController.RequestTransition(parentA, CrowState.Gestando);
        if (!transitionA.Success)
        {
            message = transitionA.Message;
            return false;
        }

        var transitionB = _stateController.RequestTransition(parentB, CrowState.Gestando);
        if (!transitionB.Success)
        {
            _stateController.RequestTransition(parentA, CrowState.Disponivel); // Rollback
            message = transitionB.Message;
            return false;
        }

        // Armazena apenas os IDs no runtime
        _activeGestaions[targetChildId] = new BreedingRuntime(targetChildId, parentA.ID, parentB.ID, gestationDays);
        
        message = $"Sucesso: Gestação de [{targetChildId}] iniciada. Duração: {gestationDays} dias.";
        return true;
    }

    private void ProcessGestationTicks(int currentDay)
    {
        List<string> completedIds = new List<string>();

        foreach (var kvp in _activeGestaions)
        {
            kvp.Value.DaysRemaining--;
            if (kvp.Value.DaysRemaining <= 0)
            {
                completedIds.Add(kvp.Key);
            }
        }

        foreach (var childId in completedIds)
        {
            BreedingRuntime runtime = _activeGestaions[childId];

            // Resolve os pais pelo ID para aplicar a mudança de estado
            Crow parentA = _crowResolver(runtime.ParentAId);
            Crow parentB = _crowResolver(runtime.ParentBId);

            if (parentA != null) _stateController.RequestTransition(parentA, CrowState.Disponivel);
            if (parentB != null) _stateController.RequestTransition(parentB, CrowState.Disponivel);

            Crow newChild = _geneticsEngine.GenerateOffspring(childId, parentA, parentB);

            // Dispara o evento com o resultado puro. A ponte do Unity que se vire para logar.
            OnChildBorn?.Invoke(newChild);

            _activeGestaions.Remove(childId);
        }
    }

    public void Dispose()
    {
        if (_clock != null)
            _clock.OnDayEnded -= ProcessGestationTicks;
    }
}