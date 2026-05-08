// Assets/Scripts/Expedition/ExplorationEngine.cs
using System;

public class ExplorationEngine
{
    private readonly IRandomSource _rng;

    public ExplorationEngine(IRandomSource rng)
    {
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));
    }

    // ADICIONE 'CrowRole role' AQUI NA ASSINATURA
    public ExplorationResult EvaluateDailyRecon(int crowSpeed, int crowFocus, int regionDifficulty, CrowRole role)
    {
        // Bônus de Especialista (Fase 7.1)
        int roleBonus = (role == CrowRole.Batedor) ? 2 : 0;

        int speedRoll = crowSpeed + roleBonus + _rng.Next(1, 7);
        int focusRoll = crowFocus + roleBonus + _rng.Next(1, 7);

        bool success = speedRoll >= regionDifficulty;
        int progressGain = success ? 1 : 0;
        
        int injury = 0; 
        if (focusRoll < regionDifficulty - 2) injury = 2; 
        else if (focusRoll < regionDifficulty) injury = 1; 

        int revealPower = 0;
        if (success) revealPower = (focusRoll > regionDifficulty + 2) ? 2 : 1;

        return new ExplorationResult(success, injury, revealPower, progressGain);
    }
}