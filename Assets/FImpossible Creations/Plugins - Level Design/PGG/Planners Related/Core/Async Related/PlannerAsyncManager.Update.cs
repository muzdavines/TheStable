
namespace FIMSpace.Generating.Planning
{
    public partial class PlannerAsyncManager
    {
        int _currentPreparingI = 0;
        int _currentComputingI = 0;
        int _currentComputingDuplicI = 0;
        float _elapsedIterations = 0;
        float _progr_lastElapsedIterations = 0;
        float _targetPlannerIterations = 1f;
        float _progr_step = 0.01f;

        /// <summary> 0 is prepare   1 are procedures   2 are post procedures </summary>
        int _generatingStage = 0;

        public float GeneratingProgress { get; private set; }
        public float SmoothGeneratingProgress { get; private set; }

        public void OverrideProgressDisplay(float progr)
        {
            GeneratingProgress = progr;
            UpdateGeneratingProgress();
        }

        float _progr_fake_progr = 0f;
        float _progr_sd_smooth = 0f;

        void UpdateGeneratingProgress()
        {

            if (_generatingStage >= 3) // Finish generating
            {
                CompleteAllGenerating();
                GeneratingProgress = 1f;
                IsGeneratingDone = true;
                return;
            }

            if (_currentComputingI >= Planner.BasePlanners.Count) // Switch to next stage
            {
                _generatingStage += 1;

                if ( _generatingStage == 2) // To Post Procedures
                {
                    Planner.GenerationIteration = 0;
                }

                _currentComputingI = 0;
                return;
            }
            else // Progress compute
            {
                #region Just for Progress Bar

                if (_targetPlannerIterations > 0f)
                    GeneratingProgress = _elapsedIterations / _targetPlannerIterations;
                else
                    GeneratingProgress = 0.1f;

                #endregion
            }

            FieldPlanner planner = Planner.BasePlanners[_currentComputingI];

            if (planner.DisableWholePlanner) // Skip
            {
                _currentComputingI += 1;
            }
            else
            {
                if (_generatingStage == 0)
                {
                    Instances[_currentComputingI].UpdateGeneratingStage_Prepare();
                }
                else if (_generatingStage == 1)
                {
                    Instances[_currentComputingI].UpdateGeneratingStage_Procedures();
                }
                else if (_generatingStage == 2)
                {
                    Instances[_currentComputingI].UpdateGeneratingStage_PostProcedures();
                }
            }
        }

        void CompleteAllGenerating()
        {
            for (int i = 0; i < Planner.BasePlanners.Count; i++)
            {
                Planner.BasePlanners[i].OnCompleateAllGenerating();
            }
        }

    }
}
