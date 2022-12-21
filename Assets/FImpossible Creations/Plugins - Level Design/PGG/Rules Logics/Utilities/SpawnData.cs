using System;
using System.Collections.Generic;
using UnityEngine;


namespace FIMSpace.Generating
{
    [System.Serializable]
    public class SpawnData
    {
        public bool Enabled = true;
        [NonSerialized] public FieldCell OwnerCell;
        public Vector3Int OwnerCellPos;
        public int idInStampObjects;
        //public string tag;
        public GameObject Prefab;
        internal List<GameObject> AdditionalGenerated;
        public bool WasTemporaryPrefab { get; private set; }

        /// <summary> Useful only when using AdditionalGenerated list</summary>
        public bool DontSpawnMainPrefab = false;

        public FieldSetup ExecutedFrom;
        public FieldModification OwnerMod;
        public SpawnRuleBase OwnerRule;

        [NonSerialized] public FieldSpawner Spawner;
        public OStamperSet OStamp;
        public OStamperMultiSet OMulti;
        public Mesh PreviewMesh;

        //private GameObject temporaryPrefabOverride;

        public Vector3 Offset = Vector3.zero;
        public Vector3 RotationOffset = Vector3.zero;
        public Vector3 LocalRotationOffset = Vector3.zero;
        public Vector3 LocalScaleMul = Vector3.one;
        public Vector3 DirectionalOffset = Vector3.zero;

        /// <summary> Assigned during 'Check' and before 'CellInfluence' </summary>
        public Vector3 TempPositionOffset;
        /// <summary> Assigned during 'Check' and before 'CellInfluence' </summary>
        public Vector3 TempRotationOffset;
        /// <summary> Assigned during 'Check' and before 'CellInfluence' </summary>
        public Vector3 TempScaleMul = Vector3.one;

        /// <summary> Directional offset for objects trying to get position/rotation of this spawn </summary>
        public Vector3 OutsidePositionOffset = Vector3.zero;
        /// <summary> Rotation offset for objects trying to get position/rotation of this spawn </summary>
        public Vector3 OutsideRotationOffset = Vector3.zero;

        public enum ECombineMode
        {
            None, Combine, CombineStatic
        }

        public ECombineMode CombineMode = ECombineMode.None;
        public bool ForceSetStatic = false;

        /// <summary> Returning directional offset of spawn + pivot helper: OutsidePositionOffset </summary>
        public Vector3 GetDirectionalOffsetWithMods()
        {
            //return DirectionalOffset + OutsidePositionOffset;
            return DirectionalOffset + Quaternion.Euler(OutsideRotationOffset) * OutsidePositionOffset;
        }

        /// <summary> Returning world space directional offset of spawn + pivot helper: OutsidePositionOffset offsetted accordingly to pivot correction rotation </summary>
        public Vector3 GetDirectionalOffsetWithSeparatelyModOffset()
        {
            return Quaternion.Euler(RotationOffset) * (DirectionalOffset + Quaternion.Euler(OutsideRotationOffset) * OutsidePositionOffset);
        }

        /// <summary> Returning rotation offset of spawn + pivot helper: OutsideRotationOffset </summary>
        public Vector3 GetRotationOffsetWithMods(bool local = false)
        {
            if (!local)
                return RotationOffset + OutsideRotationOffset;
            else
                return LocalRotationOffset + OutsideRotationOffset;
        }

        //public Vector3 Offset;
        //public Vector3 RotationOffset;
        //public Vector3 LocalScaleMul;
        //public Vector3 DirectionalOffset;

        public enum ESpawnMark { Omni, Left, Right, Forward, Back, LeftForward, RightForward, LeftBack, RightBack }

        public ESpawnMark SpawnMark = ESpawnMark.Omni;
        //[SerializeField] private string customStigma = "";
        private List<string> customStigmas = new List<string>();

        public string Stigmas
        {
            get
            {
                string stigms = "";
                for (int i = 0; i < customStigmas.Count; i++) { if (i < customStigmas.Count - 1) stigms += customStigmas[i] + ","; else stigms += customStigmas[i]; }
                return stigms;
            }
        }

        public bool isTemp { get; internal set; }
#if UNITY_2019_4_OR_NEWER
            = false;
#endif
        /// <summary> Helper variable to prevent spawning objects when was destroyed in queue before </summary>
        //[NonSerialized] public bool Destroyed = false;

        /// <summary> Custom events executed before spawn object is instantiated in game scene </summary>
        public List<Action<SpawnData>> OnPreGeneratedEvents = new List<Action<SpawnData>>();
        /// <summary> Custom events executed when spawn object is instantiated in game scene </summary>
        public List<Action<GameObject>> OnGeneratedEvents = new List<Action<GameObject>>();


