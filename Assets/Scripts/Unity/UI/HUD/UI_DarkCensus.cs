using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class UI_DarkCensus : MonoBehaviour
{
    [Header("Passagem do Tempo")]
    [SerializeField] private TextMeshProUGUI _txtDay;

    [Header("Censo do Mosteiro")]
    [SerializeField] private TextMeshProUGUI _txtTotalAlive;
    [SerializeField] private TextMeshProUGUI _txtTraining;
    [SerializeField] private TextMeshProUGUI _txtGestating;
    [SerializeField] private TextMeshProUGUI _txtExpedition;

    private void Start()
    {
        // Aguarda a injeção do Core via Bootstrap
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.StateController != null)
        {
            SubscribeToEvents();
            RefreshAll();
        }
        else if (GameBootstrap.Instance != null)
        {
            GameBootstrap.Instance.OnCoreInitialized += () => 
            {
                SubscribeToEvents();
                RefreshAll();
            };
        }
    }

    private void SubscribeToEvents()
    {
        var boot = GameBootstrap.Instance;

        // Assina o Relógio
        if (boot.Clock != null)
            boot.Clock.OnDayEnded += HandleDayEnded;

        // Assina as Mutações Biológicas
        if (boot.StateController != null)
            boot.StateController.OnStateChanged += HandleStateChanged;
        
        if (boot.Breeding != null)
            boot.Breeding.OnChildBorn += HandleChildBorn;
    }

    private void HandleDayEnded(int currentDay)
    {
        if (_txtDay != null) _txtDay.text = $"Dia {currentDay}";
    }

    private void HandleStateChanged(Crow crow) => RefreshCensorship();
    private void HandleChildBorn(Crow newCrow) => RefreshCensorship();

    private void RefreshAll()
    {
        if (GameBootstrap.Instance.Clock != null)
            HandleDayEnded(GameBootstrap.Instance.Clock.CurrentDay);
            
        RefreshCensorship();
    }

    /// <summary>
    /// Varredura O(N) limpa sobre os corvos ativos.
    /// </summary>
    private void RefreshCensorship()
    {
        if (GameBootstrap.Instance.CrowRepo == null) return;

        var allCrows = GameBootstrap.Instance.CrowRepo.GetAllCrows();

        int totalAlive = 0, inTraining = 0, gestating = 0, inExpedition = 0;

        foreach (var crow in allCrows)
        {
            if (crow.CurrentState == CrowState.Morto || crow.CurrentState == CrowState.Perdido)
                continue;

            totalAlive++;

            if (crow.CurrentState == CrowState.EmTreino) inTraining++;
            else if (crow.CurrentState == CrowState.Gestando) gestating++;
            else if (crow.CurrentState == CrowState.EmExpedicao) inExpedition++;
        }

        // Atualiza os textos
        if (_txtTotalAlive != null) _txtTotalAlive.text = totalAlive.ToString();
        if (_txtTraining != null) _txtTraining.text = inTraining.ToString();
        if (_txtGestating != null) _txtGestating.text = gestating.ToString();
        if (_txtExpedition != null) _txtExpedition.text = inExpedition.ToString();
    }

    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null)
        {
            if (GameBootstrap.Instance.Clock != null)
                GameBootstrap.Instance.Clock.OnDayEnded -= HandleDayEnded;

            if (GameBootstrap.Instance.StateController != null)
                GameBootstrap.Instance.StateController.OnStateChanged -= HandleStateChanged;

            if (GameBootstrap.Instance.Breeding != null)
                GameBootstrap.Instance.Breeding.OnChildBorn -= HandleChildBorn;
        }
    }
}