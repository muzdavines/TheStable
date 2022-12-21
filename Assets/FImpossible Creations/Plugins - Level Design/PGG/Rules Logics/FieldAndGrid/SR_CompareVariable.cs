using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.FieldAndGrid
{
    public class SR_CompareVariable : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Compare Variable"; }
        public override string Tooltip() { return ""; }

        public EProcedureType Type { get { return EProcedureType.Rule; } }

        public ESR_DistanceRule VariableMustBe = ESR_DistanceRule.Equal;
        public float ThisValue = 1f;
        public SpawnerVariableHelper CompareVariable = new SpawnerVariableHelper(FieldVariable.EVarType.Number);

        public override List<SpawnerVariableHelper> GetVariables() { return CompareVariable.GetListedVariable(); }
        public override void GUIRefreshVariables() { CompareVariable.requiredType = FieldVariable.EVarType.Number; }

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            float value = CompareVariable.GetValue(1f);
            CellAllow = PGGUtils.Compare(value, VariableMustBe, ThisValue);
        }

        #region Inspector Window

#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            base.NodeBody(so);
            if (CompareVariable != null) CompareVariable.requiredType = FieldVariable.EVarType.Number;

            if (FGenerators.CheckIfExist_NOTNULL(CompareVariable.GetVariable()))
                if (FGenerators.CheckIfExist_NOTNULL(CompareVariable.GetVariable().reference))
                    if (CompareVariable.GetVariable().reference.ValueType == FieldVariable.EVarType.Bool)
                    {
                        if (ThisValue > 0)
                            EditorGUILayout.HelpBox("When target variable is bool, then > 0 means TRUE / TOGGLED", MessageType.None);
                        else
                            EditorGUILayout.HelpBox("When target variable is bool, then <= 0 means FALSE", MessageType.None);
                    }

        }
#endif

        #endregion

    }
}