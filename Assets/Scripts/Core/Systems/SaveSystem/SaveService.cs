// Assets/Scripts/Core/SaveSystem/SaveService.cs
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Corvus.Core.SaveSystem
{
    public class SaveService
    {
        private readonly string _saveFilePath;
        private readonly string _infoFilePath;

        // Construtor agora recebe o SlotID para gerar os arquivos dinamicamente
        public SaveService(string basePath, int slotId)
        {
            _saveFilePath = Path.Combine(basePath, $"slot_{slotId}.sav");
            _infoFilePath = Path.Combine(basePath, $"slot_{slotId}.info");
        }

        public bool HasSaveFile()
        {
            // O Slot só é válido se ambos os arquivos existirem
            return File.Exists(_saveFilePath) && File.Exists(_infoFilePath);
        }

        // Grava os dois arquivos ao mesmo tempo
        public async Task<bool> SaveGameAsync(SaveGameDTO saveData, SaveSlotMetadata metadata)
        {
            try
            {
                // 1. Gravação do Metadado (Leve, texto puro em JSON)
                string infoJson = JsonConvert.SerializeObject(metadata, Formatting.Indented);
                await File.WriteAllTextAsync(_infoFilePath, infoJson);

                // 2. Gravação do Save Completo (Pesado, Base64)
                string saveJson = JsonConvert.SerializeObject(saveData, Formatting.None);
                byte[] bytes = Encoding.UTF8.GetBytes(saveJson);
                string encryptedData = Convert.ToBase64String(bytes);

                await File.WriteAllTextAsync(_saveFilePath, encryptedData);
                
                return true; 
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Carrega apenas os dados pesados (o menu já leu os leves)
        public async Task<SaveGameDTO> LoadGameAsync()
        {
            if (!HasSaveFile()) return null;

            try
            {
                string encryptedData = await File.ReadAllTextAsync(_saveFilePath);
                byte[] bytes = Convert.FromBase64String(encryptedData);
                string json = Encoding.UTF8.GetString(bytes);

                return JsonConvert.DeserializeObject<SaveGameDTO>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void DeleteSave()
        {
            if (File.Exists(_saveFilePath)) File.Delete(_saveFilePath);
            if (File.Exists(_infoFilePath)) File.Delete(_infoFilePath);
        }
    }
}