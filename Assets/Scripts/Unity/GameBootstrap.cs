// Assets/Scripts/Unity/GameBootstrap.cs
using UnityEngine;
using System.Collections.Generic;

public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }

    // Referências para os sistemas do Core
    public GameClock Clock { get; private set; }
    public MapManager Map { get; private set; }
    public SimpleCrowRepository CrowRepo { get; private set; }
    public CrowStateController StateController { get; private set; }
    public TrainingManager Training { get; private set; }
    public ExpeditionManager Expeditions { get; private set; }
    public ProgressionManager Progression { get; private set; }
    public InfluenceManager Influence { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCore();
        }
        else { Destroy(gameObject); }
    }

    private void InitializeCore()
    {
        // 1. Instanciar bases
        Clock = new GameClock(1);
        StateController = new CrowStateController();
        CrowRepo = new SimpleCrowRepository();
        var rng = new SystemRandom();

        // 2. Criar Dados Simulados (3 Regiões)
        var reg1 = new Region("REG_BASE", "Mosteiro de Skellig", 2);
        var reg2 = new Region("REG_MAR", "Mar do Norte", 4);
        var reg3 = new Region("REG_NORTE", "Costa Nórdica", 6);
        reg1.AddNeighbor(reg2);
        reg2.AddNeighbor(reg3);
        
        Map = new MapManager(new[] { reg1, reg2, reg3 });
        Map.UnlockStartingRegion("REG_BASE"); // Começamos aqui

        // 3. Criar Dados Simulados (3 Corvos)
        var genetics = new GeneticSeed(null); // Genética pura para o MVP
        CrowRepo.AddCrow(new Crow("Corvo_Batedor", 5, 3, 3, 25, genetics));
        CrowRepo.AddCrow(new Crow("Corvo_Mensageiro", 3, 5, 4, 20, genetics));
        CrowRepo.AddCrow(new Crow("Corvo_Alfa", 4, 4, 5, 30, genetics));

        // 4. Inicializar Gerentes de Influência e Progressão
        var demoData = new Dictionary<string, DemographicsData> {
            { "REG_BASE", new DemographicsData(100, 2) },
            { "REG_MAR", new DemographicsData(0, 5) },
            { "REG_NORTE", new DemographicsData(1000, 8) }
        };
        Influence = new InfluenceManager(Map, Clock, demoData);
        Progression = new ProgressionManager(Clock, Map);

        // 5. Inicializar Motores e Orquestradores de Missão
        var exploreEngine = new ExplorationEngine(rng);
        var evangelEngine = new EvangelizationEngine(rng);
        var revealService = new MapRevealService(Map, rng);

        Training = new TrainingManager(StateController, Clock, CrowRepo, Progression);
        
        Expeditions = new ExpeditionManager(
            StateController, Clock, exploreEngine, evangelEngine, 
            Map, revealService, Influence, CrowRepo
        );

        Debug.Log("[Bootstrap] Core inicializado com sucesso. Dados mockados injetados.");
    }
}