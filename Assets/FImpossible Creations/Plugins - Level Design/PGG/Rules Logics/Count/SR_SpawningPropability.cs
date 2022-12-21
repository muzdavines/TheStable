using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Count
{
    public partial class SR_SpawningPropability : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Spawning Probability"; }
        public override string Tooltip() { return "Percentage probability to allow/disallow spawning in checked cell\n[Lightweight]"; }
        public EProcedureType Type { get { return EProcedureType.Rule; } }

        [Range(0f,1f)] public float Propability = 1f;
        public SpawnerVariableHelper PropabilityMulVariable = new SpawnerVariableHelper(FieldVariable.EVarType.Number);
        public override List<SpawnerVariableHelper> GetVariables() { return PropabilityMulVariable.GetListedVariable(); }

        #region Back Compability thing
#if UNITY_EDITOR
        public override void NodeBody(UnityEditor.SerializedObject so)
        {
            base.NodeBody(so);
            PropabilityMulVariable.requiredType = FieldVariable.EVarType.Number;
        }
#endif
        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            float mul = PropabilityMulVariable.GetValue(1f);
            if (FGenerators.GetRandom(0f, 1f ) < Propability * mul)
            {
                CellAllow = true;
            }
        }

    }
}