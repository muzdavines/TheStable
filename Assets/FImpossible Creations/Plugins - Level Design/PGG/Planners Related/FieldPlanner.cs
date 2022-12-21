using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Planning.GeneratingLogics;
using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Checker;
using FIMSpace.Graph;
using System;
using FIMSpace.Generating.Planner.Nodes;

namespace FIMSpace.Generating.Planning
{
    /// <summary>
    /// It's never sub-asset -> it's always project file asset
    /// </summary>
    public partial class FieldPlanner : ScriptableObject, IPlanNodesContainer
    {
        public static FieldPlanner CurrentGraphExecutingPlanner = null;
        public static FieldPlanner CurrentGraphPreparingPlanner = null;

        /// <summary> Assigned automatically at start if null </summary>
        private BuildPlannerPreset.BuildPlannerLayer parentLayer = null;

        /// <summary> For future implementation of build layers </summary>
        public List<FieldPlanner> GetPlanners()
        {
            if (parentLayer == null) ParentBuildPlanner.RefreshBuildLayers();
            return parentLayer.FieldPlanners;
        }

        public enum EFieldType
        {
            FieldPlanner,
            BuildField,
            InternalField
        }

        [HideInInspector] public BuildPlannerPreset ParentBuildPlanner;
        [NonSerialized] public ShapeGeneratorBase _tempOverrideShape = null;
        [HideInInspector] public ShapeGeneratorBase ShapeGenerator = null;
        public bool DisableWholePlanner = false;
        public bool DontGenerateIt { get { return DisableWholePlanner || Discarded; } }

        public string tag = "";

        public PE_Start ProceduresBegin { get { return proceduresBegin; } }

        [SerializeField, HideInInspector] private PE_Start proceduresBegin;
        public PE_Start PostProceduresBegin { get { return postProceduresBegin; } }
        [SerializeField, HideInInspector] private PE_Start postProceduresBegin;

        public List<PGGPlanner_NodeBase> FProcedures = new List<PGGPlanner_NodeBase>();
        public List<PGGPlanner_NodeBase> FPostProcedures = new List<PGGPlanner_NodeBase>();
        public List<FieldVariable> FVariables = new List<FieldVariable>();

        [HideInInspector] public bool ExposeShape = true;

        [HideInInspector] public EFieldType FieldType = EFieldType.FieldPlanner;

        [Tooltip("You can assign other FieldSetups later, in the BuildPlannerExecutor component")]
        [HideInInspector] public FieldSetup DefaultFieldSetup = null;

        [Space(4)]
        [PGG_SingleLineTwoProperties("ExposeInstanceCount", 86, 140, 18)] public int Instances = 1;
        /// <summary> Returning Instances-1 </summary>
        public int Duplicates { get { return Instances - 1; } }
        /// <summary> Returning duplicateIndex + 1 </summary>
        public int InstanceIndex { get { return IndexOfDuplicate + 1; } }
        [HideInInspector] [Tooltip("Toggle if you want to allow changing duplicates count in executor")] public bool ExposeInstanceCount = false;

        [Space(6)]

        [PGG_SingleLineTwoProperties("AlwaysPushOut", 106, 124)] public bool DisableCollision = false;
        [HideInInspector] public bool AlwaysPushOut = true;


        //[PGG_SingleLineTwoProperties("AllowRotateBy90", 96, 124)] public bool RoundPosition = true;
        /*[HideInInspector] */public bool AllowRotateBy90 = true;

        [Space(7)]
        //[PGG_SingleLineTwoProperties("RoundToScale", 120, 124, 18)]
        [Tooltip("Size of single cell used for this field planner preview (in the executor component size will adapt to the FieldSetup cell size)")]
        public Vector3 PreviewCellSize = new Vector3(1f, 1f, 1f);

        //[Space(6)]
        //[Tooltip("Making position be only full numbers like  1 , 2 , 3  rounding fractions like  1.4  2.7  etc.")]
        [Tooltip("Rounding field position accordingly to the scale, so there is no x = 0.5 but x = 0  x = 1 etc.")]
        public bool RoundToScale = false;



        [HideInInspector] public bool UseCheckerScale = false;
        [HideInInspector] public Vector3 CheckerScale = Vector3.one;

