using FIMSpace.Generating.Checker;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace FIMSpace.Generating.Planning.GeneratingLogics
{
    public class SG_NoShape : ShapeGeneratorBase
    {
        public override string TitleName() { return "No Shape"; }

        public override CheckerField3D GetChecker(FieldPlanner planner)
        {
            CheckerField3D checker = new CheckerField3D();
            return checker;
        }
    }
}