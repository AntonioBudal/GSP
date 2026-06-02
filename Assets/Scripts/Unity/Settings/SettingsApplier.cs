// Assets/Scripts/Unity/Settings/SettingsApplier.cs
using UnityEngine;
using UnityEngine.Audio;
using Corvus.Core.Settings;

public static class SettingsApplier
{
    public static void ApplyVideoSettings(VideoSettings video)
    {
        FullScreenMode unityMode = FullScreenMode.ExclusiveFullScreen;
        switch (video.ScreenMode)
        {
            case AppScreenMode.ExclusiveFullScreen: unityMode = FullScreenMode.ExclusiveFullScreen; break;
            case AppScreenMode.FullScreenWindow: unityMode = FullScreenMode.FullScreenWindow; break;
            case AppScreenMode.MaximizedWindow: unityMode = FullScreenMode.MaximizedWindow; break;
            case AppScreenMode.Windowed: unityMode = FullScreenMode.Windowed; break;
        }

        // Ignora o aviso de obsolescência da API antiga de RefreshRate da Unity
#pragma warning disable CS0618 
        Screen.SetResolution(video.ResolutionWidth, video.ResolutionHeight, unityMode, video.RefreshRate);
#pragma warning restore CS0618

        QualitySettings.vSyncCount = video.VSync;

        Debug.Log($"[SettingsApplier] Vídeo: {video.ResolutionWidth}x{video.ResolutionHeight} | {unityMode} | VSync: {video.VSync}");
    }

    // A MÁGICA AQUI: Forçamos o caminho completo 'Corvus.Core.Settings.AudioSettings' 
    // para a Unity não confundir com o 'UnityEngine.AudioSettings'
    public static void ApplyAudioSettings(Corvus.Core.Settings.AudioSettings audio, AudioMixer mixer)
    {
        if (mixer == null) return;

        float masterDb = audio.MasterVolume > 0.0001f ? Mathf.Log10(audio.MasterVolume) * 20f : -80f;
        float musicDb = audio.MusicVolume > 0.0001f ? Mathf.Log10(audio.MusicVolume) * 20f : -80f;
        float sfxDb = audio.SfxVolume > 0.0001f ? Mathf.Log10(audio.SfxVolume) * 20f : -80f;

        mixer.SetFloat("MasterVolume", masterDb);
        mixer.SetFloat("MusicVolume", musicDb);
        mixer.SetFloat("SfxVolume", sfxDb);

        Debug.Log($"[SettingsApplier] Áudio: Master({masterDb:F1}dB) | Music({musicDb:F1}dB) | SFX({sfxDb:F1}dB)");
    }
}