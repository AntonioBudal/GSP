// Assets/Scripts/Unity/GameBootstrap.cs
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Corvus.Core.SaveSystem; // A linha que resolve o erro!

public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }

    public GameClock Clock { get; private set; }
    public MapManager Map { get; private set; }
    public SimpleCrowRepository CrowRepo { get; private set; }
    public CrowStateController StateController { get; private set; }
    public TrainingManager Training { get; private set; }
    public ExpeditionManager Expeditions { get; private set; }
    public ProgressionManager Progression { get; private set; }
    public InfluenceManager Influence { get; private set; }
    public BreedingManager Breeding { get; private set; }
    public Corvus.Core.Progression.OnboardingManager Onboarding { get; private set; }
    
    // O Orquestrador de Save
    public SaveManager SaveManager { get; private set; }

    public event System.Action OnCoreInitialized;

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Inicia o universo de forma assíncrona
            await InitializeCoreAsync();
        }
        else { Destroy(gameObject); }
    }
private async Task InitializeCoreAsync()
{
        // 1. Checa o Disco buscando o Slot selecionado na UI do Menu
        int activeSlot = GameStateController.Instance.SelectedSlotID;
        string basePath = Application.persistentDataPath;
        
        // Injeta os dois argumentos exigidos pela nova arquitetura de múltiplos slots
        SaveService saveService = new SaveService(basePath, activeSlot);
        
        SaveGameDTO save = null;
        if (saveService.HasSaveFile())
        {
            save = await saveService.LoadGameAsync();
        }

        // 2. Instanciar bases
        int startDay = (save != null) ? save.CurrentDay : 1;
        Clock = new GameClock(startDay);
        
        StateController = new CrowStateController();
        var rng = new SystemRandom(); 
        var nativeRng = new System.Random(); // O nativo para a Genética

        // 3. Montar a Geografia
        var reg1 = new Region("REG_BASE", "Mosteiro de Skellig", 2);
        var reg2 = new Region("REG_MAR", "Mar do Norte", 4);
        var reg3 = new Region("REG_NORTE", "Costa Nórdica", 6);
        reg1.AddNeighbor(reg2);
        reg2.AddNeighbor(reg3);
        
        // 4. Injetar DTOs nos Repositórios
        if (save != null)
        {
            Map = new MapManager(new[] { reg1, reg2, reg3 }, save.Regions);
            CrowRepo = new SimpleCrowRepository(save.Crows);
            Map.TryDiscoverRegion("REG_MAR"); 
            Debug.Log($"[Bootstrap] Jogo Carregado. Bem-vindo de volta ao Dia {Clock.CurrentDay} no Slot {activeSlot}.");
        }
        else
        {
            Map = new MapManager(new[] { reg1, reg2, reg3 });
            Map.UnlockStartingRegion("REG_BASE"); 
            Map.TryDiscoverRegion("REG_MAR");      

            CrowRepo = new SimpleCrowRepository();
            var genetics = new GeneticSeed(null);
            CrowRepo.AddCrow(new Crow("Corvo_Batedor", 5, 3, 3, 25, genetics));
            CrowRepo.AddCrow(new Crow("Corvo_Mensageiro", 3, 5, 4, 20, genetics));
            CrowRepo.AddCrow(new Crow("Corvo_Alfa", 4, 4, 5, 30, genetics));
            Debug.Log($"[Bootstrap] Novo Jogo Iniciado no Slot {activeSlot}. Dados padrão injetados.");
        }

        // 5. Inicializar Gerentes de Influência e Progressão
        var demoData = new Dictionary<string, DemographicsData> {
            { "REG_BASE", new DemographicsData(100, 2) },
            { "REG_MAR", new DemographicsData(0, 5) },
            { "REG_NORTE", new DemographicsData(1000, 8) }
        };
        Influence = new InfluenceManager(Map, Clock, demoData, save?.Influence);
        Progression = new ProgressionManager(Clock, Map, save?.Progression);

        // 6. Motores e Orquestradores
        var exploreEngine = new ExplorationEngine(rng);
        var evangelEngine = new EvangelizationEngine(rng);
        var revealService = new MapRevealService(Map, rng);
        var geneticsEngine = new GeneticsEngine(nativeRng); 

        Training = new TrainingManager(StateController, Clock, CrowRepo, Progression, save?.Trainings, save?.Fatigue);
        
        Expeditions = new ExpeditionManager(
            StateController, Clock, exploreEngine, evangelEngine, 
            Map, revealService, Influence, CrowRepo, 
            Progression, 
            save?.Expeditions
        );

        Breeding = new BreedingManager(StateController, Clock, geneticsEngine, CrowRepo.GetCrow);
        
        Breeding.OnChildBorn += (newCrow) => 
        {
            CrowRepo.AddCrow(newCrow);
            Debug.Log($"[Berçário] Um novo corvo nasceu: {newCrow.ID}.");
        };

        // 6.5 Injetar Onboarding (Tutorial)
        Onboarding = new Corvus.Core.Progression.OnboardingManager(Expeditions, Training, Clock, Progression, null);

        // 7. Inicializar o Sistema de Save Manual
        // Como activeSlot já foi declarado e validado no Passo 1, apenas o reutilizamos aqui de forma limpa
        SaveManager = new SaveManager(activeSlot, saveService, Clock, Map, CrowRepo, Progression, Influence, Expeditions, Training);
        
        OnCoreInitialized?.Invoke();
    }

    private void EvaluateExtinction(Crow deadCrow)
    {
        // 1. O Repositório tem alguma ave viável?
        bool hasLivingCrows = false;
        foreach (var crow in CrowRepo.GetAllCrows())
        {
            if (crow.CurrentState != CrowState.Morto && crow.CurrentState != CrowState.Perdido)
            {
                hasLivingCrows = true;
                break;
            }
        }

        // 2. O Berçário tem alguma semente prometida? (BreedingManager precisa expor a contagem)
        // Se o seu BreedingManager.cs não tem esse método, assumiremos a extinção imediata pela morte das aves ativas para o MVP.
        bool hasPendingBirths = Breeding.HasActiveGestations(); 

        if (!hasLivingCrows && !hasPendingBirths)
        {
            Debug.LogError("<color=#FF0000>[SISTEMA CRÍTICO] A linhagem se extinguiu. O mosteiro caiu.</color>");
            GameStateController.Instance.ChangeState(GameState.GameOver); // Adicione 'GameOver' no enum GameState
            
            // Avisa a UI para jogar a tela de luto (Fase 9.2)
            UIManager.Instance.ShowExtinctionScreen();
        }
    }

    private void OnDestroy()
    {
        if (Breeding != null) Breeding.Dispose();
    }
}