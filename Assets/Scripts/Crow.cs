public class Crow
{
    public string ID { get; }
    public CrowState CurrentState { get; internal set; }

    // Atributos de Sobrevivência
    public int Speed { get; internal set; }
    public int Focus { get; internal set; }
    public int Resilience { get; internal set; }
    
    // Custo Irreversível
    public int Lifespan { get; internal set; }

    public Crow(string id, int baseSpeed, int baseFocus, int baseResilience, int initialLifespan)
    {
        ID = id;
        CurrentState = CrowState.Disponivel;
        
        Speed = baseSpeed;
        Focus = baseFocus;
        Resilience = baseResilience;
        Lifespan = initialLifespan;
    }
}