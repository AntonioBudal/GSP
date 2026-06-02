// Assets/Scripts/Unity/GameStateController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Corvus.Core.Settings; // Necessário para acessar o SettingsManager

public enum GameState
{
    Boot,
    Menu,
    Playing,
    Paused,
    GameOver 
}

public class GameStateController : MonoBehaviour
{
    public static GameStateController Instance { get; private set; }
    
    public GameState CurrentState { get; private set; }
    public int SelectedSlotID { get; set; } = -1; 

    // A única origem da verdade global para as configurações
    public SettingsManager Settings { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 

            // Instancia o Manager imediatamente para que outros scripts possam assinar os eventos
            string basePath = Application.persistentDataPath;
            var service = new SettingsService(basePath);
            Settings = new SettingsManager(service);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        if (SceneManager.GetActiveScene().name == "Scene_Boot")
        {
            ChangeState(GameState.Boot);

            // Aguarda a leitura do disco e a primeira aplicação de áudio/vídeo
            await Settings.InitializeAsync();

            // Após as configurações carregarem, vai para o Menu
            LoadMainMenu(); 
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[Engine] Estado alterado para: {newState}");
    }

    private void LoadMainMenu()
    {
        Debug.Log("[Engine] Boot concluído. Carregando Scene_MainMenu...");
        SceneManager.LoadScene("Scene_MainMenu");
        ChangeState(GameState.Menu);
    }

    public void LoadMainScene()
    {
        if (SelectedSlotID < 0)
        {
            Debug.LogError("[Engine] Tentativa de iniciar o jogo sem um Slot de Save definido!");
            return;
        }

        Debug.Log($"[Engine] Carregando Scene_Main com o Slot {SelectedSlotID}...");
        SceneManager.LoadScene("Scene_Main");
        ChangeState(GameState.Playing);
    }
}