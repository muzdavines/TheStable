using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Loops
{

    public class PR_IteratorLoop : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Iterator (Loop)"; }
        public override string GetNodeTooltipDescription { get { return "Running loop iteration"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Logic; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.8f, 0.55f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(188, 131); } }
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

        [Port(EPortPinType.Input, EPortNameDisplay.Default, 1)] public IntPort Iterations;
        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public IntPort IterationIndex;
        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "Stop (Break)")] public BoolPort Stop;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Iterations.TriggerReadPort(true);
            int targetIters = Iterations.GetInputValue;

            for (int c = 0; c < targetIters; c++)
            {
                Stop.TriggerReadPort(true);
                if (Stop.GetInputValue == true) break;

                IterationIndex.Value = c;
                CallOtherExecutionWithConnector(1, print);
            }
        }

    }
}