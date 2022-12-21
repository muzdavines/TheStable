using FIMSpace.Graph;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Logic
{

    public class PR_EqualIfSwitchExecution : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "If => Execute A or B" : "If true\\false => Execute A or B"; }
        public override Color GetNodeColor() { return new Color(0.3f, 0.8f, 0.55f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(isMulti ? 200 : 180, isMulti ? 112 : 92); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override int OutputConnectorsCount { get { return 2; } }
        public override int HotOutputConnectionIndex { get { return 1; } }
        public override int AllowedOutputConnectionIndex { get { return outputId; } }

        public override string GetOutputHelperText(int outputId = 0)
        {
            if (outputId == 0) return "False/Null";
            return "True";
        }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Logic; } }

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue)] public BoolPort FalseOrTrue;
        public enum ECheckMultiConditions { AllNeedsToBeTrue, JustOneTrueRequired }
        [HideInInspector] public ECheckMultiConditions MultiCheckMode = ECheckMultiConditions.AllNeedsToBeTrue;

        int outputId = 0;
        bool isMulti = false;

        void CheckForMultiState()
        {
            if (FalseOrTrue.Connections.Count > 1)
            {
                isMulti = true;
            }
            else
            {
                isMulti = false;
            }
        }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            FalseOrTrue.TriggerReadPort(true);

            CheckForMultiState();

            if (isMulti == false)
            {
                int targetId;
                if (FalseOrTrue.GetInputValue) targetId = 1; else targetId = 0;
                outputId = targetId;
            }
            else
            {
                int targetId;
                if (GetMultiResult(FalseOrTrue, MultiCheckMode)) targetId = 1; else targetId = 0;
                outputId = targetId;
            }
        }

        public bool GetMultiResult(BoolPort port, ECheckMultiConditions mode)
        {
            bool result;
            if (mode == ECheckMultiConditions.JustOneTrueRequired) result = false;
            else result = true;

            for (int i = 0; i < port.Connections.Count; i++)
            {
                if (port.Connections[i].PortReference == null) continue;
                object connVal = port.Connections[i].PortReference.GetPortValue;

                if (connVal is bool)
                {
                    bool cv = (bool)connVal;

                    if (cv)
                    {
                        if (mode == ECheckMultiConditions.JustOneTrueRequired) return true;
                    }
                    else
                    {
                        if (mode == ECheckMultiConditions.AllNeedsToBeTrue)
                        {
                            if (cv == false) return false;
                        }
                    }
                }
            }

            return result;
        }


#if UNITY_EDITOR
        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("Input Value: " + FalseOrTrue.GetPortValueSafe);
            GUILayout.Label("Is null?: " + FGenerators.CheckIfIsNull(FalseOrTrue.GetPortValueSafe));
        }

        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            CheckForMultiState();

            if (sp == null) sp = baseSerializedObject.FindProperty("FalseOrTrue");

            baseSerializedObject.Update();
            EditorGUILayout.PropertyField(sp);

            if (isMulti)
            {
                SerializedProperty spc = sp.Copy(); spc.Next(false);
                EditorGUILayout.PropertyField(spc, GUIContent.none);
            }

            baseSerializedObject.ApplyModifiedProperties();

            base.Editor_OnNodeBodyGUI(setup);
        }
#endif

    }
}