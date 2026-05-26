using System.Collections.Generic;
using UnityEngine;

public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("Áreas do Editor")]
    [SerializeField] private TerritoryArea[] _territories;

    [Header("Conexão com a UI")]
    [SerializeField] private MapRenderer _mapRenderer;

    private Dictionary<string, LogicalNode> _mapGrid = new Dictionary<string, LogicalNode>();

    private void Start()
    {
        // 1. Gera a matemática pura
        GenerateLogicalMap();
        LinkNeighbors();

        // 2. Transforma a matemática em Dados do Core e Dados da UI
        List<Region> coreRegions = new List<Region>();
        List<MapRenderer.RegionCoordinate> uiCoordinates = new List<MapRenderer.RegionCoordinate>();

        // Dicionário temporário para podermos conectar as instâncias na Etapa B
        Dictionary<string, Region> regionInstances = new Dictionary<string, Region>();

        // --- ETAPA A: Criação das Entidades ---
        foreach (var kvp in _mapGrid)
        {
            LogicalNode node = kvp.Value;
            if (node.IsLand)
            {
                // A. Cria a Entidade do Core
                Region newRegion = new Region(node.ID, $"{node.TerritoryID} {node.ID}", 1);
                
                regionInstances.Add(newRegion.ID, newRegion);
                coreRegions.Add(newRegion);

                // B. Cria a Coordenada Visual para o MapRenderer
                uiCoordinates.Add(new MapRenderer.RegionCoordinate
                {
                    RegionID = node.ID,
                    Position = node.LocalPosition
                });
            }
        }

        // --- ETAPA B: Conexão Bidirecional Segura ---
        foreach (var kvp in _mapGrid)
        {
            LogicalNode node = kvp.Value;
            if (node.IsLand)
            {
                Region currentRegion = regionInstances[node.ID];

                foreach (var logicalNeighbor in node.Neighbors)
                {
                    if (logicalNeighbor.IsLand && regionInstances.TryGetValue(logicalNeighbor.ID, out Region neighborRegion))
                    {
                        // Usa o método encapsulado do seu Domínio!
                        // Como o seu AddNeighbor já faz neighbor.AddNeighbor(this) e checa duplicatas,
                        // a conexão é montada com perfeição matemática.
                        currentRegion.AddNeighbor(neighborRegion);
                    }
                }
            }
        }

        // 3. Alimenta o Core
        GameBootstrap.Instance.Map.InitializeProceduralGraph(coreRegions); 

        // 4. Manda a UI desenhar o mapa final
        _mapRenderer.BuildVisualMap(uiCoordinates);
    }

    private void GenerateLogicalMap()
    {
        int globalIndex = 0;

        foreach (var territory in _territories)
        {
            Bounds bounds = territory.Polygon.bounds;
            float step = territory.NodeSpacing;

            // --- A TRAVA DE SEGURANÇA CONTRA LOOPS INFINITOS ---
            if (step <= 1f) 
            {
                Debug.LogError($"[Salva-Vidas] O NodeSpacing do território {territory.TerritoryID} estava perigosamente baixo ou zerado ({step}). Forçando para 50f para evitar travamento.");
                step = 50f;
            }

            for (float x = bounds.min.x; x < bounds.max.x; x += step)
            {
                for (float y = bounds.min.y; y < bounds.max.y; y += step)
                {
                    Vector2 point = new Vector2(x, y);

                    if (territory.Polygon.OverlapPoint(point))
                    {
                        string id = $"R_{globalIndex++}";
                        _mapGrid.Add(id, new LogicalNode
                        {
                            ID = id,
                            LocalPosition = point,
                            IsLand = true,
                            TerritoryID = territory.TerritoryID
                        });
                    }
                }
            }
        }
    }

    private void LinkNeighbors()
    {
        var allNodes = new List<LogicalNode>(_mapGrid.Values);

        for (int i = 0; i < allNodes.Count; i++)
        {
            for (int j = i + 1; j < allNodes.Count; j++)
            {
                float dist = Vector2.Distance(allNodes[i].LocalPosition, allNodes[j].LocalPosition);
                
                // A distância máxima para ser vizinho é o Step * 1.5 (para pegar diagonais)
                // Ajuste este valor se as linhas começarem a cruzar de forma estranha
                if (dist <= 75f) 
                {
                    allNodes[i].Neighbors.Add(allNodes[j]);
                    allNodes[j].Neighbors.Add(allNodes[i]);
                }
            }
        }
    }
}