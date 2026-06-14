using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // A nossa "verdade absoluta" carregada em memória
    public SaveData CurrentSave { get; private set; }

    private string saveFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Caminho seguro e padronizado da Unity para salvar arquivos em qualquer plataforma
        saveFilePath = Path.Combine(Application.persistentDataPath, "corvus_save.json");
        
        // Já inicializa a memória (carrega do disco ou cria um novo)
        LoadGame();
    }

    private void Update()
    {
        // Atalhos de Debug para testarmos o fluxo
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadGame();
        }
    }

    public void SaveGame()
    {
        // Sincroniza o dia do TimeManager com o nosso dado de Save antes de gravar
        if (TimeManager.Instance != null)
        {
            CurrentSave.currentDay = TimeManager.Instance.CurrentDay;
        }

        // Converte o objeto C# para JSON (o parâmetro 'true' formata com quebras de linha para podermos ler o arquivo)
        string json = JsonUtility.ToJson(CurrentSave, true);
        
        // Escreve no disco
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"[SaveManager] Jogo salvo em: {saveFilePath}");
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            // Reconstrói o objeto C# a partir do JSON
            CurrentSave = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"[SaveManager] Jogo carregado. Dia atual do save: {CurrentSave.currentDay}");
        }
        else
        {
            // Se o arquivo não existir, criamos um estado do zero
            CurrentSave = new SaveData();
            Debug.Log("[SaveManager] Nenhum save encontrado. Novo estado criado.");
        }

        // Sincroniza o TimeManager com o dia que acabamos de carregar
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetCurrentDay(CurrentSave.currentDay);
        }
    }
}