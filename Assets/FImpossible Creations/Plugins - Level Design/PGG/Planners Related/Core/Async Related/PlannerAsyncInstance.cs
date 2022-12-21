using System;

namespace FIMSpace.Generating.Planning
{
    public partial class PlannerAsyncManager
    {
        public class PlannerAsyncInstance
        {

            PlannerAsyncManager manager;
            FieldPlanner planner;


            public PlannerAsyncInstance(PlannerAsyncManager manager, FieldPlanner planner)
            {
                this.manager = manager;
                this.planner = planner;
            }



            FieldPlannerPrepareThread currentPrepareThread = null;
            FieldPlannerProceduresThread currentProceduresThread = null;

            public Action<FieldPlanner> OnDuplicatesGenerated = null;

            /// <summary> Prepare and refresh each field planner + prepare duplicates </summary>
            public void UpdateGeneratingStage_Prepare()
            {
                if (planner.PreparationWasStarted == false)
                {
                    planner.PrePrepareForGenerating(manager._currentComputingI, manager._currentPreparingI);
                    if (OnDuplicatesGenerated != null) OnDuplicatesGenerated.Invoke(planner);

                    if (manager.ComputeUsingAsync)
                    {
                        try
                        {
                            currentPrepareThread = new FieldPlannerPrepareThread(manager, planner);
                            currentPrepareThread.Start();
                        }
                        catch (System.Exception e)
                        {
                            manager.ComputeUsingAsync = false;
                            if (planner.ParentBuildPlanner) planner.ParentBuildPlanner.AsyncGenerating = false;
                            currentPrepareThread = null;
                            UnityEngine.Debug.Log("[Planner Generator Async Break]");
                            UnityEngine.Debug.LogException(e);
                            planner.PrepareForGenerating(manager._currentComputingI, manager._currentPreparingI, manager.Planner.LatestGenerated);
                        }
                    }
                    else
                    {
                        planner.PrepareForGenerating(manager._currentComputingI, manager._currentPreparingI, manager.Planner.LatestGenerated);
                    }
                }

                if (planner.PreparationDone)
                {
                    #region Progress Detection

                    if (planner.PreparationWasDoneFlag == false)
                    {
                        manager._elapsedIterations += 1f;
                        manager._currentPreparingI += 1;
                        planner.PreparationWasDoneFlag = true;
                        manager._currentComputingDuplicI = 0;
                    }

                    #endregion

                    // Progress forward to next planner preparation
                    manager._currentComputingI += 1;
                }
            }




            /// <summary> Run each field planner procedures </summary>
            public void UpdateGeneratingStage_Procedures()
            {

                if (planner.Discarded == false)
                {
                    if (planner.ExecutionWasStarted == false)
                    {
                        planner.PreRunProcedures(manager.Planner.LatestGenerated);

                        if (manager.ComputeUsingAsync)
                        {
                            currentProceduresThread = new FieldPlannerProceduresThread(manager, planner);
                            currentProceduresThread.Start();
                        }
                        else
                        {
                            planner.RunStartProcedures(manager.Planner.LatestGenerated);
                        }
                    }
                    else
                    {
                        if (manager.ComputeUsingAsync)
                            if (currentProceduresThread == null || currentProceduresThread.Failed)
                            {
                                manager.ComputeUsingAsync = false;
                                if (planner.ParentBuildPlanner) planner.ParentBuildPlanner.AsyncGenerating = false;
                                currentProceduresThread = null;
                                UnityEngine.Debug.Log("[Planner Generator] Error during executing async method - disabling async generation!");
                                planner.RunStartProcedures(manager.Planner.LatestGenerated);
                            }
                    }
                }

                if (planner.ExecutionDone || planner.Discarded) // Done
                {
                    #region Progress Detection

                    if (planner.ExecutionWasDoneFlag == false)
                    {
                        planner.ParentBuildPlanner.GenerationIteration += 1;
                        manager._elapsedIterations += 1f;
                        planner.ExecutionWasDoneFlag = true;
                        manager._currentComputingDuplicI = 0;
                    }

                    #endregion

                    bool duplDone = false;

                    #region Duplicates Execution

                    if (planner.GetDuplicatesPlannersList() != null)
                    {
                        var dupls = planner.GetDuplicatesPlannersList();
                        if (dupls.Count == 0)
                        {
                            duplDone = true;
                        }
                        else
                        {
                            if (manager._currentComputingDuplicI >= dupls.Count)
                            {
                                duplDone = true;
                            }
                            else
                            {
                                var duplPlan = dupls[manager._currentComputingDuplicI];

                                if (duplPlan.Discarded == false)
                                {
                                    if (duplPlan.WasExecuted == false)
                                    {
                                        duplPlan.PreRunProcedures(manager.Planner.LatestGenerated);

                                        if (manager.ComputeUsingAsync)
                                        {
                                            currentProceduresThread = new FieldPlannerProceduresThread(manager, duplPlan);
                                            currentProceduresThread.Start();
                                        }
                                        else
                                        {
                                            duplPlan.RunStartProcedures(manager.Planner.LatestGenerated);
                                        }
                                    }
                                    else
                                    {
                                        if (manager.ComputeUsingAsync)
                                            if (currentProceduresThread == null || currentProceduresThread.Failed)
                                            {
                                                manager.ComputeUsingAsync = false;
                                                if (planner.ParentBuildPlanner) duplPlan.ParentBuildPlanner.AsyncGenerating = false;
                                                currentProceduresThread = null;
                                                UnityEngine.Debug.Log("[Planner Generator] Error during executing async method - disabling async generation!");
                                                duplPlan.RunStartProcedures(manager.Planner.LatestGenerated);
                                            }
                                    }
                                }

                                if (duplPlan.ExecutionDone || duplPlan.Discarded)
                                {
                                    planner.ParentBuildPlanner.GenerationIteration += 1;
                                    manager._elapsedIterations += 1;
                                    manager._currentComputingDuplicI += 1;
                                    if (manager._currentComputingDuplicI >= dupls.Count) duplDone = true;
                                }
                            }
                        }
                    }
                    else duplDone = true;

                    #endregion

                    if (duplDone) // Progress forward to next planner execution
                        manager._currentComputingI += 1;
                }
            }

