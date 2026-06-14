using UnityEngine;
using System.Linq;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager Instance { get; private set; }

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
        // Inscreve o gerenciador no evento de passagem de dias do TimeManager
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayAdvanced += ProcessExpeditions;
        }
    }

    private void OnDestroy()
    {
        // Sempre desinscrever eventos para evitar memory leaks
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayAdvanced -= ProcessExpeditions;
        }
    }

    private void Update()
    {
        // Atalho de Debug: Aperte 'E' para enviar o primeiro corvo disponível para a primeira fronteira
        if (Input.GetKeyDown(KeyCode.E))
        {
            var availableRaven = SaveManager.Instance.CurrentSave.ravens
                .FirstOrDefault(r => r.state == RavenState.Available);
                
            var frontierProvince = SaveManager.Instance.CurrentSave.provinces
                .FirstOrDefault(p => p.status == ProvinceStatus.Frontier);

            if (availableRaven != null && frontierProvince != null)
            {
                SendExpedition(availableRaven.id, frontierProvince.id);
            }
            else
            {
                Debug.LogWarning("[ExpeditionManager] Sem corvos disponíveis ou sem fronteiras para explorar!");
            }
        }

        if (Input.GetKeyDown(KeyCode.U)) UIManager.Instance.OpenPopup(PopupType.Map);
    }

    public void SendExpedition(string ravenId, string provinceId)
    {
        // 1. Validações e busca dos dados
        var raven = SaveManager.Instance.CurrentSave.ravens.FirstOrDefault(r => r.id == ravenId);
        if (raven == null || raven.state != RavenState.Available) return;

        // 2. Calcula o tempo da viagem. 
        // Exemplo simples: 5 dias base - Velocidade do Corvo. (Mínimo de 1 dia).
        int baseDays = 5;
        int duration = Mathf.Max(1, baseDays - raven.speed);

        // 3. Tranca o corvo
        raven.state = RavenState.Exploring;

        // 4. Cria e salva a expedição
        var newExpedition = new ExpeditionData(raven.id, provinceId, duration);
        SaveManager.Instance.CurrentSave.activeExpeditions.Add(newExpedition);

        Debug.Log($"[ExpeditionManager] Corvo {raven.id} partiu para {provinceId}. Retorna em {duration} dias.");
    }

    // Este método é chamado automaticamente toda vez que o TimeManager avança 1 dia
    private void ProcessExpeditions()
    {
        var activeExpeditions = SaveManager.Instance.CurrentSave.activeExpeditions;

        // Iteramos de trás para frente porque vamos remover itens da lista durante o loop
        for (int i = activeExpeditions.Count - 1; i >= 0; i--)
        {
            var exp = activeExpeditions[i];
            exp.remainingDays--;

            if (exp.remainingDays <= 0)
            {
                CompleteExpedition(exp);
                activeExpeditions.RemoveAt(i); // Remove do save
            }
        }
    }

    private void CompleteExpedition(ExpeditionData exp)
    {
        // 1. Libera o corvo
        var raven = SaveManager.Instance.CurrentSave.ravens.FirstOrDefault(r => r.id == exp.ravenId);
        if (raven != null)
        {
            raven.state = RavenState.Available;
            Debug.Log($"[ExpeditionManager] Corvo {raven.id} retornou da expedição!");
        }

        // 2. Manda o MapManager revelar a área
        if (MapManager.Instance != null)
        {
            MapManager.Instance.RevealProvince(exp.targetProvinceId);
        }
    }
}