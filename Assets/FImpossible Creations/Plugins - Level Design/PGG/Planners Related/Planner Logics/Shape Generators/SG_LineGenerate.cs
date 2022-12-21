using FIMSpace.Generating.Checker;
using System.Collections.Generic;
using UnityEngine;
using static FIMSpace.Generating.Checker.CheckerField3D;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.GeneratingLogics
{

    public class SG_LineGenerate : ShapeGeneratorBase
    {
        public override string TitleName() { return "Basic Line Generate"; }

        public Vector3 PathStart = new Vector3(-6, 0, -4);
        public Vector3 PathEnd = new Vector3(6, 0, 4);

        [Space(4)]
        [Range(0f, 1f)]
        public float NonDiagonal = 0.4f;
        public List<LineFindHelper> Directions;

        private void Awake()
        {
            if (Directions == null || Directions.Count == 0)
            {
                PGGUtils.TransferFromListToList(CheckerField3D.GetDefaultDirections, Directions);
            }
        }

        [Space(4)]
        [PGG_SingleLineTwoProperties("Type", 60, 42, 10)]
        public int Radius = 1;
        [HideInInspector] public CheckerField3D.LineFindHelper.ERadiusType Type = CheckerField3D.LineFindHelper.ERadiusType.RectangleRadius;
        [PGG_SingleLineTwoProperties("YRadius", 110, 72, 10)]
        public bool ClearOverpaint = false;
        [HideInInspector] public bool YRadius = false;

        public override CheckerField3D GetChecker(FieldPlanner planner)
        {
            CheckerField3D checker = new CheckerField3D();
            if (PathStart == Vector3.zero && PathEnd == Vector3.zero) return checker;
            checker.AddLinesTowards(PathStart.V3toV3Int(), PathEnd.V3toV3Int(), NonDiagonal, Radius, Directions, Type, YRadius, ClearOverpaint);
            //checker.RecalculateMultiBounds();
            return checker;
        }

//#if UNITY_EDITOR
//        public override void DrawGUI(SerializedObject so)
//        {
//            GUILayout.Space(3);
//            EditorGUILayout.HelpBox("If you need controll on the start/end path points, use line generation node and inherit settings you set here. You can set start and end position zero to not trigger any generation here.", MessageType.Info);
//            GUILayout.Space(3);
//            base.DrawGUI(so);
//        }
//#endif

    }
}