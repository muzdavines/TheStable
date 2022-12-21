#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FIMSpace.Generating.Rules.Modelling
{
    public class SR_ReplacePrefab : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Replace Spawned Prefab"; }
        public override string Tooltip() { return "Replace spawned prefab with other one before generating object, this rule is dedicated to be used with injecting and overriding field setup's variables"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        [Header("This rule is dedicated for injecting")]
        public GameObject ReplaceSpawnWith = null;
        public SpawnerVariableHelper GameObjVariable = new SpawnerVariableHelper(FieldVariable.EVarType.GameObject);

        #region Back Compability thing
#if UNITY_EDITOR
        public override void NodeBody(UnityEditor.SerializedObject so)
        {
            base.NodeBody(so);
            GameObjVariable.requiredType = FieldVariable.EVarType.GameObject;
        }
#endif
        #endregion

        public override List<SpawnerVariableHelper> GetVariables() { return GameObjVariable.GetListedVariable(); }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            GameObject targetObj = ReplaceSpawnWith;

            if (GameObjVariable.IsType(FieldVariable.EVarType.GameObject))
            {
                GameObject varMat = GameObjVariable.GetGameObjValue();
                if (FGenerators.RefIsNull(varMat) == false) targetObj = varMat;
            }

            if (FGenerators.RefIsNull(targetObj)) return;

            Action<SpawnData> replaceSpawn =
            (o) =>
            {
                o.Prefab = targetObj;
            };

            spawn.OnPreGeneratedEvents.Add(replaceSpawn);
        }

    }
}