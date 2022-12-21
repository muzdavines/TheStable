using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Structure which is holding data about instantiated spawns
    /// </summary>
    [System.Serializable]
    public struct InstantiatedData
    {
        public SpawnData spawn;
        public GameObject instantiated;
        public List<GameObject> additionalInstantiated;
        public List<IGenerating> additionalEmitters;
        public GameObject instantiatedContainer;

        public FieldSetup LastFieldSetup;
        public Transform LastContainer;
        public Matrix4x4 LastMatrix;


        public static InstantiatedData InstantiateSpawnData(SpawnData spawn, FieldSetup fieldSetup, Transform targetContainer, Matrix4x4 spawningMatrix)
        {
            InstantiatedData data = new InstantiatedData();

            data.LastFieldSetup = fieldSetup;
            data.LastMatrix = spawningMatrix;
            data.LastContainer = targetContainer;

            data.spawn = spawn;

            if (spawn.OnPreGeneratedEvents.Count != 0)
                for (int pe = 0; pe < spawn.OnPreGeneratedEvents.Count; pe++)
                {
                    spawn.OnPreGeneratedEvents[pe].Invoke(spawn);
                }

            if (spawn.Prefab != null || spawn.DontSpawnMainPrefab)
            {
                if (spawn.DontSpawnMainPrefab == false)
                {
                    GameObject spawned = null;

                    if (spawn.idInStampObjects == -2)
                    {
                        if (spawn.OwnerMod.DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp)
                        {
                            spawned = new GameObject(spawn.OwnerMod.name);
                            ObjectStampEmitter emitter = spawned.AddComponent<ObjectStampEmitter>();
                            emitter.PrefabsSet = spawn.OwnerMod.OStamp;
                            emitter.AlwaysDrawPreview = true;
                        }
                    }
                    else if (spawn.idInStampObjects == -1)
                    {
                        if (spawn.OwnerMod.DrawSetupFor == FieldModification.EModificationMode.ObjectMultiEmitter)
                        {
                            OStamperSet stamp = spawn.OwnerMod.OMultiStamp.PrefabsSets[FGenerators.GetRandom(0, spawn.OwnerMod.OMultiStamp.PrefabsSets.Count)];
                            spawned = new GameObject(stamp.name);
                            ObjectStampEmitter emitter = spawned.AddComponent<ObjectStampEmitter>();
                            emitter.PrefabsSet = stamp;
                            emitter.AlwaysDrawPreview = true;
                        }
                    }

                    if (spawned == null)
                    {
                        spawned = FGenerators.InstantiateObject(spawn.Prefab);
                        spawned.hideFlags = HideFlags.None;
                    }

                    spawned.transform.SetParent(targetContainer, true);

                    Vector3 targetPosition = fieldSetup.GetCellWorldPosition(spawn.OwnerCell);

                    Quaternion rotation = spawn.Prefab.transform.rotation * Quaternion.Euler(spawn.RotationOffset);

                    spawned.transform.position = spawningMatrix.MultiplyPoint(targetPosition + spawn.Offset + rotation * spawn.DirectionalOffset);

                    if (spawn.LocalRotationOffset != Vector3.zero) rotation *= Quaternion.Euler(spawn.LocalRotationOffset);
                    spawned.transform.rotation = spawningMatrix.rotation * rotation;
                    spawned.transform.localScale = Vector3.Scale(spawn.LocalScaleMul, spawn.Prefab.transform.lossyScale);

                    data.instantiated = spawned;

                    // Collecting generators
                    if (spawn.OwnerMod != null)
                    {
                        if (spawned.transform.childCount > 0)
                        {
                            for (int ch = 0; ch < spawned.transform.childCount; ch++)
                            {
                                IGenerating[] emitters = spawned.transform.GetChild(ch).GetComponentsInChildren<IGenerating>();
                                if (emitters.Length > 0) if (data.additionalEmitters == null) data.additionalEmitters = new List<IGenerating>();
                                for (int i = 0; i < emitters.Length; i++) data.additionalEmitters.Add(emitters[i]);
                            }
                        }

                        IGenerating emitter = spawned.GetComponent<IGenerating>();
                        if (emitter != null)
                        {
                            if (data.additionalEmitters == null) data.additionalEmitters = new List<IGenerating>();
                            data.additionalEmitters.Add(emitter);
                        }
                    }

                    // Post Events support
                    if (spawn.OnGeneratedEvents.Count != 0)
                        for (int pe = 0; pe < spawn.OnGeneratedEvents.Count; pe++)
                            spawn.OnGeneratedEvents[pe].Invoke(spawned);
                }
                else
                {
                    // Post Events support
                    if (spawn.OnGeneratedEvents.Count != 0)
                        for (int pe = 0; pe < spawn.OnGeneratedEvents.Count; pe++)
                            spawn.OnGeneratedEvents[pe].Invoke(null);
                }


                // Additional generated feature
                if (spawn.AdditionalGenerated != null)
                {
                    for (int sa = 0; sa < spawn.AdditionalGenerated.Count; sa++)
                    {
                        var spwna = spawn.AdditionalGenerated[sa];

                        if (spwna == null) continue;
                        spwna.transform.SetParent(targetContainer, true);

                        if (spwna.transform.childCount > 0)
                        {
                            for (int ch = 0; ch < spwna.transform.childCount; ch++)
                            {
                                IGenerating[] emitters = spwna.transform.GetChild(ch).GetComponentsInChildren<IGenerating>();
                                if (emitters.Length > 0) if (data.additionalEmitters == null) data.additionalEmitters = new List<IGenerating>();
                                for (int i = 0; i < emitters.Length; i++) data.additionalEmitters.Add(emitters[i]);
                            }
                        }

                        IGenerating emitter = spwna.GetComponent<IGenerating>();
                        if (emitter != null)
                        {
                            if (data.additionalEmitters == null) data.additionalEmitters = new List<IGenerating>();
                            data.additionalEmitters.Add(emitter);
                        }

                        if (spwna != null)
                        {
                            if (data.additionalInstantiated == null) data.additionalInstantiated = new List<GameObject>();
                            data.additionalInstantiated.Add(spwna);
                        }
                    }

                }

            }

            return data;
        }

        internal void DestroyGeneratedObjects()
        {
            FGenerators.DestroyObject(instantiated);

            if (additionalInstantiated != null)
            {
                for (int i = 0; i < additionalInstantiated.Count; i++)
                    FGenerators.DestroyObject(additionalInstantiated[i]);

                additionalInstantiated.Clear();
            }
        }

        internal void TransferInstantiatedToList(List<GameObject> list, bool clear)
        {
            if (instantiated) list.Add(instantiated);

            if (additionalInstantiated != null)
            {
                for (int i = 0; i < additionalInstantiated.Count; i++)
                    list.Add(additionalInstantiated[i]);
            }

            if (clear)
            {
                instantiated = null;
                if (additionalInstantiated != null) additionalInstantiated.Clear();
            }
        }
    }

}
