using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {
        public enum EMeshGenerator { Loft, Lathe, Extrude, Sweep, CustomMeshAndExtras, Primitive }
        public EMeshGenerator GenTechnique = EMeshGenerator.Loft;

        public enum EOrigin { Unchanged, BottomCenter, Center, BottomLeft, TopCenter, BottomCenterBack, BottomCenterFront }
        public EOrigin Origin = EOrigin.Unchanged;

        public enum EUVFit { FitX, FitY, FitXY }
        public EUVFit UVFit = EUVFit.FitXY;

        public Vector2 UVMul = Vector2.one;


        public enum ENormalsMode { NormalsAsSubdivView, HardNormals }
        public ENormalsMode NormalsMode = ENormalsMode.NormalsAsSubdivView;
        public float HardNormals = 0f;

        public enum ESubdivideCompute { AngleLimit, LengthLimit }
        public ESubdivideCompute SubdivMode = ESubdivideCompute.AngleLimit;


        public float width = 1.5f;
        public float height = 2f;
        public float depth = 0.5f;
        public float _loft_depthDim = 1.5f;

        public Material Material = null;

        internal void PasteSettingsFrom(TileMeshSetup tileMesh_CopyFrom)
        {
            tileMesh_CopyFrom.PasteAllSetupTo(this);
        }

        internal void AdjustCopiesCount()
        {
            var list = Instances;
            if (Copies < 0) Copies = 1;

            if (_instances.Count == Copies) return;

            if (list.Count < Copies)
            {
                while (list.Count < Copies)
                {
                    TileMeshCombineInstance inst = new TileMeshCombineInstance();

                    Vector3 lastPos = Vector3.zero;
                    Vector3 lastRot = Vector3.zero;
                    Vector3 lastScale = Vector3.one;

                    if (list.Count > 0)
                    {
                        lastPos = list[list.Count - 1].Position;
                        lastRot = list[list.Count - 1].Rotation;
                        lastScale = list[list.Count - 1].Scale;
                    }

                    inst.Position = new Vector3(lastPos.x + 0.4f, lastPos.y, lastPos.z);
                    inst.Rotation = lastRot;
                    inst.Scale = lastScale;
                    list.Add(inst);
                }
            }
            else
            {
                while (list.Count > Copies) list.RemoveAt(list.Count - 1);
            }
        }

    }
}