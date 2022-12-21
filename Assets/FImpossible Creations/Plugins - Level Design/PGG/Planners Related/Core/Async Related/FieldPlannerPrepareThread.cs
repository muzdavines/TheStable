using FIMSpace.FTex;

namespace FIMSpace.Generating.Planning
{
    public partial class PlannerAsyncManager
    {
        public class FieldPlannerPrepareThread : FThread
        {
            FieldPlanner planner;
            PlannerAsyncManager manager;
            public FieldPlannerPrepareThread(PlannerAsyncManager manager, FieldPlanner planner)
            {
                this.manager = manager;
                this.planner = planner;
            }

            protected override void ThreadOperations()
            {
                planner.PrepareForGenerating(manager._currentComputingI, manager._currentPreparingI, manager.Planner.LatestGenerated);
            }
        }
    }
}
