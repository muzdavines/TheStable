#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Planning.GeneratingLogics;
using System.Collections;

namespace FIMSpace.Generating
{
    public partial class BuildPlannerExecutor : MonoBehaviour
    {

        public bool IsGenerating { get; private set; } = false;
        public float GeneratingProgress { get { if (instantiationProgress != null) return instantiationProgress.Value * _progressMul; if (generatingInstance == null) return 0f; return generatingInstance.GeneratingProgress * _progressMul; } }
        public float GeneratingProgressSmooth { get { if (instantiationProgress != null) return instantiationProgress.Value * _progressMul; if (generatingInstance == null) return 0f; return generatingInstance.GeneratingProgressSmooth * _progressMul; } }
        private bool willInstantiateInCoroutine = false;
        private float _progressMul { get { if (instantiationProgress != null) return 1f; if (willInstantiateInCoroutine) return 0.5f; else return 1f; } }
        private float? instantiationProgress = null;

        /// <summary> Generated Painters or Flexible Painters </summary>
        public List<PGGGeneratorRoot> GeneratedGenerators { get { return generatedGenerators; } }
        [SerializeField, HideInInspector] private List<PGGGeneratorRoot> generatedGenerators = new List<PGGGeneratorRoot>();

        public PlanGenerationPrint GeneratedPreview { get; private set; } = null;
        [SerializeField][HideInInspector] private List<GridPainter> generatedPainters = new List<GridPainter>();

        public List<GridPainter> GeneratedPainters { get { return generatedPainters; } }
        [SerializeField][HideInInspector] private List<FlexibleGenerator> generatedFlexiblePainters = new List<FlexibleGenerator>();
        public List<FlexibleGenerator> GeneratedFlexiblePainters { get { return generatedFlexiblePainters; } }

        BuildPlannerPreset generatingInstance = null;



        public void ClearGenerated()
        {
            if (generatingInstance) generatingInstance.ClearGeneration();

            for (int i = _generated.Count - 1; i >= 0; i--)
            {
                if (_generated[i] == null) continue;
                FGenerators.DestroyObject(_generated[i]);
            }

            if (generatedGenerators == null) generatedGenerators = new List<PGGGeneratorRoot>();

            _generated.Clear();
            GeneratedPreview = null;
            GeneratedGenerators.Clear();
            GeneratedPainters.Clear();
            GeneratedFlexiblePainters.Clear();
            latestGeneratedPreviewSeed = null;
        }




