using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Transforming
{

    public class PR_SetFieldOrigin : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Set Field Origin" : "Set Field Origin"; }
        public override string GetNodeTooltipDescription { get { return "Change Field origin point which is very important for rotating field"; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.72f, 0.9f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(190, _EditorFoldout ? 104 : 86); } }
        public override bool IsFoldable { get { return true; } }

        [Port(EPortPinType.Input, EPortNameDisplay.HideName, EPortValueDisplay.Default)] public PGGVector3Port LocalOrigin;

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default)] [Tooltip("Using self if no input")] public PGGPlannerPort Planner;
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            LocalOrigin.TriggerReadPort();

            FieldPlanner planner = GetPlannerFromPort(Planner);
            object val = LocalOrigin.GetPortValue;

            if (val == null || LocalOrigin.Connections.Count == 0)
            {
                planner.LatestChecker.CenterizeOrigin();
            }
            else if (val is Vector3)
            {
                planner.LatestChecker.ChangeOrigin((Vector3)val);
            }

            if (Debugging)
            {
                if (val == null || LocalOrigin.Connections.Count == 0)
                    DebuggingInfo = "Centering origin";
                else
                    DebuggingInfo = "Setting origin to " + val;

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

            if (LocalOrigin.Connections.Count == 0)
            {
                GUILayout.Space(-8);
                EditorGUILayout.LabelField("No Connection - Centering", EditorStyles.centeredGreyMiniLabel);
            }
        }

#endif

    }
}