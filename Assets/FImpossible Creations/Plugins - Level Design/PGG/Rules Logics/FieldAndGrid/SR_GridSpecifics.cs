#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.FieldAndGrid
{
    public partial class SR_GridSpecifics : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Grid Specifics"; }
        public override string Tooltip() { return "Allow or not allow to spawn when some grid specific value is correct"; }
        public EProcedureType Type { get { return EProcedureType.Rule; } }

        public enum EGridSpec
        {
            GridSizeIsEven, GridSizeIsGreater, GridSizeIsLower, GridSizeIsEqualTo
        }

        public EGridSpec Condition = EGridSpec.GridSizeIsEven;

        public enum EGridAxis
        {
            X, Y, Z, XYZ, XZ
        }

        public EGridAxis ConditionMetAxis = EGridAxis.Z;
        public int SizeRange = 2;

        #region Editor Stuff

#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            if (Condition == EGridSpec.GridSizeIsEven)
            {
                GUIIgnore.Clear(); GUIIgnore.Add("SizeRange");
            }
            else GUIIgnore.Clear();

            base.NodeBody(so);
        }
#endif

        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);
            CellAllow = true;

            if ( Condition == EGridSpec.GridSizeIsEven)
            {
                if ( ConditionMetAxis == EGridAxis.X) CellAllow = (grid.Width % 2 == 0);
                else if( ConditionMetAxis == EGridAxis.Y) CellAllow = (grid.Height % 2 == 0);
                else if( ConditionMetAxis == EGridAxis.Z) CellAllow = (grid.Depth % 2 == 0);
                else if( ConditionMetAxis == EGridAxis.XZ) CellAllow = (grid.Depth % 2 == 0) && (grid.Width % 2 == 0);
                else if( ConditionMetAxis == EGridAxis.XYZ) CellAllow = (grid.Depth % 2 == 0) && (grid.Height % 2 == 0) && (grid.Width % 2 == 0);
            }
            else if (Condition == EGridSpec.GridSizeIsGreater)
            {
                Vector3Int gridSize = new Vector3Int();
                gridSize.x = Mathf.Abs(grid.MaxX.Pos.x - grid.MinX.Pos.x) + 1;
                gridSize.y = Mathf.Abs(grid.MaxY.Pos.y - grid.MinY.Pos.y) + 1;
                gridSize.z = Mathf.Abs(grid.MaxZ.Pos.z - grid.MinZ.Pos.z) + 1;
                
                if (ConditionMetAxis == EGridAxis.X) CellAllow = gridSize.x > SizeRange;
                else if (ConditionMetAxis == EGridAxis.Y) CellAllow = CellAllow = gridSize.y > SizeRange;
                else if (ConditionMetAxis == EGridAxis.Z) CellAllow = CellAllow = gridSize.z > SizeRange;
            }
            else if (Condition == EGridSpec.GridSizeIsLower)
            {
                Vector3Int gridSize = new Vector3Int();
                gridSize.x = Mathf.Abs(grid.MaxX.Pos.x - grid.MinX.Pos.x) + 1;
                gridSize.y = Mathf.Abs(grid.MaxY.Pos.y - grid.MinY.Pos.y) + 1;
                gridSize.z = Mathf.Abs(grid.MaxZ.Pos.z - grid.MinZ.Pos.z) + 1;

                if (ConditionMetAxis == EGridAxis.X) CellAllow = gridSize.x < SizeRange;
                else if (ConditionMetAxis == EGridAxis.Y) CellAllow = gridSize.y < SizeRange;
                else if (ConditionMetAxis == EGridAxis.Z) CellAllow = gridSize.z < SizeRange;
            }
            else if (Condition == EGridSpec.GridSizeIsEqualTo)
            {
                Vector3Int gridSize = new Vector3Int();
                gridSize.x = Mathf.Abs(grid.MaxX.Pos.x - grid.MinX.Pos.x) + 1;
                gridSize.y = Mathf.Abs(grid.MaxY.Pos.y - grid.MinY.Pos.y) + 1;
                gridSize.z = Mathf.Abs(grid.MaxZ.Pos.z - grid.MinZ.Pos.z) + 1;

                if (ConditionMetAxis == EGridAxis.X) CellAllow = gridSize.x == SizeRange;
                else if (ConditionMetAxis == EGridAxis.Y) CellAllow = gridSize.y == SizeRange;
                else if (ConditionMetAxis == EGridAxis.Z) CellAllow = gridSize.z == SizeRange;
            }
        }
    }
}