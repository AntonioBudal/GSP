// Assets/Scripts/Unity/Narrative/SoliloquyData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Soliloquy_", menuName = "Project Corvus/Narrative/Soliloquy")]
public class SoliloquyData : ScriptableObject
{
    [Header("Identificação")]
    [Tooltip("ID único para controle interno e futura localização (ex: SOL_DEATH_01)")]
    public string ID;

    [Header("Gatilho e Relevância")]
    public TriggerCondition Condition;
    
    [Tooltip("Se duas falas tentarem tocar ao mesmo tempo, a de maior prioridade vence.")]
    public int Priority = 1;

    [Header("Conteúdo")]
    [TextArea(3, 6)]
    public string Text;
}