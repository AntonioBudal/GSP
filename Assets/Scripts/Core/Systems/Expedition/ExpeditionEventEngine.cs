using System;

public class ExpeditionEventEngine
{
    public EventResolution Evaluate(Crow crow, ExpeditionEvent expeditionEvent)
    {
        if (crow == null)
            throw new ArgumentNullException(nameof(crow), "O motor rejeita aves nulas.");

        // O motor só pergunta, não fuça
        int crowValue = crow.GetStat(expeditionEvent.RequiredStat);
        
        bool passed = crowValue >= expeditionEvent.Threshold;

        return new EventResolution(passed, expeditionEvent, crowValue);
    }
}