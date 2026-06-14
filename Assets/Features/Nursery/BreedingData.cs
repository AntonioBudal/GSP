using System;

[Serializable]
public class BreedingData
{
    public string parentA_Id;
    public string parentB_Id;
    public int remainingDays;

    public BreedingData() { }

    public BreedingData(string parentA_Id, string parentB_Id, int remainingDays)
    {
        this.parentA_Id = parentA_Id;
        this.parentB_Id = parentB_Id;
        this.remainingDays = remainingDays;
    }
}