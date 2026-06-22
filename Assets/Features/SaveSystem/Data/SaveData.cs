using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int currentDay;
    
    public List<RavenData> ravens = new List<RavenData>();
    public List<ProvinceStateData> provinces = new List<ProvinceStateData>();
    public List<ExpeditionData> activeExpeditions = new List<ExpeditionData>();
    public List<TrainingData> activeTrainings = new List<TrainingData>();
    public List<BreedingData> activeBreedings = new List<BreedingData>();

    public SaveData()
    {
        currentDay = 1;
    }
}