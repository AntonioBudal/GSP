// Assets/Scripts/Unity/GameStateController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

// O estado macro da engine (diferente do CrowState que é do domínio)
public enum GameState
{
    Boot,
    Menu,
    Playing,
    Paused,
    GameOver // <--- ADICIONE ESTA LINHA
}

public class GameStateController : MonoBehaviour
{
    // Este é um dos poucos casos onde o padrão Singleton é aceitável na arquitetura,
    // pois ele é a raiz absoluta do ciclo de vida da engine.
    public static GameStateController Instance { get; private set; }
    
    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // A magia acontece aqui: este objeto não será destruído ao trocar de cena
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // O fluxo inicial do Boot
        if (SceneManager.GetActiveScene().name == "Scene_Boot")
        {
            ChangeState(GameState.Boot);
            Invoke(nameof(LoadMainScene), 0.5f); // Pequeno delay apenas para garantir inicializações pesadas
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[Engine] Estado alterado para: {newState}");
    }

    private void LoadMainScene()
    {
        Debug.Log("[Engine] Boot concluído. Carregando Scene_Main...");
        SceneManager.LoadScene("Scene_Main");
        ChangeState(GameState.Playing);
    }
}