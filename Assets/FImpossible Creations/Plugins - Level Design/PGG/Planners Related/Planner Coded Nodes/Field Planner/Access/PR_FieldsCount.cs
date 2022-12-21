using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Access
{

    public class PR_FieldsCount : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Fields Count" : "Get Total Fields Count"; }
        public override string GetNodeTooltipDescription { get { return "Getting count of Field Planners or instances"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(190, insOf ? 128 : 104); } }
        public override bool IsFoldable { get { return false; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public enum ECountOf
        {
            FieldPlanners, AllInstances, InstancesOf
        }

        public ECountOf CountOf = ECountOf.AllInstances;
        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public IntPort Count;
        [HideInInspector][Port(EPortPinType.Input)] public PGGPlannerPort InstancesOf;
        bool insOf { get { return CountOf == ECountOf.InstancesOf; } } // Shortcut

        public override void OnStartReadingNode()
        {
            if (CurrentExecutingPlanner == null) return;
            if (CurrentExecutingPlanner.ParentBuildPlanner == null) return;

            if (CountOf == ECountOf.FieldPlanners)
            {
                Count.Value = CurrentExecutingPlanner.ParentBuildPlanner.BasePlanners.Count;
            }
            else if (CountOf == ECountOf.AllInstances)
            {
                Count.Value = CurrentExecutingPlanner.ParentBuildPlanner.CountAllAvailablePlanners();
            }
            else if (CountOf == ECountOf.InstancesOf)
            {
                InstancesOf.TriggerReadPort(true);
                FieldPlanner planner = GetPlannerFromPort(InstancesOf, false);
                if (planner == null) return;
                Count.Value = planner.Instances;
            }
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (insOf)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("InstancesOf");
                EditorGUILayout.PropertyField(sp, true);
                InstancesOf.AllowDragWire = true;
            }
            else
            {
                InstancesOf.AllowDragWire = false;
            }
        }
#endif

    }
}