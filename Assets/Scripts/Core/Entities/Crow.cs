public class Crow
{
    public string ID { get; }
    public CrowState CurrentState { get; internal set; }

    public int Speed { get; internal set; }
    public int Focus { get; internal set; }
    public int Resilience { get; internal set; }
    public int Lifespan { get; internal set; }

    // O peso do sangue instalado na ave (Fase 4)
    public GeneticSeed Genetics { get; }

    // NOVA PROPRIEDADE (Fase 7)
    public CrowRole Role { get; internal set; }

    public Crow(string id, int speed, int focus, int resilience, int lifespan, GeneticSeed genetics)
    {
        ID = id;
        CurrentState = CrowState.Disponivel;
        
        Speed = speed;
        Focus = focus;
        Resilience = resilience;
        Lifespan = lifespan;
        
        Genetics = genetics ?? new GeneticSeed(null);
        
        // Todo corvo nasce como "Geral" (sem especialização)
        Role = CrowRole.Geral; 
    }

    public int GetStat(CrowStat stat)
    {
        switch (stat)
        {
            case CrowStat.Speed: return Speed;
            case CrowStat.Focus: return Focus;
            case CrowStat.Resilience: return Resilience;
            default: return 0;
        }
    }
}