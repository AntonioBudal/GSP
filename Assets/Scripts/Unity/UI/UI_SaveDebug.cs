using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

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

        // F9 = Carregar o Jogo
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("[SaveDebug] Reiniciando a cena para forçar o Load (F9)...");
            
            if (GameBootstrap.Instance != null)
            {
                Destroy(GameBootstrap.Instance.gameObject);
            }
            
            SceneManager.LoadScene("Scene_Boot");
        }

        // F12 = Deletar Save
        if (Input.GetKeyDown(KeyCode.F12))
        {
            string savePath = Path.Combine(Application.persistentDataPath, "corvus_save.sav");

            if (File.Exists(savePath))
            {
                File.Delete(savePath);

                Debug.Log("<color=#FF0000>[SaveDebug] Arquivo de Save destruído (F12).</color> Pressione F9 para renascer um novo universo.");
            }
        }
    }
}