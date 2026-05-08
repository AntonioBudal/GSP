// Assets/Scripts/Evangelization/EvangelizationEngine.cs
using System;

public class EvangelizationEngine
{
    private readonly IRandomSource _rng;

    public EvangelizationEngine(IRandomSource rng)
    {
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));
    }

    // ADICIONE 'CrowRole role' AQUI NA ASSINATURA
    public EvangelizationResult EvaluateDailyPreaching(int crowFocus, int crowResilience, int baseResistance, CrowRole role)
    {
        // Bônus de Especialista (Fase 7.1)
        int roleBonus = (role == CrowRole.Mensageiro) ? 2 : 0;

        int survivalRoll = crowResilience + roleBonus + _rng.Next(1, 7);
        int preachingRoll = crowFocus + roleBonus + _rng.Next(1, 7);
        
        int injury = 0;
        if (survivalRoll < baseResistance - 2) injury = 2; 
        else if (survivalRoll < baseResistance) injury = 1; 

        int converted = 0;
        if (injury < 2 && preachingRoll >= baseResistance)
        {
            int margin = preachingRoll - baseResistance;
            converted = (margin * 5) + 10;
        }

        return new EvangelizationResult(converted, injury, 1);
    }
}