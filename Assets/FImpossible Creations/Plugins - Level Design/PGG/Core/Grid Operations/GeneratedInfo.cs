using FIMSpace.Generating.Checker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Class with informations and management about generated-instantiated game objects by FieldSetup
    /// </summary>
    [System.Serializable]
    public class InstantiatedFieldInfo
    {
        [System.NonSerialized] public FlexibleGeneratorSetup FlexSetup;
        public FieldSetup InternalField;
        public FieldSetup ParentSetup { get { if (InternalField) return InternalField; return FlexSetup.FieldPreset; } }
        public FieldSetup RunningFieldSetup { get { return FlexSetup.RuntimeFieldSetup; } }

        public CellsController GenController { get { return FlexSetup.CellsController; } }
        [HideInInspector] public List<GameObject> Instantiated;

        // Dictionary
        public List<CellInstanitations> InstantiatedInfos;
        [NonSerialized] public Dictionary<Vector3Int, CellInstanitations> InstantiatedInfoPerCell;

        public GameObject MainContainer;
        public Transform FieldTransform;

        [HideInInspector] public FGenGraph<FieldCell, FGenPoint> Grid;

        public Bounds RoomBounds;

        public LightProbeGroup GeneratedLightProbes;
        public List<ReflectionProbe> GeneratedReflectionProbes;
        public List<BoxCollider> GeneratedTriggers;

        public List<CheckerField> OptionalCheckerFieldsData;
        public List<InstantiationContainer> InstantiationContainers = new List<InstantiationContainer>();

        public bool Initialized { get { return initialized; } }
        [SerializeField] [HideInInspector] private bool initialized = false;
        public void Initialize(FlexibleGeneratorSetup flex)
        {
            if (Instantiated == null) Instantiated = new List<GameObject>();
            if (InstantiatedInfos == null) InstantiatedInfos = new List<CellInstanitations>();
            RefreshReferences(flex);
        }

        public void RefreshReferences(FlexibleGeneratorSetup flex)
        {
            FlexSetup = flex;
            RestoreDictionary();
        }

        public void SetupContainer(Transform container = null)
        {
            FieldTransform = container;
        }


        #region Basic Access


        /// <summary>
        /// List of grid cells positions in world with center origin
        /// </summary>
        public List<Vector3> GetGridWorldPositions()
        {
            List<Vector3> pos = new List<Vector3>();
            Vector3 size = ParentSetup.GetCellUnitSize();
            for (int i = 0; i < Grid.AllApprovedCells.Count; i++)
            {
                pos.Add(Grid.AllApprovedCells[i].WorldPos(size));
            }

            return pos;
        }


        public Transform GetTriggersContainer()
        {
            for (int i = 0; i < GeneratedTriggers.Count; i++)
            {
                if (GeneratedTriggers[i].transform.parent.name.Contains("Triggers")) return GeneratedTriggers[i].transform.parent;
            }

            return null;
        }

        internal void Clear(bool destroyAll)
        {
            if (Instantiated == null) Instantiated = new List<GameObject>();

            if (destroyAll)
            {
                for (int i = 0; i < Instantiated.Count; i++)
                {
                    if (Instantiated[i]) FGenerators.DestroyObject(Instantiated[i]);
                }
            }

            if (InstantiatedInfoPerCell != null) InstantiatedInfoPerCell.Clear();
            if (InstantiatedInfos != null) InstantiatedInfos.Clear();

            Instantiated.Clear();

            if (CustomInstantiatedList != null) CustomInstantiatedList.Clear();
            if (CustomToDestroyList != null) CustomToDestroyList.Clear();

            Grid = null;
        }

        public float GetCellSize()
        {
            return ParentSetup.GetCellUnitSize().x;
        }


        internal CellInstanitations GetInfoAt(Vector3Int parentCellPos)
        {
            CellInstanitations datas = null;

            if (InstantiatedInfoPerCell.TryGetValue(parentCellPos, out datas))
            {
                return datas;
            }

            return null;
        }


        #endregion


        #region Managed Containers


        /// <summary>
        /// Making sure there is main container for instantiated objects
        /// </summary>
        public void CheckInstantiationContainer()
        {
            if (ParentSetup == null) return;
            if (FieldTransform == null) FieldTransform = new GameObject(ParentSetup.name).transform;
        }


        /// <param name="justPacks"> Set to false if you want to get container of FieldModificator which parent pack is argument pack </param>
        public InstantiationContainer GetContainerOf(ModificatorsPack pack, bool generateIfNotExistingYet, bool justPacks = true)
        {
            CheckInstantiationContainer();

            InstantiationContainer container = null;
            for (int i = 0; i < InstantiationContainers.Count; i++)
            {
                if (InstantiationContainers[i].Mod != null) continue;
                if (InstantiationContainers[i].Pack == pack) container = InstantiationContainers[i];
            }

            if (container == null || container.Transform == null)
                if (!justPacks)
                    for (int i = 0; i < InstantiationContainers.Count; i++)
                    {
                        if (InstantiationContainers[i].Mod == null) continue;
                        if (InstantiationContainers[i].Pack == pack) container = InstantiationContainers[i];
                    }

            if (generateIfNotExistingYet)
                if (container == null || container.Transform == null)
                {
                    container = new InstantiationContainer(pack);
                    container.Transform.SetParent(FieldTransform, true);
                    container.Transform.ResetCoords();
                    InstantiationContainers.Add(container);
                }

            return container;
        }


        public InstantiationContainer GetContainerOf(FieldModification mod, bool generateIfNotExistingYet)
        {
            CheckInstantiationContainer();

            InstantiationContainer container = null;
            for (int i = 0; i < InstantiationContainers.Count; i++)
            {
                if (InstantiationContainers[i].Mod == mod) container = InstantiationContainers[i];
            }

            if (generateIfNotExistingYet)
                if (container == null || container.Transform == null)
                {
                    Transform targetContainer = FieldTransform;

                    if (mod.ParentPack != null)
                    {
                        var packContainer = GetContainerOf(mod.ParentPack, true);
                        targetContainer = packContainer.Transform;
                    }

                    container = new InstantiationContainer(mod);
                    container.Transform.SetParent(targetContainer, true);
                    container.Transform.ResetCoords();

                    InstantiationContainers.Add(container);
                }

            return container;
        }


        #endregion


        #region Dictionary Handling

        private void RestoreDictionary()
        {
            if (InstantiatedInfoPerCell == null)
            {
                if (InstantiatedInfos == null) InstantiatedInfos = new List<CellInstanitations>();
                InstantiatedInfoPerCell = new Dictionary<Vector3Int, CellInstanitations>();

                for (int i = InstantiatedInfos.Count - 1; i >= 0; i--)
                {
                    if (FGenerators.CheckIfExist_NOTNULL(InstantiatedInfos[i]))
                    {
                        if (InstantiatedInfos[i].Count > 0)
                        {
                            if (FGenerators.CheckIfExist_NOTNULL(InstantiatedInfos[i][0].spawn))
                            {
                                InstantiatedInfoPerCell.Add(InstantiatedInfos[i][0].spawn.OwnerCellPos, InstantiatedInfos[i]);
                            }
                            else
                                InstantiatedInfos.RemoveAt(i);
                        }
                        else
                        {
                            InstantiatedInfos.RemoveAt(i);
                        }
                    }
                    else
                        InstantiatedInfos.RemoveAt(i);
                }
            }
        }


        public void AddInfo(Vector3Int pos, CellInstanitations datas)
        {
            InstantiatedInfos.Add(datas);
            InstantiatedInfoPerCell.Add(pos, datas);
            //InstantiatedInfoPerCell.Add(pos, InstantiatedInfos[InstantiatedInfos.Count - 1]);
        }

        #endregion


        #region Generating

        public void AcquireInstantiation(InstantiatedData data)
        {
            RestoreDictionary();

            if (FGenerators.CheckIfIsNull(data.spawn))
            {
                UnityEngine.Debug.Log("Null Spawn!");
                return;
            }

            CellInstanitations datas = null;
            // Reserve space for cell position
            if (InstantiatedInfoPerCell.TryGetValue(data.spawn.OwnerCellPos, out datas) == false)
            {
                datas = new CellInstanitations();
                AddInfo(data.spawn.OwnerCellPos, datas);
            }

            if (datas == null)
            {
                UnityEngine.Debug.Log("Null instantiated list!");
                return;
            }

            datas.Add(data);

            Instantiated.Add(data.instantiated);

            if (useCustomInstantiatedList) CustomInstantiatedList.Add(data.instantiated);

            if (data.additionalInstantiated != null)
            {
                if (useCustomInstantiatedList)
                {
                    for (int i = 0; i < data.additionalInstantiated.Count; i++)
                    {
                        Instantiated.Add(data.additionalInstantiated[i]);
                        CustomInstantiatedList.Add(data.additionalInstantiated[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < data.additionalInstantiated.Count; i++)
                    {
                        Instantiated.Add(data.additionalInstantiated[i]);
                    }
                }
            }

            if (data.additionalEmitters != null)
            {
                for (int i = 0; i < data.additionalEmitters.Count; i++)
                {
                    data.additionalEmitters[i].Generate();
                    data.additionalEmitters[i].IG_CallAfterGenerated();
                }
            }
        }

        /// <returns> True if found spawns to replace in choosed cell </returns>
        internal bool ReplaceWith(FieldCell cellSpawnData, CellsController generatingController)
        {
            if (FGenerators.CheckIfIsNull(cellSpawnData)) return false;

            CellInstanitations datas;
            InstantiatedInfoPerCell.TryGetValue(cellSpawnData.Pos, out datas);

            if (datas != null)
            {
                bool diff = false;
                if (cellSpawnData.GetJustCellSpawnCount() != datas.Count)
                {
                    diff = true;
                }

                if (!diff)
                {
                    for (int i = 0; i < datas.Count; i++)
                    {
                        if (datas[i].spawn.FindDifference(cellSpawnData.GetSpawnsJustInsideCell()[i]))
                        {
                            diff = true; break;
                        }
                    }
                }

                if (diff)
                {
                    DestroyInstantiatedInCell(datas);
                    generatingController.ExtractInstantiation(cellSpawnData);
                }

                return true;
            }

            return false;
        }

        #endregion


        /// <summary> If you want to controll destroying objects with custom coding - CustomToDestroyList </summary>
        private bool useCustomToDestroyList = false;
        /// <summary> If you want to controll just instantiated objects for animation or something - CustomInstantiatedList</summary>
        private bool useCustomInstantiatedList = false;
        public List<GameObject> CustomToDestroyList { get; private set; }
        public List<GameObject> CustomInstantiatedList { get; private set; }
        public void DestroyInstantiatedInCell(CellInstanitations datas)
        {
            if (useCustomToDestroyList)
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    datas[i].TransferInstantiatedToList(CustomToDestroyList, true);
                }
            }
            else
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    datas[i].DestroyGeneratedObjects();
                }
            }

            datas.List.Clear();
            PGGUtils.CheckForNullsO(Instantiated);
        }

        /// <summary>
        /// Using custom instantiate controll and destroying for custom animations etc.
        /// </summary>
        public void SetCustomHandling(bool customInstantiate, bool customDestroy)
        {
            if (customInstantiate)
            {
                useCustomInstantiatedList = customInstantiate;
                if (CustomInstantiatedList == null) CustomInstantiatedList = new List<GameObject>();
            }

            if (customDestroy)
            {
                useCustomToDestroyList = customDestroy;
                if (CustomToDestroyList == null) CustomToDestroyList = new List<GameObject>();
            }
        }

    }
}
