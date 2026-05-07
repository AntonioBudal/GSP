// --- ARQUIVO: ExpeditionModels.cs ---
// Contém todas as estruturas e contratos de dados passivos da Expedição

public enum CrowStat
{
    Speed,
    Focus,
    Resilience
}

public readonly struct ExpeditionEvent
{
    public string Name { get; }
    public CrowStat RequiredStat { get; }
    public int Threshold { get; }

    public ExpeditionEvent(string name, CrowStat requiredStat, int threshold)
    {
        Name = name;
        RequiredStat = requiredStat;
        Threshold = threshold >= 0 ? threshold : 0;
    }
}

public readonly struct EventResolution
{
    public bool Passed { get; }
    public ExpeditionEvent EventData { get; }
    public int CrowStatValue { get; }

    public EventResolution(bool passed, ExpeditionEvent eventData, int crowStatValue)
    {
        Passed = passed;
        EventData = eventData;
        CrowStatValue = crowStatValue;
    }
}

public class ExpeditionRuntime
{
    public Crow Target { get; }
    public int DaysElapsed { get; set; }
    public int Successes { get; set; }
    public int Failures { get; set; }

    public ExpeditionRuntime(Crow target)
    {
        Target = target;
        DaysElapsed = 0;
        Successes = 0;
        Failures = 0;
    }
}

public readonly struct ExpeditionProgressEvent
{
    public int CurrentDay { get; }
    public string CrowId { get; }
    public int DaysElapsed { get; }

    public ExpeditionProgressEvent(int currentDay, string crowId, int daysElapsed)
    {
        CurrentDay = currentDay;
        CrowId = crowId;
        DaysElapsed = daysElapsed;
    }
}