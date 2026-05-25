// Assets/Scripts/Core/Map/MapManager.cs
using System;
using System.Collections.Generic;
using Corvus.Core.SaveSystem; // A linha crucial que resolve o erro

public class MapManager

{
    private readonly Dictionary<string, Region> _regions;

    public IEnumerable<Region> GetAllRegions() => _regions.Values;

    // Evento único e limpo com payload rico para o Unity
    public event Action<MapStateResult> OnRegionStateChanged;

    public MapManager(IEnumerable<Region> initialGraph, List<RegionSaveData> saveData = null)
    {
        _regions = new Dictionary<string, Region>();

        if (initialGraph != null)
        {
            // Validação 1: IDs duplicados
            foreach (var region in initialGraph)
            {
                if (_regions.ContainsKey(region.ID))
                    throw new ArgumentException($"Grafo inválido: ID duplicado detectado [{region.ID}].");
                
                _regions[region.ID] = region;
            }

            // Validação 2: Vizinhos órfãos
            foreach (var region in _regions.Values)
            {
                foreach (var neighborId in region.Neighbors)
                {
                    if (!_regions.ContainsKey(neighborId))
                        throw new ArgumentException($"Grafo inválido: Região [{region.ID}] aponta para vizinho inexistente [{neighborId}].");
                }
            }

            // --- NOVA ROTINA DE SAVE ---
            // Se o jogo está sendo carregado do disco, sobrescreve a névoa padrão
            if (saveData != null)
            {
                foreach (var savedRegion in saveData)
                {
                    if (_regions.TryGetValue(savedRegion.ID, out Region r))
                    {
                        r.CurrentState = savedRegion.CurrentState;
                    }
                }
            }
        }
    }

    public Region GetRegion(string regionId)
    {
        _regions.TryGetValue(regionId, out Region region);
        return region;
    }

    
    

    /// <summary>
    /// Ponto de ignição do mapa. Força a base inicial do jogador a existir.
    /// </summary>
    public MapStateResult UnlockStartingRegion(string regionId)
    {
        if (!_regions.TryGetValue(regionId, out Region target))
            return new MapStateResult(false, regionId, FogState.Oculto, FogState.Oculto, "Erro: Região inicial inexistente.");

        target.CurrentState = FogState.Explorado;
        var result = new MapStateResult(true, regionId, FogState.Oculto, FogState.Explorado, $"A base foi estabelecida em [{target.Name}].");
        OnRegionStateChanged?.Invoke(result);
        return result;
    }

    /// <summary>
    /// Levanta a névoa. Exige que um vizinho conectado já não esteja oculto.
    /// </summary>
    public MapStateResult TryDiscoverRegion(string targetRegionId)
    {
        if (!_regions.TryGetValue(targetRegionId, out Region target))
            return new MapStateResult(false, targetRegionId, FogState.Oculto, FogState.Oculto, "Erro: Região alvo não existe.");

        if (target.CurrentState != FogState.Oculto)
            return new MapStateResult(false, targetRegionId, target.CurrentState, target.CurrentState, "Bloqueio: Região já descoberta.");

        // Trava de Topologia (A Névoa)
        bool hasKnownNeighbor = false;
        foreach (var neighborId in target.Neighbors)
        {
            if (_regions[neighborId].CurrentState != FogState.Oculto)
            {
                hasKnownNeighbor = true;
                break;
            }
        }

        if (!hasKnownNeighbor)
            return new MapStateResult(false, targetRegionId, target.CurrentState, target.CurrentState, "Bloqueio de Névoa: Nenhuma conexão com regiões conhecidas.");

        // Sucesso
        target.CurrentState = FogState.Descoberto;
        var result = new MapStateResult(true, targetRegionId, FogState.Oculto, FogState.Descoberto, $"O véu recuou: [{target.Name}] agora é visível.");
        OnRegionStateChanged?.Invoke(result);
        return result;
    }

    public MapStateResult TryExploreRegion(string regionId)
    {
        if (!_regions.TryGetValue(regionId, out Region target))
            return new MapStateResult(false, regionId, FogState.Oculto, FogState.Oculto, "Erro: Região alvo não existe.");

        if (target.CurrentState == FogState.Oculto)
            return new MapStateResult(false, regionId, FogState.Oculto, FogState.Oculto, "Bloqueio: A região precisa ser Descoberta antes de ser Explorada.");

        if (target.CurrentState == FogState.Explorado)
            return new MapStateResult(false, regionId, target.CurrentState, target.CurrentState, "Bloqueio: Região já explorada.");

        target.CurrentState = FogState.Explorado;
        var result = new MapStateResult(true, regionId, FogState.Descoberto, FogState.Explorado, $"Exploração concluída: [{target.Name}].");
        OnRegionStateChanged?.Invoke(result);
        return result;
    }

    public void InitializeProceduralGraph(IEnumerable<Region> generatedGraph)
    {
        foreach (var region in generatedGraph)
        {
            if (!_regions.ContainsKey(region.ID))
            {
                _regions.Add(region.ID, region);
            }
        }
    }
}