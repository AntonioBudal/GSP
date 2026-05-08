using System;

public class ExplorationEngine
{
    private readonly IRandomSource _rng;

    public ExplorationEngine(IRandomSource rng)
    {
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));
    }

    public ExplorationResult EvaluateDailyRecon(int crowSpeed, int crowFocus, int regionDifficulty)
    {
        int speedRoll = crowSpeed + _rng.Next(1, 7);
        int focusRoll = crowFocus + _rng.Next(1, 7);

        bool success = speedRoll >= regionDifficulty;
        int progressGain = success ? 1 : 0;
        
        // Avaliação de Dano
        int injury = 0; 
        if (focusRoll < regionDifficulty - 2) injury = 2; // Letal
        else if (focusRoll < regionDifficulty) injury = 1; // Leve (Atraso/Fadiga)

        // Poder de Revelação: Um sucesso com Foco crítico revela mais do mapa
        int revealPower = 0;
        if (success) revealPower = (focusRoll > regionDifficulty + 2) ? 2 : 1;

        return new ExplorationResult(success, injury, revealPower, progressGain);
    }
}