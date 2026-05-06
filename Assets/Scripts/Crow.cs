using System;

public enum CrowState
{
    Disponivel,
    EmTreino,
    Fadigado,
    EmExpedicao,
    Gestando,
    Perdido,
    Morto
}

public class Crow
{
    public string ID { get; }
    
    // O set é internal. Ninguém fora do domínio do sistema altera isso sozinho.
    public CrowState CurrentState { get; internal set; }

    public Crow(string id)
    {
        ID = id;
        CurrentState = CrowState.Disponivel;
    }
}