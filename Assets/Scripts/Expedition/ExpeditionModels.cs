// --- Assets/Scripts/Expedition/ExpeditionModels.cs ---

// 1. Tipos e Enums
public enum CrowStat
{
    Speed,
    Focus,
    Resilience
}

public enum MissionType
{
    Reconhecimento, // Revela o mapa (Fase 5)
    Evangelizacao   // Converte hereges (Fase 6)
}

// 2. Eventos Visuais (Para a ponte do Unity)
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

// 3. Estruturas da Fase 3 (Motor Genérico)
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

// 4. Estruturas da Fase 5 (Motor de Exploração)
public readonly struct ExplorationResult
{
    public bool Success { get; }
    public int Injury { get; }        // 0 = Ileso, 1 = Fadiga, 2 = Dano Letal
    public int RevealPower { get; }   // Quantos vizinhos a ave conseguiu mapear
    public int ProgressGain { get; }  // Progresso real na exploração

    public ExplorationResult(bool success, int injury, int revealPower, int progressGain)
    {
        Success = success;
        Injury = injury;
        RevealPower = revealPower;
        ProgressGain = progressGain;
    }
}

// 5. O Runtime Atualizado
public class ExpeditionRuntime
{
    public string CrowId { get; }
    public MissionType Mission { get; }
    public string TargetRegionId { get; }
    public int DaysElapsed { get; set; }
    public int Progress { get; set; }

    public ExpeditionRuntime(string crowId, MissionType mission, string targetRegionId)
    {
        CrowId = crowId;
        Mission = mission;
        TargetRegionId = targetRegionId;
        DaysElapsed = 0;
        Progress = 0;
    }
}