        public void GeneratePreview()
        {
            latestGeneratedPreviewSeed = null;
            generatingSet = EGenerating.JustPreview;

            if (GeneratedPreview != null) ClearGenerated();

            IsGenerating = true;

            if (RandomSeed) Seed = Random.Range(-9999, 9999);

            RefreshVariablesReferences();
            generatingInstance = BuildPlannerPreset.DeepCopy();
            generatingInstance.AsyncGenerating = Async;

            for (int i = 0; i < generatingInstance.BuildVariables.Count; i++)
            {
                generatingInstance.BuildVariables[i].SetValue(_plannerPrepare.PlannerVariablesOverrides[i].GetValue());
            }

            AdjustDuplicatesCounts();

            for (int p = 0; p < generatingInstance.BasePlanners.Count; p++) AdjustTargetDuplicatesCount(p);


            for (int p = 0; p < generatingInstance.BasePlanners.Count; p++)
            {
                var plannerInst = generatingInstance.BasePlanners[p];

                if (plannerInst.FieldType == FieldPlanner.EFieldType.InternalField) continue;

                var tgtCompos = _plannerPrepare.FieldSetupCompositions[p];

                plannerInst.DefaultFieldSetup = tgtCompos.Setup;

                plannerInst.CheckerScale = tgtCompos.GetCellSize();
                plannerInst.UseCheckerScale = true;
                plannerInst.DisableWholePlanner = !tgtCompos.OverrideEnabled;
                plannerInst.ShapeGenerator = tgtCompos.InitShapes[0];

                // Get override shape if enabled
                var newShape = GetShapeFor(plannerInst);
                if (newShape != null) plannerInst.ShapeGenerator = newShape;

                plannerInst.Instances = tgtCompos.Instances;

                for (int v = 0; v < plannerInst.Variables.Count; v++)
                {
                    plannerInst.Variables[v].SetValue(tgtCompos.PlannerVariablesOverrides[v]);
                }

            }



            var manager = generatingInstance.RunProceduresAndGeneratePrint(Seed);

            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                if (_plannerPrepare.UseDuplicatesOverrides[i] == false) continue;
                var overr = _plannerPrepare.DuplicatesOverrides[i];

                // Duplicate instances are generated during planner execution
                // so we can get reference to them through the event
                manager.GetAsyncInstances[i].OnDuplicatesGenerated = (FieldPlanner p) =>
                {
                    var dups = p.GetDuplicatesPlannersList();

                    if (dups != null)
                    {
                        for (int d = 0; d < dups.Count; d++)
                        {
                            #region Supporting duplicates overrides compositions

                            var overrComp = overr.DuplicatesCompositions[d];

                            if (overrComp.OverrideCellSize)
                            {
                                var dupPlan = dups[d];
                                dupPlan.CheckerScale = overrComp.GetCellSize();
                                //UnityEngine.Debug.Log("set scale on " + p + " to " + overrComp.OverridingCellSize);
                            }

                            var shapeGen = GetShapeFor(dups[d]);
                            if (shapeGen != null) dups[d].ShapeGenerator = shapeGen;

                            #endregion
                        }
                    }
                };
            }


            #region Playmode Generating Progress Invoke

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {

                InvokeRepeating("UpdateGeneratingProgress", 0.001f, 0.01f);
            }

            #endregion

        }


