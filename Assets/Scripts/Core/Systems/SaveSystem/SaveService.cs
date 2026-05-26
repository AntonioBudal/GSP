// Assets/Scripts/Core/SaveSystem/SaveService.cs
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Corvus.Core.SaveSystem
{
    /// <summary>
    /// Serviço de I/O puro. Não contém referências à Unity.
    /// </summary>
    public class SaveService
    {
        private readonly string _saveFilePath;

        // O caminho do arquivo agora é injetado via construtor!
        public SaveService(string saveFilePath)
        {
            _saveFilePath = saveFilePath;
        }

        public bool HasSaveFile()
        {
            return File.Exists(_saveFilePath);
        }

        public async Task<bool> SaveGameAsync(SaveGameDTO saveData)
        {
            try
            {
                // 1. Converte o objeto C# para JSON
                string json = JsonConvert.SerializeObject(saveData, Formatting.None);

                // 2. Ofuscação simples usando Base64
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                string encryptedData = Convert.ToBase64String(bytes);

                // 3. Escreve no disco fora da thread principal (assíncrono)
                await File.WriteAllTextAsync(_saveFilePath, encryptedData);
                
                return true; // Sucesso
            }
            catch (Exception)
            {
                // Qualquer falha de I/O será capturada silenciosamente e reportada como false.
                return false;
            }
        }

        public async Task<SaveGameDTO> LoadGameAsync()
        {
            if (!HasSaveFile()) return null;

            try
            {
                // 1. Lê o arquivo bloqueando o mínimo possível a thread
                string encryptedData = await File.ReadAllTextAsync(_saveFilePath);

                // 2. Reverte o Base64 para JSON
                byte[] bytes = Convert.FromBase64String(encryptedData);
                string json = Encoding.UTF8.GetString(bytes);

                // 3. Reconstrói o DTO C#
                SaveGameDTO saveData = JsonConvert.DeserializeObject<SaveGameDTO>(json);
                
                return saveData;
            }
            catch (Exception)
            {
                return null; // Falha na leitura ou save corrompido
            }
        }

        public void DeleteSave()
        {
            if (HasSaveFile())
            {
                File.Delete(_saveFilePath);
            }
        }
    }
}