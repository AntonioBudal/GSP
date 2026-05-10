using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public GameClock Clock { get; private set; }
    public static TimeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Clock = new GameClock(0);
            
            // Assinando os eventos atualizados do GameClock para debug visual
            Clock.OnDayStarted += (day) => Debug.Log($"--- Iniciando Dia {day} ---");
            Clock.OnDayProcessing += (day) => Debug.Log($"Processando Dia {day}...");
            Clock.OnDayEnded += (day) => Debug.Log($"Dia {day} Encerrado.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AdvanceTime(int days)
    {
        Clock.AdvanceTime(days);
    }
}