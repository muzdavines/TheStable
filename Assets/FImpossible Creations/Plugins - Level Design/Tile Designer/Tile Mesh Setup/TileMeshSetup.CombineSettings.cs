using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {
        public int Copies = 1;
        [SerializeField] private List<TileMeshCombineInstance> _instances = new List<TileMeshCombineInstance>();
        public List<TileMeshCombineInstance> Instances
        {
            get
            {
                CheckInstances();
                return _instances;
            }
        }

        internal void CheckInstances()
        {
            if (Copies < 1) Copies = 1;
            if (_instances == null) _instances = new List<TileMeshCombineInstance>();
            if (_instances.Count == 0) _instances.Add(new TileMeshCombineInstance());
        }

        [System.Serializable]
        public class TileMeshCombineInstance
        {
            public bool Enabled = true;

            public enum EMeshMode { Default, JustCollider, Remove, ForceDontRemove }
            public EMeshMode MeshMode = EMeshMode.Default;

            public Vector3 Position = Vector3.zero;
            public Vector3 Rotation = Vector3.zero;
            public Vector3 Scale = Vector3.one;
            public Material OverrideMaterial = null;

            public bool FoldoutAdvanced = false;
            public bool FlipNormals = false;
            public Vector2 UVOffset = Vector2.zero;
            public float UVRotate = 0f;
            public Vector2 UVReScale = Vector2.one;

            public bool UseInCollider = true;
            [NonSerialized] public TileMeshSetup _BakeParent = null;
            [NonSerialized] public Mesh _ModMesh = null;
            public void RefreshModMesh(bool forceRefresh = false)
            {
                if (_ModMesh == null || forceRefresh)
                {
                    if (_BakeParent != null)
                        _ModMesh = Mesh.Instantiate(_BakeParent.LatestGeneratedMesh);
                }
            }

            public bool MeshModeApplyToAll = true;
            public List<int> MeshModeApplyMasks = new List<int>();

            public Quaternion GetRotation()
            {

                if (SepAxisRotMode)
                {
                    Quaternion rot = Quaternion.identity;
                    rot *= Quaternion.AngleAxis(Rotation.x, Vector3.right);
                    rot *= Quaternion.AngleAxis(Rotation.y, Vector3.up);
                    rot *= Quaternion.AngleAxis(Rotation.z, Vector3.forward);
                    return rot;
                }
                else
                {
                    return Quaternion.Euler(Rotation);
                }

            }

            internal Matrix4x4 GenerateMatrix()
            {
                return Matrix4x4.TRS(Position, GetRotation(), Scale);
            }

            internal TileMeshCombineInstance Copy()
            {
                return (TileMeshCombineInstance)MemberwiseClone();
            }

            [NonSerialized] public bool _bake_Combined = false;

            public bool SepAxisRotMode = false;
        }

    }
}