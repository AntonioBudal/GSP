using System.Collections.Generic;
using UnityEngine;

public class RoomManager_Training : MonoBehaviour
{
    [Header("Configuração de Instalações")]
    [Tooltip("O Prefab tipado da ave (evita GetComponent desnecessário)")]
    [SerializeField] private PhysicalCrow _physicalCrowPrefab; 
    
    [Tooltip("Lugares físicos onde as aves ficarão posicionadas")]
    [SerializeField] private Transform[] _tortureStations;

    // Cache 1: Mapeia o ID do corvo para a sua instância física atual na sala
    private Dictionary<string, PhysicalCrow> _activeCrows = new Dictionary<string, PhysicalCrow>();
    
    // Cache 2: Fila de estações livres (evita varrer arrays procurando espaço)
    private Queue<Transform> _availableStations = new Queue<Transform>();

    private void Awake()
    {
        // Alimenta a fila com todas as estações disponíveis no início
        foreach (var station in _tortureStations)
        {
            _availableStations.Enqueue(station);
        }
    }

    private void OnEnable()
    {
        // Gerenciamento estrito de ciclo de vida e eventos
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.StateController != null)
        {
            GameBootstrap.Instance.StateController.OnStateChanged += HandleStateChanged;
            FullSync(); // Sincroniza o estado atual caso a sala seja ativada depois do jogo começar
        }
    }

    private void OnDisable()
    {
        // Prevenção de Memory Leaks
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.StateController != null)
        {
            GameBootstrap.Instance.StateController.OnStateChanged -= HandleStateChanged;
        }
    }

    /// <summary>
    /// Reage APENAS à ave que sofreu mutação de estado.
    /// </summary>
    private void HandleStateChanged(Crow crow)
    {
        bool isInRoom = _activeCrows.TryGetValue(crow.ID, out PhysicalCrow physicalCrow);
        bool shouldBeInRoom = crow.CurrentState == CrowState.EmTreino;

        if (shouldBeInRoom && !isInRoom)
        {
            SpawnCrow(crow);
        }
        else if (!shouldBeInRoom && isInRoom)
        {
            RemoveCrow(crow.ID, physicalCrow);
        }
    }

    /// <summary>
    /// Sincronização inicial em lote (chamada apenas ao ligar a sala).
    /// </summary>
    private void FullSync()
    {
        var allCrows = GameBootstrap.Instance.CrowRepo.GetAllCrows();
        foreach (var crow in allCrows)
        {
            HandleStateChanged(crow);
        }
    }

    private void SpawnCrow(Crow crow)
    {
        if (_availableStations.Count == 0)
        {
            Debug.LogWarning($"[Matadouro] Não há estações livres para alocar a ave [{crow.ID}].");
            return;
        }

        Transform station = _availableStations.Dequeue();
        
        // Em um ecossistema com Object Pool, trocaríamos o Instantiate por um PoolManager.Get()
        PhysicalCrow newCrow = Instantiate(_physicalCrowPrefab, station.position, Quaternion.identity, station);
        
        // Chamada tipada, segura e rastreável via IDE. Adeus, SendMessage.
        newCrow.SetupCrow(crow.ID); 
        
        _activeCrows.Add(crow.ID, newCrow);
    }

    private void RemoveCrow(string crowId, PhysicalCrow physicalCrow)
    {
        // A estação (pai do objeto) volta para a fila de disponibilidade
        _availableStations.Enqueue(physicalCrow.transform.parent);
        
        _activeCrows.Remove(crowId);
        
        // Em um ecossistema com Object Pool, trocaríamos o Destroy por um PoolManager.Release()
        Destroy(physicalCrow.gameObject); 
    }
}