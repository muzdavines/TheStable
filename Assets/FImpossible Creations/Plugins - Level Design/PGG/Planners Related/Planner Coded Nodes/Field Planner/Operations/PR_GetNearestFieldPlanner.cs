using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Operations
{

    public class PR_GetNearestFieldPlanner : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return (wasCreated && IsFoldable) ? "   Nearest Field Planner" : "Nearest Field Planner"; }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(210, _EditorFoldout ? 144 : 104); } }
        public override bool IsFoldable { get { return Measure == EMeasureMode.ByNearestCell; } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Tooltip("Measure mode for finding nearest field")]
        public EMeasureMode Measure = EMeasureMode.ByNearestCell;

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default)] public PGGStringPort Tagged;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGPlannerPort Planner;

        [HideInInspector]
        [Port(EPortPinType.Output)]
        [Tooltip("Nearest cell of current field")]
        public PGGCellPort MyNearestCell;
        [HideInInspector]
        [Port(EPortPinType.Output)]
        [Tooltip("Nearest contact cell of the nearest found field")]
        public PGGCellPort OtherNearestCell;

        public enum EMeasureMode
        {
            [Tooltip("'By Nearest Cell' mode is most performance heavy!")]
            ByNearestCell, ByOrigin, ByBoundsCenter
        }


        private FieldCell latestNearestCell = null;
        private FieldCell latestOtherNearestCell = null;
        private FieldCell targetNearestCell = null;
        private FieldCell targetOtherNearestCell = null;
        public override void OnStartReadingNode()
        {
            Planner.SetIDsOfPlanner(null);
            latestNearestCell = null;
            MyNearestCell.Clear();
            OtherNearestCell.Clear();

            FieldPlanner myPlanner = CurrentExecutingPlanner;
            if (myPlanner == null) return;

            CheckerField3D myChecker = myPlanner.LatestChecker;
            if (myChecker == null) return;

            BuildPlannerPreset planner = myPlanner.ParentBuildPlanner;
            if (planner == null) return;

            if (planner.BasePlanners.Count == 0) return;

            System.Collections.Generic.List<FieldPlanner> planners = planner.CollectAllAvailablePlanners(true, true);

            string tagged = Tagged.GetInputValue;

            FieldPlanner nearest = null;
            float nearestDist = float.MaxValue;

            if (string.IsNullOrEmpty(tagged)) // All available fields
            {
                for (int i = 0; i < planners.Count; i++)
                {
                    FieldPlanner plan = planners[i];
                    if (plan == myPlanner) continue;

                    float dist = MeasureDistance(myChecker, plan.LatestChecker);

                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = plan;
                    }
                }
            }
            else // Just fields with certain tags
            {
                for (int i = 0; i < planners.Count; i++)
                {
                    FieldPlanner plan = planners[i];
                    if (plan == myPlanner) continue;
                    if (plan.tag != tagged) continue;

                    float dist = MeasureDistance(myChecker, plan.LatestChecker);
                    if (dist < nearestDist)
                    {
                        targetNearestCell = latestNearestCell;
                        targetOtherNearestCell = latestOtherNearestCell;
                        nearestDist = dist;
                        nearest = plan;
                    }
                }
            }

            if (nearest != null)
            {
                if (Measure == EMeasureMode.ByNearestCell)
                {
                    MyNearestCell.ProvideFullCellData(targetNearestCell, myChecker, myPlanner.LatestResult);
                    OtherNearestCell.ProvideFullCellData(targetOtherNearestCell, nearest.LatestChecker, nearest.LatestResult);
                }

                Planner.SetIDsOfPlanner(nearest);
            }
        }

        float MeasureDistance(CheckerField3D from, CheckerField3D to)
        {
            if (Measure == EMeasureMode.ByNearestCell)
            {
                FieldCell nearest = from.GetNearestCellTo(to);
                latestNearestCell = nearest;
                FieldCell otherNearest = from._nearestCellOtherField;
                latestOtherNearestCell = otherNearest;

                if (FGenerators.NotNull(nearest) && FGenerators.NotNull(otherNearest))
                {
                    return Vector3.Distance(from.GetWorldPos(nearest), to.GetWorldPos(otherNearest));
                }
            }
            else if (Measure == EMeasureMode.ByOrigin)
            {
                return Vector3.Distance(from.RootPosition, to.RootPosition);
            }
            else if (Measure == EMeasureMode.ByBoundsCenter)
            {
                //UnityEngine.Debug.DrawLine(from.GetFullBoundsWorldSpace().center, to.GetFullBoundsWorldSpace().center, Color.green, 1.01f);
                return Vector3.Distance(from.GetFullBoundsWorldSpace().center, to.GetFullBoundsWorldSpace().center);
            }

            return float.MaxValue;
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("Tagged");
            SerializedProperty s = sp.Copy();
            EditorGUILayout.PropertyField(sp, GUILayout.Width(NodeSize.x - 89));
            GUILayout.Space(-20); s.Next(false);
            EditorGUILayout.PropertyField(s);

            if (Measure == EMeasureMode.ByNearestCell)
            {
                if (_EditorFoldout)
                {
                    s.Next(false);
                    EditorGUILayout.PropertyField(s); s.Next(false);
                    EditorGUILayout.PropertyField(s);
                }
            }
            else
            {
                _EditorFoldout = false;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("Found Planner Index: [" + Planner.GetPlannerIndex() + "] [" + Planner.GetPlannerDuplicateIndex() + "]");
        }
#endif

    }
}