using UnityEngine;
using TMPro;

public class UI_TopBarController : MonoBehaviour
{
    [Header("Textos de Contagem")]
    [SerializeField] private TextMeshProUGUI _txtTotalAlive;
    [SerializeField] private TextMeshProUGUI _txtTraining;
    [SerializeField] private TextMeshProUGUI _txtGestating;
    [SerializeField] private TextMeshProUGUI _txtExpedition;

    private void Start()
    {
        // Precisamos garantir que o Bootstrap carregou o core antes de assinar os eventos
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.StateController != null)
        {
            SubscribeToEvents();
            RefreshCensorship(); // Primeira leitura ao ligar o jogo
        }
        else if (GameBootstrap.Instance != null)
        {
            // Se o Bootstrap ainda estiver carregando, esperamos ele terminar
            GameBootstrap.Instance.OnCoreInitialized += () => 
            {
                SubscribeToEvents();
                RefreshCensorship();
            };
        }
    }

    private void SubscribeToEvents()
    {
        // A UI escuta as mudanças de estado dos corvos vivos
        GameBootstrap.Instance.StateController.OnStateChanged += HandleStateChanged;
        
        // A UI escuta o nascimento de novas aves (pois elas são injetadas direto no Repo)
        if (GameBootstrap.Instance.Breeding != null)
        {
            GameBootstrap.Instance.Breeding.OnChildBorn += HandleChildBorn;
        }
    }

    private void HandleStateChanged(Crow crow) => RefreshCensorship();
    private void HandleChildBorn(Crow newCrow) => RefreshCensorship();

    /// <summary>
    /// Lê a fonte da verdade no Repositório e recalcula os blocos.
    /// </summary>
    private void RefreshCensorship()
    {
        var allCrows = GameBootstrap.Instance.CrowRepo.GetAllCrows();

        int totalAlive = 0;
        int inTraining = 0;
        int gestating = 0;
        int inExpedition = 0;

        foreach (var crow in allCrows)
        {
            // Ignora os caídos e perdidos da contagem de aves ativas
            if (crow.CurrentState == CrowState.Morto || crow.CurrentState == CrowState.Perdido)
                continue;

            totalAlive++;

            // Agrupa os corvos por ocupação
            if (crow.CurrentState == CrowState.EmTreino) inTraining++;
            else if (crow.CurrentState == CrowState.Gestando) gestating++;
            else if (crow.CurrentState == CrowState.EmExpedicao) inExpedition++;
        }

        // Atualiza a interface
        _txtTotalAlive.text = totalAlive.ToString();
        _txtTraining.text = inTraining.ToString();
        _txtGestating.text = gestating.ToString();
        _txtExpedition.text = inExpedition.ToString();
    }

    private void OnDestroy()
    {
        // Limpeza de segurança para evitar Memory Leaks
        if (GameBootstrap.Instance != null)
        {
            if (GameBootstrap.Instance.StateController != null)
                GameBootstrap.Instance.StateController.OnStateChanged -= HandleStateChanged;

            if (GameBootstrap.Instance.Breeding != null)
                GameBootstrap.Instance.Breeding.OnChildBorn -= HandleChildBorn;
        }
    }
}