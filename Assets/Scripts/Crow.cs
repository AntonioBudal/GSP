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
    public string ID { get; private set; }
    public CrowState CurrentState { get; private set; }

    public Crow(string id)
    {
        ID = id;
        CurrentState = CrowState.Disponivel;
    }

    /// <summary>
    /// Tenta alterar o estado da ave.
    /// Na Fase 1.2 aceita qualquer mudança. Na Fase 1.3, receberá injeção de regras ou será blindado.
    /// </summary>
    public bool TrySetState(CrowState newState, out string resultMessage)
    {
        // Guardando o estado anterior para o relatório do Manager
        CrowState oldState = CurrentState;
        
        // Aplica a mutação
        CurrentState = newState;

        // Retorna o que aconteceu para que o Manager (ou script de teste) decida como logar
        resultMessage = $"Corvo [{ID}] mudou de [{oldState}] para [{CurrentState}]";
        return true;
    }
}