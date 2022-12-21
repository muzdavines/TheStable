#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace FIMSpace.Generating.Rules.QuickSolutions
{
    public class SR_FloorPlacer : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Floor Placer"; }
        public override string Tooltip() { return "Spawning floor tiles with pre-defined rules and aligning rotation"; }
        public EProcedureType Type { get { return EProcedureType.Rule; } }

        public enum EMode { JustPlaceOnGrid, PlaceExceptEdges, AlignToGridMargins, }
        public EMode Mode = EMode.JustPlaceOnGrid;

        public enum EAlignMode { AlignAll, JustSides, JustDiagonals, JustInDiagonals, JustOutDiagonals }
        public EAlignMode AlignMode = EAlignMode.AlignAll;



        [Space(5)] public float YawOffset = 0f;

        [PGG_SingleLineSwitch("OffsetMode", 58, "Select if you want to offset postion with cell size or world units", 140)]
        public Vector3 DirectOffset = Vector3.zero;
        [HideInInspector] public ESR_Measuring OffsetMode = ESR_Measuring.Units;

        public bool RotateBy90Only = true;
        [Tooltip("Using some additional sets of logics to detect all alignments correctly, it's experimental")]
        public bool V2 = false;
        //public bool UsePresetOffsetParams = false;
        [Space(4)]
        public bool Advanced = false;
        [Tooltip("Which cells should be considered as able for spawning floor tiles")]
        [HideInInspector] public ESR_Space SpawnOnCheck = ESR_Space.InGrid;
        [HideInInspector]
        [PGG_SingleLineSwitch("CheckMode", 80, "Select if you want to use Tags, SpawnStigma or CellData", 120)]
        public string SpawnOnTag = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [Space(4)]
        [Tooltip("Writing something in 'OutsideCondition' will be used like OR logic if 'SpawnOnCheck' is different than 'Occupied'")]
        [HideInInspector] public ESR_Space OutsideCondition = ESR_Space.OutOfGrid;
        [HideInInspector]
        [PGG_SingleLineSwitch("CheckOutMode", 80, "Select if you want to use Tags, SpawnStigma or CellData", 120)]
        [Tooltip("Writing something in 'OutsideCondition' will be used like OR logic if 'OutsideCondition' is different than 'Occupied'")]
        public string OutsideOnTag = "";
        [HideInInspector] public ESR_Details CheckOutMode = ESR_Details.Tag;
        [HideInInspector] public bool NegateOutsideCheck = false;

#if UNITY_EDITOR
        public override void NodeHeader()
        {
            base.NodeHeader();

            if (GUIIgnore.Count != 2)
            {
                GUIIgnore.Clear(); GUIIgnore.Add(""); GUIIgnore.Add("V2");
            }

            if (Mode != EMode.AlignToGridMargins)
            {
                GUIIgnore[0] = "AlignMode";
            }
            else
            {
                GUIIgnore[0] = "";
                if (Mode == EMode.AlignToGridMargins)
                {
                    //if (AlignMode == EAlignMode.JustInDiagonals)
                    {
                        GUIIgnore[1] = "";
                    }
                }
            }
        }

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {

            if (Advanced)
            {
                GUILayout.Space(2);
                SerializedProperty sp = so.FindProperty("SpawnOnCheck");
                EditorGUILayout.PropertyField(sp); sp.Next(false);
                if (SpawnOnCheck == ESR_Space.Occupied) EditorGUILayout.PropertyField(sp);
                sp.Next(false); sp.Next(false);

                EditorGUILayout.PropertyField(sp); sp.Next(false);
                EditorGUILayout.PropertyField(sp); sp.Next(false); sp.Next(false);
                EditorGUILayout.PropertyField(sp);
            }
            else
            {
                SpawnOnCheck = ESR_Space.InGrid;
                SpawnOnTag = "";

                OutsideCondition = ESR_Space.OutOfGrid;
                OutsideOnTag = "";
            }

            base.NodeFooter(so, mod);
        }
#endif


        private bool IsOutside(FieldCell cell)
        {
            if (Advanced)
            {
                if (NegateOutsideCheck)
                    return !CellConditionsAllows(cell, OutsideOnTag, CheckOutMode, OutsideCondition);
                else
                    return CellConditionsAllows(cell, OutsideOnTag, CheckOutMode, OutsideCondition);
            }
            else
            {
                return CellConditionsAllows(cell, OutsideOnTag, CheckOutMode, OutsideCondition);
            }
        }

        private bool IsInside(FieldCell cell)
        {
            return CellConditionsAllows(cell, SpawnOnTag, CheckMode, SpawnOnCheck);
        }


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (!Advanced) // Backup - Probably will be removed in the next update
            {

                if (Mode == EMode.JustPlaceOnGrid)
                {
                    AssignSpawnCoords(spawn, Vector3.forward, preset);
                }
                else if (Mode == EMode.PlaceExceptEdges)
                {
                    var cells = grid.Get3x3Square(cell, false);

                    for (int i = 0; i < cells.Length; i++)
                        if (FGenerators.CheckIfIsNull(cells[i])) { return; }
                        else
                        if (cells[i].InTargetGridArea == false) { return; }

                    AssignSpawnCoords(spawn, Vector3.forward, preset);
                }
                else if (Mode == EMode.AlignToGridMargins)
                {
                    var cells = SpawnRules.GetTargetNeightboursPLUS(cell, grid, ESR_Space.OutOfGrid, "", ESR_Details.Tag);

                    FieldCell fwdCell = null;
                    for (int i = 0; i < cells.Count; i++)
                        if (cells[i].InTargetGridArea == false) { fwdCell = cells[i]; break; }

                    if (AlignMode == EAlignMode.AlignAll)
                    {
                        if (FGenerators.CheckIfIsNull(fwdCell))
                        {
                            var diagCells = SpawnRules.GetTargetNeightboursDiagonal(cell, grid, ESR_Space.OutOfGrid, "", ESR_Details.Tag);

                            FieldCell diagCell = null;
                            for (int i = 0; i < diagCells.Count; i++)
                                if (diagCells[i].InTargetGridArea == false) { diagCell = diagCells[i]; break; }

                            if (FGenerators.CheckIfExist_NOTNULL(diagCell)) // Diagonal cell exists
                                AssignSpawnCoords(spawn, diagCell.Pos - cell.Pos, preset);
                        }
                        else
                            AssignSpawnCoords(spawn, fwdCell.Pos - cell.Pos, preset);
                    }
                    else // Just sides or just diagonals
                    {
                        var aroundCells = SpawnRules.Get3x3Square(cell, grid);
                        var diagCells = SpawnRules.GetTargetNeightboursDiagonal(cell, grid, ESR_Space.OutOfGrid, "", ESR_Details.Tag);

                        FieldCell diagCell = null;
                        FieldCell mostSeparatedDiagCell = null;
                        int mostSepEmptys = -1;

                        for (int i = 0; i < diagCells.Count; i++)
                            if (diagCells[i].InTargetGridArea == false)
                            {
                                diagCell = diagCells[i];
                                int emptys = 0;

                                // Get empty cells around diagonal cell
                                for (int c = 0; c < aroundCells.Count; c++)
                                {
                                    var aCell = aroundCells[c];
                                    if (aCell.InTargetGridArea == false)
                                    {
                                        int dist = ManhattanDistance(aCell.Pos, diagCell.Pos);
                                        if (dist == 1)
                                        {
                                            emptys += 1;
                                        }
                                    }
                                }

                                if (emptys > 0)
                                    if (emptys > mostSepEmptys)
                                    {
                                        mostSepEmptys = emptys;
                                        mostSeparatedDiagCell = diagCell;
                                    }

                            }

                        if (FGenerators.CheckIfIsNull(mostSeparatedDiagCell)) mostSeparatedDiagCell = diagCell;

                        bool haveDiagonalIn = false;
                        bool haveDiagonalOut = false;

                        if (FGenerators.CheckIfExist_NOTNULL(diagCell)) // Diagonal cell exists
                        {
                            if (FGenerators.CheckIfIsNull(fwdCell))
                            {// Diagonal OUT requirement 
                                haveDiagonalOut = true;
                            }
                            else // Diagonal IN
                            {
                                int diagCellsC = 0;

                                for (int i = 0; i < diagCells.Count; i++)
                                {
                                    if (diagCells[i].InTargetGridArea) continue;
                                    diagCellsC++;
                                }

                                if (diagCellsC >= 3) haveDiagonalIn = true;

                                if (V2)
                                    // Exception situation -> check non diagonals for confirm
                                    if (diagCellsC == 2)
                                    {
                                        int sided = 0;
                                        var sideCells = SpawnRules.GetTargetNeightboursPLUS(cell, grid, ESR_Space.OutOfGrid, "", ESR_Details.Tag);

                                        for (int i = 0; i < sideCells.Count; i++)
                                        {
                                            if (sideCells[i].InTargetGridArea) continue;
                                            sided++;
                                        }

                                        if (sided > 1) haveDiagonalIn = true;
                                    }
                            }
                        }

                        if (AlignMode == EAlignMode.JustDiagonals)
                        {
                            if (haveDiagonalIn || haveDiagonalOut) AssignSpawnCoords(spawn, mostSeparatedDiagCell.Pos - cell.Pos, preset);
                        }
                        else if (AlignMode == EAlignMode.JustInDiagonals)
                        {
                            if (haveDiagonalIn)
                            {
                                //Debug.DrawLine(cell.WorldPos(), diagCell.WorldPos(), Color.yellow, 1.1f);
                                //Debug.DrawLine(cell.WorldPos(), mostSeparatedDiagCell.WorldPos(), Color.red, 1.1f);
                                if (V2)
                                {
                                    if (mostSepEmptys > 1)
                                        AssignSpawnCoords(spawn, mostSeparatedDiagCell.Pos - cell.Pos, preset);
                                }
                                else
                                    AssignSpawnCoords(spawn, mostSeparatedDiagCell.Pos - cell.Pos, preset);

                            }
                        }
                        else if (AlignMode == EAlignMode.JustOutDiagonals)
                        {
                            if (haveDiagonalOut)
                            {
                                AssignSpawnCoords(spawn, mostSeparatedDiagCell.Pos - cell.Pos, preset);
                            }
                        }
                        else if (AlignMode == EAlignMode.JustSides)
                        {
                            if (!haveDiagonalIn && !haveDiagonalOut) if (FGenerators.CheckIfExist_NOTNULL(fwdCell)) AssignSpawnCoords(spawn, fwdCell.Pos - cell.Pos, preset);
                        }
                    }
                }

            }
            else
            {
                // Checking for allowing spawning in checked cell position

                if (Mode == EMode.JustPlaceOnGrid)
                {
                    if (!IsInside(cell)) return;
                    AssignSpawnCoords(spawn, Vector3.forward, preset);
                }
                else if (Mode == EMode.PlaceExceptEdges)
                {
                    if (!IsInside(cell)) return;
                    var cells = grid.Get3x3Square(cell, false);

                    for (int i = 0; i < cells.Length; i++)
                    {
                        if (IsOutside(cells[i]))
                            return;
                    }

                    AssignSpawnCoords(spawn, Vector3.forward, preset);
                }
                else if (Mode == EMode.AlignToGridMargins)
                {
                    if (!IsInside(cell)) return;
                    var cells = grid.GetPLUSSquare(cell, true);

                    FieldCell fwdCell = null;
                    bool allow = false;
                    for (int i = 0; i < cells.Length; i++)
                        if (IsOutside(cells[i]))
                        { fwdCell = cells[i]; allow = true; break; }

                    if (AlignMode == EAlignMode.AlignAll)
                    {
                        if (!allow)
                        {
                            var diagCells = grid.GetDiagonalCross(cell, true);

                            FieldCell diagCell = null;
                            for (int i = 0; i < diagCells.Length; i++)
                                if (IsOutside(diagCells[i])) { diagCell = diagCells[i]; break; }

                            if (diagCell.NotNull())
                                AssignSpawnCoords(spawn, diagCell.Pos - cell.Pos, preset);
                        }
                        else
                            AssignSpawnCoords(spawn, fwdCell.Pos - cell.Pos, preset);
                    }
                    else // Just sides or just diagonals
                    {
                        var aroundCells = grid.Get3x3Square(cell, true);
                        var diagCells = grid.GetDiagonalCross(cell, true);

                        FieldCell diagCell = null;
                        FieldCell mostSeparatedDiagCell = null;
                        int mostSepEmptys = -1;

                        for (int i = 0; i < diagCells.Length; i++)
                            if (IsOutside(diagCells[i]))
                            {
                                diagCell = diagCells[i];
                                int emptys = 0;

                                // Get empty cells around diagonal cell
                                for (int c = 0; c < aroundCells.Length; c++)
                                {
                                    var aCell = aroundCells[c];
                                    if (IsOutside(aCell))
                                    {
                                        int dist = ManhattanDistance(aCell.Pos, diagCell.Pos);
                                        if (dist == 1)
                                        {
                                            emptys += 1;
                                        }
                                    }
                                }

                                if (emptys > 0)
                                    if (emptys > mostSepEmptys)
                                    {
                                        mostSepEmptys = emptys;
                                        mostSeparatedDiagCell = diagCell;
                                    }

                            }

                        if (FGenerators.CheckIfIsNull(mostSeparatedDiagCell)) mostSeparatedDiagCell = diagCell;

                        bool haveDiagonalIn = false;
                        bool haveDiagonalOut = false;

                        if (FGenerators.CheckIfExist_NOTNULL(diagCell)) // Diagonal cell exists
                        {
                            if (FGenerators.CheckIfIsNull(fwdCell))
                            {// Diagonal OUT requirement 
                                haveDiagonalOut = true;
                            }
                            else // Diagonal IN
                            {
                                int diagCellsC = 0;

                                for (int i = 0; i < diagCells.Length; i++)
                                {
                                    if (!IsOutside(diagCells[i])) continue;
                                    diagCellsC++;
                                }

                                if (diagCellsC >= 3) haveDiagonalIn = true;

                                if (V2)
                                    // Exception situation -> check non diagonals for confirm
                                    if (diagCellsC == 2)
                                    {
                                        int sided = 0;
                                        var sideCells = SpawnRules.GetTargetNeightboursPLUS(cell, grid);

                                        for (int i = 0; i < sideCells.Count; i++)
                                        {
                                            if (!IsOutside(sideCells[i])) continue;
                                            sided++;
                                        }

                                        if (sided > 1) haveDiagonalIn = true;
                                    }
                            }
                        }

                        if (AlignMode == EAlignMode.JustDiagonals)
                        {
                            if (haveDiagonalIn || haveDiagonalOut) AssignSpawnCoords(spawn, mostSeparatedDiagCell.Pos - cell.Pos, preset);
                        }
                        else if (AlignMode == EAlignMode.JustInDiagonals)
                        {
                            if (haveDiagonalIn)
                            {
                                //Debug.DrawLine(cell.WorldPos(), diagCell.WorldPos(), Color.yellow, 1.1f);
                                //Debug.DrawLine(cell.WorldPos(), mostSeparatedDiagCell.WorldPos(), Color.red, 1.1f);
                                if (V2)
                                {
                                    if (mostSepEmptys > 1)
                                        AssignSpawnCoords(spawn, mostSeparatedDiagCell.Pos - cell.Pos, preset);
                                }
                                else
                                    AssignSpawnCoords(spawn, mostSeparatedDiagCell.Pos - cell.Pos, preset);

                            }
                        }
                        else if (AlignMode == EAlignMode.JustOutDiagonals)
                        {
                            if (haveDiagonalOut)
                            {
                                AssignSpawnCoords(spawn, mostSeparatedDiagCell.Pos - cell.Pos, preset);
                            }
                        }
                        else if (AlignMode == EAlignMode.JustSides)
                        {
                            if (!haveDiagonalIn && !haveDiagonalOut) if (FGenerators.CheckIfExist_NOTNULL(fwdCell)) AssignSpawnCoords(spawn, fwdCell.Pos - cell.Pos, preset);
                        }
                    }

                }
            }
        }


        public void AssignSpawnCoords(SpawnData spawn, Vector3 normal, FieldSetup presetUsed)
        {
            Vector3 targetRot = Quaternion.LookRotation(normal).eulerAngles;

            if (RotateBy90Only)
            {
                targetRot.y = Mathf.Round((targetRot.y - 0.1f) / 90f) * 90f;
            }

            spawn.RotationOffset = targetRot + Vector3.up * YawOffset;
            spawn.DirectionalOffset = GetUnitOffset(DirectOffset, OffsetMode, presetUsed);

            CellAllow = true;
        }


    }
}