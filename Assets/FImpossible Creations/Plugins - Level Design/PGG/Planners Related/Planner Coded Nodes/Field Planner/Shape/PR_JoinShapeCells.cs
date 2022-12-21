using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Shape
{

    public class PR_JoinShapeCells : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "   Join Field-Shape Cells" : "Join Field-Shape Cells"; }
        public override string GetNodeTooltipDescription { get { return "Joining cells of one Field Shape with another."; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return true; } }

        public override Vector2 NodeSize { get { return new Vector2(210, _EditorFoldout ? 104 : 86); } }

        [Port(EPortPinType.Input, 1)] public PGGPlannerPort JoinWith;
        [HideInInspector][Port(EPortPinType.Input, 1)] public PGGPlannerPort ApplyTo;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.CellsManipulation; } }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            ApplyTo.TriggerReadPort(true);
            JoinWith.TriggerReadPort(true);

            FieldPlanner plan = GetPlannerFromPort(ApplyTo, false);
            CheckerField3D myChe = ApplyTo.GetInputCheckerSafe;
            if (plan) myChe = plan.LatestResult.Checker;
            if (myChe == null) { return; }

            CheckerField3D oChe = JoinWith.GetInputCheckerSafe;
            if (oChe == null) { return; }

            myChe.Join(oChe);

            if (plan) plan.LatestResult.Checker = myChe;

            #region Debugging Gizmos
#if UNITY_EDITOR
            if (Debugging)
            {
                DebuggingInfo = "Joining fields cells";
                CheckerField3D myChec = myChe;
                CheckerField3D oChec = oChe;

                DebuggingGizmoEvent = () =>
                {
                    Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                    for (int i = 0; i < myChec.ChildPositionsCount; i++)
                        Gizmos.DrawCube(myChec.GetWorldPos(i), myChe.RootScale);
                    Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
                    for (int i = 0; i < oChec.ChildPositionsCount; i++)
                        Gizmos.DrawCube(oChec.GetWorldPos(i), oChec.RootScale);
                };
            }
#endif
            #endregion

        }

#if UNITY_EDITOR

        private UnityEditor.SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                ApplyTo.AllowDragWire = true;
                GUILayout.Space(1);

                if (sp == null) sp = baseSerializedObject.FindProperty("ApplyTo");
                UnityEditor.SerializedProperty scp = sp.Copy();
                UnityEditor.EditorGUILayout.PropertyField(scp);
            }
            else
            {
                ApplyTo.AllowDragWire = false;
            }
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            CheckerField3D chA = ApplyTo.GetInputCheckerSafe;
            if (chA != null) GUILayout.Label("Planner Cells: " + chA.ChildPositionsCount);

            CheckerField3D chB = JoinWith.GetInputCheckerSafe;
            if (chB != null) GUILayout.Label("JoinWith Cells: " + chB.ChildPositionsCount);
        }

#endif

    }
}