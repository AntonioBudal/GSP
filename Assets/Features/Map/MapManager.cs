using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Map Configuration")]
    // Referência de todas as províncias que existem no seu jogo
    [SerializeField] private List<ProvinceConfig> allProvinces;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Atalho de Debug: Aperte 'M' para revelar a primeira província de Fronteira que o sistema achar
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Busca no save se existe alguma fronteira
            var frontier = SaveManager.Instance.CurrentSave.provinces
                .FirstOrDefault(p => p.status == ProvinceStatus.Frontier);
            
            if (frontier != null)
            {
                RevealProvince(frontier.id);
            }
            else
            {
                Debug.Log("[MapManager] Nenhuma província de fronteira encontrada para revelar.");
            }
        }
    }

    // Método central chamado quando um corvo conclui a exploração
    public void RevealProvince(string provinceId)
    {
        // 1. Busca a configuração estática do mapa
        var config = allProvinces.FirstOrDefault(c => c.id == provinceId);
        if (config == null)
        {
            Debug.LogError($"[MapManager] Província {provinceId} não existe nas configurações!");
            return;
        }

        // 2. Busca ou cria o estado mutável no Save
        var saveState = SaveManager.Instance.CurrentSave.provinces.FirstOrDefault(p => p.id == provinceId);
        if (saveState == null)
        {
            saveState = new ProvinceState(provinceId, ProvinceStatus.Revealed);
            SaveManager.Instance.CurrentSave.provinces.Add(saveState);
        }
        else
        {
            saveState.status = ProvinceStatus.Revealed;
        }

        Debug.Log($"[MapManager] Área Revelada: {config.provinceName}");

        // 3. Atualiza os vizinhos para "Fronteira"
        foreach (var neighbor in config.neighbors)
        {
            var neighborState = SaveManager.Instance.CurrentSave.provinces.FirstOrDefault(p => p.id == neighbor.id);
            
            // Se o vizinho não existe no save, significa que está Oculto.
            // Oculto é o estado padrão da inexistência de dados (para poupar tamanho de save).
            if (neighborState == null)
            {
                SaveManager.Instance.CurrentSave.provinces.Add(new ProvinceState(neighbor.id, ProvinceStatus.Frontier));
                Debug.Log($"[MapManager] Nova Fronteira Descoberta: {neighbor.provinceName} ({neighbor.id})");
            }
        }
    }
}