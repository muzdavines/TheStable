using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Rules.Transforming.Legacy
{
    public partial class SR_Scale : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Scale"; }
        public override string Tooltip() { return "Setting scale of target spawned prefabs"; }

        public EProcedureType Type { get { return EProcedureType.Event; } }

        public Vector3 ScaleMultiplier = Vector3.one;
        public SpawnerVariableHelper ScaleMulVariable = new SpawnerVariableHelper(FieldVariable.EVarType.Vector3);
        public override List<SpawnerVariableHelper> GetVariables()
        { return ScaleMulVariable.GetListedVariable(); }

        public Vector3 RandomizeScale = Vector3.zero;

        #region Back Compability thing
#if UNITY_EDITOR
        public override void NodeBody(UnityEditor.SerializedObject so)
        {
            base.NodeBody(so);
            ScaleMulVariable.requiredType = FieldVariable.EVarType.Vector3;
        }
#endif
        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);

            Vector3 scaling = Vector3.Scale(ScaleMultiplier, ScaleMulVariable.GetVector3(Vector3.one));
            scaling += new Vector3(
                FGenerators.GetRandom(-RandomizeScale.x, RandomizeScale.x),
                FGenerators.GetRandom(-RandomizeScale.y, RandomizeScale.y),
                FGenerators.GetRandom(-RandomizeScale.z, RandomizeScale.z)
                );

            spawn.TempScaleMul = scaling;
        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            spawn.LocalScaleMul = spawn.TempScaleMul;
        }
    }
}