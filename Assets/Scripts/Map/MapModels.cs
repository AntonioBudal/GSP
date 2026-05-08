using System.Collections.Generic;

public enum FogState
{
    Oculto,
    Descoberto,
    Explorado
}

// O resultado estruturado de qualquer tentativa no mapa
public readonly struct MapStateResult
{
    public bool Success { get; }
    public string RegionId { get; }
    public FogState PreviousState { get; }
    public FogState NewState { get; }
    public string Message { get; }

    public MapStateResult(bool success, string regionId, FogState prev, FogState next, string message)
    {
        Success = success;
        RegionId = regionId;
        PreviousState = prev;
        NewState = next;
        Message = message;
    }
}

// O Nó imutável
public sealed class Region
{
    public string ID { get; }
    public string Name { get; }
    public int BaseDifficulty { get; }
    public FogState CurrentState { get; internal set; }
    
    // Totalmente imune a mutações externas
    public IReadOnlyList<string> Neighbors { get; }

    public Region(string id, string name, int baseDifficulty, IEnumerable<string> neighbors = null)
    {
        ID = id;
        Name = name;
        BaseDifficulty = baseDifficulty;
        CurrentState = FogState.Oculto;
        
        // Cópia dura e prevenção de auto-referência
        var tempList = new List<string>();
        if (neighbors != null)
        {
            foreach (var n in neighbors)
            {
                if (n != id && !tempList.Contains(n)) 
                {
                    tempList.Add(n);
                }
            }
        }
        Neighbors = tempList.AsReadOnly();
    }
}