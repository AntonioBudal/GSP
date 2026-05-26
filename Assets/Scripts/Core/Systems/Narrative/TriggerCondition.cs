// Assets/Scripts/Core/Narrative/TriggerCondition.cs
public enum TriggerCondition
{
    Ocioso,             // Falas aleatórias quando nada está acontecendo
    PrimeiraMorte,      // Quando o primeiro corvo morre (Lifespan = 0 ou Injury = 2)
    NovaRegiaoDescoberta,
    CorvoFadigado,
    InicioEspecializacao,
    MilestoneAtingido
}