            internal void Stop()
            {
                if (currentPrepareThread != null)
                {
                    if (currentPrepareThread.IsDone == false) currentPrepareThread.Abort();
                }

                if (currentProceduresThread != null)
                {
                    if (currentProceduresThread.IsDone == false) currentProceduresThread.Abort();
                }
            }


            /// <summary> Run each field planner procedures </summary>
            public void UpdateGeneratingStage_PostProcedures()
            {
                if (planner.Discarded == false)
                {
                    if (planner.PostExecutionWasStarted == false)
                    {
                        planner.PreRunPostProcedures(manager.Planner.LatestGenerated);

                        if (manager.ComputeUsingAsync)
                        {
                            currentProceduresThread = new FieldPlannerProceduresThread(manager, planner, true);
                            currentProceduresThread.Start();
                        }
                        else
                        {
                            planner.RunPostProcedures(manager.Planner.LatestGenerated);
                        }
                    }
                    else
                    {
                        if (manager.ComputeUsingAsync)
                            if (currentProceduresThread == null || currentProceduresThread.Failed)
                            {
                                manager.ComputeUsingAsync = false;
                                if (planner.ParentBuildPlanner) planner.ParentBuildPlanner.AsyncGenerating = false;
                                currentProceduresThread = null;
                                UnityEngine.Debug.Log("[Planner Generator] Error during executing async method - disabling async generation!");
                                planner.RunPostProcedures(manager.Planner.LatestGenerated);
                            }
                    }
                }

                if (planner.PostExecutionDone || planner.Discarded) // Done
                {
                    #region Progress Detection

                    if (planner.PostExecutionDoneFlag == false)
                    {
                        planner.ParentBuildPlanner.GenerationIteration += 1;
                        manager._elapsedIterations += 1f;
                        planner.PostExecutionDoneFlag = true;
                        manager._currentComputingDuplicI = 0;
                    }

                    #endregion

                    bool duplDone = false;

                    #region Duplicates Execution

                    if (planner.GetDuplicatesPlannersList() != null)
                    {
                        var dupls = planner.GetDuplicatesPlannersList();
                        if (dupls.Count == 0)
                        {
                            duplDone = true;
                        }
                        else
                        {
                            if (manager._currentComputingDuplicI >= dupls.Count)
                            {
                                duplDone = true;
                            }
                            else
                            {
                                var duplPlan = dupls[manager._currentComputingDuplicI];

                                if (duplPlan.Discarded == false)
                                {
                                    if (duplPlan.PostExecutionWasStarted == false)
                                    {
                                        duplPlan.PreRunPostProcedures(manager.Planner.LatestGenerated);

                                        if (manager.ComputeUsingAsync)
                                        {
                                            currentProceduresThread = new FieldPlannerProceduresThread(manager, duplPlan, true);
                                            currentProceduresThread.Start();
                                        }
                                        else
                                        {
                                            duplPlan.RunPostProcedures(manager.Planner.LatestGenerated);
                                        }
                                    }
                                    else
                                    {
                                        if (manager.ComputeUsingAsync)
                                            if (currentProceduresThread == null || currentProceduresThread.Failed)
                                            {
                                                manager.ComputeUsingAsync = false;
                                                UnityEngine.Debug.Log("[Planner Generator] Error during executing async method - disabling async generation!");
                                                if (planner.ParentBuildPlanner) duplPlan.ParentBuildPlanner.AsyncGenerating = false;
                                                currentProceduresThread = null;
                                                duplPlan.RunPostProcedures(manager.Planner.LatestGenerated);
                                            }
                                    }
                                }

                                if (duplPlan.PostExecutionDone || duplPlan.Discarded)
                                {
                                    planner.ParentBuildPlanner.GenerationIteration += 1;
                                    manager._elapsedIterations += 1;
                                    manager._currentComputingDuplicI += 1;
                                    if (manager._currentComputingDuplicI >= dupls.Count) duplDone = true;
                                }
                            }
                        }
                    }
                    else duplDone = true;

                    #endregion

                    if (duplDone) // Progress forward to next planner execution
                        manager._currentComputingI += 1;
                }
            }



        }

    }

}
