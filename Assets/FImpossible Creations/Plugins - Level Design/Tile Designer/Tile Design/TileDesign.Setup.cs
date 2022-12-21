using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileDesign
    {
        public bool Static = false;
        public string Tag = "Untagged";
        public int Layer = 0;
        public Material DefaultMaterial;

        public Material GetMaterial(TileMeshSetup setup = null)
        {
            if (setup != null)
            {
                if (setup.Material) return setup.Material;
            }

            if (DefaultMaterial) return DefaultMaterial;

            return DefaultDiffuseMaterial;
        }

        public static Material DefaultDiffuseMaterial
        {
            get
            {
                return new Material(Shader.Find("Diffuse"));
            }
        }


#if UNITY_EDITOR
        public List<UnityEditor.MonoScript> _editor_ToAttach = new List<UnityEditor.MonoScript>();
        public void Editor_SyncToAttach()
        {
            _string_ToAttach.Clear();
            for (int i = 0; i < _editor_ToAttach.Count; i++)
            {
                if (_editor_ToAttach[i] == null) { _string_ToAttach.Add(""); continue; }
                _string_ToAttach.Add(_editor_ToAttach[i].name);
            }
        }
#endif
        public List<string> _string_ToAttach = new List<string>();

        public List<SendMessageHelper> SendMessages = new List<SendMessageHelper>();

        #region Send Message Helper

        [System.Serializable]
        public class SendMessageHelper
        {
            public string Message = "SetMyValue";
            public enum EMessageSend { EditorGenerate, PlaymodeStart }
            public EMessageSend SendOn = EMessageSend.PlaymodeStart;

            public bool SendValue = true;
            public float MessageValue = 1f;
            public string MessageString = "";

            internal void SendTo(GameObject pf)
            {
                if (SendValue == false)
                {
                    pf.SendMessage(Message, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    if (string.IsNullOrEmpty(MessageString))
                        pf.SendMessage(Message, MessageValue, SendMessageOptions.DontRequireReceiver);
                    else
                        pf.SendMessage(Message, MessageString, SendMessageOptions.DontRequireReceiver);

                }
            }

            internal SendMessageHelper Copy()
            {
                return (SendMessageHelper)MemberwiseClone();
            }
        }

        #endregion



        public bool AddRigidbody = false;
        public bool IsKinematic = true;
        public float RigidbodyMass = 10f;

        public PhysicMaterial CollidersMaterial = null;

        public enum EColliderMode { None, BoundingBox, MultipleBoundingBoxes, SphereCollider, MeshColliders, CombinedMeshCollider }
        public EColliderMode ColliderMode = EColliderMode.CombinedMeshCollider;

        public float ScaleColliders = 1f;
        public bool ConvexCollider = false;
        [Range(0f, 1f)] public float MeshColliderReduction = 0f;

        public Vector3 ExpandThinCollider = Vector3.zero;

        public GameObject GeneratePrefab()
        {
            _UsedCombinedCollisionMesh = null;

            GameObject pf = new GameObject(DesignName);

            for (int i = 0; i < LatestGeneratedMeshes.Count; i++)
            {
                if (i == 0)
                {
                    MeshFilter filter = pf.AddComponent<MeshFilter>();
                    filter.sharedMesh = LatestGeneratedMeshes[0];

                    MeshRenderer rend = pf.AddComponent<MeshRenderer>();
                    rend.sharedMaterial = LatestGeneratedMeshesMaterials[0];
                }
                else
                {
                    GameObject subMesh = new GameObject(DesignName + (i + 1));
                    subMesh.transform.SetParent(pf.transform);

                    subMesh.transform.localPosition = Vector3.zero;
                    subMesh.transform.localRotation = Quaternion.identity;
                    subMesh.transform.localScale = Vector3.one;

                    MeshFilter filter = subMesh.AddComponent<MeshFilter>();
                    filter.sharedMesh = LatestGeneratedMeshes[i];

                    MeshRenderer rend = subMesh.AddComponent<MeshRenderer>();
                    rend.sharedMaterial = LatestGeneratedMeshesMaterials[i];
                }
            }

            pf.isStatic = Static;
            pf.layer = Layer;
            pf.tag = Tag;


            #region Generate rigidbody and colliders

            if (AddRigidbody)
            {
                Rigidbody rig = pf.AddComponent<Rigidbody>();
                rig.isKinematic = IsKinematic;
                rig.mass = RigidbodyMass;
            }

            if (_LatestGen_Bounds.size.sqrMagnitude > 0f)
            {
                if (ColliderMode == EColliderMode.BoundingBox)
                {
                    BoxCollider box = pf.AddComponent<BoxCollider>();

                    Vector3 boxCenter = _LatestGen_Bounds.center;
                    Vector3 boxSize = _LatestGen_Bounds.size * ScaleColliders;

                    box.sharedMaterial = CollidersMaterial;

                    if (ExpandThinCollider != Vector3.zero)
                    {
                        //Vector3 boxAbsSize = new Vector3(Mathf.Abs(boxSize.x), Mathf.Abs(boxSize.y), Mathf.Abs(boxSize.z));

                        if (ExpandThinCollider.x != 0f)
                            //if (boxAbsSize.x < 0.011f)
                            {
                                boxSize.x = Mathf.Abs(ExpandThinCollider.x);
                                boxCenter.x = ExpandThinCollider.x / 2f;
                            }

                        if (ExpandThinCollider.y != 0f)
                            //if (boxAbsSize.y < 0.011f)
                            {
                                boxSize.y = Mathf.Abs(ExpandThinCollider.y);
                                boxCenter.y = ExpandThinCollider.y / 2f;
                            }

                        if (ExpandThinCollider.z != 0f)
                            //if (boxAbsSize.z < 0.011f)
                            {
                                boxSize.z = Mathf.Abs(ExpandThinCollider.z);
                                boxCenter.z = ExpandThinCollider.z / 2f;
                            }
                    }

                    box.size = boxSize;
                    box.center = boxCenter;
                }
                else if (ColliderMode == EColliderMode.MultipleBoundingBoxes)
                {
                    var filters = FTransformMethods.FindComponentsInAllChildren<MeshFilter>(pf.transform);

                    for (int f = 0; f < filters.Count; f++)
                    {
                        BoxCollider box = filters[f].gameObject.AddComponent<BoxCollider>();
                        box.size = filters[f].sharedMesh.bounds.size * ScaleColliders;
                        box.center = filters[f].sharedMesh.bounds.center;
                        box.sharedMaterial = CollidersMaterial;
                    }
                }
                else if (ColliderMode == EColliderMode.SphereCollider)
                {
                    SphereCollider sph = pf.AddComponent<SphereCollider>();
                    sph.radius = _LatestGen_Bounds.extents.x * ScaleColliders;
                    sph.center = _LatestGen_Bounds.center;
                    sph.sharedMaterial = CollidersMaterial;
                }
                else if (ColliderMode == EColliderMode.MeshColliders)
                {
                    var filters = FTransformMethods.FindComponentsInAllChildren<MeshFilter>(pf.transform);
                    for (int f = 0; f < filters.Count; f++)
                    {
                        MeshCollider msh = filters[f].gameObject.AddComponent<MeshCollider>();
                        msh.sharedMesh = filters[f].sharedMesh;
                        msh.sharedMaterial = CollidersMaterial;
                        msh.convex = ConvexCollider;
                    }
                }
                else if (ColliderMode == EColliderMode.CombinedMeshCollider)
                {
                    MeshCollider msh = pf.AddComponent<MeshCollider>();
                    msh.sharedMesh = GetCombinedCollisionMesh();
                    _UsedCombinedCollisionMesh = msh.sharedMesh;
                    msh.sharedMaterial = CollidersMaterial;
                    msh.convex = ConvexCollider;
                }
            }


            #endregion


            for (int i = 0; i < _string_ToAttach.Count; i++)
            {
                //pf.AddComponent(_string_ToAttach[i]);
                var compType = System.Type.GetType(_string_ToAttach[i]);
                if (compType != null) pf.AddComponent(compType);
            }

            for (int i = 0; i < SendMessages.Count; i++)
            {
                var mess = SendMessages[i];
                /*if (mess.SendOn == SendMessageHelper.EMessageSend.EditorGenerate) */
                mess.SendTo(pf);
            }

            return pf;
        }

        internal Bounds GetFullBounds()
        {
            Bounds b = new Bounds();

            for (int i = 0; i < LatestGeneratedMeshes.Count; i++)
            {
                b.Encapsulate(LatestGeneratedMeshes[i].bounds);
            }

            return b;
        }

        public void AddInstanceTo(Material key, TileMeshSetup.TileMeshCombineInstance tileInst, Dictionary<Material, List<TileMeshSetup.TileMeshCombineInstance>> inst)
        {
            if (!inst.ContainsKey(key)) inst.Add(key, new List<TileMeshSetup.TileMeshCombineInstance>());
            inst[key].Add(tileInst);
        }


        /// <summary>
        /// Generating all tile mesh setups and combining onto final tile design ready objects
        /// </summary>
        public void FullGenerateStack()
        {
            LatestGeneratedMeshes.Clear();
            LatestGeneratedMeshesMaterials.Clear();


            #region Prepare, generate all tile meshes

            for (int i = 0; i < TileMeshes.Count; i++)
            {
                var tile = TileMeshes[i];
                tile.FullGenerateMesh();
                
            }

            #endregion


            #region Collect meshes and instances, categorize by materials

            Dictionary<Material, List<TileMeshSetup.TileMeshCombineInstance>> materialMeshes = new System.Collections.Generic.Dictionary<Material, List<TileMeshSetup.TileMeshCombineInstance>>();
            Material defaultMat = DefaultMaterial;
            if (defaultMat == null) defaultMat = DefaultDiffuseMaterial;


            for (int i = 0; i < TileMeshes.Count; i++)
            {
                TileMeshSetup tile = TileMeshes[i];

                for (int c = 0; c < tile.Copies; c++)
                {
                    TileMeshSetup.TileMeshCombineInstance inst = tile.Instances[c];
                    inst._bake_Combined = false;

                    if (inst.Enabled == false) continue;
                    if (inst.MeshMode != TileMeshSetup.TileMeshCombineInstance.EMeshMode.Default) continue;

                    inst._BakeParent = tile;

                    Material targetMat = inst.OverrideMaterial;
                    if (targetMat == null) targetMat = tile.Material;
                    if (targetMat == null) targetMat = defaultMat;

                    inst._ModMesh = null;
                    if (inst.UVOffset != Vector2.zero) { inst.RefreshModMesh(); FMeshUtils.OffsetUV(inst._ModMesh, inst.UVOffset); }
                    if (inst.UVRotate != 0f) { inst.RefreshModMesh(); FMeshUtils.RotateUV(inst._ModMesh, inst.UVRotate); }
                    if (inst.UVReScale != Vector2.one) { inst.RefreshModMesh(); FMeshUtils.RescaleUV(inst._ModMesh, inst.UVReScale); }
                    if (inst.FlipNormals) { inst.RefreshModMesh(); FMeshUtils.FlipNormals(inst._ModMesh); }

                    AddInstanceTo(targetMat, inst, materialMeshes);
                }

            }

            #endregion


            #region Combining Meshes


            Mesh combined;
            List<CombineInstance> combination = new List<CombineInstance>();
            int indexer = 1;

            foreach (var item in materialMeshes)
            {
                combined = new Mesh();
                combination.Clear();

                for (int i = 0; i < item.Value.Count; i++)
                {
                    CombineInstance comb = new CombineInstance();

                    if (item.Value[i]._ModMesh == null)
                        comb.mesh = item.Value[i]._BakeParent.LatestGeneratedMesh;
                    else
                        comb.mesh = item.Value[i]._ModMesh;

                    comb.transform = item.Value[i].GenerateMatrix();
                    combination.Add(comb);

                    item.Value[i]._bake_Combined = true;
                }

                combined.CombineMeshes(combination.ToArray(), true, true, false);
                combined.name = DesignName + indexer.ToString();
                LatestGeneratedMeshes.Add(combined);
                LatestGeneratedMeshesMaterials.Add(item.Key);

                indexer += 1;
            }


            #endregion


            #region Generating Removing shape combination

            combination.Clear();
            bool willRemove = false;

            Mesh removeCombination = new Mesh();
            for (int i = 0; i < TileMeshes.Count; i++)
            {
                var tile = TileMeshes[i];

                for (int c = 0; c < tile.Copies; c++)
                {
                    var inst = tile.Instances[c];

                    if (inst.Enabled == false) continue;
                    if (inst.MeshMode != TileMeshSetup.TileMeshCombineInstance.EMeshMode.Remove) continue;

                    willRemove = true;
                    CombineInstance comb = new CombineInstance();
                    comb.mesh = tile.LatestGeneratedMesh;
                    comb.transform = inst.GenerateMatrix();
                    combination.Add(comb);
                }
            }

            #endregion


            // Subtracting remove shape from main meshes
            if (willRemove)
            {
                // Combine prepared remove shape
                removeCombination.CombineMeshes(combination.ToArray(), true, true, false);

                for (int i = 0; i < LatestGeneratedMeshes.Count; i++)
                {
                    LatestGeneratedMeshes[i] = FMeshUtils.MeshesOperation(LatestGeneratedMeshes[i], removeCombination, Parabox.CSG.CSG.BooleanOp.Subtraction);
                }
            }


            #region Adding forced to not remove

            bool areNotRemoved = false;

            for (int i = 0; i < TileMeshes.Count; i++)
            {
                TileMeshSetup tile = TileMeshes[i];

                for (int c = 0; c < tile.Copies; c++)
                {
                    TileMeshSetup.TileMeshCombineInstance inst = tile.Instances[c];

                    inst._BakeParent = tile;

                    if (inst.Enabled == false) continue;
                    if (inst.MeshMode != TileMeshSetup.TileMeshCombineInstance.EMeshMode.ForceDontRemove) continue;

                    areNotRemoved = true;
                    inst._bake_Combined = false;

                    Material targetMat = inst.OverrideMaterial;
                    if (targetMat == null) targetMat = tile.Material;
                    if (targetMat == null) targetMat = defaultMat;

                    inst._ModMesh = null;
                    if (inst.UVOffset != Vector2.zero) { inst.RefreshModMesh(); FMeshUtils.OffsetUV(inst._ModMesh, inst.UVOffset); }
                    if (inst.UVRotate != 0f) { inst.RefreshModMesh(); FMeshUtils.RotateUV(inst._ModMesh, inst.UVRotate); }
                    if (inst.UVReScale != Vector2.one) { inst.RefreshModMesh(); FMeshUtils.RescaleUV(inst._ModMesh, inst.UVReScale); }
                    if (inst.FlipNormals) { inst.RefreshModMesh(); FMeshUtils.FlipNormals(inst._ModMesh); }

                    AddInstanceTo(targetMat, inst, materialMeshes);
                }
            }

            if (areNotRemoved)
            {
                combination = new List<CombineInstance>();
                indexer = 1;

                foreach (var item in materialMeshes)
                {
                    combined = new Mesh();
                    combination.Clear();
                    bool wasComb = false;

                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        if (item.Value[i]._bake_Combined) continue;

                        CombineInstance comb = new CombineInstance();

                        if (item.Value[i]._ModMesh == null)
                            comb.mesh = item.Value[i]._BakeParent.LatestGeneratedMesh;
                        else
                            comb.mesh = item.Value[i]._ModMesh;

                        comb.transform = item.Value[i].GenerateMatrix();
                        combination.Add(comb);
                        item.Value[i]._bake_Combined = true;
                        wasComb = true;
                    }

                    if (wasComb)
                    {
                        combined.CombineMeshes(combination.ToArray(), true, true, false);
                        combined.name = DesignName + indexer.ToString();
                        LatestGeneratedMeshes.Add(combined);
                        LatestGeneratedMeshesMaterials.Add(item.Key);
                    }

                    indexer += 1;
                }
            }

            #endregion


            RefreshGenerateMeshesInfo();

        }

        public bool IsSomethingGenerated { get { return LatestGeneratedMeshes.Count > 0; } }

        /// <summary> Multiple meshes for supporting multiple materials </summary>
        public List<Mesh> LatestGeneratedMeshes = new List<Mesh>();
        public List<Material> LatestGeneratedMeshesMaterials = new List<Material>();

        [NonSerialized] public Mesh _UsedCombinedCollisionMesh = null;
        public Mesh GetCombinedCollisionMesh()
        {
            Mesh combined = new Mesh();

            List<CombineInstance> combination = new List<CombineInstance>();

            for (int i = 0; i < TileMeshes.Count; i++)
            {
                TileMeshSetup tile = TileMeshes[i];

                for (int c = 0; c < tile.Copies; c++)
                {
                    TileMeshSetup.TileMeshCombineInstance inst = tile.Instances[c];

                    if (inst.Enabled == false) continue;

                    if (inst.MeshMode == TileMeshSetup.TileMeshCombineInstance.EMeshMode.Default)
                    {
                        if (!inst.UseInCollider) continue;
                    }
                    else if (inst.MeshMode != TileMeshSetup.TileMeshCombineInstance.EMeshMode.JustCollider)
                    {
                        continue;
                    }

                    CombineInstance comb = new CombineInstance();
                    comb.mesh = inst._BakeParent.LatestGeneratedMesh;
                    if (inst.FlipNormals) FMeshUtils.FlipNormals(comb.mesh);
                    comb.transform = inst.GenerateMatrix();
                    combination.Add(comb);
                }
            }

            combined.CombineMeshes(combination.ToArray(), true, true, false);


            // Remove shape if used
            Mesh combinedRemoveShape = new Mesh();
            combination = new List<CombineInstance>();
            bool willRemove = false;

            for (int i = 0; i < TileMeshes.Count; i++)
            {
                TileMeshSetup tile = TileMeshes[i];

                for (int c = 0; c < tile.Copies; c++)
                {
                    TileMeshSetup.TileMeshCombineInstance inst = tile.Instances[c];

                    if (inst.Enabled == false) { continue; }
                    if (inst.MeshMode != TileMeshSetup.TileMeshCombineInstance.EMeshMode.Remove) { continue; }
                    if (inst.UseInCollider == false) continue;
                    //if (inst._BakeParent == null) { UnityEngine.Debug.Log("zcxxx"); continue; }

                    willRemove = true;

                    CombineInstance comb = new CombineInstance();
                    comb.mesh = tile.LatestGeneratedMesh;
                    //comb.mesh = inst._BakeParent.LatestGeneratedMesh;
                    comb.transform = inst.GenerateMatrix();
                    combination.Add(comb);
                }
            }


            if (willRemove)
            {
                combinedRemoveShape.CombineMeshes(combination.ToArray(), true, true, false);
                combined = FMeshUtils.MeshesOperation(combined, combinedRemoveShape, Parabox.CSG.CSG.BooleanOp.Subtraction);

            }

            combined.name = DesignName + "_Collider";
            return combined;
        }


        public int _LatestGen_Meshes = 0;
        public int _LatestGen_Vertices = 0;
        public int _LatestGen_Tris = 0;

        public Bounds _LatestGen_Bounds = new Bounds();

        /// <summary>
        /// Refreshing info about current generated combined result
        /// </summary>
        internal void RefreshGenerateMeshesInfo()
        {
            _LatestGen_Meshes = LatestGeneratedMeshes.Count;
            _LatestGen_Vertices = 0;
            _LatestGen_Tris = 0;
            _LatestGen_Bounds = new Bounds();

            for (int i = 0; i < LatestGeneratedMeshes.Count; i++)
            {
                var mesh = LatestGeneratedMeshes[i];
                _LatestGen_Vertices += mesh.vertexCount;
                _LatestGen_Tris += mesh.triangles.Length / 3;
                _LatestGen_Bounds.Encapsulate(mesh.bounds);
            }
        }

#if UNITY_EDITOR
        public static TileDesign _copyGameObjectSetFrom = null;
        public void CopyGameObjectParameters()
        {
            _copyGameObjectSetFrom = this;
        }

        public static void PasteGameObjectParameters(TileDesign from, TileDesign to)
        {
            to.Static = from.Static;
            to.Layer = from.Layer;
            to.Tag = from.Tag;

            to._editor_ToAttach.Clear();
            to._string_ToAttach.Clear();

            for (int i = 0; i < from._editor_ToAttach.Count; i++)
            {
                to._editor_ToAttach.Add(from._editor_ToAttach[i]);
            }

            to.Editor_SyncToAttach();

            to.SendMessages.Clear();

            for (int i = 0; i < from.SendMessages.Count; i++)
            {
                to.SendMessages.Add(from.SendMessages[i].Copy());
            }

            to.Tag = from.Tag;
        }

        public static TileDesign _copyColliderSetFrom = null;
        internal void CopyColliderParameters()
        {
            _copyColliderSetFrom = this;
        }

        public static void PasteColliderParameters(TileDesign from, TileDesign to)
        {
            to.AddRigidbody = from.AddRigidbody;
            to.RigidbodyMass = from.RigidbodyMass;
            to.IsKinematic = from.IsKinematic;

            to.ColliderMode = from.ColliderMode;
            to.ScaleColliders = from.ScaleColliders;
            to.CollidersMaterial = from.CollidersMaterial;
        }
#endif
    }
}