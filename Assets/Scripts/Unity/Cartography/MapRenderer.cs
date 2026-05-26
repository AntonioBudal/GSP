using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MapRenderer : MonoBehaviour
{
    [Header("Prefabs de UI")]
    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private GameObject _edgePrefab;
    [SerializeField] private GameObject _fogHolePrefab;

    [Header("Containers (Ordem de Sorting)")]
    [SerializeField] private RectTransform _holesContainer; 
    [SerializeField] private RectTransform _edgesContainer; 
    [SerializeField] private RectTransform _nodesContainer; 

    // --- A NOVA ESTRUTURA DE DADOS ENXUTA ---
    [System.Serializable]
    public struct RegionCoordinate
    {
        public string RegionID;
        [Tooltip("Posição X e Y do nó no pergaminho")]
        public Vector2 Position;
    }

    [Header("Cartografia (Mapeamento de Coordenadas)")]
    public List<RegionCoordinate> mapCoordinates;

    private class VisualEdge
    {
        public string RegionA;
        public string RegionB;
        public GameObject EdgeObject;
    }

    private Dictionary<string, UI_MapNodeController> _spawnedNodes = new Dictionary<string, UI_MapNodeController>();
    private Dictionary<string, GameObject> _spawnedHoles = new Dictionary<string, GameObject>();
    private List<VisualEdge> _visualEdges = new List<VisualEdge>();
    private IObjectPool<GameObject> _fogHolePool;

    private void Awake()
    {
        _fogHolePool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(_fogHolePrefab, _holesContainer),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: Destroy,
            defaultCapacity: 15,
            maxSize: 50
        );
    }

    /// <summary>
    /// Retorna o RectTransform de um nó visual específico para ancoragem de peões.
    /// </summary>
    public RectTransform GetNodeRect(string regionId)
    {
        if (_spawnedNodes.TryGetValue(regionId, out var node))
        {
            return node.transform as RectTransform;
        }
        return null;
    }

    public void BuildVisualMap(List<RegionCoordinate> generatedCoordinates)
    {
        var mapManager = GameBootstrap.Instance.Map;
        mapCoordinates = generatedCoordinates; 

      
        if (mapManager == null) return;

        // 1. Geração Dinâmica de Nós e Furos
        foreach (var coord in mapCoordinates)
        {
            Region region = mapManager.GetRegion(coord.RegionID); 
            if (region == null) continue;

            // --- INSTANCIAÇÃO AUTOMATIZADA DO NÓ ---
            GameObject nodeObj = Instantiate(_nodePrefab, _nodesContainer);
            RectTransform nodeRect = nodeObj.GetComponent<RectTransform>();
            
            nodeRect.sizeDelta = new Vector2(100f, 100f);
            nodeRect.anchorMin = new Vector2(0.5f, 0.5f);
            nodeRect.anchorMax = new Vector2(0.5f, 0.5f);
            
            // CORREÇÃO: Converte a coordenada Global do Polígono para a escala Local do Canvas
            Vector3 localNodePos = _nodesContainer.InverseTransformPoint(coord.Position);
            nodeRect.localPosition = localNodePos;
            
            UI_MapNodeController nodeCtrl = nodeObj.GetComponent<UI_MapNodeController>();
            if (nodeCtrl != null) nodeCtrl.Setup(region); 
            
            _spawnedNodes.Add(region.ID, nodeCtrl);

            // --- INSTANCIAÇÃO DO FURO DA NÉVOA ---
            GameObject holeObj = _fogHolePool.Get();
            RectTransform holeRect = holeObj.GetComponent<RectTransform>();
            
            // CORREÇÃO: Aplica a mesma conversão para o furo
            Vector3 localHolePos = _holesContainer.InverseTransformPoint(coord.Position);
            holeRect.localPosition = localHolePos;
            
            holeObj.SetActive(region.CurrentState != FogState.Oculto);
            
            _spawnedHoles.Add(region.ID, holeObj);
        }

        // 2. Criar as Arestas (Linhas)
        HashSet<string> drawnEdges = new HashSet<string>();

        foreach (var coord in mapCoordinates)
        {
            Region region = mapManager.GetRegion(coord.RegionID);
            if (region == null) continue;

            foreach (var neighborId in region.Neighbors)
            {
                string edgeKey = coord.RegionID.CompareTo(neighborId) < 0 
                    ? $"{coord.RegionID}-{neighborId}" 
                    : $"{neighborId}-{coord.RegionID}";
                
                if (!drawnEdges.Contains(edgeKey) && _spawnedNodes.ContainsKey(neighborId))
                {
                    GameObject newEdge = DrawEdge(
                        _spawnedNodes[coord.RegionID].GetComponent<RectTransform>(), 
                        _spawnedNodes[neighborId].GetComponent<RectTransform>()
                    );
                    
                    _visualEdges.Add(new VisualEdge { RegionA = coord.RegionID, RegionB = neighborId, EdgeObject = newEdge });
                    drawnEdges.Add(edgeKey);
                }
            }
        }

        UpdateAllEdgesVisibility();
        mapManager.OnRegionStateChanged += HandleRegionStateChanged;
    }

    private GameObject DrawEdge(RectTransform from, RectTransform to)
    {
        GameObject edgeObj = Instantiate(_edgePrefab, _edgesContainer);
        RectTransform edgeRect = edgeObj.GetComponent<RectTransform>();
        
        // Mantém a espessura visual correta no Canvas local
        edgeRect.localScale = Vector3.one;
        
        // Como ambos os nós têm a mesma âncora (0.5), podemos usar a anchoredPosition diretamente
        Vector2 dir = to.anchoredPosition - from.anchoredPosition;
        float distance = dir.magnitude;
        
        edgeRect.anchoredPosition = from.anchoredPosition + dir / 2f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        edgeRect.localRotation = Quaternion.Euler(0, 0, angle);
        edgeRect.sizeDelta = new Vector2(distance, 5f);
        
        return edgeObj;
    }

    private void HandleRegionStateChanged(MapStateResult result)
    {
        if (_spawnedNodes.TryGetValue(result.RegionId, out var node))
            node.UpdateVisualState(result.NewState);
        
        if (_spawnedHoles.TryGetValue(result.RegionId, out var hole))
            hole.SetActive(result.NewState != FogState.Oculto);

        UpdateAllEdgesVisibility();
    }

    private void UpdateAllEdgesVisibility()
    {
        var mapManager = GameBootstrap.Instance.Map;
        if (mapManager == null) return;

        foreach (var edge in _visualEdges)
        {
            var regA = mapManager.GetRegion(edge.RegionA);
            var regB = mapManager.GetRegion(edge.RegionB);

            if (regA == null || regB == null) continue;
            edge.EdgeObject.SetActive(!(regA.CurrentState == FogState.Oculto && regB.CurrentState == FogState.Oculto));
        }
    }

    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Map != null)
            GameBootstrap.Instance.Map.OnRegionStateChanged -= HandleRegionStateChanged;
    }

    // ==========================================
    // MAGIA DE ENGENHARIA: VISUALIZAÇÃO NO EDITOR
    // ==========================================
   private void OnDrawGizmos()
    {
        if (_nodesContainer == null || mapCoordinates == null) return;

        // Limpa a matrix para desenharmos livremente no World Space
        Gizmos.matrix = Matrix4x4.identity; 
        
        foreach (var coord in mapCoordinates)
        {
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
            // coord.Position já é World Space graças ao Polígono
            Gizmos.DrawSphere(new Vector3(coord.Position.x, coord.Position.y, 0), 20f);
        }
    }
}