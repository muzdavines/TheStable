using FIMSpace.FTex;

namespace FIMSpace.Generating.Planning
{
    public partial class PlannerAsyncManager
    {
        public class FieldPlannerProceduresThread : FThread
        {
            FieldPlanner planner;
            PlannerAsyncManager manager;
            bool isPostProcedures;
            public FieldPlannerProceduresThread(PlannerAsyncManager manager, FieldPlanner planner, bool postProcedures = false)
            {
                this.manager = manager;
                this.planner = planner;
                isPostProcedures = postProcedures;
            }

            protected override void ThreadOperations()
            {
                if (!isPostProcedures)
                    planner.RunStartProcedures(manager.Planner.LatestGenerated);
                else
                    planner.RunPostProcedures(manager.Planner.LatestGenerated);
            }
        }
    }
}
