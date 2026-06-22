using UnityEngine;
using System.Linq;

public class MortalityManager : MonoBehaviour
{
    public static MortalityManager Instance { get; private set; }

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
            TimeManager.Instance.OnDayAdvanced += ProcessAgingAndMortality;
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayAdvanced -= ProcessAgingAndMortality;
        }
    }

    private void ProcessAgingAndMortality()
    {
        var save = SaveManager.Instance.CurrentSave;

        // 1. Prevenção de Inchaço de Save (Save Bloat):
        // Remove permanentemente os corvos mortos do dia anterior.
        save.ravens.RemoveAll(r => r.state == RavenState.Dead);

        // 2. Envelhecimento e Teste de Mortalidade
        foreach (var raven in save.ravens)
        {
            if (raven.state == RavenState.Dead) continue;

            raven.age++;

            // Só existe risco se a idade atingir ou ultrapassar o Lifespan
            if (raven.age >= raven.lifespan)
            {
                int excessDays = raven.age - raven.lifespan;
                
                // Fórmula: 10% base + 15% por dia extra
                int deathChance = 10 + (excessDays * 15);
                
                int roll = UnityEngine.Random.Range(0, 100); // Rola de 0 a 99

                if (roll < deathChance)
                {
                    raven.state = RavenState.Dead;
                    Debug.Log($"[MortalityManager] O corvo {raven.id} não resistiu ao tempo e morreu aos {raven.age} dias. (Risco era de {deathChance}%)");
                    
                    CancelActiveTasksForRaven(raven.id);
                }
            }
        }
    }

    private void CancelActiveTasksForRaven(string ravenId)
    {
        var save = SaveManager.Instance.CurrentSave;
        
        // Cancela e remove qualquer expedição ativa
        int expRemoved = save.activeExpeditions.RemoveAll(e => e.ravenId == ravenId);
        if (expRemoved > 0) 
            Debug.Log($"[MortalityManager] Expedição abortada! O corvo {ravenId} morreu durante a jornada.");

        // Cancela e remove qualquer treinamento ativo
        int trainRemoved = save.activeTrainings.RemoveAll(t => t.ravenId == ravenId);
        if (trainRemoved > 0) 
            Debug.Log($"[MortalityManager] Treino cancelado. O corvo {ravenId} faleceu nas instalações.");

        // Cancela o berçário de forma drástica (A reprodução inteira falha se um pai morre)
        var breeding = save.activeBreedings.FirstOrDefault(b => b.parentA_Id == ravenId || b.parentB_Id == ravenId);
        if (breeding != null)
        {
            save.activeBreedings.Remove(breeding);
            
            // Descobre quem foi o pai que sobreviveu e o liberta do ninho
            string survivingParentId = breeding.parentA_Id == ravenId ? breeding.parentB_Id : breeding.parentA_Id;
            var survivingParent = save.ravens.FirstOrDefault(r => r.id == survivingParentId);
            
            if (survivingParent != null && survivingParent.state == RavenState.Breeding)
            {
                survivingParent.state = RavenState.Available;
            }
            
            Debug.Log($"[MortalityManager] Tragédia no ninho. A reprodução foi perdida pela morte de {ravenId}. O parceiro {survivingParentId} sobreviveu e está disponível.");
        }
    }
}