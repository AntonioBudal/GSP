using System;

namespace Corvus.Core.Progression
{
    public enum OnboardingStep
    {
        Despertar,       // Deve abrir o mapa
        PrimeiroVoo,     // Deve enviar uma expedição
        Espera,          // Deve avançar o dia
        SangueSuor,      // Deve treinar um corvo
        Preparacao,      // Deve sobreviver até o milestone inicial
        Concluido        // Tutorial livre
    }

    // DTO para o SaveSystem
    public class OnboardingSaveData
    {
        public OnboardingStep CurrentStep { get; set; }
    }

    public class OnboardingManager : IDisposable
    {
        private OnboardingStep _currentStep;

        // Dependências para escutar
        private readonly ExpeditionManager _expeditions;
        private readonly TrainingManager _training;
        private readonly GameClock _clock;
        private readonly ProgressionManager _progression;

        // O evento que a HUD da Unity vai escutar
        public event Action<string, string> OnObjectiveUpdated; // Lore, Diretiva

        public OnboardingManager(ExpeditionManager expeditions, TrainingManager training, 
                                 GameClock clock, ProgressionManager progression, 
                                 OnboardingSaveData saveData = null)
        {
            _expeditions = expeditions;
            _training = training;
            _clock = clock;
            _progression = progression;

            _currentStep = saveData?.CurrentStep ?? OnboardingStep.Despertar;

            // Assinamos os eventos silenciosamente
            _expeditions.OnExpeditionStarted += HandleExpeditionStarted;
            _clock.OnDayEnded += HandleDayEnded;
            _training.OnBaseTrainingExecuted += HandleTrainingExecuted;
            _progression.OnMilestoneReached += HandleMilestoneReached;
        }

        public OnboardingSaveData GetSaveSnapshot()
        {
            return new OnboardingSaveData { CurrentStep = _currentStep };
        }

        // ==========================================
        // FLUXO E MÁQUINA DE ESTADOS
        // ==========================================

        public void StartOnboarding()
        {
            // É chamado pela Unity assim que o jogo começa
            BroadcastCurrentObjective();
        }

        // Este método público existe caso o UI_MapWindow precise avisar que o mapa foi aberto
        public void NotifyMapOpened()
        {
            if (_currentStep == OnboardingStep.Despertar)
            {
                AdvanceStep(OnboardingStep.PrimeiroVoo);
            }
        }

        private void HandleExpeditionStarted(string crowId, string regionId)
        {
            if (_currentStep == OnboardingStep.PrimeiroVoo)
            {
                AdvanceStep(OnboardingStep.Espera);
            }
        }

        private void HandleDayEnded(int day)
        {
            if (_currentStep == OnboardingStep.Espera)
            {
                AdvanceStep(OnboardingStep.SangueSuor);
            }
        }

        private void HandleTrainingExecuted(string crowId)
        {
            if (_currentStep == OnboardingStep.SangueSuor)
            {
                AdvanceStep(OnboardingStep.Preparacao);
            }
        }

        private void HandleMilestoneReached(MilestoneID milestone, string title)
        {
            if (_currentStep == OnboardingStep.Preparacao && milestone == MilestoneID.SobrevivenciaInicial)
            {
                AdvanceStep(OnboardingStep.Concluido);
            }
        }

        private void AdvanceStep(OnboardingStep nextStep)
        {
            _currentStep = nextStep;
            BroadcastCurrentObjective();
        }

        private void BroadcastCurrentObjective()
        {
            string lore = "";
            string directive = "";

            switch (_currentStep)
            {
                case OnboardingStep.Despertar:
                    lore = "O Mosteiro desperta. Nossas vistas estão cegas.";
                    directive = "Abra o Mapa de Cartografia"; // A UI colocará o "□"
                    break;
                case OnboardingStep.PrimeiroVoo:
                    lore = "Terras desconhecidas aguardam. Que as asas negras rasguem o véu.";
                    directive = "Explore uma Região";
                    break;
                case OnboardingStep.Espera:
                    lore = "As bestas estão no céu. Apenas nos resta aguardar.";
                    directive = "Termine o dia no Quarto";
                    break;
                case OnboardingStep.SangueSuor:
                    lore = "O primeiro voo foi concluído. O que retorna fraco, deve ser fortalecido.";
                    directive = "Treine um Corvo";
                    break;
                case OnboardingStep.Preparacao:
                    lore = "A rotina profana se estabelece. A genética nos aguarda.";
                    directive = "Sobreviva 7 dias";
                    break;
                case OnboardingStep.Concluido:
                    OnObjectiveUpdated?.Invoke("", ""); 
                    return;
            }

            OnObjectiveUpdated?.Invoke(lore, directive);
        }

        public void Dispose()
        {
            if (_expeditions != null) _expeditions.OnExpeditionStarted -= HandleExpeditionStarted;
            if (_clock != null) _clock.OnDayEnded -= HandleDayEnded;
            if (_training != null) _training.OnBaseTrainingExecuted -= HandleTrainingExecuted;
            if (_progression != null) _progression.OnMilestoneReached -= HandleMilestoneReached;
        }
    }
}