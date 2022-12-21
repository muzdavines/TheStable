using FIMSpace.Generating.Checker;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.GeneratingLogics
{

    public class SG_ManualRectangles : ShapeGeneratorBase
    {
        public override string TitleName() { return "Manual Rectangles"; }
        public List<ShapeCellGroup> CellSets = new List<ShapeCellGroup>();
        public int drawSize = 30;
        public int depthLevel = 0;
        public int selectedManualShape = 0;


        public override CheckerField3D GetChecker(FieldPlanner planner)
        {
            int choose = FGenerators.GetRandom(0, CellSets.Count);
            return CellSets[choose].GetChecker();
        }

#if UNITY_EDITOR
        public override void OnGUIModify()
        {
        }

        public override void DrawGUI(SerializedObject so)
        {
            if (CellSets == null)
            {
                CellSets = new List<ShapeCellGroup>();
            }

            if (CellSets.Count == 0) CellSets.Add(new ShapeCellGroup());

            CellsSelectorDrawer.DrawCellsSelector(CellSets[selectedManualShape], ref drawSize, ref depthLevel);
        }
#endif
    }
}