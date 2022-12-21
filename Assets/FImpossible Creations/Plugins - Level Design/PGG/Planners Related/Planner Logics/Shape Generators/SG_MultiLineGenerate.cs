//using FIMSpace.Generating.Checker;
//using System.Collections.Generic;
//#if UNITY_EDITOR
//using UnityEngine;
//using static FIMSpace.Generating.Checker.CheckerField3D;
//#endif

//namespace FIMSpace.Generating.Planning.GeneratingLogics
//{

//    public class SG_MultiLineGenerate : ShapeGeneratorBase
//    {
//        public override string TitleName() { return "Multi Line Generate"; }

//        public List<Vector3> LinePoints = new List<Vector3>() { new Vector3(-6, 0, -4), new Vector3(6, 0, 4) };

//        [Space(4)]
//        [Range(0f,1f)]
//        public float NonDiagonal = 0.4f;
//        public List<LineFindHelper> Directions;

//        private void Awake()
//        {
//            if (Directions == null || Directions.Count == 0)
//            {
//                PGGUtils.TransferFromListToList(CheckerField3D.defaultLineFindDirections, Directions);
//            }
//        }

//        [Space(4)]
//        [PGG_SingleLineTwoProperties("Type", 60, 42, 10)]
//        public int Radius = 1;
//        [HideInInspector] public CheckerField3D.LineFindHelper.ERadiusType Type = CheckerField3D.LineFindHelper.ERadiusType.RectangleRadius;
//        [PGG_SingleLineTwoProperties("YRadius", 110, 72, 10)]
//        public bool ClearOverpaint = false;
//        [HideInInspector] public bool YRadius = false;

//        public override CheckerField3D GetChecker()
//        {
//            if (LinePoints.Count < 2)
//            {
//                LinePoints.Clear();
//                LinePoints = new List<Vector3>() { new Vector3(-6, 0, -4), new Vector3(6, 0, 4) };
//            }

//            CheckerField3D checker = new CheckerField3D();

//            for (int i = 0; i < LinePoints.Count-1; i++)
//            {
//                bool clear = false;
//                if ( ClearOverpaint)
//                {
//                    if (i == 0 || i == LinePoints.Count - 1) clear = true;
//                }

//                checker.AddLinesTowards(LinePoints[i].V3toV3Int(), LinePoints[i+1].V3toV3Int(), NonDiagonal, Radius, Directions, Type, YRadius, clear);
//            }

//            checker.RecalculateMultiBounds();
//            return checker;
//        }
//    }
//}