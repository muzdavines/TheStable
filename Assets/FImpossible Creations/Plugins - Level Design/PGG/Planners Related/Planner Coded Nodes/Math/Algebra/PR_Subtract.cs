using FIMSpace.Graph;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Algebra
{

    public class PR_Subtract : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Subtract"; }
        public override string GetNodeTooltipDescription { get { return "Basic subtract operation.\nWhen using field ports it will remove fields from provided list of fields"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(120, 100); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }


        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "A", 1, typeof(int))] public PGGUniversalPort InValA;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue, "B", 1, typeof(int))] public PGGUniversalPort InValB;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGUniversalPort OutVal;

        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGPlannerPort OutPlanners;
        [SerializeField] [HideInInspector] private bool plannerPort = false;
        bool wasReading = false;

        public override void OnStartReadingNode()
        {
            InValA.TriggerReadPort(true);
            InValB.TriggerReadPort(true);


            #region planners support

            IFGraphPort plPort = InValA.GetConnectedPortOfType(typeof(PGGPlannerPort));
            if (plPort != null)
            {
                PGGPlannerPort aPort = plPort as PGGPlannerPort;
                PGGPlannerPort bPort = null;

                IFGraphPort _bPort = InValB.GetConnectedPortOfType(typeof(PGGPlannerPort));
                if (_bPort != null) bPort = _bPort as PGGPlannerPort;

                List<FieldPlanner> planners = GetPlannersFromPort(aPort, false, false, true);

                if (bPort != null)
                    foreach (var item in GetPlannersFromPort(bPort, false, false, true)) planners.Remove(item);

                plannerPort = true;

                OutPlanners.AssignPlannersList(planners);
                return;
            }
            else
                plannerPort = false;

            #endregion


            InValA.Variable.SetValue(InValA.GetPortValue);
            InValB.Variable.SetValue(InValB.GetPortValue);

            OutVal.Variable.AlgebraOperation(InValA.Variable, InValB.Variable, FieldVariable.EAlgebraOperation.Subtract);
        }


        public override void DONT_USE_IT_YET_OnReadPort(IFGraphPort port)
        {
            #region planners switch

            if (wasReading)
            {
                IFGraphPort plPort = InValA.GetConnectedPortOfType(typeof(PGGPlannerPort));
                if (plPort != null) plannerPort = true;
            }

            #endregion

            base.DONT_USE_IT_YET_OnReadPort(port);
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("InValB");
            SerializedProperty s = sp.Copy();


            EditorGUILayout.PropertyField(s, GUIContent.none);
            s.Next(false);
            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(19);

            if (plannerPort) { OutVal.AllowDragWire = false; OutPlanners.AllowDragWire = true; s.Next(false); }
            else { OutVal.AllowDragWire = true; OutPlanners.AllowDragWire = false; }

            EditorGUILayout.PropertyField(s, GUIContent.none);
            GUILayout.EndHorizontal();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            EditorGUILayout.LabelField("In A: " + InValA.GetPortValueSafe);
            EditorGUILayout.LabelField("In B: " + InValB.GetPortValueSafe);
            EditorGUILayout.LabelField("Out Value: " + OutVal.GetPortValueSafe);
        }

#endif

    }
}