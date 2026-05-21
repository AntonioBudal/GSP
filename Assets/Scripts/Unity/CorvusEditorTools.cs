#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CorvusEditorTools
{
    // Cria um botão no menu superior da Unity: Corvus -> QA -> Apagar Save
    [MenuItem("Corvus/QA/Apagar Save")]
    public static void DeleteSaveFile()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "corvus_save.sav");

        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"<color=#FF0000>[Corvus QA]</color> O arquivo de Save foi aniquilado do disco. Caminho: {savePath}");
        }
        else
        {
            Debug.LogWarning($"<color=#E67E22>[Corvus QA]</color> Nenhum save encontrado. O abismo já está vazio.");
        }
    }
}
#endif