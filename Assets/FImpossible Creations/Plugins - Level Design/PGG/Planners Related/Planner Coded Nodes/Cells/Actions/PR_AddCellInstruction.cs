using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Cells.Actions
{

    public class PR_AddCellInstruction : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "   Add Cell Instruction" : "Add Cell Instruction"; }
        public override string GetNodeTooltipDescription { get { return "Adding cell instruction in provided cell which can be used later by Field Setups"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(224, GetHeight()); } }

        float GetHeight()
        {
            if (Operation == ECellOperation.SetGhostCell)
            {
                return 84f + (_EditorFoldout ? 20 : 0);
            }
            else
            {
                return (DirExpand ? 124 : 104) + (_EditorFoldout ? 20 : 0);
            }
        }

        public override bool DrawInputConnector { get { return true; } }
        public override bool DrawOutputConnector { get { return true; } }
        public override bool IsFoldable { get { return true; } }
        bool DirExpand { get { return UseDirection; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }

        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGCellPort Cell;

        public enum ECellOperation
        {
            AddCellInstructionID,
            PreventSpawnTagged,
            SetGhostCell
            //CellInstruction,
            //AddCellData,
            //CellData,
            //SetCellDirection,
            //InjectSpawn
        }

        [HideInInspector] public ECellOperation Operation = ECellOperation.AddCellInstructionID;

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, 1)] public IntPort ID;
        [HideInInspector] public bool UseDirection = false;

        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, 1)] public PGGStringPort DataString;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, 1)] public PGGVector3Port Dir;
        [HideInInspector] [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.Default, 1)] public PGGStringPort Tags;

        [HideInInspector][Tooltip("If provided diagonal direction like (1,0,0.9) then choosing x or z direction depending which one has greater value")] public bool FlattenDiagonalDir = true;

        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            Cell.TriggerReadPort(true);
            if (UseDirection) Dir.TriggerReadPort(true);

            if (Operation == ECellOperation.AddCellInstructionID)
                ID.TriggerReadPort(true);
            else if (Operation == ECellOperation.PreventSpawnTagged)
                Tags.TriggerReadPort(true);

            FieldCell cell = Cell.GetInputCellValue;
            CheckerField3D cellsShape = null;

            if (FGenerators.CheckIfIsNull(cell))
            {
                var dta = Cell.GetAnyData();
                if (dta.ParentChecker != null)
                {
                    cellsShape = dta.ParentChecker;
                }

                if (cellsShape == null)
                    return;
            }

            //if (cellsShape != null) cellsShape.DebugLogDrawCellsInWorldSpace(Color.magenta);

            PlannerResult cellReslt = Cell.GetInputResultValue;
            FieldPlanner tgtOwner = null;

            if (cellReslt != null)
            {
                tgtOwner = cellReslt.ParentFieldPlanner;
                if (tgtOwner == null) tgtOwner = newResult.ParentFieldPlanner;
            }

            PlannerResult reslt = null;
            if (tgtOwner != null) reslt = tgtOwner.LatestResult;

            if (cellsShape != null && reslt == null) reslt = newResult;

            if (Operation == ECellOperation.PreventSpawnTagged)
                PrepareDefinition();

            if (FGenerators.CheckIfExist_NOTNULL(cell))
            {
                AddInstruction(cell, reslt);

                #region Debugging Gizmos
#if UNITY_EDITOR
                if (Debugging)
                    if (reslt.Checker != null)
                    {
                        DebuggingInfo = "Adding cell instruction";
                        Vector3 scl = reslt.Checker.RootScale;
                        Vector3 wPos = reslt.Checker.GetWorldPos(cell);

                        DebuggingGizmoEvent = () =>
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(wPos, scl * 0.7f);
                        };
                    }
