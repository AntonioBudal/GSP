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
        // Inscreve no motor de tempo
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
        // Atalho de Debug: Aperte 'T' para enviar o primeiro corvo disponível para o treino de Velocidade
        if (Input.GetKeyDown(KeyCode.T))
        {
            var availableRaven = SaveManager.Instance.CurrentSave.ravens
                .FirstOrDefault(r => r.state == RavenState.Available);

            if (availableRaven != null)
            {
                StartTraining(availableRaven.id, TrainingType.Speed);
            }
            else
            {
                Debug.LogWarning("[TrainingManager] Nenhum corvo disponível para treinar!");
            }
        }
    }

    public void StartTraining(string ravenId, TrainingType type)
    {
        var raven = SaveManager.Instance.CurrentSave.ravens.FirstOrDefault(r => r.id == ravenId);
        if (raven == null || raven.state != RavenState.Available) return;

        // Define a duração com base na sua regra de negócio
        int duration = 0;
        switch (type)
        {
            case TrainingType.Speed: duration = 3; break;
            case TrainingType.Endurance: duration = 5; break;
            case TrainingType.Focus: duration = 7; break;
        }

        // Tranca o estado do corvo
        raven.state = RavenState.Training;

        // Registra o treino ativo no save
        var newTraining = new TrainingData(raven.id, type, duration);
        SaveManager.Instance.CurrentSave.activeTrainings.Add(newTraining);

        Debug.Log($"[TrainingManager] Corvo {raven.id} iniciou treino de {type}. Duração: {duration} dias.");
    }

    private void ProcessTrainings()
    {
        var activeTrainings = SaveManager.Instance.CurrentSave.activeTrainings;

        // Loop reverso pois removeremos itens da lista
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
            // Aplica a evolução dos atributos baseada no tipo de treino
            switch (training.type)
            {
                case TrainingType.Speed: raven.speed++; break;
                case TrainingType.Endurance: raven.lifespan++; break;
                case TrainingType.Focus: raven.focus++; break;
            }

            // Libera o corvo
            raven.state = RavenState.Available;
            
            Debug.Log($"[TrainingManager] Corvo {raven.id} terminou o treino! Novo status -> Vel: {raven.speed}, Vida: {raven.lifespan}, Foco: {raven.focus}");
        }
    }
}