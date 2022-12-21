using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Grid
{

    public class MR_GridSize : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Grid Size" : "Get Grid Size"; }
        public override string GetNodeTooltipDescription { get { return "Getting grid size \\ dimensions"; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override bool IsFoldable { get { return false; } }
        public override Vector2 NodeSize { get { return new Vector2(188, _EditorFoldout ? 102 : 84); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public enum ESizeType
        {
            Cells, Units
        }

        [HideInInspector] public ESizeType SizeIn = ESizeType.Units;
        [HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.HideName, EPortValueDisplay.HideValue)] public PGGVector3Port Size;

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }

        public override void OnStartReadingNode()
        {
            var grid = MG_Grid;
            if (grid == null) return;

            Size.Value = MG_Grid.GetMaxSizeInCells();

            if ( SizeIn == ESizeType.Cells)
            {
                return;
            }

            var setup = MG_Preset;
            if (setup == null) return;

            Size.Value = Vector3.Scale(setup.GetCellUnitSize(), Size.Value);
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("SizeIn");
            SerializedProperty spc = sp.Copy();
            EditorGUIUtility.labelWidth = 50;
            UnityEditor.EditorGUILayout.PropertyField(spc, GUILayout.Width(NodeSize.x - 80)); spc.Next(false);
            EditorGUIUtility.labelWidth = 0;

            GUILayout.Space(-19);

            EditorGUILayout.PropertyField(spc);
            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            UnityEditor.EditorGUILayout.LabelField("Debugging:", UnityEditor.EditorStyles.helpBox);
            GUILayout.Label("Position Value: " + Size.Value);
        }

#endif

    }
}