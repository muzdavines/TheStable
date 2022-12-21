using FIMSpace.Graph;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Logic
{

    public class PR_BoolAccumulate : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "If => Return Bool" : "If => Return Bool (Accumulate Bools)"; }
        public override Color GetNodeColor() { return new Color(0.3f, 0.8f, 0.55f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(200, 100); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Logic; } }

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideValue)] public BoolPort MultipleBools;
        [HideInInspector] [Port(EPortPinType.Output, true)] public BoolPort Result;
        public enum ECheckMultiConditions { AllNeedsToBeTrue, JustOneTrueRequired }
        [HideInInspector] public ECheckMultiConditions MultiCheckMode = ECheckMultiConditions.AllNeedsToBeTrue;

        public override void OnStartReadingNode()
        {
            MultipleBools.TriggerReadPort(true);
            Result.Value = GetMultiResult(MultipleBools, MultiCheckMode);
        }

        public bool GetMultiResult(BoolPort port, ECheckMultiConditions mode)
        {
            bool result;
            if (mode == ECheckMultiConditions.JustOneTrueRequired) result = false;
            else result = true;

            for (int i = 0; i < port.Connections.Count; i++)
            {
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
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("MultipleBools");
            EditorGUILayout.PropertyField(sp, GUILayout.Width(NodeSize.x - 90));

            GUILayout.Space(-19);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(NodeSize.x - 90);
            SerializedProperty spc = sp.Copy(); spc.Next(false);
            EditorGUILayout.PropertyField(spc, GUILayout.Width(28)); spc.Next(false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(spc, GUIContent.none);

            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("Result: " + Result.Value);
        }

#endif



    }
}