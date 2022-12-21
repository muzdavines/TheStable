using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Rules;
using FIMSpace.Graph;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Planning.ModNodes.Cells
{

    public class MR_GetNearestCellWith : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Nearest Cell With" : "Get Nearest Cell With Parameters"; }
        public override string GetNodeTooltipDescription { get { return "Finding nearest cell with provided specifics."; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { float extra = CellMustBe == ESR_Space.Occupied ? 20 : 0f; return new Vector2(220, (_EditorFoldout ? 162 : 120) + extra); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        public ESR_Space CellMustBe = ESR_Space.Empty;
        [Port(EPortPinType.Input, 1)] public IntPort MaxCellsDistance;
        [HideInInspector] [Port(EPortPinType.Input)] public PGGStringPort OccupiedBy;
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;
        [HideInInspector] [Port(EPortPinType.Output, "Found Cell")] public PGGModCellPort FoundCell;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGModCellPort OriginCell;
        [HideInInspector] public bool DiagonalSearch = true;

        public override void OnCreated()
        {
            base.OnCreated();
            MaxCellsDistance.Value = 4;
        }


        public override void OnStartReadingNode()
        {
            FoundCell.Clear();
            MaxCellsDistance.TriggerReadPort(true);

            int cellsDistance = MaxCellsDistance.GetInputValue;
            if (cellsDistance < 1) return;

            OriginCell.TriggerReadPort(true);
            FieldCell originCell;
            if (OriginCell.IsConnected) originCell = OriginCell.GetInputCellValue;
            else originCell = MG_Cell;

            if (FGenerators.IsNull(originCell)) return;

            OccupiedBy.TriggerReadPort(true);
            string tagVal = OccupiedBy.GetInputValue;

            for (int x = 1; x <= cellsDistance; x++)
            {
                if (CheckCell(tagVal, originCell, new Vector3Int(x, 0, 0))) break;
                if (CheckCell(tagVal, originCell, new Vector3Int(-x, 0, 0))) break;
                if (CheckCell(tagVal, originCell, new Vector3Int(0, 0, x))) break;
                if (CheckCell(tagVal, originCell, new Vector3Int(0, 0, -x))) break;

                if (DiagonalSearch)
                {
                    if (CheckCell(tagVal, originCell, new Vector3Int(x, 0, x))) break;
                    if (CheckCell(tagVal, originCell, new Vector3Int(-x, 0, x))) break;
                    if (CheckCell(tagVal, originCell, new Vector3Int(x, 0, -x))) break;
                    if (CheckCell(tagVal, originCell, new Vector3Int(-x, 0, -x))) break;
                }
            }
        }

        bool CheckCell(string tagVal, FieldCell originCell, Vector3Int posOffset)
        {
            FieldCell c;
            c = MG_Grid.GetCell(originCell.Pos + posOffset, true);

            if (CellIsRight(c, tagVal))
            {
                FoundCell.ProvideFullCellData(c);

                //Vector3 a = MG_Cell.WorldPos(MG_Preset.GetCellUnitSize());
                //Vector3 b = c.WorldPos(MG_Preset.GetCellUnitSize());
                //UnityEngine.Debug.Log("dist = " + Vector3.Distance(a, b) + " a " + a + " b " + b);
                //Debug.DrawLine(a, b, Color.yellow, 1f);

                return true;
            }

            return false;
        }

        bool CellIsRight(FieldCell c, string tagVal)
        {
            if (CellMustBe == ESR_Space.OutOfGrid || CellMustBe == ESR_Space.InGrid)
            {
                if (CellMustBe == ESR_Space.OutOfGrid)
                {
                    if (c == null) return true;
                    if (c.InTargetGridArea == false) return true;
                    return false;
                }
                else // In Grid
                {
                    if (c == null) return false;
                    if (c.InTargetGridArea) return true;
                    return false;
                }
            }
            else
            {
                if (CellMustBe == ESR_Space.Empty)
                {
                    if (c.GetJustCellSpawnCount() == 0) return true;
                }
                else // Occupied
                {
                    if (FGenerators.NotNull(SpawnRuleBase.CellSpawnsHaveSpecifics(c, tagVal, CheckMode))) return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();
            OccupiedBy.AllowDragWire = false;

            if (sp == null) sp = baseSerializedObject.FindProperty("OccupiedBy");
            SerializedProperty spc = sp.Copy();

            if (CellMustBe == ESR_Space.Occupied)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(spc, GUIContent.none, GUILayout.Width(NodeSize.x - 110)); spc.Next(false);
                EditorGUILayout.PropertyField(spc, GUIContent.none); spc.Next(false);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                spc.Next(false);
                spc.Next(false);
            }

            EditorGUILayout.PropertyField(spc, GUIContent.none); spc.Next(false);

            if (_EditorFoldout)
            {
                EditorGUIUtility.labelWidth = 149;
                EditorGUILayout.PropertyField(spc); spc.Next(false);
                EditorGUILayout.PropertyField(spc);
                EditorGUIUtility.labelWidth = 0;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}