using UnityEngine;

[RequireComponent(typeof(HUDTimeView))]
public class HUDTimePresenter : MonoBehaviour
{
    private HUDTimeView _view;
    private GameClock _clock;

    private void Awake()
    {
        _view = GetComponent<HUDTimeView>();
        
        // Bloqueia a UI até o universo existir
        _view.SetInteractable(false); 
        _view.UpdateDayText(0);

        // Assina o clique do botão da View
        _view.NextDayButton.onClick.AddListener(OnNextDayClicked);
    }

    private void Start()
    {
        // Tenta se conectar ao Core. Se já estiver pronto (ex: recarregou a cena rápido), vai direto.
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Clock != null)
        {
            InitializeWithCore(GameBootstrap.Instance.Clock);
        }
        else if (GameBootstrap.Instance != null)
        {
            // Fica na escuta aguardando o "Clarim" do Bootstrap
            GameBootstrap.Instance.OnCoreInitialized += HandleCoreInitialized;
        }
    }

    private void HandleCoreInitialized()
    {
        // Limpa a inscrição para não ser chamado duas vezes
        GameBootstrap.Instance.OnCoreInitialized -= HandleCoreInitialized;
        InitializeWithCore(GameBootstrap.Instance.Clock);
    }

    private void InitializeWithCore(GameClock clock)
    {
        _clock = clock;

        // O Presenter assina os eventos do Domínio
        _clock.OnDayEnded += HandleDayEnded;

        // Atualiza a View para o estado inicial lido do Save/Novo Jogo
        _view.UpdateDayText(_clock.CurrentDay);
        _view.SetInteractable(true);
    }

    private void OnNextDayClicked()
    {
        if (_clock == null) return;

        // Boa Prática de UX: Desabilitar o botão momentaneamente 
        // para evitar que o jogador "spamme" o clique e pule 5 dias por acidente
        _view.SetInteractable(false);

        _clock.AdvanceTime(1);

        // Como o AdvanceTime é síncrono no nosso domínio, podemos reativar logo em seguida.
        _view.SetInteractable(true);
    }

    private void HandleDayEnded(int currentDay)
    {
        // O Model avisou que o tempo passou. A View obedece.
        _view.UpdateDayText(currentDay);
    }

    // A REGRA DE OURO PARA EVITAR MEMORY LEAKS NA UNITY
    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null)
        {
            GameBootstrap.Instance.OnCoreInitialized -= HandleCoreInitialized;
        }

        if (_clock != null)
        {
            _clock.OnDayEnded -= HandleDayEnded;
        }

        if (_view != null && _view.NextDayButton != null)
        {
            _view.NextDayButton.onClick.RemoveListener(OnNextDayClicked);
        }
    }
}