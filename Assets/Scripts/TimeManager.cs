using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private bool logDays = true;

    public GameClock Clock { get; private set; } = new GameClock();

    public int CurrentDay => Clock.CurrentDay;

    private void Awake()
    {
        Clock.OnDayAdvanced += HandleDayAdvanced;
    }

    private void OnDestroy()
    {
        Clock.OnDayAdvanced -= HandleDayAdvanced;
    }

    public void AdvanceTime(int days)
    {
        if (!Clock.AdvanceDays(days))
        {
            Debug.LogWarning("Relógio Central: valor inválido.");
        }
    }

    private void HandleDayAdvanced(int day)
    {
        if (logDays)
            Debug.Log($"Relógio Central: Dia [{day}]");
    }
}