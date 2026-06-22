using UnityEngine;
using System.Linq;

public class TrainingManager : MonoBehaviour
{
    public static TrainingManager Instance { get; private set; }

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
            TimeManager.Instance.OnDayAdvanced += ProcessTrainings;
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayAdvanced -= ProcessTrainings;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            var availableRaven = SaveManager.Instance.CurrentSave.ravens
                .FirstOrDefault(r => r.state == RavenState.Available);

            if (availableRaven != null)
            {
                StartTraining(availableRaven.id, TrainingType.Speed);
            }
        }
    }

    // ASSINATURA CORRIGIDA PARA RETORNAR BOOL
    public bool StartTraining(string ravenId, TrainingType type)
    {
        var raven = SaveManager.Instance.CurrentSave.ravens.FirstOrDefault(r => r.id == ravenId);
        
        if (raven == null || raven.state != RavenState.Available)
        {
            Debug.LogWarning($"[TrainingManager] Falha ao treinar. Corvo {ravenId} indisponível.");
            return false; // Retorna falha para a UI
        }

        int duration = type == TrainingType.Speed ? 3 : type == TrainingType.Endurance ? 5 : 7;

        raven.state = RavenState.Training;
        var newTraining = new TrainingData(raven.id, type, duration);
        SaveManager.Instance.CurrentSave.activeTrainings.Add(newTraining);

        Debug.Log($"[TrainingManager] Sucesso: Corvo {ravenId} iniciou treino de {type}. Duração: {duration} dias.");
        return true; // Retorna sucesso para a UI
    }

    private void ProcessTrainings()
    {
        var activeTrainings = SaveManager.Instance.CurrentSave.activeTrainings;

        for (int i = activeTrainings.Count - 1; i >= 0; i--)
        {
            var training = activeTrainings[i];
            training.remainingDays--;

            if (training.remainingDays <= 0)
            {
                CompleteTraining(training);
                activeTrainings.RemoveAt(i);
            }
        }
    }

    private void CompleteTraining(TrainingData training)
    {
        var raven = SaveManager.Instance.CurrentSave.ravens.FirstOrDefault(r => r.id == training.ravenId);
        if (raven != null)
        {
            switch (training.type)
            {
                case TrainingType.Speed: raven.speed++; break;
                case TrainingType.Endurance: raven.lifespan++; break;
                case TrainingType.Focus: raven.focus++; break;
            }

            raven.state = RavenState.Available;
            Debug.Log($"[TrainingManager] Corvo {raven.id} terminou o treino!");
        }
    }
}