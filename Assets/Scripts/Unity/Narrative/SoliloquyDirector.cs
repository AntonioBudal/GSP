// Assets/Scripts/Unity/Narrative/SoliloquyDirector.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoliloquyDirector : MonoBehaviour
{
    public static SoliloquyDirector Instance { get; private set; }

    [Header("Banco de Dados")]
    [Tooltip("Arraste os ScriptableObjects de falas aqui, ou os carregaremos da pasta Resources.")]
    public List<SoliloquyData> allSoliloquies;

    // Fila de falas pendentes
    private Queue<SoliloquyData> _pendingThoughts = new Queue<SoliloquyData>();
    
    // Controle de quais falas já foram reproduzidas (para não repetir eventos únicos)
    private HashSet<string> _playedSoliloquyIds = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Se a lista estiver vazia no Inspector, tenta carregar da pasta Resources/Soliloquies
        if (allSoliloquies == null || allSoliloquies.Count == 0)
        {
            allSoliloquies = Resources.LoadAll<SoliloquyData>("Soliloquies").ToList();
        }
    }

    private void Start()
    {
        // Se o Bootstrap já estiver pronto (ex: cenas recarregadas muito rápido)
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Clock != null)
        {
            SubscribeToCoreEvents();
        }
        else if (GameBootstrap.Instance != null)
        {
            // Fica esperando o Bootstrap gritar que terminou
            GameBootstrap.Instance.OnCoreInitialized += SubscribeToCoreEvents;
        }
    }

    private void SubscribeToCoreEvents()
    {
        if (GameBootstrap.Instance != null)
            GameBootstrap.Instance.OnCoreInitialized -= SubscribeToCoreEvents;

        if (GameBootstrap.Instance.Clock != null)
        {
            GameBootstrap.Instance.Clock.OnDayEnded += HandleDayTick;
        }

        if (GameBootstrap.Instance.Progression != null)
        {
            GameBootstrap.Instance.Progression.OnMilestoneReached += HandleMilestone;
        }

        // --- NOVO: CONEXÃO DA MORTE COM A NARRATIVA ---
        if (GameBootstrap.Instance.StateController != null)
        {
            GameBootstrap.Instance.StateController.OnCrowDied += HandleCrowDeath;
            Debug.Log("[SoliloquyDirector] Inscrito nas fatalidades do StateController.");
        }
    }

    // O Handler direto que ouve o Domínio
    private void HandleCrowDeath(Crow deadCrow)
    {
        Debug.Log($"[Soliloquy] O diretor notou a morte de {deadCrow.ID}. Invocando luto.");
        TriggerSoliloquy(TriggerCondition.PrimeiraMorte);
    }

    // --- OS GATILHOS ---

    private void HandleMilestone(MilestoneID id, string title)
    {
        TriggerSoliloquy(TriggerCondition.MilestoneAtingido);
    }

    private void HandleDayTick(int currentDay)
    {
        Debug.Log($"[Soliloquy] O relógio bateu no dia {currentDay}.");
        
        if (currentDay % 5 == 0 && _pendingThoughts.Count == 0)
        {
            Debug.Log("[Soliloquy] Tentando disparar gatilho OCIOSO...");
            TriggerSoliloquy(TriggerCondition.Ocioso);
        }
    }

    // Método exposto para que outros sistemas (como o TrainingManager via HUD) possam disparar mortes.
    public void NotifyCrowDeath()
    {
        TriggerSoliloquy(TriggerCondition.PrimeiraMorte);
    }

    // --- O MOTOR DA FILA ---

    private void TriggerSoliloquy(TriggerCondition condition)
    {
        // 1. Busca todas as falas que atendem à condição e que ainda NÃO tocaram
        var candidates = allSoliloquies
            .Where(s => s.Condition == condition && !_playedSoliloquyIds.Contains(s.ID))
            .OrderByDescending(s => s.Priority)
            .ToList();


        Debug.Log($"[Soliloquy] Encontrou {candidates.Count} falas candidatas para {condition}.");
        if (candidates.Count > 0)
        {
            // Pega a de maior prioridade (ou a primeira da lista ordenada)
            var chosen = candidates[0];
            
            _pendingThoughts.Enqueue(chosen);
            _playedSoliloquyIds.Add(chosen.ID); // Marca como usada
            
            // Avisa o UIManager para exibir a fala se a interface não estiver ocupada
            UIManager.Instance.TryDisplayNextSoliloquy();
        }
    }

    // Método chamado pela UI quando ela está pronta para ler a próxima fala
    public SoliloquyData GetNextThought()
    {
        if (_pendingThoughts.Count > 0)
        {
            return _pendingThoughts.Dequeue();
        }
        return null;
    }

    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Progression != null)
        {
            GameBootstrap.Instance.Progression.OnMilestoneReached -= HandleMilestone;
            GameBootstrap.Instance.Clock.OnDayEnded -= HandleDayTick;
        }
    }
}