using UnityEngine;
using System.Linq;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager Instance { get; private set; }

    [Header("Configurações")]
    public int baseExpeditionDays = 5;

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
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayAdvanced += ProcessExpeditions;
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayAdvanced -= ProcessExpeditions;
        }
    }

    // Método exposto para a UI prever quantos dias a viagem vai durar
    public int GetEstimatedDuration(int ravenSpeed)
    {
        return Mathf.Max(1, baseExpeditionDays - ravenSpeed);
    }

    public bool SendExpedition(string ravenId, string provinceId)
    {
        var save = SaveManager.Instance.CurrentSave;
        var raven = save.ravens.FirstOrDefault(r => r.id == ravenId);

        if (raven == null || raven.state != RavenState.Available)
        {
            Debug.LogWarning($"[ExpeditionManager] Falha: Corvo {ravenId} indisponível.");
            return false; // UI deve bloquear
        }

        int duration = GetEstimatedDuration(raven.speed);
        raven.state = RavenState.Exploring;

        save.activeExpeditions.Add(new ExpeditionData(raven.id, provinceId, duration));
        
        Debug.Log($"[ExpeditionManager] Sucesso: Corvo {ravenId} partiu para {provinceId}. Retorna em {duration} dias.");
        return true; // UI deve fechar ou confirmar
    }

    private void ProcessExpeditions()
    {
        var activeExpeditions = SaveManager.Instance.CurrentSave.activeExpeditions;

        for (int i = activeExpeditions.Count - 1; i >= 0; i--)
        {
            var exp = activeExpeditions[i];
            exp.remainingDays--;

            if (exp.remainingDays <= 0)
            {
                CompleteExpedition(exp);
                activeExpeditions.RemoveAt(i);
            }
        }
    }

    private void CompleteExpedition(ExpeditionData exp)
    {
        var save = SaveManager.Instance.CurrentSave;
        var raven = save.ravens.FirstOrDefault(r => r.id == exp.ravenId);
        
        if (raven != null)
        {
            raven.state = RavenState.Available;
            Debug.Log($"[ExpeditionManager] Corvo {raven.id} retornou da expedição!");
        }

        // Manda o MapManager executar a regra de negócio da descoberta
        if (MapManager.Instance != null)
        {
            MapManager.Instance.RevealProvince(exp.targetProvinceId);
        }
    }
}