        public Vector3 GetScale { get { if (UseCheckerScale) return CheckerScale; else return PreviewCellSize; } }
        public float GetScaleF { get { if (UseCheckerScale) return CheckerScale.x; else return PreviewCellSize.x; } }


        public int MaxRetries = 64;

        private CheckerField3D previewChecker;
        public PlannerResult LatestResult;
        public CheckerField3D LatestChecker { get { if (LatestResult == null) return GetInitialChecker(); return LatestResult.Checker; } }
        public int IndexOnPrint = -1;
        public int IndexOfDuplicate = -1;
        public int IndexOnPreset = -1;
        private string printName;

        public enum EViewGraph { Procedures_Placement, PostProcedures_Cells }
        [HideInInspector] public EViewGraph GraphView = EViewGraph.Procedures_Placement;
        public string ArrayNameString
        {
            get
            {
                if (IsDuplicate) return "[" + IndexOnPreset + "][" + IndexOfDuplicate + "]";
                else return "[" + IndexOnPreset + "]";
            }
        }


        #region Async Support Variables

        [System.NonSerialized] public bool ExecutionWasStarted = false;
        [System.NonSerialized] public bool ExecutionDone = false;
        [System.NonSerialized] public bool ExecutionWasDoneFlag = false;

        [System.NonSerialized] public bool PostExecutionWasStarted = false;
        [System.NonSerialized] public bool PostExecutionDone = false;
        [System.NonSerialized] public bool PostExecutionDoneFlag = false;

        [System.NonSerialized] public bool PreparationWasStarted = false;
        [System.NonSerialized] public bool PreparationDone = false;
        [System.NonSerialized] public bool PreparationWasDoneFlag = false;

        #endregion


        public List<PGGPlanner_NodeBase> Procedures { get { return FProcedures; } }

        public List<PGGPlanner_NodeBase> PostProcedures { get { return FPostProcedures; } }

        public List<FieldVariable> Variables { get { return FVariables; } }

        public ScriptableObject ScrObj { get { return this; } }

        public FieldPlanner.LocalVariables GraphLocalVariables
        {
            get
            {
                if (localVars == null) RefreshLocalVariables();
                return localVars;
            }
        }

        private FieldPlanner.LocalVariables localVars;

        private void Awake()
        {
            RefreshStartGraphNodes();

            //if (Variables == null || Variables.Count == 0)
            //{
            //    Variables = new List<FieldVariable>();
            //    FieldVariable def = new FieldVariable("Spawn Propability Multiplier", 1f);
            //    def.helper.x = 0; def.helper.y = 5;
            //    Variables.Add(def);

            //    def = new FieldVariable("Spawn Count Multiplier", 1f);
            //    def.helper.x = 0; def.helper.y = 5;
            //    Variables.Add(def);
            //}

        }


        void OnValidate()
        {
            RefreshStartGraphNodes();
        }

        public void RefreshStartGraphNodes()
        {
            //proceduresBegin = null;

            for (int p = 0; p < Procedures.Count; p++)
            {
                if (Procedures[p] is PE_Start)
                {
                    proceduresBegin = Procedures[p] as PE_Start;
                    break;
                }
            }

            for (int p = 0; p < PostProcedures.Count; p++)
            {
                if (PostProcedures[p] is PE_Start)
                {
                    postProceduresBegin = PostProcedures[p] as PE_Start;
                    break;
                }
            }
        }

        internal void RefreshGraphs()
        {
            FGraph_RunHandler.RefreshConnections(Procedures);
            FGraph_RunHandler.RefreshConnections(PostProcedures);
        }

        public void RefreshPreviewWith(CheckerField3D checker)
        {
            if (ParentBuildPlanner == null) return;

            //UnityEngine.Debug.Log("checker[ " + IndexOnPrint + "  /  " + ParentBuildPlanner.LatestGenerated.PlannerResults.Count + "]");
            if (DuplicateParent == null)
            {
                ParentBuildPlanner.LatestGenerated.PlannerResults[IndexOnPrint].Checker = checker;
            }
            else
            {
                ParentBuildPlanner.LatestGenerated.PlannerResults[IndexOnPrint].DuplicateResults[IndexOfDuplicate].Checker = checker;
            }

        }

