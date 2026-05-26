using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UniversalRoomManager : MonoBehaviour
{
    public enum RoomBehavior 
    { 
        Normal,         // Apenas coloca e tira o objeto da sala (Ex: Treino)
        Bercario        // Coloca, tira e atualiza a barra de progresso (Gestação)
    }

    [Header("Identidade da Sala")]
    [Tooltip("Define como a sala opera sob o capô.")]
    [SerializeField] private RoomBehavior _behavior;
    
    [Tooltip("Qual estado faz a entidade vir parar nesta sala?")]
    [SerializeField] private CrowState _targetState;

    [Header("Instalações Físicas")]
    [Tooltip("O Prefab que representa a entidade nesta sala (Corvo, Ninho, etc).")]
    [SerializeField] private GameObject _prefab;
    
    [Tooltip("Os Transforms vazios que marcam os locais disponíveis.")]
    [SerializeField] private Transform[] _stations;

    private Dictionary<string, GameObject> _activeEntities = new Dictionary<string, GameObject>();
    private Queue<Transform> _availableStations = new Queue<Transform>();

    private void Awake()
    {
        // Enche a fila de estações vazias
        foreach (var station in _stations)
        {
            _availableStations.Enqueue(station);
        }
    }

    private void OnEnable()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.StateController != null)
        {
            GameBootstrap.Instance.StateController.OnStateChanged += HandleStateChanged;
            
            // Se for Berçário, assina o relógio para mover as barras de progresso
            if (_behavior == RoomBehavior.Bercario && GameBootstrap.Instance.Clock != null)
            {
                GameBootstrap.Instance.Clock.OnDayEnded += HandleDayEnded;
            }

            FullSync();
        }
    }

    private void OnDisable()
    {
        if (GameBootstrap.Instance != null)
        {
            if (GameBootstrap.Instance.StateController != null)
                GameBootstrap.Instance.StateController.OnStateChanged -= HandleStateChanged;
            
            if (_behavior == RoomBehavior.Bercario && GameBootstrap.Instance.Clock != null)
                GameBootstrap.Instance.Clock.OnDayEnded -= HandleDayEnded;
        }
    }

    private void HandleStateChanged(Crow crow)
    {
        bool isInRoom = _activeEntities.TryGetValue(crow.ID, out GameObject entity);
        bool shouldBeInRoom = crow.CurrentState == _targetState;

        if (shouldBeInRoom && !isInRoom) SpawnEntity(crow);
        else if (!shouldBeInRoom && isInRoom) RemoveEntity(crow.ID, entity);
    }

    private void FullSync()
    {
        var allCrows = GameBootstrap.Instance.CrowRepo.GetAllCrows();
        foreach (var crow in allCrows) HandleStateChanged(crow);
    }

    private void SpawnEntity(Crow crow)
    {
        if (_availableStations.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] Estações esgotadas para a ave [{crow.ID}].");
            return;
        }

        Transform station = _availableStations.Dequeue();
        GameObject newObj = Instantiate(_prefab, station.position, Quaternion.identity, station);
        
        // Se o prefab for o corvo físico interativo, avisa ele quem ele é
        var physicalCrow = newObj.GetComponent<PhysicalCrow>();
        if (physicalCrow != null) physicalCrow.SetupCrow(crow.ID);

        _activeEntities.Add(crow.ID, newObj);
    }

    private void RemoveEntity(string crowId, GameObject entity)
    {
        // Devolve a estação para o pool e destrói o objeto
        _availableStations.Enqueue(entity.transform.parent);
        _activeEntities.Remove(crowId);
        Destroy(entity);
    }

    private void HandleDayEnded(int currentDay)
    {
        // Esta lógica só roda se a sala estiver configurada como Bercario no Editor
        if (_behavior != RoomBehavior.Bercario) return;

        foreach (var kvp in _activeEntities)
        {
            string crowId = kvp.Key;
            GameObject nestObj = kvp.Value;

            // Como o PhysicalNest foi apagado, o RoomManager busca a Image diretamente no Prefab.
            // (Requisito: O Prefab do Ninho deve ter um componente Image do tipo 'Filled').
            Image fillImage = nestObj.GetComponentInChildren<Image>();
            
            if (fillImage != null)
            {
                // Aqui você vai ligar a leitura do progresso do seu Core de Genética
                // Exemplo:
                // float progress = GameBootstrap.Instance.Breeding.GetGestationProgressPercentage(crowId);
                // fillImage.fillAmount = Mathf.Clamp01(progress);
            }
        }
    }
}