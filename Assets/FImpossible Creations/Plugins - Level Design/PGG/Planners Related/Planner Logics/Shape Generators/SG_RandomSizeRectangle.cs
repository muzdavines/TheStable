using FIMSpace.Generating.Checker;
using UnityEngine;

namespace FIMSpace.Generating.Planning.GeneratingLogics
{

    public class SG_RandomSizeRectangle : ShapeGeneratorBase
    {
        public override string TitleName() { return "Random Size Rectangle"; }

        public MinMax Width = new MinMax(3, 4);
        public MinMax Depth = new MinMax(3, 4);
        public MinMax YLevels = new MinMax(1, 1);
        public bool OriginInCenter = false;
        public override CheckerField3D GetChecker(FieldPlanner planner)
        {
            CheckerField3D checker = new CheckerField3D();
            checker.SetSize(Width.GetRandom(), YLevels.GetRandom(), Depth.GetRandom());
            if (OriginInCenter) checker.CenterizeOrigin();

            return checker;
        }
    }
}