        public void PrepareOnPrint(PlanGenerationPrint gen, int duplicateId)
        {
            CurrentGraphPreparingPlanner = this;
            IndexOfDuplicate = duplicateId;
            LatestResult = PlannerResult.GenerateInstance(ParentBuildPlanner, this);

            if (duplicateId >= 0) // If it's duplicate -> include it inside main planner duplicates list
            {
                gen.PlannerResults[IndexOnPrint].AddDuplicateResultSlot(PlannerResult.GenerateInstance(ParentBuildPlanner, this));
            }
            else // Add base result and prepare support for duplicates
            {
                LatestResult.PrepareDuplicateSupport();
                gen.PlannerResults.Add(LatestResult);
            }

            PrepareInitialChecker();
            LatestResult.Checker = GetInitialChecker();

            if (duplicateId >= 0) // If it's duplicate -> include it inside main planner duplicates list
            {
                gen.PlannerResults[IndexOnPrint].DuplicateResults[duplicateId] = LatestResult.Copy();
            }
            else // Add base result and prepare support for duplicates
            {
                gen.PlannerResults[IndexOnPrint] = LatestResult;
            }

            if (ParentBuildPlanner.OnIteractionCallback != null)
            {

                if (PlannerRuleBase.Debugging)
                {
                    gen.DebugInfo = "Initializing  " + printName;
                    gen._debugLatestExecuted = LatestResult.Checker;
                }

                ParentBuildPlanner.OnIteractionCallback.Invoke(gen);
            }
        }

        /// <summary> Can't be async </summary>
        public void PrepareProcedures()
        {
            for (int i = 0; i < FProcedures.Count; i++)
            {
                if (FProcedures[i] == null) continue;
                if (FProcedures[i].Enabled == false) continue;
                FProcedures[i].ToRB().PreGeneratePrepare();
            }

            for (int i = 0; i < FPostProcedures.Count; i++)
            {
                if (FPostProcedures[i] == null) continue;
                if (FPostProcedures[i].Enabled == false) continue;
                FPostProcedures[i].ToRB().PreGeneratePrepare();
            }
        }

        public void RefreshOnReload()
        {
            RefreshLocalVariables();
        }

        void RefreshLocalVariables()
        {
            if (localVars == null) localVars = new FieldPlanner.LocalVariables(this);
            localVars.RefreshList();
        }


        /// <summary> Can't be async </summary>
        internal void PrePrepareForGenerating(int indexOnPreset, int preparationIndex)
        {
            RefreshStartGraphNodes();
            RefreshLocalVariables();
            //RefreshGraphs();

            printName = name;

            PreparationWasStarted = true;

            if (duplicatePlanners != null) duplicatePlanners.Clear();

            IndexOnPreset = indexOnPreset;
            int i = indexOnPreset;

            IndexOnPrint = preparationIndex;

            PGGUtils.CheckForNulls(FProcedures);
            PGGUtils.CheckForNulls(FPostProcedures);

            PrepareProcedures();
            RefreshGraphs();

            for (int p = 0; p < Procedures.Count; p++)
            {
                Procedures[p].ToRB().ParentPlanner = this;
            }

            for (int p = 0; p < PostProcedures.Count; p++)
            {
                PostProcedures[p].ToRB().ParentPlanner = this;
            }

            for (int d = 0; d < PlannersInBuild[i].Duplicates; d++)
            {
                var dupl = Instantiate(PlannersInBuild[i]);
                dupl.IndexOfDuplicate = d;
                PlannersInBuild[i].AddDuplicateReference(dupl);
                dupl.PrepareProcedures();
            }
        }

        /// <summary>
        /// Call before RunStartProcedures. Can't be async
        /// </summary>
        public void PreRunProcedures(PlanGenerationPrint gen)
        {
            CurrentGraphExecutingPlanner = this;

            for (int i = 0; i < FProcedures.Count; i++)
            {
                if (FProcedures[i].Enabled == false) continue;
                FProcedures[i].ToRB().Prepare(gen);
            }
        }

        /// <summary>
        /// Call before RunStartProcedures. Can't be async
        /// </summary>
        public void PreRunPostProcedures(PlanGenerationPrint gen)
        {
            CurrentGraphExecutingPlanner = this;

            for (int i = 0; i < FPostProcedures.Count; i++)
            {
                if (FPostProcedures[i].Enabled == false) continue;
                FPostProcedures[i].ToRB().Prepare(gen);
            }
        }

