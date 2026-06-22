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
        }
    }

    // ASSINATURA CORRIGIDA PARA RETORNAR BOOL
    public bool StartBreeding(string parentA_Id, string parentB_Id)
    {
        var save = SaveManager.Instance.CurrentSave;
        var parentA = save.ravens.FirstOrDefault(r => r.id == parentA_Id);
        var parentB = save.ravens.FirstOrDefault(r => r.id == parentB_Id);

        if (parentA == null || parentB == null || parentA.state != RavenState.Available || parentB.state != RavenState.Available)
        {
            Debug.LogWarning("[NurseryManager] Falha: Pais indisponíveis ou não encontrados.");
            return false; // Retorna falha para a UI
        }

        parentA.state = RavenState.Breeding;
        parentB.state = RavenState.Breeding;

        save.activeBreedings.Add(new BreedingData(parentA_Id, parentB_Id, 4));
        Debug.Log($"[NurseryManager] Sucesso: {parentA_Id} e {parentB_Id} no ninho.");
        return true; // Retorna sucesso para a UI
    }

    private void ProcessBreedings()
    {
        var activeBreedings = SaveManager.Instance.CurrentSave.activeBreedings;

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
            int newSpeed = Mathf.RoundToInt((parentA.speed + parentB.speed) / 2f) + 1;
            int newLifespan = Mathf.RoundToInt((parentA.lifespan + parentB.lifespan) / 2f) + 1;
            int newFocus = Mathf.RoundToInt((parentA.focus + parentB.focus) / 2f) + 1;

            if (RavenManager.Instance != null)
            {
                RavenManager.Instance.CreateRaven(newSpeed, newLifespan, newFocus);
            }

            parentA.state = RavenState.Available;
            parentB.state = RavenState.Available;
            
            Debug.Log($"[NurseryManager] Filhote gerado. Pais liberados.");
        }
    }
}