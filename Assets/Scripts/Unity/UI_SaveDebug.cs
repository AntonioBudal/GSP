// Assets/Scripts/Unity/UI_SaveDebug.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_SaveDebug : MonoBehaviour
{
    private bool _isSaving = false;

    private async void Update()
    {
        // F5 = Salvar o Jogo
        if (Input.GetKeyDown(KeyCode.F5) && !_isSaving)
        {
            if (GameBootstrap.Instance != null && GameBootstrap.Instance.SaveManager != null)
            {
                _isSaving = true;
                Debug.Log("[SaveDebug] Iniciando gravação manual (F5)...");
                
                bool success = await GameBootstrap.Instance.SaveManager.SaveCurrentGameStateAsync();
                
                if (success) Debug.Log("<color=#00FF00>[SaveDebug] Jogo Salvo com Sucesso!</color>");
                else Debug.LogError("[SaveDebug] Falha ao salvar o jogo.");
                
                _isSaving = false;
            }
        }

        // F9 = Carregar o Jogo (Reiniciando o Universo)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("[SaveDebug] Reiniciando a cena para forçar o Load (F9)...");
            
            // Destrói o Bootstrap antigo para que ele recrie o universo do zero
            if (GameBootstrap.Instance != null)
            {
                Destroy(GameBootstrap.Instance.gameObject);
            }
            
            // Recarrega a cena atual
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}