        public void RunStartProcedures(PlanGenerationPrint gen)
        {
            ExecutionWasStarted = true;
            WasExecuted = true;

            if (proceduresBegin == null)
            {
                CompleteGenerating();
                return;
            }
            
            if (proceduresBegin.OutputConnections.Count == 0)
            {
                CompleteGenerating();
                return;
            }

            PlannerRuleBase operation = proceduresBegin.FirstOutputConnection as PlannerRuleBase;
            if (operation == null)
            {
                CompleteGenerating();
                return;
            }

            CurrentGraphExecutingPlanner = this;

            CallExecution(operation, gen);

            CompleteGenerating();
        }


        public void RunPostProcedures(PlanGenerationPrint gen)
        {
            PostExecutionWasStarted = true;

            if (postProceduresBegin == null)
            {
                CompletePostGenerating();
                return;
            }

            if (postProceduresBegin.OutputConnections.Count == 0)
            {
                CompletePostGenerating();
                return;
            }

            PlannerRuleBase operation = postProceduresBegin.FirstOutputConnection as PlannerRuleBase;
            if (operation == null)
            {
                CompletePostGenerating();
                return;
            }

            CurrentGraphExecutingPlanner = this;
            CallExecution(postProceduresBegin, gen);

            CompletePostGenerating();
        }


        internal void CallExecution(PlannerRuleBase rule, PlanGenerationPrint newResult)
        {
            rule.Execute(newResult, LatestResult);

            if (IndexOnPrint != -1)
            {
                if (DuplicateParent == null)
                {
                    newResult.PlannerResults[IndexOnPrint] = LatestResult;
                }
                else
                {
                    if (newResult.PlannerResults[IndexOnPrint].DuplicateResults != null)
                    {
                        newResult.PlannerResults[IndexOnPrint].DuplicateResults[IndexOfDuplicate] = LatestResult;
                    }
                }
            }

            if (ParentBuildPlanner.OnIteractionCallback != null)
            {
                if (PlannerRuleBase.Debugging)
                {
                    newResult.DebugInfo = "Field Planner '" + name + "'\n\nProcedure: " + rule.GetDisplayName() + "\n\n" + rule.DebuggingInfo;
                    newResult.DebugGizmosAction = rule.DebuggingGizmoEvent;
                }

                ParentBuildPlanner.OnIteractionCallback.Invoke(newResult);
            }

            //UnityEngine.Debug.Log(start._E_GetDisplayName() + " connections = " + start.OutputConnections.Count);
            if (rule.FirstOutputConnection == null)
            {
                return;
            }

            if (rule.AllowedOutputConnectionIndex > -1)
            {
                for (int c = 0; c < rule.OutputConnections.Count; c++)
                {
                    if (rule.OutputConnections[c].ConnectionFrom_AlternativeID != rule.AllowedOutputConnectionIndex) continue;
                    //UnityEngine.Debug.Log("out index = " + start.AllowedOutputConnectionIndex + " alt ind = " + start.OutputConnections[c].ConnectionFrom_AlternativeID);

                    CallExecution(
                    rule.OutputConnections[c].GetOther(rule) as PlannerRuleBase,
                     newResult);
                }
            }
            else
            {
                for (int c = 0; c < rule.OutputConnections.Count; c++)
                {
                    CallExecution(
                    rule.OutputConnections[c].GetOther(rule) as PlannerRuleBase,
                     newResult);
                }
            }

        }

        void CompleteGenerating()
        {
            ExecutionDone = true;
        }

        void CompletePostGenerating()
        {
            PostExecutionDone = true;
        }

        internal void OnCompleateAllGenerating()
        {
            if (!Discarded)
                if (LatestResult != null)
                {
                    for (int c = 0; c < LatestResult.CellsInstructions.Count; c++)
                    {
                        if (FGenerators.NotNull(LatestResult.CellsInstructions[c].HelperCellRef)) continue;

                        FieldCell setCellRef = 
                            LatestResult.Checker.GetCell(LatestResult.CellsInstructions[c].HelperCellRef.Pos);
                        
                        LatestResult.CellsInstructions[c].HelperCellRef = setCellRef;
                    }
                }

            if (IsDuplicate == false)
                if (duplicatePlanners != null)
                    for (int d = 0; d < Duplicates; d++)
                    {
                        duplicatePlanners[d].OnCompleateAllGenerating();
                    }
        }


#if UNITY_EDITOR
#endif

    }

}