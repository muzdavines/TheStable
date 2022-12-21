using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors
{

    public class PR_Cross : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Cross" : "Cross (X to Z axis \\ Z to X)"; }
        public override string GetNodeTooltipDescription { get { return "Cross product for the provided vector values"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.45f, 0.8f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(240, 128); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [Port(EPortPinType.Input, "In")] public PGGVector3Port InVal;
        [Port(EPortPinType.Input)] public PGGVector3Port CrossRhs;
        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public PGGVector3Port Out;

        public override void OnCreated()
        {
            CrossRhs.Value = new Vector3(0, 1, 0);
            base.OnCreated();
        }

        public override void OnStartReadingNode()
        {
            InVal.TriggerReadPort(true);
            CrossRhs.TriggerReadPort(true);
            Out.Value = Vector3.Cross(InVal.GetInputValue, CrossRhs.GetInputValue);
        }

    }
}