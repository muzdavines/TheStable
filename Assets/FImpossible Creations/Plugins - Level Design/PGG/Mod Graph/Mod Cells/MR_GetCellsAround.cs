using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.ModNodes.Cells
{

    public class MR_GetCellsAround : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Get Cells Around" : "Get Cells Around"; }
        public override string GetNodeTooltipDescription { get { return "Gathering cells in square space around origin cell"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(210, _EditorFoldout ? 142 : 104); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }


        [Port(EPortPinType.Input, 1)] public IntPort CellsDistance;
        [Port(EPortPinType.Output)] public PGGModCellPort ResultCells;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGModCellPort OriginCell;
        [HideInInspector] public bool IncludeSelf = false;
        

        public override void OnCreated()
        {
            base.OnCreated();
            CellsDistance.Value = 2;
        }

        public override void OnStartReadingNode()
        {
            OriginCell.TriggerReadPort(true);
            CellsDistance.TriggerReadPort(true);
            var cell = OriginCell.GetInputCellValue;

            if (FGenerators.IsNull(cell)) if (OriginCell.IsConnected == false) cell = MG_Cell;
            if (FGenerators.IsNull(cell)) return;

            int dist = CellsDistance.GetInputValue;
            var list = MG_Grid.GetDistanceSquare2DList(cell, dist, MG_Preset.GetCellUnitSize().x,  MG_Preset.GetCellUnitSize().x * dist);
            
            if (!IncludeSelf) if (MG_Cell != null) if (list.Contains(MG_Cell)) list.Remove(MG_Cell);

            ResultCells.ProvideCellsList(list);
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (sp == null) sp = baseSerializedObject.FindProperty("OriginCell");

            if (_EditorFoldout)
            {
                OriginCell.AllowDragWire = true;
                EditorGUILayout.PropertyField(sp);
                SerializedProperty spc = sp.Copy(); spc.Next(false); EditorGUILayout.PropertyField(spc);
            }
            else
                OriginCell.AllowDragWire = false;

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}