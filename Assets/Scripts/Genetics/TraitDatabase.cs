using System.Collections.Generic;

public static class TraitDatabase
{
    private static readonly Dictionary<CrowTrait, TraitModifier> _database = new Dictionary<CrowTrait, TraitModifier>
    {
        { CrowTrait.OssosPesados, new TraitModifier(-1, 0, 2, 0) },
        { CrowTrait.SangueDoNorte, new TraitModifier(0, 0, 1, 10) },
        { CrowTrait.OlhoDeVidro, new TraitModifier(0, 2, -1, 0) },
        { CrowTrait.AsaLeve, new TraitModifier(2, 0, -1, -2) }
    };

    public static TraitModifier GetModifier(CrowTrait trait)
    {
        if(_database.TryGetValue(trait, out TraitModifier mod))
        {
            return mod;
        }

        return new TraitModifier(0,0,0,0);
    }
}