using FIMSpace.Graph;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.BuildSetup
{

    public class PR_IterateFields : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Iterate Fields" : "Loop Iterate Fields"; }
        public override string GetNodeTooltipDescription { get { return "Run loop iteration through every active field in build. If input left empty, then iterationg through all fields!"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(186, 172); } }

        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override int OutputConnectorsCount { get { return 2; } }
        public override int HotOutputConnectionIndex { get { return 1; } }
        public override int AllowedOutputConnectionIndex { get { return 0; } }
        public override string GetOutputHelperText(int outputId = 0)
        {
            if (outputId == 0) return "Finish";
            return "Iteration";
        }


        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Fields to iterate")] public PGGPlannerPort FieldsToIterate;
        [Tooltip("Getting all instances of the field if used multiple instances")]
        public bool GetDuplicates = true;
        [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Iteration Field")] public PGGPlannerPort IterationField;
        [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Iteration Index")] public IntPort IterationIndex;
        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Stop (break)")] public BoolPort Stop;


        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            if (FieldsToIterate.PortState() != EPortPinState.Connected)
            {
                if (ParentPlanner == null) return;
                var build = ParentPlanner.ParentBuildPlanner;
                if (build == null) return;

                IterateList(build.BasePlanners, print, false);
                return;
            }

            FieldsToIterate.TriggerReadPort(true);
            List<FieldPlanner> planners = GetPlannersFromPort(FieldsToIterate, false, true, true);
            IterateList(planners, print);
        }


        void IterateList(List<FieldPlanner> planners, PlanGenerationPrint print, bool iterateSelf = true)
        {
            if (planners == null) return;
            if (planners.Count == 0) return;

            int totalIter = 0;
            for (int c = 0; c < planners.Count; c++)
            {
                Stop.TriggerReadPort(true);
                if (Stop.GetInputValue == true) break;

                if (planners[c].DisableWholePlanner) continue;

                if (planners[c].Discarded == false) IterationField.SetIDsOfPlanner(planners[c]);
                IterationIndex.Value = totalIter;
                CallOtherExecutionWithConnector(1, print);

                if (GetDuplicates)
                {
                    var dups = planners[c].GetDuplicatesPlannersList();
                    if (dups != null)
                        for (int d = 0; d < dups.Count; d++)
                        {
                            totalIter += 1;
                            if (dups[d].Discarded) continue;
                            IterationIndex.Value = totalIter;
                            IterationField.SetIDsOfPlanner(dups[d]);
                            CallOtherExecutionWithConnector(1, print);
                        }
                }

                totalIter += 1;
            }

        }


    }
}