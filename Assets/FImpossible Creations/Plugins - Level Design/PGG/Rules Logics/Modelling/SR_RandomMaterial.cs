using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.Modelling
{
    public class SR_RandomMaterial : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Set Random Mesh Material"; }
        public override string Tooltip() { return "Applying random material from the list to target spawned prefab"; }

        public EProcedureType Type { get { return EProcedureType.Event; } }


        [Space(2)]
        public List<Material> MaterialsToChooseFrom = new List<Material>();
        [Space(2)]
        [FPD_Width(180)]
        [Tooltip("Optional feature to replace only materials with name prefix (Case Sensitive)")]
        public string RequireMaterialNamePrefix = "";

        #region There you can do custom modifications for inspector view
#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            base.NodeBody(so);
        }
#endif
        #endregion


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (MaterialsToChooseFrom.Count == 0) return;

            Material targetMat = MaterialsToChooseFrom[FGenerators.GetRandom(0, MaterialsToChooseFrom.Count)];

            if (targetMat == null) return;

            bool requirePrefix = !string.IsNullOrEmpty(RequireMaterialNamePrefix);

            Action<GameObject> setMaterial =
            (o) =>
            {
                Renderer[] rends = o.GetComponentsInChildren<Renderer>();

                for (int r = 0; r < rends.Length; r++)
                {
                    Material[] rendSharedMats = rends[r].sharedMaterials;

                    for (int y = 0; y < rendSharedMats.Length; ++y)
                    {
                        if (requirePrefix)
                        {
                            if (rendSharedMats[y].name.StartsWith(RequireMaterialNamePrefix))
                                rendSharedMats[y] = targetMat;
                        }
                        else
                            rendSharedMats[y] = targetMat;
                    }

                    rends[r].sharedMaterials = rendSharedMats;
                }
            };

            spawn.OnGeneratedEvents.Add(setMaterial);
        }

    }
}