        internal static SpawnData GenerateSpawn(FieldSpawner spawner, FieldModification mod, FieldCell owner, int toSpawn, Vector3? offset = null, Vector3? rotOffset = null, Vector3? localRotOffset = null, Vector3? scaleMul = null, ESpawnMark mark = ESpawnMark.Omni, bool checkMesh = true)
        {
            SpawnData spawn = new SpawnData();
            spawn.isTemp = false;
            spawn.OwnerCell = owner;
            if (FGenerators.CheckIfExist_NOTNULL(owner)) spawn.OwnerCellPos = owner.Pos;
            spawn.OwnerMod = mod;
            spawn.Spawner = spawner;
            spawn.idInStampObjects = toSpawn;

            if (mod)
            {
                if (mod.ParentPack)
                {
                    if (mod.ParentPack.CombineSpawns == ModificatorsPack.EPackCombine.CombineAll)
                        spawn.CombineMode = ECombineMode.Combine;
                    else if (mod.ParentPack.CombineSpawns == ModificatorsPack.EPackCombine.CombineAllAndSetStatic)
                        spawn.CombineMode = ECombineMode.CombineStatic;
                }

                if (mod.Combine != FieldModification.ECombineSet.AsParentPack)
                {
                    if (mod.Combine == FieldModification.ECombineSet.ForceNotCombine) spawn.CombineMode = ECombineMode.None;
                    else if (mod.Combine == FieldModification.ECombineSet.ForceCombine) spawn.CombineMode = ECombineMode.Combine;
                    else if (mod.Combine == FieldModification.ECombineSet.ForceCombineAndSetStatic) spawn.CombineMode = ECombineMode.CombineStatic;
                }
            }

            spawn.ForceSetStatic = spawner.SwitchSpawnedToStatic;

            //if (FieldSpawner.TemporaryPrefabOverride == null)
            if (spawner.TemporaryPrefabOverride == null)
            {
                spawn.WasTemporaryPrefab = false;
                var prRef = mod.GetPrefabRef(toSpawn);

                if (prRef == null) return spawn;
                if (prRef.GameObject == null) return spawn;

                spawn.Prefab = prRef.GameObject;
                if (checkMesh) spawn.PreviewMesh = prRef.GetMesh();
            }
            else
            {
                spawn.WasTemporaryPrefab = true;
                spawn.Prefab = spawner.TemporaryPrefabOverride;
                spawn.TryDetectMeshInPrefab();
            }

            if (offset != null) spawn.Offset = (Vector3)offset;
            if (rotOffset != null) spawn.RotationOffset = rotOffset.Value;
            if (localRotOffset != null) spawn.LocalRotationOffset = localRotOffset.Value;
            if (scaleMul != null) spawn.LocalScaleMul = (Vector3)scaleMul;

            spawn.SpawnMark = mark;

            return spawn;
        }

        public void TryDetectMeshInPrefab()
        {
            if (Prefab == null) return;
            MeshFilter f = Prefab.GetComponentInChildren<MeshFilter>();
            if (f) PreviewMesh = f.sharedMesh;
        }

        /// <summary> Outside (pivot correction) position offset in world space </summary>
        internal Vector3 GetOutsideDirectionalOffsetValue()
        {
            if (OutsidePositionOffset != Vector3.zero)
                return GetRotationOffset() * OutsidePositionOffset;
            else
                return Vector3.zero;
        }

        public SpawnData Copy(bool copyOffsets = true)
        {
            SpawnData newSpawn = (SpawnData)MemberwiseClone();

            if (copyOffsets)
            {
                newSpawn.Offset = Offset;
                newSpawn.RotationOffset = RotationOffset;
                newSpawn.LocalRotationOffset = LocalRotationOffset;
                newSpawn.LocalScaleMul = LocalScaleMul;
                newSpawn.DirectionalOffset = DirectionalOffset;
                newSpawn.TempPositionOffset = TempPositionOffset;
                newSpawn.TempRotationOffset = TempRotationOffset;
            }

            newSpawn.Enabled = true;
            newSpawn.Spawner = Spawner;
            newSpawn.OwnerMod = OwnerMod;
            newSpawn.OwnerRule = OwnerRule;

            // Create new list refs otherwise it will share the same list reference!
            newSpawn.customStigmas = new List<string>();
            for (int i = 0; i < customStigmas.Count; i++) newSpawn.customStigmas.Add(customStigmas[i]);

            return newSpawn;
        }

