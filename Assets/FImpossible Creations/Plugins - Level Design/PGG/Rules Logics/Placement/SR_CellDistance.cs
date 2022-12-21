using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Placement
{
    public class SR_CellDistance : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Distance to other cells"; }
        public override string Tooltip() { return "Measuring distance to other cells with desired parameters and allowing or disallowing when such cells are detected / not detected\n[Mediumweight]"; }
        public EProcedureType Type { get { return EProcedureType.Rule; } }

        [Tooltip("You can measure distance from other cell for this cell")]
        public ESR_DirectionMode OffsetCellMode = ESR_DirectionMode.NoOffset;
        public Vector3Int OffsetCell = Vector3Int.zero;

        [PGG_SingleLineTwoProperties("IgnoreSelf", 0, 70, 10, -20)]
        public ESR_DistanceRule DistanceMustBe = ESR_DistanceRule.Greater;
        [HideInInspector] public bool IgnoreSelf = true;

        [PGG_SingleLineSwitch("CheckMode", 70, "Select if you want to use Tags, SpawnStigma or CellData", 130)]
        public string DistanceToTagged = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [PGG_SingleLineSwitch("MeasureIn", 70, "Select if you want to offset postion with cell size or world units", 80)]
        public float Distance = 3f;
        [HideInInspector] public ESR_Measuring MeasureIn = ESR_Measuring.Cells;

        public SpawnerVariableHelper DistanceMulVariable = new SpawnerVariableHelper(FieldVariable.EVarType.Number);

        public override List<SpawnerVariableHelper> GetVariables() { return DistanceMulVariable.GetListedVariable(); }





#if UNITY_EDITOR
        public override void NodeFooter(UnityEditor.SerializedObject so, FieldModification mod)
        {
            base.NodeFooter(so, mod);
            if (Distance < 0) Distance = 0;
            if (Distance > 32) Distance = 32;


            if (FGenerators.CheckIfExist_NOTNULL(OwnerSpawner))
                if (OwnerSpawner.Parent != null)
                {
                    var preset = OwnerSpawner.Parent.ParentPreset;
                    if (preset != null)
                    {
                        float desiredDist = Distance * DistanceMulVariable.GetValue(1f);
                        int indexDistance = (int)desiredDist;
                        float worldDistance = desiredDist;

                        if (MeasureIn == ESR_Measuring.Units)
                        {
                            indexDistance = Mathf.CeilToInt(desiredDist / preset.CellSize);
                            if (indexDistance < 1) indexDistance = 1;
                        }
                        else // World distance multiplied by cell size of the room preset
                        {
                            worldDistance = desiredDist * preset.CellSize;
                        }

                        EditorGUILayout.HelpBox("Current measure cells check range: " + indexDistance + "   World Space Check Distance: " + worldDistance, MessageType.None);
                    }
                }
        }

        public override void NodeBody(SerializedObject so)
        {
            if (GUIIgnore.Count == 0) GUIIgnore.Add("");

            if (DistanceMulVariable != null) DistanceMulVariable.requiredType = FieldVariable.EVarType.Number;

            if (OffsetCellMode == ESR_DirectionMode.NoOffset)
                GUIIgnore[0] = "OffsetCell";
            else
                GUIIgnore[0] = "";

            base.NodeBody(so);
        }
#endif

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            var targetCell = cell;

            if (OffsetCellMode != ESR_DirectionMode.NoOffset) if (OffsetCell != Vector3Int.zero)
                {
                    Vector3Int off = OffsetCell;
                    if (OffsetCellMode == ESR_DirectionMode.CellRotateDirection) off = SpawnRuleBase.GetOffset(spawn.GetRotationOffset(), off);
                    targetCell = grid.GetCell(cell.Pos + off, false);
                }

            if (FGenerators.CheckIfIsNull(targetCell)) return;

            if (MeasureIn == ESR_Measuring.Cells)
            {
                if (Distance > 32) Distance = 32;
            }
            else
                if (Distance > 64) Distance = 64;

            float desiredDist = Distance * DistanceMulVariable.GetValue(1f);
            int indexDistance = (int)desiredDist;
            float worldDistance = desiredDist;

            if (MeasureIn == ESR_Measuring.Units)
            {
                indexDistance = Mathf.CeilToInt(desiredDist / preset.CellSize);
                if (indexDistance < 1) indexDistance = 1;
            }
            else // World distance multiplied by cell size of the room preset
            {
                worldDistance = desiredDist * preset.CellSize;
            }

            ESR_DistanceRule? result = null;

            List<FieldCell> cells = grid.GetDistanceSquare2DList(targetCell,  indexDistance, preset.CellSize, worldDistance);
            if (IgnoreSelf) if (cells.Contains(cell)) cells.Remove(cell);

            for (int i = 0; i < cells.Count; i++)
            {
                var dCell = cells[i];
                var spawns = dCell.CollectSpawns(OwnerSpawner.ScaleAccess);

                for (int s = 0; s < spawns.Count; s++)
                {
                    SpawnData spwn = spawns[s];
                    bool prInRange = false;
                    bool modInRange = false;
                    bool tagInRange = false;

                    if (SpawnHaveSpecifics(spwn, DistanceToTagged, CheckMode)) tagInRange = true;

                    if (prInRange || modInRange || tagInRange)
                    {
                        if (MeasureIn == ESR_Measuring.Cells)
                        {
                            float distance = (Vector3.Distance((Vector3)dCell.Pos * preset.CellSize, (Vector3)targetCell.Pos * preset.CellSize));
                            if (/*Mathf.RoundToInt*/(distance) == worldDistance) result = ESR_DistanceRule.Equal;
                            else if (distance > worldDistance) result = ESR_DistanceRule.Greater;
                            else result = ESR_DistanceRule.Lower;
                        }
                        else // Measure in units including spawn's offsets positioning
                        {
                            Vector3 targetPos = spwn.GetWorldPositionWithFullOffset(preset);
                            float distance = (Vector3.Distance(targetCell.WorldPos(preset.CellSize), targetPos));
                            if (distance == desiredDist) result = ESR_DistanceRule.Equal;
                            else if (distance > desiredDist) result = ESR_DistanceRule.Greater;
                            else result = ESR_DistanceRule.Lower;
                        }

                        if (result == DistanceMustBe)
                        {
                            CellAllow = true;
                            return;
                        }
                    }
                }
            }

            if (result == null) // Not found any cell for conditions - far away or too near
            {
                if (DistanceMustBe == ESR_DistanceRule.Greater)
                    CellAllow = true;
                else
                    CellAllow = false;
            }

        }

    }
}