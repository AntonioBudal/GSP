public class CrowStateController
{
    /// <summary>
    /// Avalia e executa a transição de estado da ave se for legal.
    /// </summary>
    public TransitionResult RequestTransition(Crow crow, CrowState targetState)
    {
        CrowState currentState = crow.CurrentState;

        // 1. Bloqueio absoluto de Estados Terminais
        if (currentState == CrowState.Morto || currentState == CrowState.Perdido)
        {
            return new TransitionResult(false, currentState, targetState, 
                $"Bloqueio: Corvo [{crow.ID}] está em estado irreversível ({currentState}).");
        }

        // 2. Tabela de Transições Válidas
        if (!IsValidTransition(currentState, targetState))
        {
            return new TransitionResult(false, currentState, targetState, 
                $"Bloqueio: Rota de transição ilegal ({currentState} -> {targetState}).");
        }

        // 3. Execução (Autorizada)
        crow.CurrentState = targetState;
        return new TransitionResult(true, currentState, targetState, 
            $"Sucesso: Corvo [{crow.ID}] transitou de {currentState} para {targetState}.");
    }

    /// <summary>
    /// Define estritamente quem pode ir para onde. O resto é rejeitado por padrão.
    /// </summary>
    private bool IsValidTransition(CrowState from, CrowState to)
    {
        switch (from)
        {
            case CrowState.Disponivel:
                return to == CrowState.EmTreino || to == CrowState.EmExpedicao || to == CrowState.Gestando;
            
            case CrowState.EmTreino:
                return to == CrowState.Fadigado;
            
            case CrowState.Fadigado:
                return to == CrowState.Disponivel;
            
            case CrowState.EmExpedicao:
                return to == CrowState.Disponivel || to == CrowState.Perdido || to == CrowState.Morto;
            
            case CrowState.Gestando:
                return to == CrowState.Disponivel;
            
            default:
                return false;
        }
    }
}