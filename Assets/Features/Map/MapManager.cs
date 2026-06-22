using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Atlas Global")]
    [SerializeField] private ProvinceDatabase database;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    private void Start()
    {
        if (GetProvinceStatus("LOTHIAN") == ProvinceStatus.Hidden)
        {
            RevealProvince("LOTHIAN");

           
            var allViews = FindObjectsByType<ProvinceView>();
            foreach (var view in allViews)
            {
                view.RefreshVisualState();
            }
        }
    }

    // --- CONSULTAS (Usadas pela UI e ProvinceView) ---

    public ProvinceConfig GetProvinceConfig(string provinceId)
    {
        if (database == null) return null;
        return database.provinces.FirstOrDefault(p => p.id == provinceId);
    }

    public ProvinceStatus GetProvinceStatus(string provinceId)
    {
        var saveState = SaveManager.Instance.CurrentSave.provinces.FirstOrDefault(p => p.provinceId == provinceId);
        
        // Se não está no save, logicamente está oculta
        return saveState != null ? saveState.status : ProvinceStatus.Hidden;
    }

    // --- REGRAS DE NEGÓCIO (Usadas pelo ExpeditionManager) ---

    public void RevealProvince(string provinceId)
    {
        var config = GetProvinceConfig(provinceId);
        if (config == null)
        {
            Debug.LogError($"[MapManager] Falha ao revelar: Província {provinceId} não existe no Database.");
            return;
        }

        // 1. Atualiza a província alvo para Revelada
        SetProvinceStatus(provinceId, ProvinceStatus.Revealed);
        Debug.Log($"[MapManager] Território Descoberto: {config.provinceName}");

        // 2. Atualiza os vizinhos Ocultos para Fronteira
        foreach (var neighbor in config.neighbors)
        {
            if (GetProvinceStatus(neighbor.id) == ProvinceStatus.Hidden)
            {
                SetProvinceStatus(neighbor.id, ProvinceStatus.Frontier);
                Debug.Log($"[MapManager] Nova Fronteira: {neighbor.provinceName}");
            }
        }

        foreach (var view in FindObjectsByType<ProvinceView>())
        {
            view.RefreshVisualState();
        }
    }

    private void SetProvinceStatus(string provinceId, ProvinceStatus newStatus)
    {
        var saveList = SaveManager.Instance.CurrentSave.provinces;
        var existingState = saveList.FirstOrDefault(p => p.provinceId == provinceId);

        if (existingState != null)
        {
            existingState.status = newStatus;
        }
        else
        {
            saveList.Add(new ProvinceStateData(provinceId, newStatus));
        }
    }
}