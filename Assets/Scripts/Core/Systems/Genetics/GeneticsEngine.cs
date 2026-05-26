using System;
using System.Collections.Generic;
using System.Linq;

public class GeneticsEngine
{
    private readonly Random _rng;

    // 1. Injeção da fonte de aleatoriedade para garantir reprodutibilidade em testes
    public GeneticsEngine(Random rng)
    {
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));
    }

    public Crow GenerateOffspring(string childId, Crow parentA, Crow parentB)
    {
        int baseSpeed, baseFocus, baseResilience, baseLifespan;

        // 2. Herança por Blocos (Identidade Genética)
        // Bloco 1: Ação (Speed + Focus) | Bloco 2: Sobrevivência (Resilience + Lifespan)
        if (_rng.NextDouble() > 0.5)
        {
            // Herda Ação do Pai A, Sobrevivência do Pai B
            baseSpeed = parentA.Speed;
            baseFocus = parentA.Focus;
            baseResilience = parentB.Resilience;
            baseLifespan = parentB.Lifespan;
        }
        else
        {
            // Herda Ação do Pai B, Sobrevivência do Pai A
            baseSpeed = parentB.Speed;
            baseFocus = parentB.Focus;
            baseResilience = parentA.Resilience;
            baseLifespan = parentA.Lifespan;
        }

        baseLifespan = Math.Max(1, baseLifespan - 2); // Degradação natural por geração

        // 3. Semente Genética
        HashSet<CrowTrait> combinedTraits = new HashSet<CrowTrait>(parentA.Genetics.Traits);
        foreach (var trait in parentB.Genetics.Traits) combinedTraits.Add(trait);

        List<CrowTrait> inheritedTraits = combinedTraits.OrderBy(x => _rng.Next()).Take(2).ToList();
        GeneticSeed childSeed = new GeneticSeed(inheritedTraits);

        // 4. Modificadores
        foreach (var trait in inheritedTraits)
        {
            TraitModifier mod = TraitDatabase.GetModifier(trait);
            baseSpeed += mod.SpeedMod;
            baseFocus += mod.FocusMod;
            baseResilience += mod.ResilienceMod;
            baseLifespan += mod.LifespanMod;
        }

        baseSpeed = Math.Max(0, baseSpeed);
        baseFocus = Math.Max(0, baseFocus);
        baseResilience = Math.Max(0, baseResilience);
        baseLifespan = Math.Max(1, baseLifespan);

        return new Crow(childId, baseSpeed, baseFocus, baseResilience, baseLifespan, childSeed);
    }
}