        GameObject GetPrefab()
        {
            //if (temporaryPrefabOverride) return temporaryPrefabOverride;
            return Prefab;
        }

        /// <summary> Returning collider or MeshFilter wit assigned mesh </summary>
        public UnityEngine.Object IsSpawnCollidable()
        {
            GameObject pr = GetPrefab();
            if (pr == null) return null;

            Collider c = pr.GetComponent<Collider>();
            if (c == null)
            {
                c = FTransformMethods.FindComponentInAllChildren<Collider>(pr.transform);
                if (c == null) return null;
            }

            if (c) return c;
            else
            {
                MeshFilter filter = pr.GetComponent<MeshFilter>();
                if (!filter)
                {
                    filter = pr.GetComponentInChildren<MeshFilter>();
                }

                if (filter) if (filter.sharedMesh != null) return filter;
            }

            return null;
        }


        public static Vector3 GetPlacementDirection(ESpawnMark mark)
        {
            switch (mark)
            {
                case ESpawnMark.Omni: return Vector3.zero;
                case ESpawnMark.Left: return Vector3.left;
                case ESpawnMark.Right: return Vector3.right;
                case ESpawnMark.Forward: return Vector3.forward;
                case ESpawnMark.Back: return Vector3.back;
                case ESpawnMark.LeftForward: return new Vector3(-1f, 0f, 1f);
                case ESpawnMark.RightForward: return new Vector3(1f, 0f, 1f);
                case ESpawnMark.LeftBack: return new Vector3(-1f, 0f, -1f);
                case ESpawnMark.RightBack: return new Vector3(1f, 0f, -1f);
            }

            return Vector3.zero;
        }

        public static Quaternion GetPlacementRotation(ESpawnMark mark)
        {
            switch (mark)
            {
                case ESpawnMark.Left: return Quaternion.Euler(0, 90, 0);
                case ESpawnMark.Right: return Quaternion.Euler(0, -90, 0);
                case ESpawnMark.Forward: return Quaternion.Euler(0, 0, 0);
                case ESpawnMark.Back: return Quaternion.Euler(0, 80, 0);
                case ESpawnMark.LeftForward: return Quaternion.Euler(0, 45, 0);
                case ESpawnMark.RightForward: return Quaternion.Euler(0, -45, 0);
                case ESpawnMark.LeftBack: return Quaternion.Euler(0, 135, 0);
                case ESpawnMark.RightBack: return Quaternion.Euler(0, -135, 0);
            }

            return Quaternion.identity;
        }

        public Transform GetModContainer(Transform mainContainer)
        {
            if (OwnerMod.TemporaryContainer == null || OwnerMod.TemporaryContainer.parent != mainContainer)
            {
                OwnerMod.TemporaryContainer = mainContainer.transform.Find(OwnerMod.name + "-Container");
                if (OwnerMod.TemporaryContainer == null)
                {
                    GameObject container = new GameObject(OwnerMod.name + "-Container");
                    container.transform.SetParent(mainContainer.transform, true);
                    OwnerMod.TemporaryContainer = container.transform;
                }
            }

            return OwnerMod.TemporaryContainer;
        }

        public void AddCustomStigma(string v)
        {
            if (customStigmas.Contains(v) == false)
            {
                customStigmas.Add(v);
            }

            //if (string.IsNullOrEmpty(customStigma)) customStigma = v;
            //else
            //{
            //    if (customStigma.Contains(v)) return;
            //    customStigma += "," + v;
            //}
        }

        internal bool GetCustomStigma(string v, bool reload = false)
        {
            if (customStigmas.Contains(v)) return true;
            //if (string.IsNullOrEmpty(customStigma)) return false;
            //UnityEngine.Debug.Log("Stigma = " + customStigma);
            //if (customStigma.Contains(v)) return true;
            return false;
        }

        /// <summary>
        /// Returning offset calculated out of spawn's world offset and directional offset
        /// </summary>
        public Vector3 GetWorldPositionWithFullOffset(FieldSetup preset = null, bool useTemp = false)
        {
            if (preset == null) if (ExecutedFrom != null) { preset = ExecutedFrom; } else return Vector3.zero;

            Vector3 off = OwnerCell.WorldPos(preset.CellSize);
            if (Offset != Vector3.zero) off += Offset; else if (useTemp) if (TempPositionOffset != Vector3.zero) off += Offset;
            if (DirectionalOffset != Vector3.zero) off += Quaternion.Euler(RotationOffset) * DirectionalOffset;
            return off;
        }

