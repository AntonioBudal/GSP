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
        // Aguarda o Bootstrap iniciar o Core
        Invoke(nameof(SubscribeToCoreEvents), 0.2f);
    }

    private void SubscribeToCoreEvents()
    {
        if (GameBootstrap.Instance == null)
        {
            Debug.LogError("[SoliloquyDirector] GameBootstrap está nulo! O script rodou antes do jogo existir.");
            return;
        }

        // Tenta assinar o Relógio (Essencial)
        if (GameBootstrap.Instance.Clock != null)
        {
            GameBootstrap.Instance.Clock.OnDayEnded += HandleDayTick;
            Debug.Log("[SoliloquyDirector] Inscrito no Relógio com sucesso.");
        }
        else
        {
            Debug.LogError("[SoliloquyDirector] GameBootstrap.Clock está nulo!");
        }

        // Tenta assinar a Progressão (Opcional, não deve quebrar o resto se não existir)
        if (GameBootstrap.Instance.Progression != null)
        {
            GameBootstrap.Instance.Progression.OnMilestoneReached += HandleMilestone;
            Debug.Log("[SoliloquyDirector] Inscrito na Progressão com sucesso.");
        }
        else
        {
            Debug.LogWarning("[SoliloquyDirector] GameBootstrap.Progression está nulo. Gatilhos de Milestone não funcionarão ainda.");
        }
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