using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Planning
{
    public partial class BuildPlannerPreset
    {
        [System.Serializable]
        public class BuildPlannerLayer
        {
            public string Name = "Build Layer";
            public List<FieldPlanner> FieldPlanners = new List<FieldPlanner>();
        }


        #region ID Lists for Build Layers

        private int[] _LayersIds = null;

        public int[] GetLayersIDList(bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _LayersIds == null || _LayersIds.Length != BuildLayers.Count)
            {
                _LayersIds = new int[BuildLayers.Count];
                for (int i = 0; i < BuildLayers.Count; i++)
                {
                    _LayersIds[i] = i;
                }
            }

            return _LayersIds;
        }

        internal void RefreshBuildLayers()
        {
            for (int l = 0; l < BuildLayers.Count; l++)
            {
                for (int i = 0; i < BuildLayers[l].FieldPlanners.Count; i++)
                {
                    BuildLayers[l].FieldPlanners[i].CallFromParentLayer(BuildLayers[l]);
                }
            }
        }

        private GUIContent[] _LayersNames = null;
        public GUIContent[] GetLayersNameList(bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _LayersNames == null || _LayersNames.Length != BuildLayers.Count)
            {
                _LayersNames = new GUIContent[BuildLayers.Count];
                for (int i = 0; i < BuildLayers.Count; i++)
                {
                    _LayersNames[i] = new GUIContent(BuildLayers[i].Name);
                }
            }
            return _LayersNames;
        }

        #endregion


        public Action<PlanGenerationPrint> OnIteractionCallback = null;


        internal FieldPlanner GetPlannerByIteration(int instId)
        {
            FieldPlanner planner = BasePlanners[0];
            int list = 0;
            int instance = 0;

            for (int i = 0; i <= instId; i++)
            {
                planner = BasePlanners[list];
                if (instance > 0) 
                    planner = planner.GetDuplicatesPlannersList()[instance - 1];

                instance += 1;

                if (instance >= BasePlanners[list].Instances)
                {
                    list += 1;
                    instance = 0;
                }

                if (list > BasePlanners.Count - 1) break;
            }

            //UnityEngine.Debug.Log("try get " + instId + " ret = " + planner.name + " : " + planner.ArrayNameString);

            return planner;
        }

        internal BuildPlannerPreset DeepCopy()
        {
            for (int i = 0; i < BasePlanners.Count; i++)
            {
                FGenerators.CheckForNulls(BasePlanners[i].Procedures);
                FGenerators.CheckForNulls(BasePlanners[i].PostProcedures);
            }

            BuildPlannerPreset nPlanner = Instantiate(this);

            for (int p = 0; p < BasePlanners.Count; p++)
            {
                var cpy = Instantiate(BasePlanners[p]);
                cpy.ParentBuildPlanner = nPlanner;
                nPlanner.BasePlanners[p] = cpy;

                for (int n = 0; n < cpy.Procedures.Count; n++)
                {
                    cpy.Procedures[n].ToRB().ParentPlanner = cpy;
                }

                for (int n = 0; n < cpy.PostProcedures.Count; n++)
                {
                    cpy.PostProcedures[n].ToRB().ParentPlanner = cpy;
                }
            }

            return nPlanner;
        }
    }
}