// Assets/Scripts/Evangelization/InfluenceManager.cs
using System;
using System.Collections.Generic;
using Corvus.Core.SaveSystem; // A linha que resolve o erro!

public class InfluenceManager : IDisposable
{
    private readonly MapManager _mapManager;
    private readonly GameClock _clock;
    private readonly Dictionary<string, InfluenceRuntime> _runtimes;

    public InfluenceManager(MapManager mapManager, GameClock clock, Dictionary<string, DemographicsData> initialData, List<InfluenceSaveData> saveData = null)
    {
        _mapManager = mapManager ?? throw new ArgumentNullException(nameof(mapManager));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _runtimes = new Dictionary<string, InfluenceRuntime>();

        if (initialData != null)
        {
            foreach (var kvp in initialData)
            {
                string regionId = kvp.Key;
                if (_mapManager.GetRegion(regionId) != null)
                {
                    // Descobre se há dados salvos para esta região
                    int savedBelievers = 0;
                    if (saveData != null)
                    {
                        var regionSave = saveData.Find(s => s.RegionID == regionId);
                        if (regionSave != null) savedBelievers = regionSave.Believers;
                    }

                    _runtimes[regionId] = new InfluenceRuntime(regionId, kvp.Value, savedBelievers);
                }
            }
        }

        _clock.OnDayEnded += ProcessDemographicShifts;
    }

    // Método para o SaveManager poder fotografar
    public IEnumerable<InfluenceRuntime> GetAllRuntimes() => _runtimes.Values;

    /// <summary>
    /// Padrão Try: Leitura segura para a interface ou motores, sem null silencioso.
    /// </summary>
    public bool TryGetInfluence(string regionId, out InfluenceRuntime runtime)
    {
        return _runtimes.TryGetValue(regionId, out runtime);
    }

    /// <summary>
    /// Única via de mutação de almas. Aplica o teto populacional e devolve resultado rico.
    /// </summary>
    public ConversionResult TryConvertBelievers(string regionId, int amountToConvert)
    {
        if (!_runtimes.TryGetValue(regionId, out var runtime))
        {
            return new ConversionResult(false, 0, 0f, $"Falha: Nenhuma demografia registrada para a região [{regionId}].");
        }

        if (amountToConvert <= 0)
        {
            return new ConversionResult(false, 0, runtime.BelieverPercentage, "Falha: Quantidade de conversão deve ser positiva.");
        }

        int previousBelievers = runtime.Believers;
        int newBelieversTotal = Math.Min(runtime.Believers + amountToConvert, runtime.Demographics.TotalPopulation);
        int actualConverted = newBelieversTotal - previousBelievers;

        runtime.Believers = newBelieversTotal;

        if (actualConverted == 0)
        {
            return new ConversionResult(true, 0, 1.0f, $"A região [{regionId}] já está totalmente convertida.");
        }

        return new ConversionResult(true, actualConverted, runtime.BelieverPercentage, 
            $"Sucesso: +{actualConverted} fiéis em [{regionId}]. Total: {runtime.BelieverPercentage:P1}");
    }

    /// <summary>
    /// Tick diário para evolução orgânica (Crescimento demográfico, perda de fé por abandono, etc.)
    /// </summary>
    private void ProcessDemographicShifts(int currentDay)
    {
        // Skeleton para a Fase 8 ou expansão sistêmica.
        // É aqui que a "evangelização degrada ou se espalha" sem ação direta do jogador.
    }

    public void Dispose()
    {
        if (_clock != null) _clock.OnDayEnded -= ProcessDemographicShifts;
    }
}