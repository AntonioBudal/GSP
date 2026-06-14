using UnityEngine;
using System.Linq;

public class NurseryManager : MonoBehaviour
{
    public static NurseryManager Instance { get; private set; }

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
            TimeManager.Instance.OnDayAdvanced += ProcessBreedings;
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayAdvanced -= ProcessBreedings;
        }
    }

    private void Update()
    {
        // Atalho de Debug: Aperte 'B' para tentar reproduzir os 2 primeiros corvos disponíveis
        if (Input.GetKeyDown(KeyCode.B))
        {
            var availableRavens = SaveManager.Instance.CurrentSave.ravens
                .Where(r => r.state == RavenState.Available)
                .Take(2)
                .ToList();

            if (availableRavens.Count == 2)
            {
                StartBreeding(availableRavens[0].id, availableRavens[1].id);
            }
            else
            {
                Debug.LogWarning("[NurseryManager] Não há 2 corvos disponíveis para cruzar!");
            }
        }
    }

    public void StartBreeding(string parentA_Id, string parentB_Id)
    {
        var save = SaveManager.Instance.CurrentSave;
        var parentA = save.ravens.FirstOrDefault(r => r.id == parentA_Id);
        var parentB = save.ravens.FirstOrDefault(r => r.id == parentB_Id);

        // Validação de segurança
        if (parentA == null || parentB == null || parentA.state != RavenState.Available || parentB.state != RavenState.Available)
        {
            Debug.LogError("[NurseryManager] Tentativa inválida de reprodução. Pais indisponíveis ou não encontrados.");
            return;
        }

        // Tranca o estado dos pais
        parentA.state = RavenState.Breeding;
        parentB.state = RavenState.Breeding;

        int duration = 4; // Duração fixa de 4 dias
        save.activeBreedings.Add(new BreedingData(parentA_Id, parentB_Id, duration));

        Debug.Log($"[NurseryManager] Corvos {parentA_Id} e {parentB_Id} iniciaram reprodução. Retornam em {duration} dias.");
    }

    private void ProcessBreedings()
    {
        var activeBreedings = SaveManager.Instance.CurrentSave.activeBreedings;

        // Loop reverso pois removeremos itens da lista
        for (int i = activeBreedings.Count - 1; i >= 0; i--)
        {
            var breeding = activeBreedings[i];
            breeding.remainingDays--;

            if (breeding.remainingDays <= 0)
            {
                CompleteBreeding(breeding);
                activeBreedings.RemoveAt(i);
            }
        }
    }

    private void CompleteBreeding(BreedingData breeding)
    {
        var save = SaveManager.Instance.CurrentSave;
        var parentA = save.ravens.FirstOrDefault(r => r.id == breeding.parentA_Id);
        var parentB = save.ravens.FirstOrDefault(r => r.id == breeding.parentB_Id);

        if (parentA != null && parentB != null)
        {
            // O algoritmo de genética determinística: Média dos pais + 1
            // Usamos Mathf.RoundToInt para arredondar corretamente caso dê número quebrado (ex: (3+4)/2 = 3.5 -> 4)
            int newSpeed = Mathf.RoundToInt((parentA.speed + parentB.speed) / 2f) + 1;
            int newLifespan = Mathf.RoundToInt((parentA.lifespan + parentB.lifespan) / 2f) + 1;
            int newFocus = Mathf.RoundToInt((parentA.focus + parentB.focus) / 2f) + 1;

            // Manda o RavenManager criar a nova ave
            if (RavenManager.Instance != null)
            {
                RavenManager.Instance.CreateRaven(newSpeed, newLifespan, newFocus);
                Debug.Log($"[NurseryManager] Filhote nascido via genética! Nova geração garantida.");
            }

            // Libera os pais
            parentA.state = RavenState.Available;
            parentB.state = RavenState.Available;
            
            Debug.Log($"[NurseryManager] Pais {parentA.id} e {parentB.id} terminaram e estão Disponíveis novamente.");
        }
    }
}