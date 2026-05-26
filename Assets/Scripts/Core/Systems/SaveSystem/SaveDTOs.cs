// Assets/Scripts/Core/SaveSystem/SaveDTOs.cs
using System;
using System.Collections.Generic;

namespace Corvus.Core.SaveSystem
{
    /// <summary>
    /// O contêiner principal que será transformado em JSON.
    /// </summary>
    [Serializable]
    public class SaveGameDTO
    {
        public int CurrentDay;
        
        public List<CrowSaveData> Crows = new List<CrowSaveData>();
        public List<RegionSaveData> Regions = new List<RegionSaveData>();
        public ProgressionSaveData Progression;
        public List<InfluenceSaveData> Influence = new List<InfluenceSaveData>();
        
        // OS 3 NOVOS PAYLOADS DE RUNTIME
        public List<ExpeditionSaveData> Expeditions = new List<ExpeditionSaveData>();
        public List<TrainingSaveData> Trainings = new List<TrainingSaveData>();
        public List<FatigueSaveData> Fatigue = new List<FatigueSaveData>();
    }

    [Serializable]
    public class ProgressionSaveData
    {
        public int TotalDaysElapsed;
        public int TotalRegionsExplored;
        public List<MilestoneID> UnlockedMilestones = new List<MilestoneID>();
    }

    [Serializable]
    public class InfluenceSaveData
    {
        public string RegionID;
        public int Believers;
    }

    /// <summary>
    /// A fotografia exata dos atributos mutáveis de um corvo no momento do save.
    /// </summary>
    [Serializable]
    public class CrowSaveData
    {
        public string ID;
        public CrowState CurrentState;
        
        public int Speed;
        public int Focus;
        public int Resilience;
        public int Lifespan;
        
        public CrowRole Role;
        
        // Achatamos a classe GeneticSeed em uma simples lista de Enums para o JSON ficar limpo
        public List<CrowTrait> Traits = new List<CrowTrait>();
    }

    /// <summary>
    /// A fotografia do estado geográfico.
    /// </summary>
    [Serializable]
    public class RegionSaveData
    {
        public string ID;
        public FogState CurrentState;
    }

    [Serializable]
    public class ExpeditionSaveData
    {
        public string CrowId;
        public MissionType Mission;
        public string TargetRegionId;
        public int DaysElapsed;
        public int Progress;
    }

    [Serializable]
    public class TrainingSaveData
    {
        public string CrowId;
        public CrowRole TargetRole;
        public int DaysRemaining;
        public int LifespanCost;
    }

    [Serializable]
    public class FatigueSaveData
    {
        public string CrowId;
        public int DaysLeft;
    }
}