using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Transforming
{

    public class PR_SetFieldPosition : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Set Field Position"; }
        public override string GetNodeTooltipDescription { get { return "Change field origin position"; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.72f, 0.9f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 220 : 200, _EditorFoldout ? 122 : 82); } }
        public override bool IsFoldable { get { return true; } }

        [Port(EPortPinType.Input, EPortNameDisplay.HideName, EPortValueDisplay.Default, 1)] public PGGVector3Port Position;
        [Tooltip("Multiply setted value by field's cell size")]
        [HideInInspector] public bool CellSize = false;
        [HideInInspector] [Port(EPortPinType.Input, 1)] [Tooltip("Using self if no input")] public PGGPlannerPort Planner;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Position.TriggerReadPort();

            CheckerField3D checker = GetCheckerFromPort(Planner);
            //FieldPlanner planner = GetPlannerFromPort(Planner);
            object val = Position.GetPortValue;

            if (val == null)
            {
                //UnityEngine.Debug.Log("null!");
            }
            else if (val is Vector3)
            {
                Vector3 newPosition = (Vector3)val;
                if (CellSize)
                    if (CurrentExecutingPlanner != null)
                        if (CurrentExecutingPlanner.LatestResult != null)
                            if (CurrentExecutingPlanner.LatestResult.Checker != null)
                                newPosition = CurrentExecutingPlanner.LatestResult.Checker.ScaleV3(newPosition);

                checker.RootPosition = (newPosition);
                if (CurrentExecutingPlanner.RoundToScale) checker.RoundRootPositionToScale();
                //planner.SetCheckerWorldPosition(newPosition);
            }

            //if (newResult.ParentFieldPlanner.AlwaysPushOut)
            //{
            //    for (int i = 0; i < print.PlannerResults.Count; i++)
            //    {
            //        if (print.PlannerResults[i].ParentFieldPlanner == newResult.ParentFieldPlanner) continue;
            //        if (print.PlannerResults[i].ParentFieldPlanner.DisableCollision) continue;

            //        newResult.Checker.PushOutOfCollision(print.PlannerResults[i].Checker);
            //    }
            //}

            if (Debugging)
            {
                DebuggingInfo = "Setting position to " + val;
                print._debugLatestExecuted = checker;
            }
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (!_EditorFoldout) return;

            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("CellSize");
            SerializedProperty spc = sp.Copy();
            EditorGUILayout.PropertyField(spc); spc.Next(false);
            if (_EditorFoldout) EditorGUILayout.PropertyField(spc);
            EditorGUIUtility.labelWidth = 0;
            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}