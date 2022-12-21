using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Planning
{
    public partial class PlannerAsyncManager
    {
        public BuildPlannerPreset Planner = null;
        private List<PlannerAsyncInstance> Instances = new List<PlannerAsyncInstance>();
        public List<PlannerAsyncInstance> GetAsyncInstances { get { return Instances; } }

        public void SetOnDuplicatesGeneratedCallback(int planner, System.Action<FieldPlanner> instancedDuplicatesCallback)
        {
            if (planner >= Instances.Count) return;
            Instances[planner].OnDuplicatesGenerated = instancedDuplicatesCallback;
        }
       
        public bool IsGeneratingDone = false;
        public bool IsGenerating = false;
        public bool ComputeUsingAsync = true;

        public int TargetSeed;
        bool stop = false;

        /// <summary>
        /// Preparing for generating
        /// </summary>
        public PlannerAsyncManager(BuildPlannerPreset plan, int seed)
        {
            Planner = plan;
            TargetSeed = seed;
            ComputeUsingAsync = plan.AsyncGenerating;

            for (int i = 0; i < plan.BasePlanners.Count; i++)
            {
                Instances.Add(new PlannerAsyncInstance(this, plan.BasePlanners[i]));
            }

            IsGenerating = true;
            IsGeneratingDone = false;

            // Prepare and validate -------------------
            FGenerators.SetSeed(seed);
            PGGUtils.CheckForNulls(Planner.BasePlanners);


            #region Reset and Compute desired iterations for progress bar

            _targetPlannerIterations = 1f;

            for (int i = 0; i < plan.BasePlanners.Count; i++)
            {
                if (plan.BasePlanners[i].DisableWholePlanner) continue;

                plan.BasePlanners[i].ResetForGenerating();
                _targetPlannerIterations += 2;
                _targetPlannerIterations += plan.BasePlanners[i].Duplicates * 2;
            }

            if (_targetPlannerIterations > 0f)
                _progr_step = 1f / _targetPlannerIterations;
            else
                _progr_step = 1f;


            GeneratingProgress = 0f;
            SmoothGeneratingProgress = 0f;
            _progr_sd_smooth = 0f;

            #endregion

            // Call procedures async with UpdateGeneratingProgress() -------------------
        }

        public void UpdateGenerating(float dt = 0f)
        {
            if (stop) return;

            if (IsGenerating)
            {
                if (IsGeneratingDone == false)
                {
                    //UnityEngine.Debug.Log("UpdateGenerating " + _currentComputingI + " stage = " + _generatingStage + "  dup = " + _currentComputingDuplicI);
                    UpdateGeneratingProgress();
                }
                else // Complete generating
                {
                    IsGenerating = false;
                }
            }

            #region Smooth progress

            if (dt == 0f)
            {
                dt = 1f / 60f;
                //SmoothGeneratingProgress = GeneratingProgress;
            }
            //else
            {
                if (_progr_lastElapsedIterations != _elapsedIterations) _progr_fake_progr = 0f;
                if (_progr_fake_progr < 1f) _progr_fake_progr += dt * 1f;
                SmoothGeneratingProgress = Mathf.SmoothDamp(SmoothGeneratingProgress, _elapsedIterations * _progr_step + _progr_fake_progr * _progr_step, ref _progr_sd_smooth, 0.04f, 10000f, dt);
            }

            #endregion

        }

        public void Remove()
        {
            stop = true;

            //for (int i = 0; i < Instances.Count; i++) Instances[i].Stop();
        }

    }
}
