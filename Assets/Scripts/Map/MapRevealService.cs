using System;
using System.Collections.Generic;

public class MapRevealService
{
    private readonly MapManager _mapManager;
    private readonly IRandomSource _rng;

    public MapRevealService(MapManager mapManager, IRandomSource rng)
    {
        _mapManager = mapManager ?? throw new ArgumentNullException(nameof(mapManager));
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));
    }

    /// <summary>
    /// Aplica a política de revelação: Destranca a região alvo e levanta a névoa 
    /// apenas de um número de vizinhos condizente com a qualidade do reconhecimento.
    /// </summary>
    public void ApplyExplorationSuccess(string regionId, int revealPower)
    {
        _mapManager.TryExploreRegion(regionId);
        
        Region region = _mapManager.GetRegion(regionId);
        if (region == null || revealPower <= 0) return;

        // Filtra os vizinhos que ainda estão engolidos pela névoa
        List<string> hiddenNeighbors = new List<string>();
        foreach(var neighborId in region.Neighbors)
        {
            if (_mapManager.GetRegion(neighborId).CurrentState == FogState.Oculto)
            {
                hiddenNeighbors.Add(neighborId);
            }
        }

        // Embaralhamento seguro para que a revelação parcial não seja enviesada pelo array
        int n = hiddenNeighbors.Count;
        while (n > 1) 
        {
            n--;
            int k = _rng.Next(0, n + 1);
            string value = hiddenNeighbors[k];
            hiddenNeighbors[k] = hiddenNeighbors[n];
            hiddenNeighbors[n] = value;
        }

        // Aplica o poder de revelação (O mapa não vira dominó sozinho)
        int reveals = Math.Min(revealPower, hiddenNeighbors.Count);
        for(int i = 0; i < reveals; i++)
        {
            _mapManager.TryDiscoverRegion(hiddenNeighbors[i]);
        }
    }
}