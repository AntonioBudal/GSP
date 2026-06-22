using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : PopupBase
{
    [Header("UI References")]
    [SerializeField] private Button saveGameButton;
    [SerializeField] private Button restartGameButton;
    [SerializeField] private Button quitGameButton;

    protected override void BindData()
    {
        if (saveGameButton != null)
        {
            saveGameButton.onClick.RemoveAllListeners();
            saveGameButton.onClick.AddListener(() => {
                SaveManager.Instance.SaveGame();
                Close();
            });
        }

        if (restartGameButton != null)
        {
            restartGameButton.onClick.RemoveAllListeners();
            restartGameButton.onClick.AddListener(() => {
                SaveManager.Instance.DeleteSaveAndRestart();
            });
        }

        if (quitGameButton != null)
        {
            quitGameButton.onClick.RemoveAllListeners();
            quitGameButton.onClick.AddListener(() => {
                SaveManager.Instance.SaveGame();
                Application.Quit();
                
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            });
        }
    }
}