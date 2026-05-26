using System.Collections.Generic;
using UnityEngine;

public class TempleRosterPresenter : MonoBehaviour
{
    [SerializeField] private CrowCardView _crowCardPrefab;
    [SerializeField] private Transform _cardsContainer; // Um objeto com VerticalLayoutGroup

    private ICrowRepository _crowRepo;
    private List<CrowCardView> _spawnedCards = new List<CrowCardView>();

    private void Start()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.CrowRepo != null)
        {
            Initialize(GameBootstrap.Instance.CrowRepo, GameBootstrap.Instance.Clock);
        }
        else if (GameBootstrap.Instance != null)
        {
            GameBootstrap.Instance.OnCoreInitialized += WaitAndInitialize;
        }
    }

    private void WaitAndInitialize()
    {
        GameBootstrap.Instance.OnCoreInitialized -= WaitAndInitialize;
        Initialize(GameBootstrap.Instance.CrowRepo, GameBootstrap.Instance.Clock);
    }

    private void Initialize(ICrowRepository repo, GameClock clock)
    {
        _crowRepo = repo;
        
        // Assinamos o relógio para atualizar a lista sempre que o dia virar
        clock.OnDayEnded += RefreshRoster;
        
        // Primeira atualização
        RefreshRoster(clock.CurrentDay);
    }

    private void RefreshRoster(int currentDay)
    {
        // Limpa a lista antiga (idealmente faríamos Object Pooling, mas para MVP o Destroy resolve)
        foreach (var card in _spawnedCards)
        {
            Destroy(card.gameObject);
        }
        _spawnedCards.Clear();

        // Lê o repositório de domínio e cria as views
        foreach (var crow in _crowRepo.GetAllCrows())
        {
            // O "false" (worldPositionStays) é o segredo para UI não quebrar a escala
            var newCard = Instantiate(_crowCardPrefab, _cardsContainer, false);
            newCard.Setup(
                crow.ID, 
                crow.Role, 
                crow.GetStat(CrowStat.Speed), 
                crow.GetStat(CrowStat.Focus), 
                crow.GetStat(CrowStat.Resilience), 
                crow.Lifespan, 
                crow.CurrentState
            );
            _spawnedCards.Add(newCard);
        }
    }

    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null)
        {
            GameBootstrap.Instance.OnCoreInitialized -= WaitAndInitialize;
            if (GameBootstrap.Instance.Clock != null)
                GameBootstrap.Instance.Clock.OnDayEnded -= RefreshRoster;
        }
    }
}