using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class PhysicalCrow : MonoBehaviour, IInteractable
{
    [Header("Amarra do Domínio")]
    [Tooltip("O ID exato deste corvo correspondente ao banco de dados (ex: Corvo_Batedor)")]
    [SerializeField] private string _crowId;

    [Header("Indicador Visual (Prompt)")]
    [SerializeField] private GameObject _floatingPromptObject;
    [SerializeField] private TextMeshPro _promptText;
    [SerializeField] private string _actionDescription = "Inspecionar Linhagem";

    public string InteractionPrompt => _actionDescription;

    private void Start()
    {
        // Garante que o indicador inicia apagado
        if (_floatingPromptObject != null) _floatingPromptObject.SetActive(false);

        // Se inscreve no relógio para reavaliar sua presença a cada novo amanhecer
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Clock != null)
        {
            GameBootstrap.Instance.Clock.OnDayEnded += OnDayChanged;
        }

        EvaluatePresence();
    }

    public void ShowPrompt()
    {
        if (_floatingPromptObject != null)
        {
            if (_promptText != null) _promptText.text = $"[F] {_actionDescription}";
            _floatingPromptObject.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (_floatingPromptObject != null) _floatingPromptObject.SetActive(false);
    }

    public void Interact()
    {
        // Valida se a ave ainda existe e está apta antes de acionar a UI
        var crowEntity = GameBootstrap.Instance.CrowRepo.GetCrow(_crowId);
        if (crowEntity == null) return;

        // Dispara a ordem de abertura para o gerenciador global
        UIManager.Instance.OpenCrowDetails(_crowId);
    }

    private void OnDayChanged(int currentDay)
    {
        // Toda virada de dia, checa se o corvo voltou de viagem, fadigou ou faleceu
        EvaluatePresence();
    }

    /// <summary>
    /// Sincronização de Existência (Fase 2.4): Ativa ou desativa o objeto físico 
    /// dependendo do estado biológico da entidade no Core.
    /// </summary>
    public void EvaluatePresence()
    {
        if (GameBootstrap.Instance == null || GameBootstrap.Instance.CrowRepo == null) return;

        var crowEntity = GameBootstrap.Instance.CrowRepo.GetCrow(_crowId);
        if (crowEntity == null)
        {
            // Se o corvo sumiu do banco de dados, destrói sua representação física
            gameObject.SetActive(false);
            return;
        }

        // Regra de Ocultação: Se viajou, morreu ou foi perdido, desaparece do cenário 2D
        if (crowEntity.CurrentState == CrowState.EmExpedicao || 
            crowEntity.CurrentState == CrowState.Morto || 
            crowEntity.CurrentState == CrowState.Perdido)
        {
            HidePrompt();
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Clock != null)
        {
            GameBootstrap.Instance.Clock.OnDayEnded -= OnDayChanged;
        }
    }
}