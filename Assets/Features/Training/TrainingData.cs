using System;

[Serializable]
public enum TrainingType
{
    Speed,
    Endurance,
    Focus
}

[Serializable]
public class TrainingData
{
    public string ravenId;
    public TrainingType type;
    public int remainingDays;

    public TrainingData() { }

    public TrainingData(string ravenId, TrainingType type, int remainingDays)
    {
        this.ravenId = ravenId;
        this.type = type;
        this.remainingDays = remainingDays;
    }
}