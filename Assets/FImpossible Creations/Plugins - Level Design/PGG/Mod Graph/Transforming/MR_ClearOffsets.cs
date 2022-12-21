using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Rules;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Transforming
{

    public class MR_ClearOffsets : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Clear Offsets" : "Clear Spawn Offsets"; }
        public override string GetNodeTooltipDescription { get { return "Clearing position/rotation different offset values from spawn preparation"; } }
        public override Color GetNodeColor() { return new Color(0.2f, 0.72f, 0.9f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 236 : 210, _EditorFoldout ? 106 : 84); } }
        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        public enum EClear
        {
            PositionOffsets, RotationOffsets, AllOffsets
        }

        public EClear ToClear = EClear.PositionOffsets;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, 1)] 
        public PGGSpawnPort Spawn;



        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Spawn.TriggerReadPort(true);

            SpawnData sp = Spawn.GetInputCellValue as SpawnData;
            if (FGenerators.IsNull(sp)) sp = MG_Spawn;
            if (FGenerators.IsNull(sp)) return;

            if (ToClear == EClear.PositionOffsets) ClearPosOffsets(sp);
            else if (ToClear == EClear.RotationOffsets) ClearRotOffsets(sp);
            else if ( ToClear == EClear.AllOffsets)
            {
                ClearPosOffsets(sp);
                ClearRotOffsets(sp);
            }
        }

        void ClearPosOffsets(SpawnData s)
        {
            s.Offset = Vector3.zero;
            s.DirectionalOffset = Vector3.zero;
            s.TempPositionOffset = Vector3.zero;
            s.OutsidePositionOffset = Vector3.zero;
        }

        void ClearRotOffsets(SpawnData s)
        {
            s.RotationOffset = Vector3.zero;
            s.LocalRotationOffset = Vector3.zero;
            s.TempRotationOffset = Vector3.zero;
            s.OutsideRotationOffset = Vector3.zero;
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("Spawn");
                EditorGUILayout.PropertyField(sp);
            }

            baseSerializedObject.ApplyModifiedProperties();
        }

#endif

    }
}