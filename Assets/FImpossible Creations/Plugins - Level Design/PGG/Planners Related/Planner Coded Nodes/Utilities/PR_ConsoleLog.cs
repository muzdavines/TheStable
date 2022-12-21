using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Utilities
{

    public class PR_ConsoleLog : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Console Log"; }
        public override string GetNodeTooltipDescription { get { return "(This node will break async generation)\nJust calling Debug.Log(your message) to display it inside unity editor console"; } }
        public override Color GetNodeColor() { return new Color(0.4f, 0.4f, 0.4f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(180, 92); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Debug; } }

        [Port(EPortPinType.Input, EPortNameDisplay.HideName, 1)] public PGGStringPort Message;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Message.TriggerReadPort(true);

            var planner = FieldPlanner.CurrentGraphExecutingPlanner;
            
            if ( planner == null)
            {
                UnityEngine.Debug.Log(Message.GetInputValue);
                return;
            }

            if (planner.IndexOfDuplicate < 0)
                UnityEngine.Debug.Log("[" + planner.IndexOnPreset + "] " + Message.GetInputValue);
            else
                UnityEngine.Debug.Log("[" + planner.IndexOnPreset + "] Dupl[" + planner.IndexOfDuplicate + "] " + Message.GetInputValue);
        }


    }
}