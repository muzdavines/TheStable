using UnityEngine;
using System.Collections.Generic;
using FIMSpace.Generating.Planning.PlannerNodes;
using System;
using FIMSpace.Graph;
using FIMSpace.Generating.Checker;

namespace FIMSpace.Generating.Planning
{
    public partial class FieldPlanner
    {
        /// <summary> If this planner is an duplicate, then parent will be the source planner </summary>
        [System.NonSerialized] public FieldPlanner DuplicateParent = null;
        [System.NonSerialized] private List<FieldPlanner> duplicatePlanners = null;
        [System.NonSerialized] public bool WasExecuted = false;
        [System.NonSerialized] public bool Discarded = false;

        /// <summary> If preapred, already executed and not discarded </summary>
        public bool Available { get { return WasExecuted && !Discarded; } }
        public bool IsDuplicate { get { return DuplicateParent != null; } }

        /// <summary> Called when generating scene objects with grid painter, the object is generator component (grid painter) </summary>
        public List<Action<object>> OnGeneratingEvents { get; private set; }
        /// <summary> Action which will be called when generating scene objects with grid painter, the 'object' is generator component (grid painter) </summary>
        public void AddOnGeneratingEvent(Action<object> action)
        {
            if (OnGeneratingEvents == null) OnGeneratingEvents = new List<Action<object>>();
            OnGeneratingEvents.Add(action);
        }


        internal FieldPlanner GetPlannerByUniqueID(int uniqueId)
        {
            if (uniqueId == -1) return null;
            if (ParentBuildPlanner == null) return null;

            for (int i = 0; i < ParentBuildPlanner.BasePlanners.Count; i++)
            {
                var pl = ParentBuildPlanner.BasePlanners[i];
                if (i == uniqueId) return pl;
            }

            return null;
        }

        internal List<FieldPlanner> GetPlannersList()
        {
            if (ParentBuildPlanner == null) return null;
            return ParentBuildPlanner.BasePlanners;
        }


        internal int GetNodeHelperIterationIndex()
        {
            if (ParentBuildPlanner) return ParentBuildPlanner.GenerationIteration;
            return 0;
        }

        internal void CallFromParentLayer(BuildPlannerPreset.BuildPlannerLayer buildPlannerLayer)
        {
            parentLayer = buildPlannerLayer;
        }

        internal void Discard(PlanGenerationPrint print)
        {
            Discarded = true;
        }

        /// <summary> Setting planner's checker root position to provided world position and rounding it if rounding is enabled in this planner </summary>
        internal void SetCheckerWorldPosition(Vector3 newPosition)
        {
            if (LatestResult == null) return;
            if (LatestResult.Checker == null) return;

            LatestResult.Checker.RootPosition = newPosition;
            if (RoundToScale) LatestResult.Checker.RoundRootPositionToScale();
        }

        internal void SetCheckerWorldPosition(CheckerField3D checker, Vector3 newPosition)
        {
            if (LatestResult == null) return;
            if (checker == null) return;

            checker.RootPosition = newPosition;
            if (RoundToScale) checker.RoundRootPositionToScale();
        }

        internal List<FieldPlanner> GetDuplicatesPlannersList()
        {
            if (DuplicateParent) return DuplicateParent.duplicatePlanners;
            return duplicatePlanners;
        }

        /// <summary>
        /// Checking if planner if enabled, executed and not discarded
        /// </summary>
        public bool IsValid()
        {
            if (DisableWholePlanner) return false;
            if (WasExecuted == false) return false;
            if (Discarded) return false;
            return true;
        }

        internal void ResetForGenerating()
        {
            PreparationDone = false;
            PreparationWasDoneFlag = false;
            PreparationWasStarted = false;

            ExecutionDone = false;
            ExecutionWasDoneFlag = false;
            ExecutionWasStarted = false;

            PostExecutionDone = false;
            PostExecutionDoneFlag = false;
            PostExecutionWasStarted = false;

            WasExecuted = false;
            Discarded = false;
        }


        internal void PrepareForGenerating(int indexOnPreset, int preparationIndex, PlanGenerationPrint print)
        {
            int i = indexOnPreset;

            if (OnGeneratingEvents == null) OnGeneratingEvents = new List<Action<object>>();
            OnGeneratingEvents.Clear();

            PlannersInBuild[i].PrepareOnPrint(print, -1);

            for (int d = 0; d < PlannersInBuild[i].Duplicates; d++)
            {
                PlannersInBuild[i].GetDuplicatesPlannersList()[d].PrepareOnPrint(print, d);
            }

            PreparationDone = true;
            //UnityEngine.Debug.Log("prepared " + indexOnPreset);
        }

        // Planners

        private int[] _plannerIds = null;
        internal int[] GetPlannersIDList(bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _plannerIds == null || _plannerIds.Length != PlannersInBuild.Count)
            {
                _plannerIds = new int[ParentBuildPlanner.BasePlanners.Count];
                for (int i = 0; i < PlannersInBuild.Count; i++)
                {
                    _plannerIds[i] = i;
                }
            }

            return _plannerIds;
        }

        internal void AddDuplicateReference(FieldPlanner fieldPlanner)
        {
            if (duplicatePlanners == null) duplicatePlanners = new List<FieldPlanner>();
            fieldPlanner.DuplicateParent = this;
            duplicatePlanners.Add(fieldPlanner);
        }

        private GUIContent[] _plannerNames = null;
        internal GUIContent[] GetPlannersNameList(bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _plannerNames == null || _plannerNames.Length != PlannersInBuild.Count)
            {
                _plannerNames = new GUIContent[ParentBuildPlanner.BasePlanners.Count];
                for (int i = 0; i < PlannersInBuild.Count; i++)
                {
                    _plannerNames[i] = new GUIContent("[" + i + "] " + PlannersInBuild[i].name, "Parent build plan : " + ParentBuildPlanner.name);
                }
            }
            return _plannerNames;
        }


        // Planner Variables


        private int[] _VariablesIds = null;

        internal int[] GetVariablesIDList(bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _VariablesIds == null || _VariablesIds.Length != Variables.Count)
            {
                _VariablesIds = new int[Variables.Count];
                for (int i = 0; i < Variables.Count; i++)
                {
                    _VariablesIds[i] = i;
                }
            }

            return _VariablesIds;
        }

        internal int[] GetBuildVariablesIDList(bool forceRefresh = false)
        {
            return ParentBuildPlanner.GetVariablesIDList(forceRefresh);
        }

        internal GUIContent[] GetBuildVariablesNameList(bool forceRefresh = false)
        {
            return ParentBuildPlanner.GetVariablesNameList(forceRefresh);
        }

        private GUIContent[] _VariablesNames = null;
        internal GUIContent[] GetVariablesNameList(bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _VariablesNames == null || _VariablesNames.Length != Variables.Count)
            {
                _VariablesNames = new GUIContent[Variables.Count];
                for (int i = 0; i < Variables.Count; i++)
                {
                    _VariablesNames[i] = new GUIContent(Variables[i].Name);
                }
            }
            return _VariablesNames;
        }



        // Extras

        internal static IPlanNodesContainer GetNodesContainer(PlannerRuleBase nd)
        {
            if (nd)
            {
                if (nd.ParentNodesContainer != null) if (nd.ParentNodesContainer is IPlanNodesContainer) return nd.ParentNodesContainer as IPlanNodesContainer;
                return nd.ParentPlanner;
            }

            return null;
        }

    }
}