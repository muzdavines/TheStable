using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public partial class TileDesign
    {
        public string DesignName = "New Tile";
        public List<TileMeshSetup> TileMeshes = new List<TileMeshSetup>();

        public static TileDesign _CopyFrom = null;

#if UNITY_EDITOR
        public void PasteEverythingFrom(TileDesign from)
        {
            DesignName = from.DesignName;
            DefaultMaterial = from.DefaultMaterial;

            TileMeshes.Clear();

            for (int t = 0; t < from.TileMeshes.Count; t++)
            {
                TileMeshSetup meshSet = new TileMeshSetup(from.TileMeshes[t].Name);
                from.TileMeshes[t].PasteAllSetupTo(meshSet, true);
                TileMeshes.Add(meshSet);
            }

            PasteColliderParameters(from, this);
            PasteGameObjectParameters(from, this);

        }
#endif
    }
}