using System.Collections.Generic;
using UnityEngine;

public class RoomManager_Nursery : MonoBehaviour
{
    [Header("Configuração de Instalações")]
    [Tooltip("O Prefab tipado do ninho/incubadora")]
    [SerializeField] private PhysicalNest _nestPrefab;
    
    [Tooltip("Lugares físicos (Transforms) onde os ninhos ficarão posicionados")]
    [SerializeField] private Transform[] _nestStations;

    // Cache 1: Mapeamento O(1) para encontrar o ninho de um corvo instantaneamente
    private Dictionary<string, PhysicalNest> _activeNests = new Dictionary<string, PhysicalNest>();
    
    // Cache 2: Fila de estações livres
    private Queue<Transform> _availableStations = new Queue<Transform>();

    private void Awake()
    {
        // Alimenta o pool de estações no boot da sala
        foreach (var station in _nestStations)
        {
            _availableStations.Enqueue(station);
        }
    }

    private void OnEnable()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.StateController != null)
        {
            GameBootstrap.Instance.StateController.OnStateChanged += HandleStateChanged;
            GameBootstrap.Instance.Clock.OnDayEnded += HandleDayEnded;
            
            FullSync();
        }
    }

    private void OnDisable()
    {
        if (GameBootstrap.Instance != null)
        {
            if (GameBootstrap.Instance.StateController != null)
                GameBootstrap.Instance.StateController.OnStateChanged -= HandleStateChanged;
                
            if (GameBootstrap.Instance.Clock != null)
                GameBootstrap.Instance.Clock.OnDayEnded -= HandleDayEnded;
        }
    }

    /// <summary>
    /// Reação cirúrgica: Só atua sobre a ave que mutou de estado.
    /// </summary>
    private void HandleStateChanged(Crow crow)
    {
        bool isInRoom = _activeNests.TryGetValue(crow.ID, out PhysicalNest physicalNest);
        bool shouldBeInRoom = crow.CurrentState == CrowState.Gestando;

        if (shouldBeInRoom && !isInRoom)
        {
            SpawnNest(crow);
        }
        else if (!shouldBeInRoom && isInRoom)
        {
            RemoveNest(crow.ID, physicalNest);
        }
    }

    /// <summary>
    /// Tick Diário: Atualiza a barra de progresso visual de todos os ninhos ativos.
    /// </summary>
    private void HandleDayEnded(int currentDay)
    {
        foreach (var kvp in _activeNests)
        {
            string crowId = kvp.Key;
            PhysicalNest nest = kvp.Value;

            // Arquiteto: Para esta linha funcionar perfeitamente, o seu BreedingManager precisará 
            // expor um método que retorne a % de conclusão da gestação (0.0 a 1.0) com base no ID da ave.
            // Exemplo ilustrativo:
            // float progress = GameBootstrap.Instance.Breeding.GetGestationProgressPercentage(crowId);
            // nest.UpdateProgress(progress);
        }
    }

    private void FullSync()
    {
        var allCrows = GameBootstrap.Instance.CrowRepo.GetAllCrows();
        foreach (var crow in allCrows)
        {
            HandleStateChanged(crow);
        }
    }

    private void SpawnNest(Crow crow)
    {
        if (_availableStations.Count == 0)
        {
            Debug.LogWarning($"[Berçário] Não há estações livres para a gestação da ave [{crow.ID}].");
            return;
        }

        Transform station = _availableStations.Dequeue();
        
        PhysicalNest newNest = Instantiate(_nestPrefab, station.position, Quaternion.identity, station);
        newNest.Setup(crow.ID);
        
        _activeNests.Add(crow.ID, newNest);
    }

    private void RemoveNest(string crowId, PhysicalNest physicalNest)
    {
        // Devolve a estação para a fila de disponibilidade
        _availableStations.Enqueue(physicalNest.transform.parent);
        _activeNests.Remove(crowId);
        
        Destroy(physicalNest.gameObject); 
    }
}