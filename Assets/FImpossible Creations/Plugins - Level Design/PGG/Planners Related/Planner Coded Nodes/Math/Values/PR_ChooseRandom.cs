using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.PlannerNodes.Math.Values
{

    public class PR_ChooseRandom : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Choose Random"; }
        public override string GetNodeTooltipDescription { get { return "Choosing one value from provided multiple inputs"; } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.5f, 0.75f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(180, _EditorFoldout ? 102 : 82); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.Math; } }

        [HideInInspector] [Port(EPortPinType.Input, EPortValueDisplay.HideValue)] public PGGUniversalPort MultipleInput;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGUniversalPort Choosed;
        [HideInInspector] public bool RandomOnRead = false;


        int choosed = 0;
        public override void Prepare(PlanGenerationPrint print)
        {
            if (RandomOnRead) return;
            choosed = FGenerators.GetRandom(0, MultipleInput.Connections.Count);
        }

        public override void OnStartReadingNode()
        {
            if (MultipleInput.Connections.Count == 0) return;

            if (MultipleInput.Connections.Count == 1)
            {
                Choosed.Variable.SetValue(MultipleInput.Connections[0].PortReference.GetPortValue);
                return;
            }

            if(RandomOnRead) choosed = FGenerators.GetRandom(0, MultipleInput.Connections.Count);
            Choosed.Variable.SetValue(MultipleInput.Connections[choosed].PortReference.GetPortValue);
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("MultipleInput");
            SerializedProperty spc = sp.Copy();

            EditorGUILayout.PropertyField(spc, GUILayout.Width(NodeSize.x - 60));
            spc.Next(false);
            GUILayout.Space(-20);
            EditorGUILayout.PropertyField(spc, true);

            if (_EditorFoldout)
            {
                spc.Next(false);
                EditorGUIUtility.labelWidth = 110;
                EditorGUILayout.PropertyField(spc, true);
                EditorGUIUtility.labelWidth = 0;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}