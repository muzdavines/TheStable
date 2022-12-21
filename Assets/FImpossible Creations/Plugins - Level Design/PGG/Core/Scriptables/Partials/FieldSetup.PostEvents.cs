using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class FieldSetup : ScriptableObject
    {

        [NonSerialized] public List<Action> OnAfterGeneratingEvents = null;

        public void AddAfterGeneratingEvent(Action a)
        {
            if (OnAfterGeneratingEvents == null) OnAfterGeneratingEvents = new List<Action>();
            OnAfterGeneratingEvents.Add(a);
        }


        #region Post Events Variables

        [Space(4)]
        public bool AddReflectionProbes = false;
        [Tooltip("To create ReflectionProbeRuntime preset you simply click on any reflection probe component with right click and hit 'Export Runtime Preset File'")]
        public FReflectionProbePreset MainReflectionSettings;
        public bool AddMultipleProbes = false;
        [HideInInspector] public FReflectionProbePreset SmallerReflSettings;
        [HideInInspector] public FReflectionProbePreset MiniReflSettings;
        [Tooltip("pSize in cells] Apply smaller reflection probe preset if taking less cells than this count value")]
        [HideInInspector] public int SmallerReflLowerSpaceThan = 4;
        [HideInInspector] public float LimitSingleProbeSize = 0;

        [Space(4)]
        public bool AddLightProbes = false;
        [Range(1, 4)] public int ProbesPerCell = 1;

        public enum ETriggerGenerationMode { None, BoxFit, MultipleBoxesFill }
        [Space(4)] public ETriggerGenerationMode TriggerColliderGeneration = ETriggerGenerationMode.None;
        public TriggerGenerationSettings TriggerGenSettings;

        //[Space(4)]
        //[Tooltip("[Only if using mesh combine feature on Mod Packs/Field Mods]\nIf you want to weld vertices which are overlapping each other after combining the meshes")]
        //public bool WeldCombined = false;

        public bool RequiresScaledGraphs()
        {
            for (int p = 0; p < ModificatorPacks.Count; p++)
            {
                if (ModificatorPacks[p] == null) continue;
                for (int m = 0; m < ModificatorPacks[p].FieldModificators.Count; m++)
                {
                    if (ModificatorPacks[p].FieldModificators[m] == null) continue;
                    if (ModificatorPacks[p].FieldModificators[m].RequiresScaledGraphs()) return true;
                }
            }

            return false;
        }

        #endregion

        private Vector3 S(Vector3 toScale, Vector3 scale)
        {
            return Vector3.Scale(toScale, scale);
        }

        /// <summary>
        /// Generating reflection probes and others
        /// </summary>
        public void PostEvents(ref InstantiatedFieldInfo generatedRef, FGenGraph<FieldCell, FGenPoint> grid, Bounds fieldBounds, Transform container)
        {
            generatedRef.GeneratedTriggers = new List<BoxCollider>();
            generatedRef.GeneratedReflectionProbes = new List<ReflectionProbe>();


            if ((!MainReflectionSettings || AddReflectionProbes == false) && TriggerColliderGeneration == ETriggerGenerationMode.None)
            {
                if (AddLightProbes) GenerateLightProbes(grid, ref generatedRef, fieldBounds, container);
                ResetScaledGraphs();
                return;
            }


            #region Preparing Data for post generation

            // Prepare bounds shapes for trigger collider or reflection probes
            List<Bounds> optimalBounds = new List<Bounds>();

            bool generateOptimal = false;
            if (AddReflectionProbes && MainReflectionSettings && AddMultipleProbes) generateOptimal = true;
            if (!generateOptimal) if (TriggerColliderGeneration == ETriggerGenerationMode.MultipleBoxesFill) generateOptimal = true;

            if (generateOptimal)
            {
                Vector3 scale = Vector3.one;
                //if (generatedRef.ParentSetup) scale = generatedRef.ParentSetup.GetCellUnitSize();
                //scale /= 2f;

                // Generate bounds covering grid shape ------------------------------------
                FGenGraph<FieldCell, FGenPoint> graphCopy = grid.Copy();
                for (int i = 0; i <= 1000; i++)
                {
                    if (graphCopy.AllApprovedCells.Count == 0) break;

                    var startCell = graphCopy.AllApprovedCells[0];
                    Bounds iBounds = new Bounds(S(startCell.Pos, scale), scale);

                    // Nearest margins cells variables
                    FieldCell negX, negZ, posX, posZ;
                    negX = new FieldCell(); negZ = new FieldCell(); posX = new FieldCell(); posZ = new FieldCell();
                    CheckGraphForNearestMargins(graphCopy, TriggerGenSettings.MaxCellChecks, startCell, ref posX, ref negX, ref posZ, ref negZ);
                    graphCopy.RemoveCell(startCell);

                    if (negX != null && negZ != null && posX != null && posZ != null)
                        for (int x = negX.Pos.x; x <= posX.Pos.x; x++)
                            for (int z = negZ.Pos.z; z <= posZ.Pos.z; z++)
                            {
                                var mCell = graphCopy.GetCell(new Vector3Int(x, startCell.Pos.y, z), false);
                                if (FGenerators.CheckIfExist_NOTNULL(mCell)) if (mCell.InTargetGridArea)
                                    {
                                        iBounds.Encapsulate(new Bounds(S(mCell.Pos, scale), scale));
                                        graphCopy.RemoveCell(mCell);
                                    }
                            }

                    optimalBounds.Add(iBounds);

                    if (i == 1000) UnityEngine.Debug.Log("Safety end (1000 iterations, bounds created: " + optimalBounds.Count);
                }


                // Join bounds which can be connected ---------------
                if (TriggerGenSettings.JoinBounds)
                {
                    List<int> toRemove = new List<int>();
                    for (int i = optimalBounds.Count - 1; i >= 0; i--)
                    {
                        if (i >= optimalBounds.Count) i = optimalBounds.Count - 1;
                        toRemove.Clear();

                        Bounds rBound = optimalBounds[i];
                        if (rBound.size.x < 1.5f || rBound.size.z < 1.5f) continue;

                        // Checking all other bounds to find relations and possibility for joining
                        for (int b = 0; b < optimalBounds.Count; b++)
                        {
                            if (b == i) continue;
                            Bounds oBound = optimalBounds[b];

                            // Checking if bounds are next to each other and have same x size or same z side
                            if ((oBound.min.x == rBound.min.x && oBound.max.x == rBound.max.x) &&
                                ((oBound.max.z == rBound.min.z || oBound.min.z == rBound.max.z)))
                            {
                                rBound.Encapsulate(oBound);
                                toRemove.Add(b);
                            }
                            else
                            if ((oBound.min.z == rBound.min.z && oBound.max.z == rBound.max.z) &&
                                ((oBound.max.x == rBound.min.x || oBound.min.x == rBound.max.x)))
                            {
                                rBound.Encapsulate(oBound);
                                toRemove.Add(b);
                            }
                        }

                        // Aplying modified bounds
                        optimalBounds[i] = rBound;

                        // Removing joined bounds
                        for (int r = toRemove.Count - 1; r >= 0; r--)
                        {
                            optimalBounds.RemoveAt(toRemove[r]);
                        }
                    }

                    // after all join smaller ones
                    for (int i = optimalBounds.Count - 1; i >= 0; i--)
                    {
                        if (i >= optimalBounds.Count) i = optimalBounds.Count - 1;
                        toRemove.Clear();

                        Bounds rBound = optimalBounds[i];
                        if (rBound.size.x > 1.5f && rBound.size.z > 1.5f) continue;

                        // Checking all other bounds to find relations and possibility for joining
                        for (int b = 0; b < optimalBounds.Count; b++)
                        {
                            if (b == i) continue;
                            Bounds oBound = optimalBounds[b];

                            // Checking if bounds are next to each other and have same x size or same z side
                            if ((oBound.min.x == rBound.min.x && oBound.max.x == rBound.max.x) &&
                                ((oBound.max.z == rBound.min.z || oBound.min.z == rBound.max.z)))
                            {
                                rBound.Encapsulate(oBound);
                                toRemove.Add(b);
                            }
                            else
                            if ((oBound.min.z == rBound.min.z && oBound.max.z == rBound.max.z) &&
                                ((oBound.max.x == rBound.min.x || oBound.min.x == rBound.max.x)))
                            {
                                rBound.Encapsulate(oBound);
                                toRemove.Add(b);
                            }
                        }

                        // Aplying modified bounds
                        optimalBounds[i] = rBound;

                        // Removing joined bounds
                        for (int r = toRemove.Count - 1; r >= 0; r--)
                        {
                            optimalBounds.RemoveAt(toRemove[r]);
                        }
                    }
                }
            }

            #endregion


            if (MainReflectionSettings && AddReflectionProbes)
            {

                #region Reflection probes

                if (!AddMultipleProbes)
                {
                    #region Single Probe

                    GameObject probeObj = new GameObject(name + "-ReflectionProbe");
                    probeObj.transform.SetParent(container, true);

                    ReflectionProbe probe = probeObj.AddComponent<ReflectionProbe>();
                    MainReflectionSettings.AssignSettingsTo(probe);

                    probe.transform.position = fieldBounds.center;
                    probe.size = fieldBounds.size * 1.033f;

                    generatedRef.GeneratedReflectionProbes.Add(probe);
                    generatedRef.Instantiated.Add(probeObj);

                    #endregion
                }
                else
                {
                    GameObject probesContainer = new GameObject(name + "-ReflectionProbes");

                    probesContainer.transform.SetParent(container, true);
                    probesContainer.transform.localPosition = Vector3.zero;

                    generatedRef.Instantiated.Add(probesContainer);
                    //Matrix4x4 rot = Matrix4x4.Rotate(container.rotation);

                    var probesBounds = optimalBounds;

                    if (LimitSingleProbeSize > 1f)
                    {
                        probesBounds = new List<Bounds>();
                        for (int i = 0; i < optimalBounds.Count; i++)
                        {
                            Bounds b = optimalBounds[i];
                            //Vector3 scaledBSize = S(b.size, GetCellUnitSize());

                            if (b.size.magnitude > LimitSingleProbeSize)
                            {
                                // Split to few probes
                                int splitInto = Mathf.CeilToInt(b.size.magnitude / LimitSingleProbeSize);

                                if (b.size.x > b.size.z)
                                {
                                    for (int s = 0; s < splitInto; s++)
                                    {
                                        Bounds nb = b;
                                        nb.size = new Vector3(b.size.x / splitInto, b.size.y, b.size.z);
                                        Vector3 pos = b.center;
                                        pos.x -= b.extents.x;
                                        pos.x += nb.extents.x;
                                        pos.x += b.size.x / (splitInto) * s;
                                        nb.center = pos;
                                        probesBounds.Add(nb);
                                    }

                                }
                                else // Split the Z Depth
                                {
                                    for (int s = 0; s < splitInto; s++)
                                    {
                                        Bounds nb = b;
                                        nb.size = new Vector3(b.size.x, b.size.y, b.size.z / splitInto);
                                        Vector3 pos = b.center;
                                        pos.z -= b.extents.z;
                                        pos.z += nb.extents.z;
                                        pos.z += b.size.z / (splitInto) * s;
                                        nb.center = pos;
                                        probesBounds.Add(nb);
                                    }

                                }
                            }
                            else
                                probesBounds.Add(b);
                        }
                    }

                    #region Multiple Probes

                    for (int i = 0; i < probesBounds.Count; i++)
                    {
                        Bounds b = probesBounds[i];

                        Vector3 scaledBSize = S(b.size, GetCellUnitSize());

                        FReflectionProbePreset preset = MainReflectionSettings;
                        if (b.size.x < SmallerReflLowerSpaceThan || b.size.z < SmallerReflLowerSpaceThan)
                        {
                            preset = SmallerReflSettings;

                            if (MiniReflSettings)
                                if (b.size.x + b.size.z < SmallerReflLowerSpaceThan * 0.75f)
                                    preset = MiniReflSettings;
                        }

                        if (preset)
                        {
                            Vector3 size = scaledBSize * TriggerGenSettings.ScaleupTriggers;
                            size = new Vector3(size.x, fieldBounds.size.y, size.z);


                            //b.center = rot.MultiplyPoint(new Vector3(b.center.x, fieldBounds.center.y, b.center.z) * CellSize);
                            Vector3 center = S(b.center, GetCellUnitSize());
                            //Vector3 center = rot.MultiplyPoint(b.center * CellSize);
                            center = new Vector3(center.x, fieldBounds.center.y, center.z);
                            center.y = probesContainer.transform.InverseTransformPoint(center).y;

                            //center = reflProbeBounds.center;
                            //size = reflProbeBounds.size;

                            GameObject probeObj = new GameObject(name + "-ReflectionProbe");
                            probeObj.transform.SetParent(probesContainer.transform, true);

                            ReflectionProbe probe = probeObj.AddComponent<ReflectionProbe>();
                            preset.AssignSettingsTo(probe);
                            generatedRef.GeneratedReflectionProbes.Add(probe);

                            probe.transform.localPosition = center;
                            //probe.transform.position = b.center;
                            probe.size = size * 1.033f;
                            //probe.size = b.size * 1.033f;


                            // Since for unity reflection probes something like "rotation" doesn't exist :/
                            // we need to try rotating bounds, only way which may work correctly is using 90, 180 and 270 degrees rotations
                            b = FEngineering.RotateBoundsByMatrix(new Bounds(Vector3.zero, probe.size), container.rotation);
                            probe.size = b.size;

                            generatedRef.Instantiated.Add(probeObj);
                        }
                    }

                    #endregion


                    probesContainer.transform.localRotation = Quaternion.identity;

                }

                #endregion

            }


            if (AddLightProbes) GenerateLightProbes(grid, ref generatedRef, fieldBounds, container);


            if (TriggerColliderGeneration != ETriggerGenerationMode.None)
            {
                #region Trigger Colliders

                GameObject triggerContainer = new GameObject(name + "-Triggers");
                triggerContainer.layer = TriggerGenSettings.TriggersLayer;
                triggerContainer.transform.SetParent(container);
                triggerContainer.transform.localPosition = Vector3.zero;
                generatedRef.Instantiated.Add(triggerContainer);

                if (TriggerColliderGeneration == ETriggerGenerationMode.BoxFit)
                {
                    BoxCollider trigger = new GameObject(name + "-TriggerVolume").AddComponent<BoxCollider>();
                    trigger.gameObject.layer = TriggerGenSettings.TriggersLayer;
                    trigger.transform.SetParent(triggerContainer.transform);
                    trigger.isTrigger = true;
                    trigger.transform.position = fieldBounds.center;
                    trigger.size = fieldBounds.size * TriggerGenSettings.ScaleupTriggers;

                    generatedRef.GeneratedTriggers.Add(trigger);
                    generatedRef.Instantiated.Add(trigger.gameObject);
                }
                else if (TriggerColliderGeneration == ETriggerGenerationMode.MultipleBoxesFill)
                {

                    List<BoxCollider> triggerColliders = new List<BoxCollider>();
                    //Matrix4x4 rot = Matrix4x4.Rotate(container.rotation);

                    // Generating multiple trigger colliders ----------
                    for (int i = 0; i < optimalBounds.Count; i++)
                    {
                        Bounds b = optimalBounds[i];
                        Vector3 scaledBSize = S(b.size, GetCellUnitSize());

                        Vector3 size = scaledBSize * TriggerGenSettings.ScaleupTriggers;
                        Vector3 center = S(b.center, GetCellUnitSize());
                        //Vector3 center = rot.MultiplyPoint(b.center * CellSize);

                        size = new Vector3(size.x, TriggerGenSettings.FillHeight ? fieldBounds.size.y : size.y, size.z);
                        center = new Vector3(center.x, TriggerGenSettings.FillHeight ? fieldBounds.center.y : center.y, center.z);
                        if (TriggerGenSettings.FillHeight) center.y = triggerContainer.transform.InverseTransformPoint(center).y;

                        BoxCollider trigger = new GameObject(name + "-TriggerVolume").AddComponent<BoxCollider>();
                        trigger.gameObject.layer = TriggerGenSettings.TriggersLayer;
                        trigger.isTrigger = true;
                        trigger.transform.localPosition = center;
                        trigger.size = size;
                        triggerColliders.Add(trigger);
                        trigger.transform.SetParent(triggerContainer.transform, false);

                        generatedRef.GeneratedTriggers.Add(trigger);
                        generatedRef.Instantiated.Add(trigger.gameObject);

                        if (TriggerGenSettings.GenerateRelations)
                        {
                            TriggersRelation relation = trigger.gameObject.AddComponent<TriggersRelation>();
                            relation.Refresh();
                            //relation._GenBoundID = i;
                        }
                    }

                    if (triggerContainer.transform)
                        triggerContainer.transform.localRotation = Quaternion.identity;

                    // Generate relations ----------
                    if (TriggerGenSettings.GenerateRelations)
                    {
                        // Scale to detect intersections
                        for (int i = 0; i < optimalBounds.Count; i++)
                        {
                            Bounds b = optimalBounds[i];
                            b.size *= 1.1f;
                            optimalBounds[i] = b;
                        }

                        // Check intersection on every bound
                        for (int i = 0; i < optimalBounds.Count; i++)
                        {
                            TriggersRelation relation = triggerColliders[i].GetComponent<TriggersRelation>();
                            Bounds rBound = optimalBounds[i];

                            for (int b = 0; b < optimalBounds.Count; b++)
                            {
                                if (i == b) continue;
                                Bounds oBound = optimalBounds[b];

                                if (rBound.Intersects(oBound))
                                {
                                    TriggersRelation othRelation = triggerColliders[b].GetComponent<TriggersRelation>();
                                    if (othRelation) relation.AddNeightbour(othRelation);
                                }
                            }
                        }
                    }


                    // Grouping Trigger Colliders
                    if (TriggerGenSettings.GroupUpToSize > 0)
                    {
                        List<BoxCollider> tColliders = new List<BoxCollider>();

                        // Copying list since we will remove some of its elements when parenting
                        for (int i = 0; i < triggerColliders.Count; i++) tColliders.Add(triggerColliders[i]);

                        for (int i = tColliders.Count - 1; i >= 0; i--)
                        {
                            if (i >= tColliders.Count) i = tColliders.Count - 1;

                            BoxCollider coll = tColliders[i];
                            int scale = Mathf.RoundToInt((coll.bounds.extents.x + coll.bounds.extents.z) / (float)CellSize);

                            if (scale < TriggerGenSettings.GroupUpToSize) // Trigger small enough to group with neightbours
                            {
                                TriggersRelation relation = coll.gameObject.GetComponent<TriggersRelation>();
                                int conn = 0;
                                int connScale = scale;
                                if (connScale > TriggerGenSettings.GroupUpToSize) continue;

                                if (relation != null)
                                    if (relation.Neightbours != null)
                                        for (int r = 0; r < relation.Neightbours.Count; r++)
                                        {
                                            if (connScale > TriggerGenSettings.GroupUpToSize) continue;
                                            var nColl = relation.Neightbours[r].Trigger;
                                            if (nColl == null) continue;
                                            if (nColl == coll) continue;

                                            if (TriggerGenSettings.MaxGroupDistance > 0)
                                            {
                                                if (Vector3.Distance(nColl.ClosestPoint(coll.bounds.center), coll.ClosestPoint(nColl.bounds.center))
                                                     > TriggerGenSettings.MaxGroupDistance)
                                                    continue;
                                                //if (Vector3.Distance((coll.bounds.center), (nColl.bounds.center))
                                                //     > TriggerGenSettings.MaxGroupDistance)
                                                //continue;
                                            }

                                            int nScale = Mathf.RoundToInt((nColl.bounds.size.x + nColl.bounds.size.z) / (float)CellSize);
                                            if (nScale < TriggerGenSettings.GroupUpToSize) // target neightbour size allowed
                                            {
                                                nColl.transform.SetParent(coll.transform, true);
                                                BoxCollider bx = relation.Trigger as BoxCollider;
                                                tColliders.Remove(bx);
                                                conn++;
                                                connScale += nScale;
                                            }
                                        }
                            }

                        }

                        // Connecting rest of the colliders, smallest ones
                        if (TriggerGenSettings.GroupUpToSize > 0)
                            for (int i = 0; i < tColliders.Count; i++)
                            {
                                BoxCollider coll = tColliders[i];
                                //Debug.DrawRay(coll.transform.position + coll.center, Vector3.up, Color.red, 1.1f);
                                int scale = Mathf.RoundToInt((coll.bounds.extents.x + coll.bounds.extents.z) / (float)CellSize);

                                if (scale < TriggerGenSettings.GroupUpToSize) // Trigger small enough to group with neightbours
                                {
                                    TriggersRelation relation = coll.gameObject.GetComponent<TriggersRelation>();
                                    if (relation.Neightbours.Count > 0) coll.transform.SetParent(relation.Neightbours[0].transform, true);
                                }
                            }
                    }

                }

                #endregion
            }


            ResetScaledGraphs();

        }


        void GenerateLightProbes(FGenGraph<FieldCell, FGenPoint> grid, ref InstantiatedFieldInfo generatedRef, Bounds fieldBounds, Transform container)
        {
#if UNITY_EDITOR
            GameObject probeObj = new GameObject(name + "-LightProbe");

            LightProbeGroup probe = probeObj.AddComponent<LightProbeGroup>();

            List<Vector3> positions = new List<Vector3>();
            //Vector3 refSize = Vector3.one * CellSize * 0.9f;

            if (ProbesPerCell > 1)
            {
                float div = 1;
                float refOffset = (CellSize * 0.3f) / (float)(div);
                Vector3 refOff = Vector3.zero;// /* new Vector3(1f,0f,1f) * CellSize * 0.5f; */ fieldBounds.center - Vector3.up * fieldBounds.center.y;
                if (ProbesPerCell == 4) div = 2;

                float smaller = 0.25f;
                if (ProbesPerCell == 3) smaller = 0.3f;
                if (ProbesPerCell == 4) smaller = 0.4f;

                float refYOffset = (fieldBounds.size.y * smaller) / (float)(div);
                int refCount = (int)div;

                for (int i = 0; i < grid.AllApprovedCells.Count; i++)
                {
                    var cell = grid.AllApprovedCells[i];
                    Vector3 pos = cell.WorldPos(CellSize) - refOff;

                    for (int x = -refCount; x <= refCount; x++)
                        for (int y = -refCount; y <= refCount; y++)
                            for (int z = -refCount; z <= refCount; z++)
                            {
                                if (ProbesPerCell == 2) if (x == 0 || y == 0 || z == 0) continue;

                                positions.Add(pos + new Vector3(refOffset * x, refYOffset * y, refOffset * z));
                            }
                }
            }
            else
            {
                Vector3 refOff = fieldBounds.center - Vector3.up * fieldBounds.center.y;
                float refYOffset = (fieldBounds.size.y * 0.3f);
                float refOffset = (CellSize * 0.5f);
                for (int i = 0; i < grid.AllApprovedCells.Count; i++)
                {
                    var cell = grid.AllApprovedCells[i];
                    Vector3 pos = cell.WorldPos(CellSize) - refOff;
                    positions.Add(pos + new Vector3(0f, refYOffset));
                    positions.Add(pos - new Vector3(0f, refYOffset));
                }
            }

            probe.probePositions = positions.ToArray();

            generatedRef.GeneratedLightProbes = probe;
            generatedRef.Instantiated.Add(probeObj);

            probeObj.transform.SetParent(container, true);
            probeObj.transform.ResetCoords();
#endif
        }


        public static void CheckGraphForNearestMargins(FGenGraph<FieldCell, FGenPoint> grid, int maxCells, FieldCell root, ref FieldCell px, ref FieldCell nx, ref FieldCell pz, ref FieldCell nz)
        {
            FieldCell preCell = root;
            pz = null;

            // Going with x from -max to +max
            // Going with z from zero to +max
            for (int x = 0; x <= maxCells; x++)
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(x, 0, 0));
                if ((FGenerators.CheckIfIsNull(xCell)) || xCell.InTargetGridArea == false) break;

                for (int z = 0; z <= maxCells; z++)
                {
                    if (x == 0 && z == 0) continue;
                    var zCell = grid.GetCell(root.Pos + new Vector3Int(x, 0, z));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (pz == null) pz = preCell; // Getting minimum positive z value cell
                        else if (preCell.Pos.z < pz.Pos.z) pz = preCell;
                    }
                }
            }
            for (int x = 1; x <= maxCells; x++)
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(-x, 0, 0));
                if ((FGenerators.CheckIfIsNull(xCell)) || !xCell.InTargetGridArea) break;

                for (int z = 0; z <= maxCells; z++)
                {
                    if (x == 0 && z == 0) continue;
                    var zCell = grid.GetCell(root.Pos + new Vector3Int(-x, 0, z));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (pz == null) pz = preCell;
                        else if (preCell.Pos.z < pz.Pos.z) pz = preCell;
                    }
                }
            }



            preCell = root;
            px = null;
            // Going with z from -max to +max
            // Going with x from zero to +max
            for (int zz = 0; zz <= maxCells; zz++)
            {
                var zzCell = grid.GetCell(root.Pos + new Vector3Int(0, 0, zz));
                if ((FGenerators.CheckIfIsNull(zzCell)) || !zzCell.InTargetGridArea) break;

                for (int xx = 0; xx <= maxCells; xx++)
                {
                    if (zz == 0 && xx == 0) continue;
                    var xCell = grid.GetCell(root.Pos + new Vector3Int(xx, 0, zz));
                    if ((FGenerators.CheckIfExist_NOTNULL(xCell)) && xCell.InTargetGridArea) preCell = xCell;
                    else // No further cells
                    {
                        if (px == null) px = preCell;
                        else if (preCell.Pos.x < px.Pos.x) px = preCell;
                    }
                }
            }
            for (int zz = 1; zz <= maxCells; zz++)
            {
                var zzCell = grid.GetCell(root.Pos + new Vector3Int(0, 0, -zz));
                if ((FGenerators.CheckIfIsNull(zzCell)) || !zzCell.InTargetGridArea) break;

                for (int xx = 0; xx <= maxCells; xx++)
                {
                    if (zz == 0 && xx == 0) continue;
                    var xCell = grid.GetCell(root.Pos + new Vector3Int(xx, 0, -zz));
                    if ((FGenerators.CheckIfExist_NOTNULL(xCell)) && xCell.InTargetGridArea) preCell = xCell;
                    else // No further cells
                    {
                        if (px == null) px = preCell;
                        else if (preCell.Pos.x < px.Pos.x) px = preCell;
                    }
                }
            }


            preCell = root;
            nz = null;
            // Going with x from -max to +max
            // Going with z from zero to -max
            for (int x = 0; x <= maxCells; x++)
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(x, 0, 0));
                if ((FGenerators.CheckIfIsNull(xCell)) || !xCell.InTargetGridArea) break;
                for (int z = 0; z <= maxCells; z++)
                {
                    if (x == 0 && z == 0) continue;
                    var zCell = grid.GetCell(root.Pos + new Vector3Int(x, 0, -z));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (nz == null) nz = preCell; // Getting maximum negative z value cell
                        else if (preCell.Pos.z > nz.Pos.z) nz = preCell;
                    }
                }
            }
            for (int x = 1; x <= maxCells; x++)
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(-x, 0, 0));
                if ((FGenerators.CheckIfIsNull(xCell)) || !xCell.InTargetGridArea) break;
                for (int z = 0; z <= maxCells; z++)
                {
                    if (x == 0 && z == 0) continue;

                    var zCell = grid.GetCell(root.Pos + new Vector3Int(-x, 0, -z));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (nz == null) nz = preCell;
                        else if (preCell.Pos.z > nz.Pos.z) nz = preCell;
                    }
                }
            }


            preCell = root;
            nx = null;
            // Going with z from -max to +max
            // Going with x from zero to -max
            for (int zz = 0; zz <= maxCells; zz++) //var xCell = grid.GetCell(root.Pos + new Vector3Int(x, 0, 0));
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(0, 0, zz));
                if ((FGenerators.CheckIfIsNull(xCell)) || !xCell.InTargetGridArea) break;

                for (int xx = 0; xx <= maxCells; xx++)
                {
                    if (zz == 0 && xx == 0) continue;

                    var zCell = grid.GetCell(root.Pos + new Vector3Int(-xx, 0, zz));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (nx == null) nx = preCell; // Getting minimum positive z value cell
                        else if (preCell.Pos.x > nx.Pos.x) nx = preCell;
                    }
                }
            }
            for (int zz = 1; zz <= maxCells; zz++) // going with x negatively -> getcell pos - x
            {
                var xCell = grid.GetCell(root.Pos + new Vector3Int(0, 0, -zz));
                if ((FGenerators.CheckIfIsNull(xCell)) || !xCell.InTargetGridArea) break;

                for (int xx = 0; xx <= maxCells; xx++)
                {
                    if (zz == 0 && xx == 0) continue;
                    var zCell = grid.GetCell(root.Pos + new Vector3Int(-xx, 0, -zz));
                    if ((FGenerators.CheckIfExist_NOTNULL(zCell)) && zCell.InTargetGridArea) preCell = zCell;
                    else // No further cells
                    {
                        if (nx == null) nx = preCell; // Getting minimum positive z value cell
                        else if (preCell.Pos.x > nx.Pos.x) nx = preCell;
                    }
                }
            }

        }


        [System.Serializable]
        public class TriggerGenerationSettings
        {
            [FPD_Layers]
            public int TriggersLayer = 0;
            [Tooltip("Scale trigger box colliders to be slightly bigger or smaller")]
            [Range(0.75f, 1.5f)] public float ScaleupTriggers = 1f;
            [Tooltip("Treat Y position cells as separate axis or make higher trigger colliders to reach this cells")]
            public bool FillHeight = true;
            [Tooltip("Max cells to check for generating trigger collider in +-x and +-z")]
            [Range(4, 32)] public int MaxCellChecks = 10;
            //[Tooltip("When trigger have width or height of this number or higher then it will try to create one large collider than few stripped")]
            public bool JoinBounds = true;
            [Tooltip("Add relation component to each prefab containing info about aligned neightbour trigger colliders")]
            public bool GenerateRelations = true;
            [Range(0, 32)] public int GroupUpToSize = 8;
            [Range(0, 32)] public int MaxGroupDistance = 8;
        }

    }

}