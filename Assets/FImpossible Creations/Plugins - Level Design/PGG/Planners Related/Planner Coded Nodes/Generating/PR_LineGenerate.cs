using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Generating
{

    public class PR_LineGenerate : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Line Generate"; }
        public override string GetNodeTooltipDescription { get { return "Simple line-shape generator without using any collision algorithms.\nYou can use 'Join Shape Cells' node to apply line shape to the field."; } }

        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(252, _EditorFoldout ? 200 : 160); } }
        public override Color GetNodeColor() { return new Color(0.3f, 0.7f, .9f, 0.95f); }

        [Port(EPortPinType.Input)] public PGGVector3Port PathStart;
        [Port(EPortPinType.Input)] public PGGVector3Port PathEnd;
        [HideInInspector] public bool TryStartCentered = true;
        [HideInInspector] public bool RemoveFinishCell = true;

        [Port(EPortPinType.Input, 1)] public IntPort Radius;
        private int radius = 1;
        public override EPlannerNodeType NodeType
        {
            get
            {
                return EPlannerNodeType.CellsManipulation;
            }
        }

        [PGG_SingleLineTwoProperties("NonDiag", 40, 64, 10)]
        public CheckerField3D.LineFindHelper.ERadiusType Type = CheckerField3D.LineFindHelper.ERadiusType.RectangleRadius;
        [Range(0f, 1f)] [HideInInspector] public float NonDiag = 1;

        [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGPlannerPort PathShape;

        public override void OnCreated()
        {
            base.OnCreated();
            Radius.Value = 1;
        }

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            PathStart.TriggerReadPort(true);
            PathEnd.TriggerReadPort(true);
            Radius.TriggerReadPort(true);
            radius = Radius.GetInputValue;

            PathShape.JustCheckerContainer = true;

            CheckerField3D pathChecker = new CheckerField3D();
            CheckerField3D myChecker = ParentPlanner.LatestChecker;
            pathChecker.CopyParamsFrom(myChecker);

            Vector3 startWorld = PathStart.GetInputValue;
            Vector3 endWorld = PathEnd.GetInputValue;
            Vector3Int startLocal = pathChecker.WorldToLocal(startWorld).V3toV3Int();
            Vector3Int endMyLocal = pathChecker.WorldToLocal(endWorld).V3toV3Int();


            if (TryStartCentered)
            {
                Vector3 dirTowards = (endWorld - startWorld).normalized;
                Vector3Int off = FVectorMethods.ChooseDominantAxis(dirTowards).V3toV3Int();

                if (off != Vector3Int.zero)
                {
                    Vector3Int offsetInLocal = myChecker.DirectionToLocal(off).normalized.V3toV3Int();
                    Vector3Int checkAxis = PGGUtils.GetRotatedFlatDirectionFrom(offsetInLocal);
                    Vector3Int newlocal = startLocal;

                    FieldCell centered = myChecker.GetMostCenteredCellInAxis(myChecker.GetCell(startLocal), checkAxis);

                    if (FGenerators.CheckIfExist_NOTNULL(centered))
                    {
                        newlocal = centered.Pos;
                    }

                    startLocal = newlocal;
                }
            }

            //pathChecker.DebugLogDrawCellInWorldSpace(startLocal, Color.white);
            //UnityEngine.Debug.DrawLine(pathChecker.LocalToWorld(startLocal), pathChecker.LocalToWorld(endMyLocal), Color.black, 1.01f);
            //pathChecker.DebugLogDrawCellInWorldSpace(endMyLocal, Color.green);


            pathChecker.AddLinesTowards(startLocal, endMyLocal, NonDiag, radius, null, Type, false, true, RemoveFinishCell);
            PathShape.ProvideShape(pathChecker);

            #region Debugging Gizmos
#if UNITY_EDITOR
            if (Debugging)
            {
                DebuggingInfo = "Generating path from position " + PathStart.GetInputValue + " towards " + PathEnd.GetInputValue;

                DebuggingGizmoEvent = () =>
                {
                    Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                    Gizmos.DrawCube(PathStart.GetInputValue, Vector3.one * 0.3f) ;
                    Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
                    Gizmos.DrawCube(PathEnd.GetInputValue, Vector3.one * 0.3f) ;
                    Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
                    Gizmos.DrawLine(PathStart.GetInputValue, PathEnd.GetInputValue);
                };

            }
#endif
            #endregion


        }

#if UNITY_EDITOR

        UnityEditor.SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();
            if (_EditorFoldout)
            {
                GUILayout.Space(1);
                if (sp == null) sp = baseSerializedObject.FindProperty("TryStartCentered");
                SerializedProperty spc = sp.Copy();
                EditorGUILayout.PropertyField(spc, true);
                spc.Next(false); EditorGUILayout.PropertyField(spc, true);
            }
            baseSerializedObject.ApplyModifiedProperties();
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("PathStart: " + PathStart.GetInputValue);
            GUILayout.Label("PathEnd: " + PathEnd.GetInputValue);
        }
#endif
    }
}