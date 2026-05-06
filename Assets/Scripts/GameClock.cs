using System;

public class GameClock
{
    public int CurrentDay { get; private set; } = 0;

    public event Action<int>? OnDayAdvanced;

    public bool AdvanceDays(int days)
    {
        if (days <= 0) return false;

        for (int i = 0; i < days; i++)
        {
            CurrentDay++;
            OnDayAdvanced?.Invoke(CurrentDay);
        }

        return true;
    }
}