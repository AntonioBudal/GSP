// Assets/Scripts/Core/Settings/SettingsModels.cs
using System;

namespace Corvus.Core.Settings
{
    // NOVO: Enumeração pura em C# para substituir o FullScreenMode da Unity
    public enum AppScreenMode
    {
        ExclusiveFullScreen,
        FullScreenWindow,
        MaximizedWindow,
        Windowed
    }

    [Serializable]
    public class SettingsProfile
    {
        public VideoSettings Video = new VideoSettings();
        public AudioSettings Audio = new AudioSettings();
        public GameplaySettings Gameplay = new GameplaySettings();

        public SettingsProfile Clone()
        {
            return new SettingsProfile
            {
                Video = new VideoSettings
                {
                    ResolutionWidth = this.Video.ResolutionWidth,
                    ResolutionHeight = this.Video.ResolutionHeight,
                    RefreshRate = this.Video.RefreshRate,
                    ScreenMode = this.Video.ScreenMode,
                    VSync = this.Video.VSync
                },
                Audio = new AudioSettings
                {
                    MasterVolume = this.Audio.MasterVolume,
                    MusicVolume = this.Audio.MusicVolume,
                    SfxVolume = this.Audio.SfxVolume
                },
                Gameplay = new GameplaySettings
                {
                    Language = this.Gameplay.Language
                }
            };
        }
    }

    [Serializable]
    public class VideoSettings
    {
        public int ResolutionWidth = 1920;
        public int ResolutionHeight = 1080;
        public int RefreshRate = 60;
        // Usa o nosso enum C# puro em vez do da Unity
        public AppScreenMode ScreenMode = AppScreenMode.ExclusiveFullScreen; 
        public int VSync = 1; 
    }

    [Serializable]
    public class AudioSettings
    {
        public float MasterVolume = 1.0f;
        public float MusicVolume = 0.8f;
        public float SfxVolume = 1.0f;
    }

    [Serializable]
    public class GameplaySettings
    {
        public string Language = "pt-BR";
    }
}