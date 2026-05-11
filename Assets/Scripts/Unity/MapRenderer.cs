// Assets/Scripts/Unity/MapRenderer.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private GameObject _edgePrefab; // Uma simples Image UI branca

    [Header("Containers (Organização no Canvas)")]
    [SerializeField] private Transform _edgesContainer;
    [SerializeField] private Transform _nodesContainer;

    [System.Serializable]
    public struct NodeAnchor
    {
        public string RegionID;
        public RectTransform AnchorPoint;
    }

    [Header("Posições Manuais do Editor")]
    public List<NodeAnchor> nodeAnchors;

    private Dictionary<string, UI_MapNodeController> _spawnedNodes = new Dictionary<string, UI_MapNodeController>();

    private void Start()
    {
        // Aguarda um frame para garantir que o Bootstrap carregou o MapManager
        Invoke(nameof(BuildVisualMap), 0.1f);
    }

    private void BuildVisualMap()
    {
        var mapManager = GameBootstrap.Instance.Map;
        if (mapManager == null) return;

        // 1. Instanciar os Nós
        foreach (var anchor in nodeAnchors)
        {
            // Substitua 'mapManager.Regions' pelo método real que você usa para obter todas as regiões no seu Core (ex: GetAllRegions())
            // Assumiremos aqui que existe um método para buscar a região ou uma lista exposta.
            // (Se der erro aqui, criaremos um GetRegion no MapManager).
            Region region = mapManager.GetRegion(anchor.RegionID); 
            
            if (region != null)
            {
                var nodeObj = Instantiate(_nodePrefab, _nodesContainer);
                nodeObj.transform.position = anchor.AnchorPoint.position;
                
                var controller = nodeObj.GetComponent<UI_MapNodeController>();
                controller.Setup(region);
                _spawnedNodes.Add(anchor.RegionID, controller);
            }
        }

        // 2. Desenhar as Linhas (Arestas)
        HashSet<string> drawnEdges = new HashSet<string>();

        foreach (var anchor in nodeAnchors)
        {
            Region region = mapManager.GetRegion(anchor.RegionID);
            if (region == null) continue;

            foreach (var neighborId in region.Neighbors)
            {
                // Garante que não desenhamos a linha A->B e B->A duas vezes
                string edgeKey = anchor.RegionID.CompareTo(neighborId) < 0 
                    ? $"{anchor.RegionID}-{neighborId}" 
                    : $"{neighborId}-{anchor.RegionID}";
                
                if (!drawnEdges.Contains(edgeKey) && _spawnedNodes.ContainsKey(neighborId))
                {
                    DrawEdge(_spawnedNodes[anchor.RegionID].transform as RectTransform, 
                             _spawnedNodes[neighborId].transform as RectTransform);
                    drawnEdges.Add(edgeKey);
                }
            }
        }

        // 3. Assinar o evento purista para reação visual
        mapManager.OnRegionStateChanged += HandleRegionStateChanged;
    }

    private void DrawEdge(RectTransform from, RectTransform to)
    {
        // O EdgePrefab é uma imagem esticada e rotacionada matematicamente entre os dois pontos
        var edgeObj = Instantiate(_edgePrefab, _edgesContainer);
        var edgeRect = edgeObj.GetComponent<RectTransform>();
        
        Vector3 dir = to.position - from.position;
        float distance = dir.magnitude;
        
        edgeRect.position = from.position + dir / 2f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        edgeRect.rotation = Quaternion.Euler(0, 0, angle);
        edgeRect.sizeDelta = new Vector2(distance, 5f); // 5f de espessura da linha
    }

    private void HandleRegionStateChanged(MapStateResult result)
    {
        if (_spawnedNodes.TryGetValue(result.RegionId, out var node))
        {
            node.UpdateVisualState(result.NewState);
        }
    }

    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Map != null)
        {
            GameBootstrap.Instance.Map.OnRegionStateChanged -= HandleRegionStateChanged;
        }
    }
}