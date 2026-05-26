// Assets/Scripts/Core/SaveSystem/SaveManager.cs
using System.Linq;
using System.Threading.Tasks;

namespace Corvus.Core.SaveSystem
{
    public class SaveManager
    {
        private readonly SaveService _saveService;
        private readonly GameClock _clock;
        private readonly MapManager _map;
        private readonly ICrowRepository _crowRepo;
        private readonly ProgressionManager _progression;
        private readonly InfluenceManager _influence;
        
        // Novas dependências
        private readonly ExpeditionManager _expeditions;
        private readonly TrainingManager _training;

        public SaveManager(SaveService saveService, GameClock clock, MapManager map, ICrowRepository crowRepo, 
                           ProgressionManager progression, InfluenceManager influence, 
                           ExpeditionManager expeditions, TrainingManager training)
        {
            _saveService = saveService;
            _clock = clock;
            _map = map;
            _crowRepo = crowRepo;
            _progression = progression;
            _influence = influence;
            _expeditions = expeditions;
            _training = training;
        }

        public async Task<bool> SaveCurrentGameStateAsync()
        {
            var dto = new SaveGameDTO
            {
                CurrentDay = _clock.CurrentDay
            };

            // 1. Fotografando os Corvos
            foreach (var crow in _crowRepo.GetAllCrows())
            {
                var crowData = new CrowSaveData
                {
                    ID = crow.ID,
                    CurrentState = crow.CurrentState,
                    Speed = crow.Speed,
                    Focus = crow.Focus,
                    Resilience = crow.Resilience,
                    Lifespan = crow.Lifespan,
                    Role = crow.Role,
                    Traits = crow.Genetics.Traits.ToList()
                };
                dto.Crows.Add(crowData);
            }

            // 2. Fotografando o Mapa
            foreach (var region in _map.GetAllRegions())
            {
                var regionData = new RegionSaveData
                {
                    ID = region.ID,
                    CurrentState = region.CurrentState
                };
                dto.Regions.Add(regionData);
            }

            // 3. Fotografando a Progressão
            dto.Progression = _progression.GetSaveSnapshot();

            // 4. Fotografando a Influência
            foreach (var runtime in _influence.GetAllRuntimes())
            {
                dto.Influence.Add(new InfluenceSaveData {
                    RegionID = runtime.RegionId, 
                    Believers = runtime.Believers
                });
            }

            // 5. Fotografando Expedições
            foreach (var exp in _expeditions.GetAllActiveExpeditions())
            {
                dto.Expeditions.Add(new ExpeditionSaveData {
                    CrowId = exp.CrowId,
                    Mission = exp.Mission,
                    TargetRegionId = exp.TargetRegionId,
                    DaysElapsed = exp.DaysElapsed,
                    Progress = exp.Progress
                });
            }

            // 6. Fotografando Treinos e Fadiga
            foreach (var t in _training.GetAllActiveTrainings())
            {
                dto.Trainings.Add(new TrainingSaveData {
                    CrowId = t.CrowId, TargetRole = t.TargetRole,
                    DaysRemaining = t.DaysRemaining, LifespanCost = t.LifespanCost
                });
            }

            foreach (var f in _training.GetAllFatigueData())
            {
                dto.Fatigue.Add(new FatigueSaveData { CrowId = f.CrowId, DaysLeft = f.DaysLeft });
            }

            // Envia para o disco (I/O assíncrono puro)
            return await _saveService.SaveGameAsync(dto);
        }
    }
}