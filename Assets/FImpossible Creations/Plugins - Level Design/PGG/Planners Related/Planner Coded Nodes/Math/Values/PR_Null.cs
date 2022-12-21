using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Values
{

    public class PR_Null : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Null Value"; }
        public override string GetNodeTooltipDescription { get { return "Just outputing null value"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get {  return new Vector2( 120, 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

         [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGUniversalPort Null;

        public override void OnCreated()
        {
            base.OnCreated();
            Null.Variable.ValueType = FieldVariable.EVarType.ProjectObject;
            Null.Variable.SetNullProjectValue();
        }

        public override void DONT_USE_IT_YET_OnReadPort(IFGraphPort port)
        {
            Null.Variable.ValueType = FieldVariable.EVarType.ProjectObject;
            Null.Variable.SetNullProjectValue();
            base.DONT_USE_IT_YET_OnReadPort(port);
        }

    }
}