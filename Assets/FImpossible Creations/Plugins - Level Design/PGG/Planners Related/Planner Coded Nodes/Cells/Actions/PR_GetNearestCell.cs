using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Actions
{

    public class PR_GetNearestCell : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return "Get Nearest Cell"; }
        public override string GetNodeTooltipDescription { get { return "Trying to find nearest cell from one field to another"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(_EditorFoldout ? 220 : 188, _EditorFoldout ? 204 : 142); } }
        public override bool DrawInputConnector { get { return false; } }
        //public override int OutputConnectionIndex { get { return 0; } } 
        //public override string GetOutputHelperText(int outputId = 0) { return "On Read Values"; }
        public override bool DrawOutputConnector { get { return false; } }
        public override bool IsFoldable { get { return true; } }

        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [Port(EPortPinType.Input)] public PGGPlannerPort A;
        [Port(EPortPinType.Input)] public PGGPlannerPort B;
        [Port(EPortPinType.Output)] public PGGCellPort NearestACell;
        [Port(EPortPinType.Output)] public PGGCellPort NearestBCell;
        //[Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue)] public PGGVector3Port DirAtoB;
        [HideInInspector][Tooltip("Can find nearest cells much faster (for grids with very big count of the cells) but will less precision")][Port(EPortPinType.Input)] public BoolPort FastCheck;
        [HideInInspector][Tooltip("If you search for near cell which also have some space on the sides")][Port(EPortPinType.Input)] public bool TryGetCentered = false;
        [HideInInspector][Tooltip("If you want to search for nearest cells multiple times for this FieldPlanner. If set to false, the search will be computed once per FieldPlanner / per duplicate")][Port(EPortPinType.Input)] public bool ResetOnRead = false;

        FieldPlanner computedFor = null;
        public override void PreGeneratePrepare()
        {
            base.PreGeneratePrepare();
            computedFor = null;
        }

        public override void OnStartReadingNode()
        {
            if (CurrentExecutingPlanner == null) return;

            if (ResetOnRead == false) if (computedFor == CurrentExecutingPlanner) return;

            A.TriggerReadPort(true);
            B.TriggerReadPort(true);

            var fieldA = GetPlannerFromPort(A, false);
            var fieldB = GetPlannerFromPort(B, false);

            if (fieldA == null) return;
            if (fieldB == null) return;

            if (fieldA == fieldB) return;

            CheckerField3D chA = fieldA.LatestChecker;
            if (chA == null || chA.ChildPositionsCount < 1) return;

            CheckerField3D chB = fieldB.LatestChecker;
            if (chB == null || chB.ChildPositionsCount < 1) return;

            FieldCell nearestA, nearestB;
            nearestA = chA.GetNearestCellTo(chB, FastCheck.GetInputValue);
            nearestB = chA._nearestCellOtherField;

            computedFor = CurrentExecutingPlanner;

            if (TryGetCentered)
            {
                FieldCell centeredB =
                 chA.GetMostCenteredCellInAxis(chB, nearestA, nearestB, new Vector3Int(0, 0, 1));

                if (FGenerators.NotNull(centeredB) && centeredB.Pos != nearestB.Pos)
                {
                    nearestA = chA._GetMostCenteredCellInAxis_MyCell;
                    nearestB = centeredB;
                }
                else
                {
                    centeredB = chA.GetMostCenteredCellInAxis(chB, nearestA, nearestB, new Vector3Int(1, 0, 0));
                    if (FGenerators.NotNull(centeredB) && centeredB.Pos != nearestB.Pos)
                    {
                        nearestA = chA._GetMostCenteredCellInAxis_MyCell;
                        nearestB = centeredB;
                    }
                }

            }

            NearestACell.ProvideFullCellData(nearestA, chA, fieldA.LatestResult);
            NearestBCell.ProvideFullCellData(nearestB, chB, fieldB.LatestResult);

            if (FGenerators.CheckIfIsNull(NearestACell.Cell)) return;
            if (FGenerators.CheckIfIsNull(NearestBCell.Cell)) return;


            #region Debugging Backup
            //chA.DebugLogDrawCellInWorldSpace(NearestACell.CellRef, Color.green);
            //UnityEngine.Debug.DrawLine(chA.GetWorldPos(NearestACell.CellRef), chB.GetWorldPos(NearestBCell.CellRef), Color.green, 1.01f);
            //chB.DebugLogDrawCellInWorldSpace(NearestBCell.CellRef, Color.green);
            #endregion

        }


#if UNITY_EDITOR

        UnityEditor.SerializedProperty sp = null;

        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            A.DisplayVariableName = true;
            B.DisplayVariableName = true;

            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                baseSerializedObject.Update();
                GUILayout.Space(4);
                if (sp == null) sp = baseSerializedObject.FindProperty("FastCheck");
                SerializedProperty spc = sp.Copy();
                EditorGUILayout.PropertyField(sp, true); spc.Next(false);
                EditorGUILayout.PropertyField(spc, true); spc.Next(false);
                EditorGUILayout.PropertyField(spc, true);
                baseSerializedObject.ApplyModifiedProperties();
            }
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            GUILayout.Label("NearestACell: " + NearestACell.GetInputCellValue);
            GUILayout.Label("NearestBCell: " + NearestBCell.GetInputCellValue);
            //GUILayout.Label("DirAtoB: " + DirAtoB.GetPortValueSafe);
            //GUILayout.Label("DirAtoB: " + DirAtoB.GetPortValueSafe);
        }
#endif

    }
}