        internal void AddChildSpawn(SpawnData data)
        {
            if (ChildSpawns == null) ChildSpawns = new List<SpawnData>();
            if (ChildSpawns.Contains(data) == false) ChildSpawns.Add(data);
        }

        [NonSerialized] public List<SpawnData> ChildSpawns;// { get; private set; }

        internal bool FindDifference(SpawnData spawnData)
        {
            if (Prefab != spawnData.Prefab) return true;
            if (Enabled != spawnData.Enabled) return true;
            if (idInStampObjects != spawnData.idInStampObjects) return true;
            if (DontSpawnMainPrefab != spawnData.DontSpawnMainPrefab) return true;
            if (ExecutedFrom != spawnData.ExecutedFrom) return true;
            if (OwnerMod != spawnData.OwnerMod) return true;
            if (OwnerRule != spawnData.OwnerRule) return true;
            if (Spawner != spawnData.Spawner) return true;
            if (OStamp != spawnData.OStamp) return true;
            if (OMulti != spawnData.OMulti) return true;
            if (Offset != spawnData.Offset) return true;
            if (RotationOffset != spawnData.RotationOffset) return true;
            if (LocalRotationOffset != spawnData.LocalRotationOffset) return true;
            if (DirectionalOffset != spawnData.DirectionalOffset) return true;
            if (TempPositionOffset != spawnData.TempPositionOffset) return true;
            if (TempRotationOffset != spawnData.TempRotationOffset) return true;
            if (AdditionalGenerated == null && spawnData.AdditionalGenerated != null) return true;
            if (AdditionalGenerated != null && spawnData.AdditionalGenerated == null) return true;
            return false;
        }

        public Vector3 GetFullOffset(bool tempIfZero = false)
        {
            if (!tempIfZero)
            {
                if (OutsidePositionOffset != Vector3.zero)
                    return Offset + GetDirectionalOffsetWithSeparatelyModOffset();
                else
                    return Offset + Quaternion.Euler(RotationOffset) * DirectionalOffset;
            }
            else
            {
                Vector3 off = GetFullOffset(false);
                if (off == Vector3.zero) off = TempPositionOffset;
                return off;
            }
        }

        public Vector3 GetPosWithFullOffset(bool tempIfZero = false, FieldSetup withOwnerCellPos = null)
        {
            Vector3 pos = GetFullOffset(tempIfZero);

            if (withOwnerCellPos)
                return withOwnerCellPos.GetCellWorldPosition(OwnerCell.Pos) + pos;
            else
                return pos;
        }

        public Vector3 GetFullRotationOffset()
        {
            if (TempRotationOffset != Vector3.zero && (RotationOffset == Vector3.zero || LocalRotationOffset == Vector3.zero)) return TempRotationOffset;
            return RotationOffset + LocalRotationOffset + OutsideRotationOffset;
        }

        public Quaternion GetRotationOffset()
        {
            return Quaternion.Euler(GetFullRotationOffset());
        }

        internal void CopyPositionTo(SpawnData spawn)
        {
            spawn.TempPositionOffset = TempPositionOffset;
            spawn.Offset = Offset;
            spawn.Offset += GetRotationOffset() * (DirectionalOffset + OutsidePositionOffset);
        }

        internal void CopyRotationTo(SpawnData spawn)
        {
            spawn.RotationOffset = RotationOffset;
            spawn.LocalRotationOffset = LocalRotationOffset;
            spawn.TempRotationOffset = TempRotationOffset;
            spawn.OutsideRotationOffset = OutsideRotationOffset;
        }

        internal void CopyScaleTo(SpawnData spawn)
        {
            spawn.LocalScaleMul = LocalScaleMul;
        }

        public Bounds GetMeshFilterOrColliderBounds()
        {
            Bounds b = new Bounds(Vector3.zero, Vector3.zero);
            if (PreviewMesh) b = PreviewMesh.bounds;

            GameObject pr = GetPrefab();

            if (pr)
            {
                Renderer filtr = pr.gameObject.GetComponentInChildren<Renderer>();

                if (filtr)
                {
                    pr.transform.position = Vector3.zero;
                    Quaternion preRot = pr.transform.rotation;
                    pr.transform.rotation = Quaternion.identity;
                    b = filtr.bounds;

                    if (filtr.transform.parent != null)
                        b.center -= filtr.transform.TransformVector(filtr.transform.localPosition);

                    pr.transform.rotation = preRot;
                }

                Collider col = pr.GetComponentInChildren<Collider>();
                if (col)
                {
                    if (col.bounds.size.magnitude > b.size.magnitude)
                        b.size = col.bounds.size;
                }
            }

            return b;
        }

    }
}
