using System;

public enum DayPhase
{
    StartOfDay,
    Processing,
    EndOfDay
}

public class GameClock
{
    public int CurrentDay { get; private set; }
    public DayPhase CurrentPhase { get; private set; }

    // Os três eventos estruturados de turno
    public event Action<int> OnDayStarted;
    public event Action<int> OnDayProcessing;
    public event Action<int> OnDayEnded;

    public GameClock(int initialDay = 0)
    {
        CurrentDay = initialDay;
    }

    // O método que o TrainingManager e o TimeManager precisam enxergar
    public void AdvanceTime(int daysToAdvance)
    {
        if (daysToAdvance <= 0) return;

        for (int i = 0; i < daysToAdvance; i++)
        {
            CurrentDay++;

            CurrentPhase = DayPhase.StartOfDay;
            OnDayStarted?.Invoke(CurrentDay);

            CurrentPhase = DayPhase.Processing;
            OnDayProcessing?.Invoke(CurrentDay);

            CurrentPhase = DayPhase.EndOfDay;
            OnDayEnded?.Invoke(CurrentDay);
        }
    }
}