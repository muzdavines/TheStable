using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Special
{

    public class PR_TightPlacement : PlannerRuleBase
    {

        public override string GetDisplayName(float maxWidth = 120) { return "Tight Placement"; }
        public override string GetNodeTooltipDescription { get { return "Trying to find placement on 'AlignTo' field resulting in smallest total bounds size. Toggling 'Fast Check' will ignore checking every possible placement on 'AlignTo' field so it will be much quicker but less precise."; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.WholeFieldPlacement; } }
        public override Vector2 NodeSize { get { return new Vector2(202, _EditorFoldout ? 196 : 159); } }
        public override Color GetNodeColor() { return new Color(0.1f, 0.7f, 1f, 0.95f); }
        public override int OutputConnectorsCount { get { return 2; } }
        public override int HotOutputConnectionIndex { get { return 1; } }
        public override int AllowedOutputConnectionIndex { get { return resultIndex; } }
        protected int resultIndex = 0;
        public override bool IsFoldable { get { return true; } }

        public override string GetOutputHelperText(int outputId = 0)
        {
            if (outputId == 0) return "Fail";
            return "Success";
        }

        [Port(EPortPinType.Input, EPortNameDisplay.Default, EPortValueDisplay.HideOnConnected)] public PGGPlannerPort AlignTo;
        [Tooltip("Stoping search when finding first small placement (not precise)")]
        public bool FastCheck = false;

        //[Space(4)]
        //[Tooltip("Enables precise search for tight placement (can increase time of generating drastically)")]
        //public bool CheckAllCells = false;
        [Port(EPortPinType.Input, 1)] public FloatPort PushOut;
        [Port(EPortPinType.Input, 1)] public FloatPort SidesSpace;

        [Tooltip("It's useful for 'Walls Separation' feature")]
        protected float PushOutDistance = 0f;
        [Tooltip("It's useful for 'Walls Separation' feature")]
        protected float EnsureSidesSpace = 0f;

        //[Space(4)]
        ////public bool AllowRotateBy90 = true;

        //[HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGVector3Port TowardsDirection;
        [HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGCellPort ContactCell;
        [HideInInspector] [Port(EPortPinType.Output, EPortValueDisplay.HideValue)] public PGGCellPort AlignedToCell;
        //[HideInInspector] [Port(EPortPinType.Output, EPortNameDisplay.Default, EPortValueDisplay.HideValue)] public PGGPlannerPort AlignedTo;

        private CheckerField3D _checkerBackup;
        private Bounds _printBounds;
        //private CheckerField3D _printMask;

        private List<FieldPlanner> _snapTo = new List<FieldPlanner>();

        protected FieldCell myCell;
        protected FieldCell otherCell;
        protected bool forceBreak = false;
        protected bool skipIteration = false;

        public override void OnCreated()
        {
            base.OnCreated();
            _EditorFoldout = true;
        }

        //bool logs = false;

        protected FieldPlanner foundOn = null;
        protected FieldPlanner alignTo = null;
        protected FieldPlanner field = null;
        protected CheckerField3D checker = null;
        protected CheckerField3D alignToChecker;
        protected Vector3Int aligningDir;
        protected Vector3 smallestRootPos;
        protected Quaternion smallestRootRot;
        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            PushOut.TriggerReadPort(true);
            SidesSpace.TriggerReadPort(true);

            myCell = null;
            otherCell = null;
            forceBreak = false;
            skipIteration = false;

            PushOutDistance = PushOut.GetInputValue;
            EnsureSidesSpace = SidesSpace.GetInputValue;

            #region Get references, planner, field, checkers

            AlignTo.TriggerReadPort(true);
            _snapTo = GetPlannersFromPort(AlignTo, true, true, true);
            if (_snapTo == null) return;
            if (_snapTo.Count == 0) return;

            bool found = false;
            foundOn = null;
            alignTo = null;
            field = FieldPlanner.CurrentGraphExecutingPlanner;
            checker = field.LatestChecker;
            _checkerBackup = checker.Copy(); // Save current checker state

            for (int a = 0; a < _snapTo.Count; a++)
            {
                if (_snapTo[a] == FieldPlanner.CurrentGraphExecutingPlanner)
                {
                    if (_snapTo.Count == 1)
                    {
                        found = true;
                        if (Debugging) foundOn = null;
                    }

                    continue; // Snap to self
                }

                if (found) break;
                alignTo = _snapTo[a];

                alignToChecker = alignTo.LatestChecker;

                if (alignToChecker == null) return;
                if (alignToChecker.AllCells.Count == 0) return;

                if (FieldPlanner.CurrentGraphExecutingPlanner == null) return;
                if (FieldPlanner.CurrentGraphExecutingPlanner.ParentBuildPlanner == null) return;

                #endregion

                _printBounds = print.GetFullBounds(); // All placed fields bounding box
                CheckerField3D _rotedcheckerBackup = null; // Save current checker state

                if (ParentPlanner.AllowRotateBy90)
                {
                    _rotedcheckerBackup = checker.Copy();
                    _rotedcheckerBackup.RootRotation = Quaternion.Euler(0, 90, 0);
                }

                List<CheckerField3D> currentCheckers = print.GetCurrentCheckersList();

                // Collect just edge cells of target checker to be aligned with to avoid checking not wanted cells which are deep inside field body
                CheckerField3D checkerEdges = alignToChecker.GetInlineChecker(false, true, true, false);

                //List<FieldCell> edgeCells = checkerEdges.AllCells;
                List<FieldCell> edgeCells = IGeneration.GetRandomizedCells(checkerEdges.Grid);

                float smallestBSize = float.MaxValue;
                float rootBndsSize = GetBoundsSize(_printBounds);
                smallestRootPos = _checkerBackup.RootPosition;
                smallestRootRot = _checkerBackup.RootRotation;
                int mostAligns = 0;
                bool foundBreak = false;
                int rots = 1;
                if (ParentPlanner.AllowRotateBy90) rots = 2;

                //logs = false;
                int alignsCWant = GetWantedAligns(checker);

                for (int i = 0; i < edgeCells.Count; i++)
                {
                    var dirs = edgeCells[i].GetAvailableOutDirs();
                    if (foundBreak) break;

                    for (int d = 0; d < dirs.Length; d++)
                    {
                        if (foundBreak) break;
                        aligningDir = dirs[d];

                        for (int r = 0; r < rots; r++)
                        {
                            OnPlacementIteration();

                            if (forceBreak) { foundBreak = true; forceBreak = false; }
                            if (foundBreak) break;

                            Bounds predictedPrintBounds = GetBoundsPlacement(checker, checkerEdges, edgeCells[i], alignToChecker, currentCheckers, dirs[d], true);
                           
                            OnAfterPlacementIteration();
                            
                            if (skipIteration)
                            {
                                skipIteration = false;
                            }
                            else
                            {

                                if (predictedPrintBounds.size.x != float.MaxValue)
                                {
                                    float newBndsSize = GetBoundsSize(predictedPrintBounds);
                                    int aligns = checker.CountAlignmentsWith(checkerEdges);


                                    #region Debugging Backup
                                    //if (logs)
                                    //{
                                    //    if (aligns > 4)
                                    //    {
                                    //        FDebug.DrawBounds2D(checker.GetFullBoundsWorldSpace(), Color.yellow);
                                    //        FDebug.DrawBounds2D(predictedPrintBounds, Color.white);
                                    //        UnityEngine.Debug.DrawRay(checker.RootPosition, Vector3.up + Vector3.one * 0.1f, Color.red, 1.01f);
                                    //        UnityEngine.Debug.Log("aligns = " + aligns + " most = " + mostAligns);
                                    //    }
                                    //}
                                    #endregion



                                    if (aligns >= mostAligns)
                                    {
                                        //if (logs) FDebug.DrawBounds2D(checker.GetFullBoundsWorldSpace(), Color.green);
                                        if (newBndsSize <= rootBndsSize)
                                        {
                                            if (aligns > alignsCWant) foundBreak = true;
                                            found = true;
                                            if (Debugging) foundOn = alignTo;
                                            if (FastCheck) foundBreak = true;
                                            smallestRootPos = checker.RootPosition;
                                            smallestRootRot = checker.RootRotation;
                                            mostAligns = aligns;

                                            //AlignedTo.SetIDsOfPlanner(alignTo);
                                            GetContactCells(checker, alignToChecker, dirs[d], PushOutDistance, ref myCell, ref otherCell);

                                            #region Debugging Backup
                                            //if (logs)
                                            //{
                                            //    UnityEngine.Debug.DrawRay(smallestRootPos, Vector3.up + Vector3.one * 0.1f, Color.red, 1.01f);
                                            //    FDebug.DrawBounds2D(checker.GetFullBoundsWorldSpace(), Color.green);
                                            //}
                                            //break;
                                            #endregion

                                        }
                                        else
                                        if (newBndsSize < smallestBSize)
                                        {
                                            if (aligns > alignsCWant) foundBreak = true;
                                            found = true; if (FastCheck) foundBreak = true;
                                            if (Debugging) foundOn = alignTo;
                                            smallestBSize = newBndsSize;
                                            smallestRootPos = checker.RootPosition;
                                            smallestRootRot = checker.RootRotation;
                                            mostAligns = aligns;

                                            //AlignedTo.SetIDsOfPlanner(alignTo);
                                            GetContactCells(checker, alignToChecker, dirs[d], PushOutDistance, ref myCell, ref otherCell);

                                            #region Debugging Backup
                                            //if (logs) FDebug.DrawBounds2D(checker.GetFullBoundsWorldSpace(), Color.cyan);
                                            //break;
                                            //FDebug.DrawBounds2D(checker.GetFullBoundsWorldSpace(), Color.cyan);
                                            //FDebug.DrawBounds2D(predictedPrintBounds, Color.cyan);
                                            #endregion
                                        }

                                    } // alignments > than previous END

                                    OnFoundPlacementIteration();

                                }
                            }

                            if (rots == 2)
                            {
                                if (r == 0) checker = _rotedcheckerBackup.Copy();
                                else checker = _checkerBackup.Copy();
                            }
                            else checker = _checkerBackup.Copy();

                        }
                    }

                    checker = _checkerBackup.Copy();
                }


                #region Debugging Backup
                //if (logs)
                //{
                //    UnityEngine.Debug.DrawRay(smallestRootPos, Vector3.up + Vector3.one * 0.1f, Color.red, 1.01f);
                //    UnityEngine.Debug.Log("mostaligns = " + mostAligns);
                //}
                #endregion

                field.LatestChecker.RootPosition = smallestRootPos;
                field.LatestChecker.RootRotation = smallestRootRot;
            }


            if (!found)
            {
                ContactCell.Clear();
                AlignedToCell.Clear();
                field.LatestChecker.RootPosition = _checkerBackup.RootPosition;
                field.LatestChecker.RootRotation = _checkerBackup.RootRotation;
            }
            else
            {
                if (field) ContactCell.ProvideFullCellData(myCell, field.LatestChecker, field.LatestResult);
                if (alignTo) AlignedToCell.ProvideFullCellData(otherCell, alignTo.LatestChecker, alignTo.LatestResult);
            }

            if (Debugging)
            {
                if (!found)
                    DebuggingInfo = "Failed To find placement for " + field.ArrayNameString;
                else
                {
                    if (foundOn == null)
                        DebuggingInfo = "No field planner to fit on -> staying in the same placement";
                    else
                    {
                        DebuggingInfo = "Found placement on " + foundOn.ArrayNameString;
                    }
                }

                print._debugLatestExecuted = field.LatestChecker;
            }

            resultIndex = -2;
            resultIndex = found ? 1 : 0;
            //SetMultiOutputID(ref resultIndex, found ? 1 : 0);
        }


        protected virtual void OnPlacementIteration()
        {

        }

        protected virtual void OnAfterPlacementIteration()
        {

        }

        protected virtual void OnFoundPlacementIteration()
        {

        }

        public static void GetContactCells(CheckerField3D my, CheckerField3D other, Vector3Int dir, float pushOutDistance, ref FieldCell myCell, ref FieldCell otherCell)
        {
            Vector3 rootBackup = my.RootPosition;

            // Align with other to avoid far push out cell gaps
            if (pushOutDistance != 0f) my.RootPosition -= my.ScaleV3(dir.V3IntToV3()) * pushOutDistance;

            FieldCell oCell = my.FindCellOfInDir(other, dir.InverseV3Int(), 1);

            if (FGenerators.CheckIfIsNull(oCell))
            {
                UnityEngine.Debug.Log("not found alignment cell");
                my.RootPosition = rootBackup; // restore
                return;
            }
            else
            {
                //other.DebugLogDrawCellInWorldSpace(oCell, Color.red);
                //my.DebugLogDrawCellInWorldSpace(my._FindCellOfInDir_MyCell, Color.green);
            }

            FieldCell mCell = my._FindCellOfInDir_MyCell;

            // Search for most centered placement in relation to other alignment area
            oCell = my.GetMostCenteredCellInAxis(other, mCell, oCell, PGGUtils.GetRotatedFlatDirectionFrom(dir));
            mCell = my._GetMostCenteredCellInAxis_MyCell;


            my.RootPosition = rootBackup; // restore
            myCell = mCell;
            otherCell = oCell;
        }


        float GetBoundsSize(Bounds b)
        {
            return (b.max - b.min).magnitude;
        }


        Bounds GetBoundsPlacement(CheckerField3D checker, CheckerField3D inline, FieldCell cell, CheckerField3D alignToChecker, List<CheckerField3D> fullMask, Vector3Int dir, bool breakOnAnyCollWithMask)
        {
            CheckerField3D.DebugHelper = false;

            checker.SetRootPositionInWorldPosCentered(inline.GetWorldPos(cell), false);
            checker.RoundRootPositionAccordingly(inline);

            checker.RootPosition += checker.ScaleV3(dir);

            checker.StepPushOutOfCollision(alignToChecker, dir);

            if (PushOutDistance != 0f) checker.RootPosition += checker.ScaleV3(dir.V3IntToV3()) * PushOutDistance;

            if (EnsureSidesSpace > 0f)
            {
                Vector3Int side = PGGUtils.GetRotatedFlatDirectionFrom(FVectorMethods.ChooseDominantAxis(dir.V3IntToV3()).V3toV3Int());

                if (checker.CheckCollisionOnSide(side, EnsureSidesSpace, fullMask))
                {
                    return new Bounds(Vector3.zero, Vector3.one * float.MaxValue);
                }
                else
                {
                    if (checker.CheckCollisionOnSide(side.InverseV3Int(), EnsureSidesSpace, fullMask))
                        return new Bounds(Vector3.zero, Vector3.one * float.MaxValue);
                    else
                    {
                        if (checker.CheckCollisionOnSide(dir, EnsureSidesSpace, fullMask))
                            return new Bounds(Vector3.zero, Vector3.one * float.MaxValue);
                    }
                }
            }


            #region Debugging Backup
            //CheckerField3D.DebugHelper =
            //    (FieldPlanner.CurrentGraphExecutingPlanner.IndexOfDuplicate == 0 &&
            //    //checker.RootPosition.x == 8f &&
            //    checker.RootPosition.z == -5f &&
            //    checker.RootRotation.eulerAngles.y == 0f);

            //if (CheckerField3D.DebugHelper)
            //{
            //    for (int f = 0; f < fullMask.Count; f++)
            //    {
            //        fullMask[f].DebugLogDrawBoundings(Color.white);
            //    }
            //    //fullMask.DebugLogDrawBoundings(Color.red);
            //}
            #endregion


            if (breakOnAnyCollWithMask) if (checker.IsCollidingWith(fullMask))
                {
                    #region Debugging Backup
                    //if (logs)
                    //{
                    //    FDebug.DrawBounds2D(checker.GetFullBoundsWorldSpace(), Color.red);
                    //}
                    #endregion
                    return new Bounds(Vector3.zero, Vector3.one * float.MaxValue);
                }

            Bounds r = new Bounds(_printBounds.center, _printBounds.size);
            r.Encapsulate(checker.GetFullBoundsWorldSpace());

            return r;
        }


        protected virtual int GetWantedAligns(CheckerField3D checker)
        {
            return Mathf.RoundToInt(Mathf.Clamp(Mathf.Log10(checker.ChildPositionsCount) * 6, 3, 8));
        }


        #region Editor Related

#if UNITY_EDITOR
        private UnityEditor.SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);

            if (_EditorFoldout)
            {
                GUILayout.Space(1);

                if (sp == null) sp = baseSerializedObject.FindProperty("ContactCell");
                UnityEditor.SerializedProperty scp = sp.Copy();
                //UnityEditor.EditorGUILayout.PropertyField(scp); scp.Next(false);
                UnityEditor.EditorGUILayout.PropertyField(scp); scp.Next(false);
                //UnityEditor.EditorGUILayout.PropertyField(scp); scp.Next(false);
                UnityEditor.EditorGUILayout.PropertyField(scp);

                //if (sp == null) sp = baseSerializedObject.FindProperty("ConnectedSelfCell");
                //UnityEditor.SerializedProperty scp = sp.Copy();
                //UnityEditor.EditorGUILayout.PropertyField(scp); scp.Next(false);
                //UnityEditor.EditorGUILayout.PropertyField(scp);
            }

            //if (WantAlignPoints.Value < 1) WantAlignPoints.Value = 1;
        }

        public override void Editor_OnAdditionalInspectorGUI()
        {
            EditorGUILayout.LabelField("Debugging:", EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            if (FGenerators.CheckIfIsNull(myCell)) EditorGUILayout.LabelField("Null contact cell");
            else EditorGUILayout.LabelField("Contact cell pos: " + myCell.Pos);
            if (FGenerators.CheckIfExist_NOTNULL(myCell)) if (ContactCell.CellData.ParentChecker != null) EditorGUILayout.LabelField("Checker Cells: " + ContactCell.CellData.ParentChecker.ChildPositionsCount);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (FGenerators.CheckIfIsNull(otherCell)) EditorGUILayout.LabelField("Null align cell");
            else EditorGUILayout.LabelField("Align cell pos: " + otherCell.Pos);
            if (FGenerators.CheckIfExist_NOTNULL(myCell)) if (AlignedToCell.CellData.ParentChecker != null) EditorGUILayout.LabelField("Checker Cells: " + AlignedToCell.CellData.ParentChecker.ChildPositionsCount);
            EditorGUILayout.EndHorizontal();
        }

#endif

        #endregion


    }
}