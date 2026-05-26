using System.Collections.Generic;

public enum CrowTrait
{
    SangueDoNorte,
    OssosPesados,
    OlhoDeVidro,
    AsaLeve
}

public readonly struct TraitModifier
{   
    public int SpeedMod { get; }
    public int FocusMod { get; }
    public int ResilienceMod { get; }
    public int LifespanMod { get; }

    public TraitModifier(int speed, int focus, int resilience, int lifespan)
    {
        SpeedMod = speed;
        FocusMod = focus;
        ResilienceMod = resilience;
        LifespanMod = lifespan;
    }
}

public class GeneticSeed
{
    public IReadOnlyList<CrowTrait> Traits {get;}

    public GeneticSeed(List<CrowTrait> traits)
    {
        Traits = traits ?? new List<CrowTrait>();
    }
}