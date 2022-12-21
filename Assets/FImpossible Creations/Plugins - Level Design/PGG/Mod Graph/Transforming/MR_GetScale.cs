using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Rules;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Transforming
{

    public class MR_GetScale : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? " Get Spawn Scale" : "Get Spawn Scale"; }
        public override string GetNodeTooltipDescription { get { return "Getting Scale of spawn"; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.72f, 0.9f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 216 : 210, _EditorFoldout ? 106 : 84); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ModGraphNode; } }

        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public PGGVector3Port Scale;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, 1)] public PGGSpawnPort Spawn;

        public override void OnCreated()
        {
            base.OnCreated();
            Scale.Value = Vector3.one;
        }

        public override void OnStartReadingNode()
        {
            Spawn.TriggerReadPort(true);

            var spawn = Spawn.GetInputCellValue;
            if (spawn == null) spawn = MG_Spawn;
            if (FGenerators.IsNull(spawn)) return;

            Scale.Value = spawn.LocalScaleMul;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("Scale");
                SerializedProperty spc = sp.Copy(); spc.Next(false);
                EditorGUILayout.PropertyField(spc);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            UnityEditor.EditorGUILayout.LabelField("Debugging:", UnityEditor.EditorStyles.helpBox);
            GUILayout.Label("Position Value: " + Scale.Value);
        }
#endif

    }
}