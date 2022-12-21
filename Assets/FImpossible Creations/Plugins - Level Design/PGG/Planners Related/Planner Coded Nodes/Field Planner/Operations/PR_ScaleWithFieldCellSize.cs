using FIMSpace.Graph;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Operations
{

    public class PR_ScaleWithFieldCellSize : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Cell Size Multiply" : "Multiply With Field Cell Size \\ Scale"; }
        public override string GetNodeTooltipDescription { get { return "Multiplying provided value with field grid single cell tile size"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(176, 81); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, 1, typeof(int))] public PGGUniversalPort InValA;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGUniversalPort OutVal;


        public override void OnStartReadingNode()
        {
            InValA.TriggerReadPort();
            var av = InValA.GetPortValueCall();

            if (av is float)
            {
                if (av is int) av = Convert.ToSingle(av);
            }

            InValA.Variable.SetValue(av);

            FieldPlanner exc = CurrentExecutingPlanner;
            if (exc == null) return;

            if (InValA.Connections.Count == 0)
            {
                OutVal.Variable.SetValue(exc.LatestResult.Checker.ScaleV3(Vector3.one));
                return;
            }

            if (av.GetType() == typeof(float))
            {
                OutVal.Variable.SetValue((float)exc.LatestResult.Checker.RootScale.x * (float)av);
            }
            else if (av.GetType() == typeof(int))
            {
                OutVal.Variable.SetValue(exc.LatestResult.Checker.RootScale.x * (int)av);
            }
            else if (av is Vector2)
            {
                OutVal.Variable.SetValue(exc.LatestResult.Checker.ScaleV3((Vector2)av));
            }
            else if (av is Vector3)
            {
                OutVal.Variable.SetValue(exc.LatestResult.Checker.ScaleV3((Vector3)av));
            }
            else
                return;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("OutVal");

            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(19);
            EditorGUILayout.PropertyField(sp, GUIContent.none);
            GUILayout.EndHorizontal();
        }


#endif
    }
}