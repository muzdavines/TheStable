using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Rules;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Transforming
{

    public class MR_SetPosition : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Set Spawn Position" : "Set Spawn Position or Offset it"; }
        public override string GetNodeTooltipDescription { get { return "Setting new position of spawn or offsetting it"; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.72f, 0.9f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 236 : 210, _EditorFoldout ? (Operation == EOperation.Set ? 121 : 122) : 84); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }

        public enum EOperation
        {
            Set, Offset, Subtract
        }

        [HideInInspector] public EOperation Operation = EOperation.Set;
        [HideInInspector] public ESR_Measuring Measure = ESR_Measuring.Units;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.HideName, EPortValueDisplay.HideValue, 1)] public PGGVector3Port Position;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, 1)] public PGGSpawnPort Spawn;
        [HideInInspector] public ESP_OffsetSpace OffsetSpace = ESP_OffsetSpace.WorldSpace;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }


        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Position.TriggerReadPort(true);
            Spawn.TriggerReadPort(true);

            SpawnData sp = Spawn.GetInputCellValue as SpawnData;
            if (FGenerators.IsNull(sp)) sp = MG_Spawn;
            if (FGenerators.IsNull(sp)) return;

            Vector3 val = Position.GetInputValue;
            if (Measure == ESR_Measuring.Cells) val = Vector3.Scale(MG_Preset.GetCellUnitSize(), val);

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

        void SetOffset(SpawnData spawn, Vector3 val, ESP_OffsetSpace space)
        {
            if (space == ESP_OffsetSpace.WorldSpace)
                spawn.Offset = val;
            else // Local
                spawn.DirectionalOffset = val;
        }

        void AddOffset(SpawnData spawn, Vector3 val, ESP_OffsetSpace space)
        {
            if (space == ESP_OffsetSpace.WorldSpace)
                spawn.Offset += val;
            else // Local
                spawn.DirectionalOffset += val;
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(31);

            float wdth = NodeSize.x - 88;

            if (Operation != EOperation.Set) wdth = 60;
            Operation = (EOperation)EditorGUILayout.EnumPopup(Operation, GUILayout.Width(wdth));

            if (Operation != EOperation.Set)
            {
                GUILayout.Space(4); Measure = (ESR_Measuring)EditorGUILayout.EnumPopup(Measure, GUILayout.Width(60));
            }

            GUILayout.EndHorizontal();
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("Position");
            GUILayout.Space(-19);
            EditorGUILayout.PropertyField(sp);

            if (_EditorFoldout)
            {
                SerializedProperty spc = sp.Copy(); spc.Next(false);
                EditorGUILayout.PropertyField(spc); spc.Next(false);
                EditorGUIUtility.labelWidth = 84;
                //if (Operation != EOperation.Set)
                    EditorGUILayout.PropertyField(spc);
                EditorGUIUtility.labelWidth = 0;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            UnityEditor.EditorGUILayout.LabelField("Debugging:", UnityEditor.EditorStyles.helpBox);
            GUILayout.Label("Position Value: " + Position.Value);
        }
#endif

    }
}