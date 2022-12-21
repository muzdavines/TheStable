using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Rules;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Transforming
{

    public class MR_SetRotation : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Set Spawn Rotation" : "Set Spawn Rotation or Offset it"; }
        public override string GetNodeTooltipDescription { get { return "Setting new rotation of spawn or offsetting it"; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.72f, 0.9f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 236 : 210, _EditorFoldout ? 122 : 84); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        public enum EOperation
        {
            Set, Offset, Subtract
        }

        public enum ERotationType
        {
            WorldRotation, LocalRotation, TempRotation
        }

        [HideInInspector] public EOperation Operation = EOperation.Set;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.HideName, EPortValueDisplay.HideValue, 1)] public PGGVector3Port Degrees;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, 1)] public PGGSpawnPort Spawn;
        [HideInInspector] public ERotationType OffsetSpace = ERotationType.WorldRotation;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }


        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Degrees.TriggerReadPort(true);
            Spawn.TriggerReadPort(true);

            SpawnData sp = Spawn.GetInputCellValue as SpawnData;
            if (FGenerators.IsNull(sp)) sp = MG_Spawn;
            if (FGenerators.IsNull(sp)) return;

            Vector3 val = Degrees.GetInputValue;

            if (Operation == EOperation.Set)
            {
                SetOffset(sp, val, OffsetSpace);
            }
            else if (Operation == EOperation.Offset)
            {
                AddOffset(sp, val, OffsetSpace);
            }
            else if (Operation == EOperation.Subtract)
            {
                AddOffset(sp, -val, OffsetSpace);
            }
        }

        void SetOffset(SpawnData spawn, Vector3 val, ERotationType space)
        {
            if (space == ERotationType.WorldRotation)
                spawn.RotationOffset = val;
            else if (space == ERotationType.LocalRotation)
                spawn.LocalRotationOffset = val;
            else if (space == ERotationType.TempRotation)
                spawn.TempRotationOffset = val;
        }

        void AddOffset(SpawnData spawn, Vector3 val, ERotationType space)
        {
            if (space == ERotationType.WorldRotation)
                spawn.RotationOffset += val;
            else if (space == ERotationType.LocalRotation)
                spawn.LocalRotationOffset += val;
            else if (space == ERotationType.TempRotation)
                spawn.TempRotationOffset += val;
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(31);

            float wdth = NodeSize.x - 88;

            Operation = (EOperation)EditorGUILayout.EnumPopup(Operation, GUILayout.Width(wdth));

            GUILayout.EndHorizontal();
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("Degrees");
            GUILayout.Space(-19);
            EditorGUILayout.PropertyField(sp);

            if (_EditorFoldout)
            {
                SerializedProperty spc = sp.Copy(); spc.Next(false);
                EditorGUILayout.PropertyField(spc); spc.Next(false);
                EditorGUIUtility.labelWidth = 90;
                EditorGUILayout.PropertyField(spc);
                EditorGUIUtility.labelWidth = 0;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            UnityEditor.EditorGUILayout.LabelField("Debugging:", UnityEditor.EditorStyles.helpBox);
            GUILayout.Label("Rotation Value: " + Degrees.Value);
        }
#endif

    }
}