// Assets/Scripts/Core/Settings/SettingsManager.cs
using System;
using System.Threading.Tasks;

namespace Corvus.Core.Settings
{
    public class SettingsManager
    {
        private readonly SettingsService _service;

        public SettingsProfile AppliedProfile { get; private set; }
        public SettingsProfile PendingProfile { get; private set; }
        public bool IsDirty { get; private set; }

        public event Action<SettingsProfile> OnSettingsApplied;

        public SettingsManager(SettingsService service)
        {
            _service = service;
        }

        public async Task InitializeAsync()
        {
            AppliedProfile = await _service.LoadSettingsAsync();

            if (AppliedProfile == null)
            {
                System.Console.WriteLine("[SettingsManager] Configurações não encontradas ou corrompidas. Gerando Defaults...");
                AppliedProfile = new SettingsProfile(); 
                await SaveAsync(); 
            }
            else
            {
                ValidateProfile(AppliedProfile);
            }

            PendingProfile = AppliedProfile.Clone();
            IsDirty = false;

            OnSettingsApplied?.Invoke(AppliedProfile);
        }

        public void UpdatePendingProfile(SettingsProfile newPending)
        {
            ValidateProfile(newPending);
            PendingProfile = newPending;
            IsDirty = true;
        }

        public void ApplyPending()
        {
            AppliedProfile = PendingProfile.Clone();
            IsDirty = false;
            OnSettingsApplied?.Invoke(AppliedProfile);
            System.Console.WriteLine("[SettingsManager] Configurações Aplicadas na Engine.");
        }

        public async Task SaveAsync()
        {
            if (IsDirty) ApplyPending();
            
            bool success = await _service.SaveSettingsAsync(AppliedProfile);
            if (success) System.Console.WriteLine("[SettingsManager] Configurações gravadas em disco com sucesso.");
            else System.Console.WriteLine("[SettingsManager] ERRO: Falha crítica ao gravar configurações.");
        }

        public void DiscardPendingChanges()
        {
            PendingProfile = AppliedProfile.Clone();
            IsDirty = false;
            System.Console.WriteLine("[SettingsManager] Mudanças descartadas. Rascunho restaurado.");
            
            OnSettingsApplied?.Invoke(AppliedProfile);
        }

        public void ResetToDefaults()
        {
            PendingProfile = new SettingsProfile();
            IsDirty = true;
            System.Console.WriteLine("[SettingsManager] Rascunho redefinido para Padrões de Fábrica. Aguardando Apply.");
        }

        private void ValidateProfile(SettingsProfile profile)
        {
            // Matemática C# pura substituindo o Mathf.Clamp01 da Unity
            profile.Audio.MasterVolume = Math.Max(0f, Math.Min(1f, profile.Audio.MasterVolume));
            profile.Audio.MusicVolume = Math.Max(0f, Math.Min(1f, profile.Audio.MusicVolume));
            profile.Audio.SfxVolume = Math.Max(0f, Math.Min(1f, profile.Audio.SfxVolume));

            if (profile.Video.ResolutionWidth < 800) profile.Video.ResolutionWidth = 800;
            if (profile.Video.ResolutionHeight < 600) profile.Video.ResolutionHeight = 600;
            if (profile.Video.RefreshRate < 30) profile.Video.RefreshRate = 60;
        }
    }
}