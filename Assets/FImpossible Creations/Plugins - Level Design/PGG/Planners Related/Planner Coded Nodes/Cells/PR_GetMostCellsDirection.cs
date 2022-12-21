using FIMSpace.Graph;
using UnityEngine;
using FIMSpace.Generating.Checker;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells
{

    public class PR_GetMostCellsDirection : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Most Cells In Dir" : "Get Direction for Most Cells in Field"; }
        public override string GetNodeTooltipDescription { get { return "Getting direction in which there is most cells without empty cells spaces"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(238, _EditorFoldout ? 158 : 118); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return false; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [Port(EPortPinType.Input, EPortValueDisplay.NotEditable, 1)] public PGGCellPort StartCell;
        [Port(EPortPinType.Input, EPortValueDisplay.NotEditable, 1)] public PGGVector3Port CheckDirection;
        [Port(EPortPinType.Output, EPortValueDisplay.NotEditable)] public PGGVector3Port MostCellsIn;

        [HideInInspector][Port(EPortPinType.Input, EPortValueDisplay.NotEditable, 1)] public PGGVector3Port AdditionalDirection;
        [HideInInspector][Port(EPortPinType.Input, EPortValueDisplay.NotEditable, 1)] public PGGStringPort AvoidCellData;

        string _avoidCellData = "";
        public override void OnStartReadingNode()
        {
            if (CurrentExecutingPlanner == null) return;

            FieldPlanner planner = CurrentExecutingPlanner;
            Checker.CheckerField3D checker = planner.LatestChecker;

            if (checker == null) return;

            StartCell.TriggerReadPort(true);
            FieldCell startCell = StartCell.GetInputCellValue;

            if (FGenerators.IsNull(startCell)) return;

            CheckDirection.TriggerReadPort(true);
            Vector3Int checkDir = CheckDirection.GetInputValue.V3toV3Int();

            AdditionalDirection.TriggerReadPort(true);
            Vector3 addDir = AdditionalDirection.GetInputValue;

            _avoidCellData = "";
            if (AvoidCellData.IsConnected)
            {
                AvoidCellData.TriggerReadPort(true);
                _avoidCellData = AvoidCellData.GetInputValue;
            }

            Vector3Int mostDir = Vector3Int.zero;
            int mostCount = 0;

            CheckInDir(checker, startCell, checkDir, ref mostCount, ref mostDir);
            CheckInDir(checker, startCell, checkDir.InverseV3Int(), ref mostCount, ref mostDir);

            if (AdditionalDirection.IsConnected)
            {
                Vector3Int addDirect = addDir.V3toV3Int();
                CheckInDir(checker, startCell, addDirect, ref mostCount, ref mostDir);
                CheckInDir(checker, startCell, addDirect.InverseV3Int(), ref mostCount, ref mostDir);
            }

            MostCellsIn.Value = mostDir;
        }

        void CheckInDir(CheckerField3D checker, FieldCell startCell, Vector3Int checkDir, ref int mostCount, ref Vector3Int mostDir)
        {
            for (int i = 1; i < 100; i++)
            {
                var checkCell = checker.GetCell(startCell.Pos + checkDir * i);
                if (FGenerators.IsNull(checkCell) || checkCell.InTargetGridArea == false) break;

                if (!string.IsNullOrEmpty(_avoidCellData)) if (checkCell.HaveCustomData(_avoidCellData)) break;

                if (i > mostCount)
                {
                    mostCount = i;
                    mostDir = checkDir;
                }
            }
        }

#if UNITY_EDITOR
        private SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);


            if (_EditorFoldout == false) return;

            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("AdditionalDirection");
            SerializedProperty spc = sp.Copy();

            EditorGUILayout.PropertyField(spc); spc.Next(false);
            EditorGUILayout.PropertyField(spc);

            baseSerializedObject.ApplyModifiedProperties();
        }

#endif

    }
}