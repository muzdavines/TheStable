using FIMSpace.Generating.Planning.PlannerNodes;


namespace FIMSpace.Generating.Planning
{

    public partial class PlannerFunctionNode
    {

        void CallExecution(PlannerRuleBase start, PlanGenerationPrint print, PlannerResult newResult)
        {
            start.Execute(print, newResult);

            if (ParentPlanner)
            {

                #region Backup

                //if (ParentPlanner.IndexOnPrint != -1)
                //{
                //    print.PlannerResults[ParentPlanner.IndexOnPrint] = newResult;

                //    if (ParentPlanner.DuplicateParent == null)
                //    {
                //        print.PlannerResults[ParentPlanner.IndexOnPrint] = newResult;
                //    }
                //    else
                //    {
                //        if (print.PlannerResults[ParentPlanner.IndexOnPrint].DuplicateResults != null)
                //        {
                //            print.PlannerResults[ParentPlanner.IndexOnPrint].DuplicateResults[ParentPlanner.IndexOfDuplicate] = newResult;
                //        }
                //        else
                //        {
                //            UnityEngine.Debug.Log("ni ma2");
                //        }
                //    }
                //}

                #endregion

                if (ParentPlanner.ParentBuildPlanner)
                    if (ParentPlanner.ParentBuildPlanner.OnIteractionCallback != null)
                        ParentPlanner.ParentBuildPlanner.OnIteractionCallback.Invoke(print);
            }

            //UnityEngine.Debug.Log("func conns = " + start.OutputConnections.Count);

            if (start.FirstOutputConnection == null) return;

            if (start.AllowedOutputConnectionIndex > -1)
            {
                for (int c = 0; c < start.OutputConnections.Count; c++)
                {
                    if (start.OutputConnections[c].ConnectionFrom_AlternativeID != start.AllowedOutputConnectionIndex) continue;

                    CallExecution(
                    start.OutputConnections[c].GetOther(start) as PlannerRuleBase,
                    print, newResult);
                }
            }
            else
            {
                for (int c = 0; c < start.OutputConnections.Count; c++)
                {
                    CallExecution(
                    start.OutputConnections[c].GetOther(start) as PlannerRuleBase,
                    print, newResult);
                }
            }

            //operation = operation.FirstOutputConnection as PlannerRuleBase;
        }
        //

    }
}