#endif
                #endregion
            }

            if (cellsShape != null)
            {
                for (int i = 0; i < cellsShape.ChildPositionsCount; i++)
                {
                    Vector3 wPos = cellsShape.GetWorldPos(i);
                    FieldCell cll = reslt.Checker.GetCellInWorldPos(wPos);
                    if (FGenerators.CheckIfExist_NOTNULL(cll)) AddInstruction(cll, reslt);
                }
            }

        }

        private InstructionDefinition definition;
        void PrepareDefinition()
        {
            definition = new InstructionDefinition();

            if (Operation == ECellOperation.PreventSpawnTagged)
            {
                definition.InstructionType = InstructionDefinition.EInstruction.PreventSpawnSelective;
                definition.InstructionArgument = Tags.GetInputValue;
                definition.Tags = Tags.GetInputValue;
            }
            else if (Operation == ECellOperation.SetGhostCell)
            {
                definition.InstructionType = InstructionDefinition.EInstruction.SetGhostCell;
            }

        }

        void AddInstruction(FieldCell cell, PlannerResult reslt)
        {
            //if (ID.GetInputValue == 1) UnityEngine.Debug.Log("do");
            SpawnInstructionGuide instr = new SpawnInstructionGuide();
            instr.pos = cell.Pos;
            instr.HelperCellRef = cell;
            instr.rot = Quaternion.identity;

            if (Operation == ECellOperation.AddCellInstructionID)
            {
                instr.Id = ID.GetInputValue;

                if (UseDirection)
                {
                    instr.WorldRot = true;

                        Vector3 tgtDir = Dir.GetInputValue;
                    if (tgtDir != Vector3.zero)
                    {
                        if (FlattenDiagonalDir) tgtDir = FVectorMethods.ChooseDominantAxis(tgtDir);
                        instr.rot = Quaternion.LookRotation(tgtDir);
                    }

                    instr.UseDirection = UseDirection;
                }
            }
            else if (Operation == ECellOperation.PreventSpawnTagged)
            {
                instr.CustomDefinition = definition;
            }
            else if (Operation == ECellOperation.SetGhostCell)
            {
                cell.IsGhostCell = true;
                return;
            }

            if (reslt == null)
            {
                UnityEngine.Debug.Log("[PGG Field Planner] No 'Result' reference to apply cell instruction!");
                return;
            }

            reslt.CellsInstructions.Add(instr);
        }


#if UNITY_EDITOR
        UnityEditor.SerializedProperty sp = null;
        UnityEditor.SerializedProperty sp_diag = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            baseSerializedObject.Update();
            if (sp == null) sp = baseSerializedObject.FindProperty("Cell");

            SerializedProperty spc = sp.Copy();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp, GUILayout.Width(NodeSize.x - 154));
            spc.Next(false);
            EditorGUILayout.PropertyField(spc, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            spc.Next(false);

            Dir.AllowDragWire = UseDirection;

            if (Operation == ECellOperation.AddCellInstructionID)
            {
                ID.AllowDragWire = true;
                Tags.AllowDragWire = false;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(spc, GUILayout.Width(NodeSize.x - 154));
                EditorGUIUtility.labelWidth = 80;
                spc.Next(false); EditorGUILayout.PropertyField(spc);
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();

                if (UseDirection)
                {
                    spc.Next(false);
                    spc.Next(false);
                    EditorGUILayout.PropertyField(spc);
                }
            }
            else if (Operation == ECellOperation.PreventSpawnTagged)
            {
                Tags.AllowDragWire = true;
                ID.AllowDragWire = false;
                spc.Next(false);
                spc.Next(false);
                spc.Next(false);
                spc.Next(false);
                EditorGUILayout.PropertyField(spc);
            }

            //else if (Operation == ECellOperation.AddCellData || Operation == ECellOperation.CellData_Plus_Direction)
            //{
            //    spc.Next(false);
            //    EditorGUILayout.PropertyField(spc);

            //    if (Operation == ECellOperation.CellData_Plus_Direction)
            //    {
            //        spc.Next(false);
            //        EditorGUILayout.PropertyField(spc);
            //    }
            //}
            //else if (Operation == ECellOperation.SetCellDirection)
            //{
            //    spc.Next(false);
            //    spc.Next(false);
            //    EditorGUILayout.PropertyField(spc);
            //}

            if (_EditorFoldout)
            {
                if (sp_diag == null) sp_diag = baseSerializedObject.FindProperty("FlattenDiagonalDir");
                EditorGUILayout.PropertyField(sp_diag);
            }

            baseSerializedObject.ApplyModifiedProperties();

        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);
            EditorGUILayout.LabelField("Cell Value: " + Cell.GetPortValueSafe);
            if (FGenerators.CheckIfExist_NOTNULL(Cell.CellData.CellRef)) EditorGUILayout.LabelField("Cell Pos: " + Cell.CellData.CellRef.Pos);
            if (FGenerators.CheckIfExist_NOTNULL(Cell.CellData.ParentChecker)) EditorGUILayout.LabelField("Checker cells: " + Cell.CellData.ParentChecker.ChildPositionsCount);
        }

#endif

    }
}