using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Values
{

    public class PR_BoolTrigger : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? " Trigger" : "Bool Value Trigger"; }
        public override string GetNodeTooltipDescription { get { return "Sets true value on execute, changes to false after being read"; } }
        public override Color GetNodeColor() { return new Color(0.975f, 0.4f, 0.4f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2( 168,80); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override bool IsFoldable { get { return true; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public BoolPort Trigger;

        public override void PreGeneratePrepare()
        {
            Trigger.Value = false;
        }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Trigger.Value = true;
            wasTrig = false;
        }

        bool wasTrig = false;
        public override void OnStartReadingNode()
        {
            if (wasTrig) { Trigger.Value = false; }
            wasTrig = true;
        }

    }
}