        PlannerDuplicatesSupport GetSupportFor(FieldPlanner planner)
        {
            if (planner.IndexOnPreset >= _plannerPrepare.DuplicatesOverrides.Count) return null;
            return _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset];
        }


        ShapeGeneratorBase GetShapeFor(FieldPlanner planner)
        {
            if (planner.IndexOnPreset < 0 || planner.IndexOnPreset >= _plannerPrepare.FieldSetupCompositions.Count)
            {
                return null;
            }

            if (_plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset].InitShapes == null || _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset].InitShapes.Count == 0)
            {
                _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset].RefreshPlannerShapesSupport(planner);
            }

            ShapeGeneratorBase deflt = _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset].InitShapes[0];

            if (planner.IndexOfDuplicate < 0 || planner.IndexOfDuplicate >= _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesShapes.Count)
            {
                return deflt;
            }

            ShapeGeneratorBase gen = _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesShapes[planner.IndexOfDuplicate];
            if (gen == null)
            {
                return deflt;
            }

            return gen;
        }


        FieldSetupComposition GetCompositionFor(FieldPlanner planner)
        {
            if (planner.IndexOnPreset >= _plannerPrepare.DuplicatesOverrides.Count) return null;
            FieldSetupComposition deflt = _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset];

            if (planner.IndexOfDuplicate < 0 || planner.IndexOfDuplicate >= _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions.Count) return deflt;

            FieldSetupComposition gen = _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions[planner.IndexOfDuplicate];
            if (gen == null) return deflt;
            return gen;
        }




        #region Generating Field Setups

        enum EGenerating { JustPreview, Base, GridPainters, FlexiblePainters }
        private EGenerating generatingSet = EGenerating.Base;
        private int? latestGeneratedPreviewSeed = null;
        public void Generate()
        {
            ClearGenerated();
            GeneratePreview();

            if (FlexibleGen == false)
                generatingSet = EGenerating.GridPainters;
            else
            {
                FlexiblePaintersGeneratorsDone = false;
                generatingSet = EGenerating.FlexiblePainters;
            }

            willInstantiateInCoroutine = false;
            if (generatingSet == EGenerating.FlexiblePainters) willInstantiateInCoroutine = true;
        }

        /// <summary>
        /// Mainly for refreshing variable value with scene transform Vector3 position feature
        /// </summary>
        public void RefreshVariablesReferences()
        {
            for (int i = 0; i < _plannerPrepare.PlannerVariablesOverrides.Count; i++)
            {
                Transform trs = null;
                if (_plannerPrepare.PlannerVariablesOverrides[i].allowTransformFollow)
                    if (_plannerPrepare.PlannerVariablesOverrides[i].additionalHelperRef)
                    {
                        trs = _plannerPrepare.PlannerVariablesOverrides[i].additionalHelperRef as Transform;
                        if (trs)
                        {
                            _plannerPrepare.PlannerVariablesOverrides[i].SetValue(transform.InverseTransformPoint(trs.position));
                        }
                    }
            }

            for (int p = 0; p < _plannerPrepare.FieldSetupCompositions.Count; p++)
            {
                var selected = _plannerPrepare.FieldSetupCompositions[p];

                for (int i = 0; i < selected.PlannerVariablesOverrides.Count; i++)
                {
                    Transform trs = null;
                    if (selected.PlannerVariablesOverrides[i].allowTransformFollow)
                        if (selected.PlannerVariablesOverrides[i].additionalHelperRef)
                        {
                            trs = selected.PlannerVariablesOverrides[i].additionalHelperRef as Transform;
                            if (trs)
                            {
                                selected.PlannerVariablesOverrides[i].SetValue(transform.InverseTransformPoint(trs.position));
                            }
                        }
                }
            }
        }

        public void SwitchFromPreviewGen()
        {
            if (generatingSet == EGenerating.JustPreview) generatingSet = EGenerating.GridPainters;
        }

        public void GenerateGridPainters()
        {

            if (latestGeneratedPreviewSeed == null || latestGeneratedPreviewSeed.Value != Seed)
            {
                GeneratePreview();

                if (FlexibleGen == false)
                    generatingSet = EGenerating.GridPainters;
                else
                    generatingSet = EGenerating.FlexiblePainters;
            }
            else
            {
                RunGeneratePainters();
            }
        }


        private void RunGeneratePainters()
        {
            if (generatingSet == EGenerating.GridPainters)
                ConvertGeneratedSchemeToGridPainters();
            else if (generatingSet == EGenerating.FlexiblePainters)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    StartCoroutine(IConvertGeneratedSchemeToFlexiblePainters());
                else
                    ConvertGeneratedSchemeToFlexiblePainters();
#else
                        StartCoroutine(IConvertGeneratedSchemeToFlexiblePainters());
#endif
            }
        }

        public void GenerateFieldSetupsOnGeneratedScheme()
        {
            seedIteration = 0;
            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                var planner = generatingInstance.BasePlanners[i];

                GenerateWithPlanner(planner);

                var duplicates = planner.GetDuplicatesPlannersList();

                if (duplicates != null)
                    for (int d = 0; d < duplicates.Count; d++)
                    {
                        GenerateWithPlanner(duplicates[d]);
                    }
            }
        }

        void GenerateWithPlanner(FieldPlanner planner)
        {

            seedIteration += 1;
        }

        int seedIteration;
        public void ConvertGeneratedSchemeToGridPainters()
        {
            seedIteration = 0;
            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                var planner = generatingInstance.BasePlanners[i];
                if (planner.DontGenerateIt) continue;

                GenerateGridPainterWithPlanner(planner);

                var duplicates = planner.GetDuplicatesPlannersList();

                if (duplicates != null)
                    for (int d = 0; d < duplicates.Count; d++)
                    {
                        if (duplicates[d].DontGenerateIt) continue;
                        GenerateGridPainterWithPlanner(duplicates[d]);
                    }
            }
        }

        public IEnumerator IConvertGeneratedSchemeToFlexiblePainters()
        {
            willInstantiateInCoroutine = true;
            instantiationProgress = 0.5f;

            float instatiationTotal = 0f;

            #region Compute count

            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                var planner = generatingInstance.BasePlanners[i];
                if (planner.DontGenerateIt) continue;
                instatiationTotal += 1f;

                var duplicates = planner.GetDuplicatesPlannersList();
                if (duplicates != null)
                    for (int d = 0; d < duplicates.Count; d++)
                    {
                        if (duplicates[d].DontGenerateIt) continue;
                        instatiationTotal += 1f;

                    }
            }

            if (instatiationTotal == 0f) instatiationTotal = 1f;

            #endregion

            float done = 0f;

            seedIteration = 0;
            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                var planner = generatingInstance.BasePlanners[i];
                if (planner.DontGenerateIt) continue;

                FlexibleGenerator painter = GenerateFlexiblePainterWithPlanner(planner);

                if (painter != null)
                {
                    while (painter.FinishedGenerating == false)
                    {
                        yield return null;
                        instantiationProgress = 0.5f + 0.5f * ((done + painter.GeneratingProgress) / instatiationTotal);
                        generatingInstance.OverrideProgressDisplay(painter.GeneratingProgress);
                    }
                }

                done += 1f;

                var duplicates = planner.GetDuplicatesPlannersList();

                if (duplicates != null)
                    for (int d = 0; d < duplicates.Count; d++)
                    {
                        if (duplicates[d].DontGenerateIt) continue;

                        painter = GenerateFlexiblePainterWithPlanner(duplicates[d]);

                        if (painter != null)
                        {
                            while (painter.FinishedGenerating == false)
                            {
                                yield return null;
                                instantiationProgress = 0.5f + 0.5f * ((done + painter.GeneratingProgress) / instatiationTotal);
                                generatingInstance.OverrideProgressDisplay(painter.GeneratingProgress);
                            }
                        }

                        done += 1f;
                    }
            }

            instantiationProgress = 1f;
        }

        public void ConvertGeneratedSchemeToFlexiblePainters()
        {
            seedIteration = 0;
            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                var planner = generatingInstance.BasePlanners[i];
                if (planner.DontGenerateIt) continue;

                FlexibleGenerator painter = GenerateFlexiblePainterWithPlanner(planner);

                var duplicates = planner.GetDuplicatesPlannersList();

                if (duplicates != null)
                    for (int d = 0; d < duplicates.Count; d++)
                    {
                        if (duplicates[d].DontGenerateIt) continue;

                        painter = GenerateFlexiblePainterWithPlanner(duplicates[d]);
                        if (painter != null) generatingInstance.OverrideProgressDisplay(painter.GeneratingProgress);
                    }
            }
        }

        void GenerateGridPainterWithPlanner(FieldPlanner planner)
        {
            string name = planner.name;
            name = name.Replace("(Clone)", "");
            if (planner.IndexOfDuplicate >= 0) name += "[" + planner.IndexOfDuplicate + "]";

            GameObject painterObj = new GameObject(name);

            GridPainter painter = painterObj.AddComponent<GridPainter>();

            painter.RandomSeed = false;
            painter.Seed = Seed + seedIteration;
            painter.FieldPreset = planner.DefaultFieldSetup;
            painter.GenerateOnStart = false;

            painter.grid = planner.LatestResult.Checker.Grid;
            painter.CellsInstructions = FGenerators.CopyList(planner.LatestResult.CellsInstructions);

            painter.SaveCells();
            GeneratedPainters.Add(painter);
            generatedGenerators.Add(painter);

            FieldSetupComposition baseComposition = _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset];

            if (baseComposition.GenType == EPGGGenType.Modificator || baseComposition.GenType == EPGGGenType.ModPack)
            {
                if (baseComposition.UseComposition == false || baseComposition.Prepared == false)
                { baseComposition = baseComposition.Copy(); if (baseComposition.GenType == EPGGGenType.Modificator) baseComposition.RefreshModSetup(); else baseComposition.RefreshModPackSetup(); }
                baseComposition.UseComposition = true;
                baseComposition.Prepared = true;
                //UnityEngine.Debug.Log("prepared true");
            }

            if (baseComposition.UseComposition && baseComposition.Prepared)
            {
                painter.Composition = baseComposition;
            }

            if (planner.IndexOnPreset < _plannerPrepare.UseDuplicatesOverrides.Count)
                if (_plannerPrepare.UseDuplicatesOverrides[planner.IndexOnPreset])
                {
                    if (planner.IsDuplicate)
                        if (planner.IndexOfDuplicate < _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions.Count)
                        {
                            var compo = _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions[planner.IndexOfDuplicate];

                            if (compo.UseComposition && compo.Prepared)
                            {
                                if (compo.Setup != null) painter.FieldPreset = compo.Setup;
                                painter.Composition = compo;
                            }
                        }
                }

            painterObj.transform.SetParent(transform, true);
            painterObj.transform.position = transform.TransformPoint(planner.LatestResult.Checker.RootPosition);
            painterObj.transform.rotation = transform.rotation * planner.LatestResult.Checker.RootRotation;

            _generated.Add(painterObj);
            seedIteration += 1;

            /*var genSetup = */painter.GetTargetGeneratingSetup();

            if (planner.OnGeneratingEvents != null)
            {
                for (int g = 0; g < planner.OnGeneratingEvents.Count; g++)
                {
                    planner.OnGeneratingEvents[g].Invoke(painter);
                }
            }

            painter.GenerateObjects();
            painterObj.transform.localScale = Vector3.one;

            AddPlannerReference(planner, painter);
        }

        FlexibleGenerator GenerateFlexiblePainterWithPlanner(FieldPlanner planner)
        {
            string name = planner.name;
            name = name.Replace("(Clone)", "");
            if (planner.IndexOfDuplicate >= 0) name += "[" + planner.IndexOfDuplicate + "]";

            GameObject painterObj = new GameObject(name);

            FlexibleGenerator painter = painterObj.AddComponent<FlexibleGenerator>();

            painter.CodedUsage = true;
            painter.RandomSeed = false;
            painter.GenerateOnStart = false;
            painter.Seed = Seed + seedIteration;
            painter.DataSetup.FieldPreset = planner.DefaultFieldSetup;
            //painter.AsyncComputing = true;
            //painter.InstantiationMaxSecondsDelay = 0.0001f;
            painter.TestGridSize = Vector2Int.zero;

            GeneratedFlexiblePainters.Add(painter);
            GeneratedGenerators.Add(painter);

            FieldSetupComposition baseComposition = _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset];

            if (baseComposition.GenType == EPGGGenType.Modificator || baseComposition.GenType == EPGGGenType.ModPack)
            {
                if (baseComposition.UseComposition == false || baseComposition.Prepared == false)
                { baseComposition = baseComposition.Copy(); if (baseComposition.GenType == EPGGGenType.Modificator) baseComposition.RefreshModSetup(); else baseComposition.RefreshModPackSetup(); }
                baseComposition.UseComposition = true; baseComposition.Prepared = true;
                //UnityEngine.Debug.Log("prepared true");
            }

            if (baseComposition.UseComposition && baseComposition.Prepared)
            {
                painter.Composition = baseComposition;
            }

            if (painter.Composition != null)
            {
                if (painter.Composition.Prepared && painter.Composition.OverrideEnabled)
                    painter.DataSetup.FieldPreset = painter.Composition.GetSetup;
            }

            if (planner.IndexOnPreset < _plannerPrepare.UseDuplicatesOverrides.Count)
                if (_plannerPrepare.UseDuplicatesOverrides[planner.IndexOnPreset])
                {
                    if (planner.IsDuplicate)
                        if (planner.IndexOfDuplicate < _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions.Count)
                        {
                            var compo = _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions[planner.IndexOfDuplicate];

                            if (compo.UseComposition && compo.Prepared)
                            {
                                if (compo.Setup != null) painter.DataSetup.FieldPreset = compo.Setup;
                                painter.Composition = compo;
                            }
                        }
                }


            painter.CheckIfInitialized();
            painter.DataSetup.RefreshReferences(painter);
            painter.Cells.Initialize(painter.DataSetup);

            painter.PrepareWithoutGridChanges();
            painter.Cells.SetWithGrid(planner.LatestResult.Checker.Grid);
            painter.Preparation.CellInstructions = FlexiblePainter.CellInstructionsToSpawnInstructions(FGenerators.CopyList(planner.LatestResult.CellsInstructions), painter.DataSetup.FieldPreset, painter.transform);

            painterObj.transform.SetParent(transform, true);
            painterObj.transform.position = transform.TransformPoint(planner.LatestResult.Checker.RootPosition);
            painterObj.transform.rotation = transform.rotation * planner.LatestResult.Checker.RootRotation;


            _generated.Add(painterObj);
            seedIteration += 1;

            painter.GenerateObjects();
            painterObj.transform.localScale = Vector3.one;

            AddPlannerReference(planner, painter);

            return painter;
        }


        void AddPlannerReference(FieldPlanner planner, PGGGeneratorRoot root)
        {
            BuildPlannerReference pRef = root.gameObject.AddComponent<BuildPlannerReference>();
            pRef.ParentExecutor = this;
            pRef.Generator = root;
            pRef.Planner = planner;
            pRef.PlannerName = planner.name.Replace("(Clone)", "");
            pRef.BuildPlannerIndex = planner.IndexOnPreset;
            pRef.BuildPlannerInstanceID = planner.InstanceIndex;
            pRef.GridSpaceBounds = PGG_MinimapUtilities.ComputeGridCellSpaceBounds(root);
            pRef.GridSpaceBounds.center += new Vector3(0f, 0.5f, 0f);
            pRef.GridSpaceBounds = PGG_MinimapUtilities.ScaleBoundsWithSetup(pRef.GridSpaceBounds, root.PGG_Setup);
        }


        #endregion



        #region Generating Progress

        public void UpdateGeneratingProgress()
        {
            if (generatingInstance == null)
                IsGenerating = false;
            else
                generatingInstance.UpdateGenerating();


            if (IsGenerating)
            {
                GeneratedPreview = generatingInstance.LatestGenerated;

                #region Repaint
#if UNITY_EDITOR
                SceneView.RepaintAll();
#endif
                #endregion

                if (generatingInstance.IsGeneratingDone) IsGenerating = false;

            }


            #region Finishing Generating

            if (IsGenerating == false)
            {

                #region Repaint
#if UNITY_EDITOR
                SceneView.RepaintAll();
#endif
                #endregion

                CancelInvoke("UpdateGeneratingProgress");

                latestGeneratedPreviewSeed = Seed;

                if (generatingSet == EGenerating.Base)
                    GenerateFieldSetupsOnGeneratedScheme();
                else
                {
                    RunGeneratePainters();
                }

                if (!FlexibleGen)
                {
                    if (RunAfterGenerating != null) RunAfterGenerating.Invoke();
                }
                else
                {
                    StartCoroutine(IEWaitForFlexAfterGenerating());
                }
            }

            #endregion

        }

        public bool FlexiblePaintersGeneratorsDone { get; private set; }
        IEnumerator IEWaitForFlexAfterGenerating()
        {
            while (FlexiblePaintersGeneratorsDone == false)
            {
                bool anyNotDone = false;

                for (int f = 0; f < GeneratedFlexiblePainters.Count; f++)
                {
                    var flex = GeneratedFlexiblePainters[f];
                    if (flex == null) continue;
                    if (flex.FinishedGenerating == false) { anyNotDone = true; break; }
                }

                if (anyNotDone) 
                    yield return null;
                else
                    FlexiblePaintersGeneratorsDone = true;
            }

            if (RunAfterGenerating != null) RunAfterGenerating.Invoke();
        }

        #endregion


    }

}
