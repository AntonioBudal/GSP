using UnityEngine;

public class PhysicalCrow : BaseInteractable
{
    [Header("Identificação do Domínio")]
    [SerializeField] private string _crowId;

    private void OnEnable()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Clock != null)
        {
            GameBootstrap.Instance.Clock.OnDayEnded += OnDayChanged;
        }
        EvaluatePresence();
    }

    private void OnDisable()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Clock != null)
        {
            GameBootstrap.Instance.Clock.OnDayEnded -= OnDayChanged;
        }
    }

    public override void Interact()
    {
        if (GameBootstrap.Instance == null || GameBootstrap.Instance.CrowRepo == null || string.IsNullOrEmpty(_crowId)) 
            return;

        var crowEntity = GameBootstrap.Instance.CrowRepo.GetCrow(_crowId);
        if (crowEntity == null) return;

        UIManager.Instance.OpenCrowDetails(_crowId);
    }

    private void OnDayChanged(int currentDay) => EvaluatePresence();

    public void SetupCrow(string id)
    {
        _crowId = id;
        EvaluatePresence();
    }

    public void EvaluatePresence()
    {
        if (GameBootstrap.Instance == null || GameBootstrap.Instance.CrowRepo == null || string.IsNullOrEmpty(_crowId)) 
            return;

        var crowEntity = GameBootstrap.Instance.CrowRepo.GetCrow(_crowId);
        if (crowEntity == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Se a ave sofreu mutação para um estado invisível no pátio físico, oculta a instância
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
}