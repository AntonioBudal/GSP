// Assets/Scripts/Core/SaveSystem/SaveScanner.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Corvus.Core.SaveSystem
{
    /// <summary>
    /// Serviço puro responsável exclusivamente por varrer o disco atrás de arquivos .info
    /// </summary>
    public static class SaveScanner
    {
        public static async Task<Dictionary<int, SaveSlotMetadata>> ScanAllSlotsAsync(string basePath)
        {
            var slots = new Dictionary<int, SaveSlotMetadata>();

            for (int i = 0; i < 10; i++)
            {
                string infoPath = Path.Combine(basePath, $"slot_{i}.info");

                if (File.Exists(infoPath))
                {
                    try
                    {
                        string json = await File.ReadAllTextAsync(infoPath);
                        var metadata = JsonConvert.DeserializeObject<SaveSlotMetadata>(json);
                        if (metadata != null)
                        {
                            slots[i] = metadata;
                        }
                    }
                    catch (Exception)
                    {
                        // Se o arquivo estiver corrompido, ignoramos e ele aparecerá como vazio na UI
                        continue;
                    }
                }
            }

            return slots;
        }
    }
}