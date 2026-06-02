// Assets/Scripts/Core/Settings/SettingsService.cs
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Corvus.Core.Settings
{
    public class SettingsService
    {
        private readonly string _filePath;

        public SettingsService(string basePath)
        {
            _filePath = Path.Combine(basePath, "corvus_settings.cfg");
        }

        public async Task<bool> SaveSettingsAsync(SettingsProfile profile)
        {
            try
            {
                string json = JsonConvert.SerializeObject(profile, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<SettingsProfile> LoadSettingsAsync()
        {
            if (!File.Exists(_filePath)) return null;

            try
            {
                string json = await File.ReadAllTextAsync(_filePath);
                return JsonConvert.DeserializeObject<SettingsProfile>(json);
            }
            catch (Exception)
            {
                // Se o arquivo estiver corrompido, retornamos nulo para forçar o fallback de Defaults
                return null;
            }
        }
    }
}