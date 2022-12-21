using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;
using System.Collections.Generic;

namespace FIMSpace.Generating
{
    public partial class ShapeDesignerWindow : EditorWindow
    {


        //private Vector2 scroller = Vector2.zero;
        static int selectedManualShape = 0;
        protected static int selectedCell = 0;
        static List<ShapeCellGroup> shapeLists = new List<ShapeCellGroup>();
        //static FieldSpawner spawnerOwner;
        //static GUIStyle boxStyle = null;
        //static GUIStyle boxStyleSel = null;
        static int depthLevel = 0;
        protected enum ESpaceView { XZ_TopDown, XY_Side, ZY_Front }
        protected static ESpaceView spaceView = ESpaceView.XZ_TopDown;

        static int drawSize = 30;


        void DrawManualRectanglesGUI()
        {
            CellsSelectorDrawer.DrawCellsSelector(shapeLists[selectedManualShape], ref drawSize, ref depthLevel);

        }

    }
}