using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Access
{

    public class PR_GetFieldParameter : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Parameter" : "Get Field Planner Parameter"; }
        public override string GetNodeTooltipDescription { get { return "Returning some parameter of choosed field"; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(186, 84); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public enum EPlannerParam
        {
            IndexInPlan, Tag, CellsCount, CellScale, FieldEulerAngles_Rotation, InstancesCount//, BoundsWidth, BoundsDepth, BoundsHeight
        }

        [Port(EPortPinType.Input, true)] public PGGPlannerPort Planner;
        public EPlannerParam OutputParam = EPlannerParam.IndexInPlan;

        [Port(EPortPinType.Output, true)] public PGGUniversalPort OutParam;

        public override void OnStartReadingNode()
        {
            base.OnStartReadingNode();

            FieldPlanner planner = GetPlannerFromPort(Planner);

            if (planner)
            {
                switch (OutputParam)
                {
                    case EPlannerParam.IndexInPlan: OutParam.Variable.SetValue(planner.IndexOnPreset); break;
                    case EPlannerParam.Tag: OutParam.Variable.SetValue(planner.tag); break;
                    case EPlannerParam.CellsCount: OutParam.Variable.SetValue(planner.LatestChecker.ChildPositionsCount); break;
                    case EPlannerParam.CellScale: OutParam.Variable.SetValue(planner.GetScale); break;
                    case EPlannerParam.FieldEulerAngles_Rotation: if (planner.LatestResult != null) if (planner.LatestResult.Checker != null) OutParam.Variable.SetValue(planner.LatestResult.Checker.RootRotation.eulerAngles); break;
                   
                    case EPlannerParam.InstancesCount: 
                        if (planner.LatestResult != null) 
                            if (planner.IsDuplicate) { OutParam.Variable.SetValue(planner.DuplicateParent.Instances); }
                            else 
                            { OutParam.Variable.SetValue(planner.Instances); } break;
                }
            }
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            if (sp == null) sp = baseSerializedObject.FindProperty("Planner");

            //UnityEditor.EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp, GUILayout.Width(20));
            OutputParam = (EPlannerParam)UnityEditor.EditorGUILayout.EnumPopup(OutputParam, GUILayout.Width(NodeSize.x - 106));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(-20);
            NodePortBase port = null;

            OutParam.AllowDragWire = false;
            port = OutParam;
            EditorGUILayout.PropertyField(baseSerializedObject.FindProperty("OutParam"));

            if (port != null) port.AllowDragWire = true;

        }
#endif

    }
}