using FIMSpace.Generating.Checker;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace FIMSpace.Generating.Planning.GeneratingLogics
{

    public class SG_StaticSizeRectangle : ShapeGeneratorBase
    {
        public override string TitleName() { return "Static Size Rectangle"; }

        public int Width = 5;
        public int YLevels = 1;
        public int Depth = 4;
        public bool OriginInCenter = false;

        public override CheckerField3D GetChecker(FieldPlanner planner)
        {
            CheckerField3D checker = new CheckerField3D();
            checker.SetSize(Width, YLevels, Depth);
            if (OriginInCenter) checker.CenterizeOrigin();
            //checker.RecalculateMultiBounds();
            return checker;
        }

#if UNITY_EDITOR
        public override void OnGUIModify()
        {
            if (Width < 1) Width = 1;
            if (Depth < 1) Depth = 1;
            if (YLevels < 1) YLevels = 1;
        }

#endif
    }
}