// Assets/Scripts/Unity/Settings/SettingsBridge.cs
using UnityEngine;
using UnityEngine.Audio;
using Corvus.Core.Settings;

[RequireComponent(typeof(GameStateController))]
public class SettingsBridge : MonoBehaviour
{
    [Header("Engine References")]
    [Tooltip("Arraste o AudioMixer principal do projeto aqui.")]
    [SerializeField] private AudioMixer _mainAudioMixer;

    private void Start()
    {
        var settingsManager = GameStateController.Instance.Settings;

        if (settingsManager != null)
        {
            settingsManager.OnSettingsApplied += HandleSettingsApplied;
            
            if (settingsManager.AppliedProfile != null)
            {
                HandleSettingsApplied(settingsManager.AppliedProfile);
            }
        }
    }

    private void HandleSettingsApplied(SettingsProfile profile)
    {
        SettingsApplier.ApplyVideoSettings(profile.Video);
        
        // Passa a estrutura do Core explicitamente
        SettingsApplier.ApplyAudioSettings(profile.Audio, _mainAudioMixer);
    }

    private void OnDestroy()
    {
        if (GameStateController.Instance != null && GameStateController.Instance.Settings != null)
        {
            GameStateController.Instance.Settings.OnSettingsApplied -= HandleSettingsApplied;
        }
    }
}