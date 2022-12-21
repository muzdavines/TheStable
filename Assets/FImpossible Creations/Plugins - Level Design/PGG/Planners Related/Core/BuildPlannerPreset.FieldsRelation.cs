using FIMSpace.Generating.Checker;
using System.Collections.Generic;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning
{

    public partial class BuildPlannerPreset
    {

        private int[] _VariablesIds = null;
        public int[] GetVariablesIDList(bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _VariablesIds == null || _VariablesIds.Length != BuildVariables.Count)
            {
                _VariablesIds = new int[BuildVariables.Count];
                for (int i = 0; i < BuildVariables.Count; i++)
                {
                    _VariablesIds[i] = i;
                }
            }

            return _VariablesIds;
        }

        private GUIContent[] _VariablesNames = null;
        public GUIContent[] GetVariablesNameList(bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _VariablesNames == null || _VariablesNames.Length != BuildVariables.Count)
            {
                _VariablesNames = new GUIContent[BuildVariables.Count];
                for (int i = 0; i < BuildVariables.Count; i++)
                {
                    _VariablesNames[i] = new GUIContent(BuildVariables[i].Name);
                }
            }
            return _VariablesNames;
        }






        private int[] _PlannersIds = null;
        public int[] GetPlannersIDList(bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _PlannersIds == null || _PlannersIds.Length != BasePlanners.Count)
            {
                _PlannersIds = new int[BasePlanners.Count];
                for (int i = 0; i < BasePlanners.Count; i++)
                {
                    _PlannersIds[i] = i;
                }
            }

            return _PlannersIds;
        }

        private GUIContent[] _PlannersNames = null;
        public GUIContent[] GetPlannersNameList(bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _PlannersNames == null || _PlannersNames.Length != BasePlanners.Count)
            {
                _PlannersNames = new GUIContent[BasePlanners.Count];
                for (int i = 0; i < BasePlanners.Count; i++)
                {
                    _PlannersNames[i] = new GUIContent("["+i+"] " + BasePlanners[i].name);
                }
            }
            return _PlannersNames;
        }

    }

}