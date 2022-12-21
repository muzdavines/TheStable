using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Access
{

    public class PR_GetFieldFromInt : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Field" : "Get Field Planner by Number"; }
        public override string GetNodeTooltipDescription { get { return "Convert index number to field port (not forwarding duplicates)"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(156, 84); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue)] public IntPort ID;
        [Port(EPortPinType.Output, true)] public PGGPlannerPort Planner;

        public override void OnStartReadingNode()
        {
            base.OnStartReadingNode();
            ID.TriggerReadPort(true);
            Planner.UniquePlannerID = ID.GetInputValue;
            Planner.DuplicatePlannerID = 0;
        }

#if UNITY_EDITOR
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            SerializedProperty sp = baseSerializedObject.FindProperty("ID");
            EditorGUILayout.PropertyField(sp, GUILayout.Width(NodeSize.x - 50));

            if (ID.BaseConnection == null)
            {
                var r = GUILayoutUtility.GetLastRect();
                r.position += new Vector2(40, 0);
                r.width -= 40;
                GUI.Label(r, "(Self)");
            }

            sp.Next(false);
            GUILayout.Space(-20);
            EditorGUILayout.PropertyField(sp);

            //base.Editor_OnNodeBodyGUI(setup);
        }
#endif

    }
}