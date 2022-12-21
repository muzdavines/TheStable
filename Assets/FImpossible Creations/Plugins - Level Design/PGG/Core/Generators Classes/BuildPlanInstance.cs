using FIMSpace.Generating.Checker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Planning
{

    public class BuildPlanInstance
    {
        public List<BuildPlanInstance> Connections = new List<BuildPlanInstance>();
        public CheckerField Checker;
        public SingleInteriorSettings SettingsReference { get; private set; }
        public int TargetDoorsCount = 1000;
        public int HelperID = 0;
        public float HelperVar = 0f;

        public bool HaveFreeConnectionSlots()
        {
            if (Connections.Count >= TargetDoorsCount) return false;
            return true;
        }

        public bool CanConnectWith(BuildPlanInstance other)
        {
            if (FGenerators.CheckIfIsNull(other)) return false;
            if (TargetDoorsCount == 0) return false;
            if (other.TargetDoorsCount == 0) return false;

            if (SettingsReference != null) // Check override allows
                if (other.SettingsReference != null)
                    if (SettingsReference.OverrideAllowDoorConnectionWithNamed.Count > 0)
                    {
                        if (SettingsReference.OverrideAllowDoorConnectionWithNamed.Contains(other.SettingsReference.GetName()))
                        {
                            return true;
                        }
                    }

            if (Connections.Count >= TargetDoorsCount) return false;
            if (SettingsReference == null) return true;

            if (other.SettingsReference == null) return true;
            else
            {
                if (other.SettingsReference.NotAllowDoorConnectionWithNamed.Contains(SettingsReference.GetName()))
                {
                    return false;
                }
                else if (SettingsReference.NotAllowDoorConnectionWithNamed.Contains(other.SettingsReference.GetName()))
                {
                    return false;
                }
            }

            return true;
        }

        public BuildPlanInstance(SingleInteriorSettings set, bool center = false, bool setSize = true)
        {
            Checker = new CheckerField();
            if (set != null)
            {
                SetSettings(set);

                if (setSize)
                    Checker = set.GetChecker(center);
                else
                    Checker = new CheckerField();
            }
        }

        public void SetSettings(SingleInteriorSettings set)
        {
            SettingsReference = set;
            TargetDoorsCount = set.JustOneDoor ? 1 : set.DoorConnectionsCount.GetRandom();
            if (FGenerators.CheckIfExist_NOTNULL(set.FieldSetup)) if (FGenerators.CheckIfExist_NOTNULL(Checker)) Checker.HelperReference = set.FieldSetup;
        }

        public void AssignFieldSetupReference(FieldSetup setup)
        {
            if (FGenerators.CheckIfExist_NOTNULL(Checker)) Checker.HelperReference = setup;
        }

        internal void AssignFieldSetupReference(BuildPlanInstance buildPlanInstance)
        {
            if (FGenerators.CheckIfExist_NOTNULL(buildPlanInstance))
                if (FGenerators.CheckIfExist_NOTNULL(buildPlanInstance.SettingsReference))
                    Checker.HelperReference = buildPlanInstance.SettingsReference.FieldSetup;
        }

        public bool Enabled = false;

        internal void DrawGizmos(float scale)
        {
            Checker.DrawGizmos(scale);
        }

        internal void SpreadDataInShapeOfChecker(CheckerField checker, string spreadCheckerData)
        {
            for (int i = 0; i < checker.ChildPos.Count; i++)
            {
                Checker.SpreadData(checker.WorldPos(i), 1, spreadCheckerData);
            }
        }

        internal void SpreadDataOn(BuildPlanInstance otherPlan)
        {
            if (SettingsReference != null)
                if (string.IsNullOrEmpty(SettingsReference.SpreadCheckerData) == false)
                    otherPlan.SpreadDataInShapeOfChecker(Checker, SettingsReference.SpreadCheckerData);
        }
    }

    public class GridPlanGeneratingHelper
    {
        public FGenGraph<FieldCell, FGenPoint> grid;
        public List<SpawnInstruction> guides;
        public BuildPlanInstance fieldInstance;
        /// <summary> field instance should be used for field setup settings but this variable can be used for simplier usage</summary>
        public FieldSetup SimplierAssign;

        public GridPlanGeneratingHelper(BuildPlanInstance instance = null)
        {
            grid = IGeneration.GetEmptyFieldGraph();
            guides = new List<SpawnInstruction>();
            if (instance == null) fieldInstance = new BuildPlanInstance(null);
            else fieldInstance = instance;
        }

        public InstantiatedFieldInfo GenerateOnGrid(Transform parent)
        {
            if (fieldInstance.SettingsReference == null)
            {
                if (SimplierAssign != null)
                {
                    return IGeneration.GenerateFieldObjectsWithContainer(SimplierAssign.name, SimplierAssign, grid, parent, guides, null);
                }

                Debug.Log("Null settings reference in generator " + parent.transform);
                return null;
            }

            return IGeneration.GenerateFieldObjectsWithContainer(fieldInstance.SettingsReference.GetName(), fieldInstance.SettingsReference.FieldSetup, grid, parent, guides, fieldInstance.SettingsReference.InjectMods);
        }

        public InstantiatedFieldInfo GenerateOnGrid(Transform parent, List<InjectionSetup> injections)
        {
            if (fieldInstance.SettingsReference == null)
            {
                if (SimplierAssign != null)
                    return IGeneration.GenerateFieldObjectsWithContainer(SimplierAssign.name, SimplierAssign, grid, parent, guides, injections);

                Debug.Log("Null settings reference in generator " + parent.transform);
                return null;
            }

            return IGeneration.GenerateFieldObjectsWithContainer(fieldInstance.SettingsReference.GetName(), fieldInstance.SettingsReference.FieldSetup, grid, parent, guides, injections);
        }
    }

}