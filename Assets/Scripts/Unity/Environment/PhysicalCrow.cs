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


    [Header("Âncoras de Sala (Posições no Mapa)")]
    [SerializeField] private Transform _patioAnchor;
    [SerializeField] private Transform _trainingRoomAnchor;
    [SerializeField] private Transform _nurseryAnchor;
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
         gameObject.SetActive(false);
         return;
     }

     // 1. Ocultação Absoluta
     if (crowEntity.CurrentState == CrowState.EmExpedicao || 
         crowEntity.CurrentState == CrowState.Morto || 
         crowEntity.CurrentState == CrowState.Perdido)
     {
         HidePrompt();
         gameObject.SetActive(false);
         return;
     }

     // 2. Presença Física - Ativa o objeto
     gameObject.SetActive(true);

     // 3. Posicionamento Espacial (Migração)
     switch (crowEntity.CurrentState)
     {
         case CrowState.Disponivel:
         case CrowState.Fadigado:
             if (_patioAnchor != null) transform.position = _patioAnchor.position;
             break;

         case CrowState.EmTreino:
             if (_trainingRoomAnchor != null) transform.position = _trainingRoomAnchor.position;
             break;

         case CrowState.Gestando:
             if (_nurseryAnchor != null) transform.position = _nurseryAnchor.position;
             break;
     }
 }

    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Clock != null)
        {
            GameBootstrap.Instance.Clock.OnDayEnded -= OnDayChanged;
        }
    }

    // Injetado pelo RoomManager no momento do nascimento/spawn
    public void SetupCrow(string id)
    {
        _crowId = id;
        EvaluatePresence();
    }
}