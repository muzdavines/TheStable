using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Rules;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Transforming
{

    public class MR_GetRotation : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? " Get Spawn Rotation" : "Get Spawn Rotation"; }
        public override string GetNodeTooltipDescription { get { return "Getting Rotation of spawn"; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.72f, 0.9f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 216 : 210, _EditorFoldout ? 106 : 84); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ModGraphNode; } }

        public enum EGetMode
        {
            GetOnlyWorldOffset,
            GetOnlyLocalOffset,
            GetFullOffset,
            GetTemporaryOffset
        }

        [HideInInspector] public EGetMode GetMode = EGetMode.GetFullOffset;
        [HideInInspector] [Port(EPortPinType.Output, true)] public PGGVector3Port Rotation;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, 1)] public PGGSpawnPort Spawn;

        public override void OnStartReadingNode()
        {
            Spawn.TriggerReadPort(true);

            var spawn = Spawn.GetInputCellValue;
            if (spawn == null) spawn = MG_Spawn;

            if (spawn == null) return;
 
            if (GetMode == EGetMode.GetOnlyWorldOffset)
                Rotation.Value = spawn.RotationOffset;
            else if (GetMode == EGetMode.GetOnlyLocalOffset)
                Rotation.Value = spawn.LocalRotationOffset;
            else
                if (GetMode == EGetMode.GetFullOffset)
                Rotation.Value = spawn.GetFullRotationOffset();
            else
                if (GetMode == EGetMode.GetTemporaryOffset)
                Rotation.Value = spawn.TempRotationOffset;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            GUILayout.BeginHorizontal();
            float wdth = NodeSize.x - 88;

            GUILayout.Space(8);
            GetMode = (EGetMode)EditorGUILayout.EnumPopup(GetMode, GUILayout.Width(wdth));
            GUILayout.Space(31);
            GUILayout.EndHorizontal();

            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("Rotation");
            GUILayout.Space(-19);
            EditorGUILayout.PropertyField(sp);

            if (_EditorFoldout)
            {
                SerializedProperty spc = sp.Copy(); spc.Next(false);
                EditorGUILayout.PropertyField(spc);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            UnityEditor.EditorGUILayout.LabelField("Debugging:", UnityEditor.EditorStyles.helpBox);
            GUILayout.Label("Rotation Value: " + Rotation.Value);
        }
#endif

    }
}