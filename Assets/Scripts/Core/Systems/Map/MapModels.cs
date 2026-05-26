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
public class Region
{
    public string ID { get; }
    public string Name { get; }
    public int BaseDifficulty { get; }
    public FogState CurrentState { get; set; }

    // 1. A lista privada e mutável (o motor do carro)
    private readonly List<string> _neighbors;
    
    // 2. A propriedade pública e somente leitura (a lataria do carro)
    public IReadOnlyList<string> Neighbors => _neighbors;

    public Region(string id, string name, int baseDifficulty)
    {
        ID = id;
        Name = name;
        BaseDifficulty = baseDifficulty;
        CurrentState = FogState.Oculto; // Estado inicial padrão
        
        // Inicializamos a lista privada no construtor
        _neighbors = new List<string>();
    }

    // 3. O método controlado para adicionar vizinhos
    public void AddNeighbor(Region neighbor)
    {
        if (neighbor != null && !_neighbors.Contains(neighbor.ID))
        {
            _neighbors.Add(neighbor.ID);
            
            // Garante que o grafo seja bidirecional
            neighbor.AddNeighbor(this); 
        }
    }
}