#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FIMSpace.Generating.Rules.Modelling
{
    public class SR_SetMaterial : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Set Mesh Material"; }
        public override string Tooltip() { return "Assigning material to meshes of target spawned prefab"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        public Material NewMaterial = null;
        public SpawnerVariableHelper MatVariable = new SpawnerVariableHelper(FieldVariable.EVarType.Material);

        #region Back Compability thing
#if UNITY_EDITOR
        public override void NodeBody(UnityEditor.SerializedObject so)
        {
            base.NodeBody(so);
            MatVariable.requiredType = FieldVariable.EVarType.Material;
        }
#endif
        #endregion

        public override List<SpawnerVariableHelper> GetVariables() { return MatVariable.GetListedVariable();  }

        [Header("Leave fields empty to not use them")]
        public Material ReplaceOnly = null;
        public bool ReplaceOnlyFirst = true;

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            Material targetMat = NewMaterial;
            if ( MatVariable.IsType(FieldVariable.EVarType.Material) )
            {
                Material varMat = MatVariable.GetMatValue();
                if (varMat != null) targetMat = varMat;
            }

            if (targetMat == null) return;

            Action<GameObject> setMaterial =
            (o) =>
            {

                Renderer[] rends = o.GetComponentsInChildren<Renderer>();

                bool broken = false;
                for (int r = 0; r < rends.Length; r++)
                {
                    Material[] rendSharedMats = rends[r].sharedMaterials;
                    for (int y = 0; y < rendSharedMats.Length; ++y)
                    {
                        if (ReplaceOnly != null)
                            if (rendSharedMats[y] != ReplaceOnly) continue;

                        rendSharedMats[y] = targetMat;
                        if (ReplaceOnlyFirst)
                        {
                            broken = true;
                            break;
                        }
                    }
                    rends[r].sharedMaterials = rendSharedMats;
                    if (broken)
                    {
                        break;
                    }
                }

            };

            spawn.OnGeneratedEvents.Add(setMaterial);
        }

    }
}