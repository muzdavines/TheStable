using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells
{

    public class PR_GetCellPosition : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Cell Position" : "Get Field Cell Position"; }
        public override string GetNodeTooltipDescription { get { return "Accessing position parameters of provided cell"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(168, 122); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [Port(EPortPinType.Input)] public PGGCellPort Cell;
        [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue)] public PGGVector3Port Position;

        public enum ESpace { WorldPosition, LocalCellPosition }
        [HideInInspector] public ESpace PositionSpace = ESpace.WorldPosition;

        public override void OnStartReadingNode()
        {
            Cell.TriggerReadPort(true);
            var inputCell = Cell.GetInputCellValue;
            var checkerVal = Cell.GetInputCheckerValue;

            if (FGenerators.CheckIfExist_NOTNULL(inputCell))
                if (FGenerators.CheckIfExist_NOTNULL(checkerVal))
                {
                    if (PositionSpace == ESpace.WorldPosition)
                        Position.Value = checkerVal.GetWorldPos(inputCell);
                    else
                        Position.Value = checkerVal.GetLocalPos(inputCell);
                }
        }

#if UNITY_EDITOR

        private SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (sp == null) sp = baseSerializedObject.FindProperty("PositionSpace");
            EditorGUILayout.PropertyField(sp, GUIContent.none);

            baseSerializedObject.ApplyModifiedProperties();
        }

#endif

    }
}