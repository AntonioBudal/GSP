// Assets/Scripts/Evangelization/EvangelizationModels.cs
using System;

public readonly struct DemographicsData
{
    public int TotalPopulation { get; }
    public int BaseResistance { get; }

    public DemographicsData(int totalPopulation, int baseResistance)
    {
        TotalPopulation = totalPopulation > 0 ? totalPopulation : 1;
        BaseResistance = baseResistance >= 0 ? baseResistance : 0;
    }
}

// Adicione isso no seu Assets/Scripts/Evangelization/EvangelizationModels.cs

public readonly struct EvangelizationResult
{
    public int ConvertedAmount { get; }
    public int Injury { get; }        // 0 = Ileso, 1 = Fadiga, 2 = Dano Letal (Martírio)
    public int ProgressGain { get; }  // Avanço na duração da missão

    public EvangelizationResult(int convertedAmount, int injury, int progressGain)
    {
        ConvertedAmount = convertedAmount;
        Injury = injury;
        ProgressGain = progressGain;
    }
}



// Payload puro de resposta para qualquer mutação de almas
public readonly struct ConversionResult
{
    public bool Success { get; }
    public int ConvertedAmount { get; }
    public float NewPercentage { get; }
    public string Message { get; }

    public ConversionResult(bool success, int convertedAmount, float newPercentage, string message)
    {
        Success = success;
        ConvertedAmount = convertedAmount;
        NewPercentage = newPercentage;
        Message = message;
    }
}

public class InfluenceRuntime
{
    public string RegionId { get; }
    public DemographicsData Demographics { get; }
    
    // Internal set mantido, mas agora estritamente orquestrado pelos métodos do Manager
    public int Believers { get; internal set; }

    public float BelieverPercentage => (float)Believers / Demographics.TotalPopulation;

    public InfluenceRuntime(string regionId, DemographicsData demographics, int initialBelievers = 0)
    {
        RegionId = regionId;
        Demographics = demographics;
        Believers = Math.Clamp(initialBelievers, 0, Demographics.TotalPopulation);
    }
}