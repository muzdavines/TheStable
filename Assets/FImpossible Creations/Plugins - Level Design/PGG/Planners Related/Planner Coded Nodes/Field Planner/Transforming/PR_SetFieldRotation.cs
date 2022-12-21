using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Transforming
{

    public class PR_SetFieldRotation : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Set Field Rotation" : "Set Field Rotation"; }
        public override string GetNodeTooltipDescription { get { return "Rotate whole field accordingly to planner's origin point"; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.72f, 0.9f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(190, _EditorFoldout ? 104 : 86); } }
        public override bool IsFoldable { get { return true; } }

        [Port(EPortPinType.Input, EPortNameDisplay.HideName, EPortValueDisplay.Default)] public PGGVector3Port Angles;

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default)] [Tooltip("Using self if no input")] public PGGPlannerPort Planner;
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Angles.TriggerReadPort();

            FieldPlanner planner = GetPlannerFromPort(Planner);
            object val = Angles.GetPortValue;

            if (val == null)
            {
            }
            else if (val is Vector3)
            {
                Vector3 newRotation = (Vector3)val;

                //if (planner.RoundPosition) newRotation = newRotation.V3toV3Int();

                planner.LatestResult.Checker.RootRotation = Quaternion.Euler(newRotation);
            }

            if (Debugging)
            {
                DebuggingInfo = "Setting rotation to " + val;
                print._debugLatestExecuted = planner.LatestResult.Checker;
            }
        }


#if UNITY_EDITOR

        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                baseSerializedObject.Update();
                if (sp == null) sp = baseSerializedObject.FindProperty("Planner");
                UnityEditor.EditorGUILayout.PropertyField(sp);
                baseSerializedObject.ApplyModifiedProperties();
            }
        }

#endif

    }
}