// Assets/Scripts/Unity/GameStateController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // Memória do slot selecionado (-1 significa nenhum)
    public int SelectedSlotID { get; set; } = -1; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Scene_Boot")
        {
            ChangeState(GameState.Boot);
            // Em vez de carregar a MainScene, agora vamos para o Menu
            Invoke(nameof(LoadMainMenu), 0.5f); 
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

    // Este método agora será chamado pelo Menu, e não mais pelo Boot
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