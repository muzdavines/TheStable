using System.Collections.Generic;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Planning.GeneratingLogics;
using System;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating
{
    public enum EPGGGenType
    {
        None, FieldSetup, Modificator, ModPack
    }

    /// <summary>
    /// Class for setting up FieldSetup variables, injections, modifying default prefabs etc.
    /// </summary>
    [System.Serializable]
    public partial class FieldSetupComposition
    {
        public bool UseComposition = false;
        public UnityEngine.Object Owner;
        public bool OverrideEnabled = true;
        public bool Prepared = false;

        public FieldSetup Setup;


        public EPGGGenType GenType = EPGGGenType.FieldSetup;

        public ModificatorsPack JustModPack;
        public FieldModification JustMod;
        public BuildPlannerPreset OtherPlanner;

        /// <summary> Parent field setup of mod pack or field modificator if not using field setup </summary>
        public FieldSetup subSetup = null;

        public FieldSetup GetSetup
        {
            get
            {
                if (GenType == EPGGGenType.FieldSetup) return Setup;
                else
                {
                    if ( subSetup == null)
                    {
                        subSetup = FieldSetup.CreateInstance<FieldSetup>();
                        subSetup.name = "Composition";
                        if ( OverrideCellSize) subSetup.NonUniformCellSize = OverridingCellSize;
                    }

                    return subSetup;
                }
            }
        }

        // Setup Overriding Related
        public List<FieldVariable> FieldSetupVariablesOverrides;
        public List<PackOverrideHelper> FieldPackagesOverrides;
        public List<ModOverrideHelper> UtilityModsOverrides;

        public bool OverrideCellSize = false;
        public Vector3 OverridingCellSize = Vector3.one;

        /// <summary>
        /// Returns true if using FieldSetup Composition and field setup reference exist
        /// same for field modificator and mod pack
        /// </summary>
        public bool IsSettedUp
        {
            get
            {
                if (GenType == EPGGGenType.None) return true;
                if (GenType == EPGGGenType.FieldSetup) { if (Setup != null) return true; }
                else if (GenType == EPGGGenType.Modificator) { if (JustMod != null) return true; }
                else if (GenType == EPGGGenType.ModPack) { if (JustModPack != null) return true; }
                return false;
            }
        }

        #region Layout Planners Related

        /// <summary> Can be provided as helper for build planners </summary>
        public FieldPlanner ParentFieldPlanner;
        /// <summary> Can be provided as helper for build planners </summary>
        public List<FieldVariable> PlannerVariablesOverrides;
        /// <summary> Can be provided as helper for build planners </summary>
        public List<ShapeGeneratorBase> InitShapes;

        /// <summary> Returning instances-1 </summary>
        public int Duplicates { get { return Instances - 1; } }
        /// <summary> Overriding Instances count if exposed </summary>
        public int Instances = 1;

        public void PrepareWithCurrentlyChoosed(UnityEngine.Object owner, FieldPlanner fieldPlanner)
        {
            if (GenType == EPGGGenType.FieldSetup) PrepareWith(owner, fieldPlanner, Setup);
            else if (GenType == EPGGGenType.Modificator) PrepareWith(owner, fieldPlanner, JustMod);
            else if (GenType == EPGGGenType.ModPack) PrepareWith(owner, fieldPlanner, JustModPack);
        }

        public void PrepareWith(UnityEngine.Object owner, FieldPlanner fieldPlanner, FieldSetup setup)
        {
            ResetSetup(fieldPlanner);
            RefreshPlannerShapesSupport(fieldPlanner);
            GenType = EPGGGenType.FieldSetup;
            Owner = owner;
            Setup = setup;
        }

        public void PrepareWith(UnityEngine.Object owner, FieldPlanner fieldPlanner, FieldModification setup)
        {
            ResetSetup(fieldPlanner);

            GenType = EPGGGenType.Modificator;
            Owner = owner;
            JustMod = setup;

            if (JustMod)
            {
                subSetup = JustMod.TryGetParentSetup();

                if (subSetup)
                {
                    OverrideCellSize = true;
                    OverridingCellSize = JustMod.ParentPreset.GetCellUnitSize();
                    AdjustFieldSetupVariables();
                }

                if (UtilityModsOverrides == null) UtilityModsOverrides = new List<ModOverrideHelper>();
                if (UtilityModsOverrides.Count == 0) UtilityModsOverrides.Add(new ModOverrideHelper());
                UtilityModsOverrides[0].UpdateModsCountWith(JustMod);

            }
        }

        public void PrepareWith(UnityEngine.Object owner, FieldPlanner fieldPlanner, ModificatorsPack setup)
        {
            ResetSetup(fieldPlanner);
            GenType = EPGGGenType.ModPack;
            Owner = owner;
            JustModPack = setup;

            if (JustModPack)
            {
                subSetup = JustModPack.TryGetParentSetup();

                if (subSetup)
                {
                    OverrideCellSize = true;
                    OverridingCellSize = subSetup.GetCellUnitSize();
                }

                RefreshWith(JustModPack);
            }
        }

        void ResetSetup(FieldPlanner fieldPlanner)
        {
            if (fieldPlanner != null) GenType = EPGGGenType.None;

            OverrideEnabled = true;
            UseComposition = true;
            Owner = null;
            Setup = null; JustMod = null; JustModPack = null; subSetup = null;
            ParentFieldPlanner = fieldPlanner;
            Prepared = true;
            //UnityEngine.Debug.Log("prepared true");

            if (fieldPlanner) Instances = fieldPlanner.Instances;

        }

        #endregion

        public void RefreshPlannerShapesSupport(FieldPlanner fieldPlanner)
        {
            if (fieldPlanner)
            {
                if (ParentFieldPlanner != fieldPlanner) Instances = fieldPlanner.Instances;
                ParentFieldPlanner = fieldPlanner;

                if (PlannerVariablesOverrides == null) PlannerVariablesOverrides = new List<FieldVariable>();
                if (InitShapes == null)
                {
                    InitShapes = new List<ShapeGeneratorBase>();
                }

                ValidatePlanner();
            }
        }


        public void RefreshWith(UnityEngine.Object owner, FieldPlanner fieldPlanner, FieldSetup setup = null, bool forceReinitialize = false)
        {
            if (owner != null) Owner = owner;

            if (setup == null)
            {
                if (fieldPlanner)
                    if (fieldPlanner.DefaultFieldSetup != null)
                        if (Setup == null)
                            Setup = fieldPlanner.DefaultFieldSetup;

                if (Setup == null)
                    Setup = setup;
            }
            else
            {
                Setup = setup;
            }


            RefreshPlannerShapesSupport(fieldPlanner);


            #region Field Packages Overriding Related

            if (setup)
            {
                RefreshVariablesWith(setup);
                if (FieldPackagesOverrides == null) FieldPackagesOverrides = new List<PackOverrideHelper>();
                AdjustModPacksCount();

                if (UtilityModsOverrides == null) UtilityModsOverrides = new List<ModOverrideHelper>();
                AdjustUtilityModsCount();

                if (forceReinitialize) ReinitializePacks();
            }

            #endregion

            Prepared = true;
            //UnityEngine.Debug.Log("prepared true");
        }

        public void RefreshVariablesWith(FieldSetup fieldSetup)
        {
            if (FieldSetupVariablesOverrides == null) FieldSetupVariablesOverrides = new List<FieldVariable>();
            FieldVariable.UpdateVariablesWith(FieldSetupVariablesOverrides, fieldSetup.Variables);
        }

        public void RefreshModTo(FieldModification mod, ModOverrideHelper modOverr)
        {
            modOverr.ParentMod = mod;
            modOverr.UpdateModsCountWith(mod);
            ApplyOverridesOf(mod, modOverr);
        }

        public bool ValidateComposition(int selectedPack)
        {
            if (GenType == EPGGGenType.None) return false;
            if (GenType == EPGGGenType.Modificator) return JustMod != null;
            if (GenType == EPGGGenType.ModPack)
            {
                if (JustModPack == null) return false;

                if (FieldPackagesOverrides == null) FieldPackagesOverrides = new List<PackOverrideHelper>();
                if (FieldPackagesOverrides.Count == 0)
                {
                    FieldPackagesOverrides.Add(new PackOverrideHelper());
                }

                return JustModPack != null;
            }

            if (FieldSetupVariablesOverrides == null) return false;
            if (Setup == null) return false;
            if (Setup.Variables == null) return false;

            if (FieldSetupVariablesOverrides.Count != Setup.Variables.Count)
            {
                AdjustFieldSetupVariables();
            }

            if (FieldPackagesOverrides.Count != Setup.ModificatorPacks.Count)
            {
                AdjustModPacksCount();
            }

            if (UtilityModsOverrides.Count != Setup.UtilityModificators.Count)
            {
                AdjustUtilityModsCount();
            }

            for (int i = 0; i < FieldPackagesOverrides.Count; i++)
            {
                var packOverr = FieldPackagesOverrides[i];

                if (packOverr.PackVariablesOverrides.Count != Setup.ModificatorPacks[i].Variables.Count)
                    packOverr.AdjustFieldSetupVariables();
            }

#if UNITY_EDITOR
            if (lastCheckedIn == -1 || UnityEditor.EditorApplication.timeSinceStartup - lastCheckedIn > 2)
                if (selectedPack > -1)
                {
                    if (selectedPack >= FieldPackagesOverrides.Count) selectedPack = 0;

                    var tgtPack = FieldPackagesOverrides[selectedPack];
                    if (tgtPack != null)
                    {
                        ModificatorsPack parentPack = tgtPack.ParentPack;

                        if (parentPack != null)
                        {
                            if (tgtPack.PackModsOverrides.Count != parentPack.FieldModificators.Count)
                            {
                                PGGUtils.AdjustCount(tgtPack.PackModsOverrides, parentPack.FieldModificators.Count);
                            }
                        }
                    }

                }
#endif

            return true;
        }

        private static readonly double lastCheckedIn = -1;


        #region Just field planners related

        /// <summary>
        /// Checking if something has changed in the planners settings and updating it
        /// </summary>
        private void ValidatePlanner()
        {
            if (ParentFieldPlanner == null) return;

            if (InitShapes.Count == 0)
            {
                InitShapes.Add(GetShape(ParentFieldPlanner));
            }
            else
            {
                for (int s = 0; s < InitShapes.Count; s++)
                {
                    if (InitShapes[s] == null)
                    {
                        InitShapes[s] = GetShape(ParentFieldPlanner);
                    }
                    else
                    {
                        var shpe = ParentFieldPlanner.ShapeGenerator;
                        if (shpe != null)
                        {
                            if (InitShapes[s].GetType() != shpe.GetType())
                            {
                                InitShapes[s] = GetShape(ParentFieldPlanner);
                            }
                        }
                    }
                }
            }
        }

        public void ReloadShape()
        {
            InitShapes = new List<ShapeGeneratorBase>();
            ValidatePlanner();
        }

        ShapeGeneratorBase GetShape(FieldPlanner planner)
        {
            if (planner == null) return null;
            if (planner.ShapeGenerator == null) return null;
            return ScriptableObject.Instantiate(planner.ShapeGenerator);
        }

        public PackOverrideHelper GetOverridesFor(ModificatorsPack pack)
        {
            for (int i = 0; i < FieldPackagesOverrides.Count; i++)
            {
                if (FieldPackagesOverrides[i].ParentPack == pack) return FieldPackagesOverrides[i];
            }

            return null;
        }

        #endregion

#if UNITY_EDITOR

        public static void DrawCompositionGUI(UnityEngine.Object caller, FieldSetupComposition selected, bool doHorizontal, FieldPlanner planner = null)
        {
            Color preGuiC = GUI.color;
            Color preBg = Color.white;

            if (doHorizontal) EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxStyle);
            GUI.color = preGuiC;
            selected.OverrideEnabled = EditorGUILayout.Toggle(GUIContent.none, selected.OverrideEnabled, GUILayout.Width(18));


            DrawGenSelectorGUI(selected);
            //selected.Setup = (FieldSetup)EditorGUILayout.ObjectField(selected.Setup, typeof(FieldSetup), false);
            //if (selected.ParentFieldPlanner == null) if (selected.Prepared == false) if ( selected.OverrideEnabled) selected.PrepareWith(caller, planner, selected.Setup);
            GUI.backgroundColor = preBg;

            if (selected.Setup != null)
            {
                GUILayout.Space(3f);
                string name = "";
                if (!planner) name = selected.Setup.name; else name = planner.name;//.Substring(0, Mathf.Min(12, planner.name.Length));

                if (selected.Prepared == false)
                {
                    if (GUILayout.Button("Create Composition"))
                    {
                        FieldSetup setp = selected.Setup;
                        selected.ResetSetup(null);
                        selected.RefreshWith(caller, planner, setp);
                        EditorUtility.SetDirty(caller);
                        FieldCompositionWindow.Init(selected, name, caller);
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(0.35f, 1f, 0.35f, 1f);
                    if (GUILayout.Button("Open Composition"))
                    {
                        if (selected.Prepared == false)
                        {
                            if (selected.ParentFieldPlanner == null)
                            {
                                selected.PrepareWith(caller, planner, selected.Setup);
                            }
                        }

                        selected.RefreshWith(caller, planner, selected.Setup);
                        EditorUtility.SetDirty(caller);
                        FieldCompositionWindow.Init(selected, name, caller);
                    }
                    GUI.backgroundColor = preBg;
                }

                GUILayout.Space(3f);
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Info, "Click to display info about Field Composition"), EditorStyles.label, GUILayout.Width(16)))
                {
                    EditorUtility.DisplayDialog("What is FieldSetup Composition?", "FieldSetup composition gives you possibility to customize previously prepared FieldSetup in detailed way.\n\nYou can choose selectively to generate or to not generate choosed Modificators, replace prefabs to spawn, replace variables etc.\n\nWhen you change something in the FieldSetup you need to update composition to keep it up to date", "Ok");
                }
            }

            if (doHorizontal) EditorGUILayout.EndHorizontal();
        }

        public static void DrawCompositionGUI2(UnityEngine.Object caller, FieldSetupComposition selected, FieldPlanner planner = null)
        {
            if (selected.GenType == EPGGGenType.None) return;

            Color preBG = GUI.backgroundColor;

            EditorGUILayout.BeginHorizontal();

            if (selected.GenType == EPGGGenType.FieldSetup)
            {

                if (selected.Setup != null)
                {

                    GUILayout.Space(3f);
                    string name = "";
                    if (!planner) name = selected.Setup.name; else name = planner.name;//.Substring(0, Mathf.Min(10, planner.name.Length));

                    if (selected.Prepared == false)
                    {
                        if (GUILayout.Button("Create Composition"))
                        {
                            FieldSetup setp = selected.Setup;
                            selected.ResetSetup(null);
                            selected.RefreshWith(caller, planner, setp);
                            EditorUtility.SetDirty(caller);
                            FieldCompositionWindow.Init(selected, name, caller);
                        }
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(0.35f, 1f, 0.35f, 1f);
                        if (GUILayout.Button("Open Composition"))
                        {
                            //if (selected.Prepared == false)
                            //{
                            //    if (selected.ParentFieldPlanner == null) selected.PrepareWith(caller, planner, selected.Setup);
                            //}

                            selected.RefreshWith(caller, planner, selected.Setup);
                            EditorUtility.SetDirty(caller);
                            FieldCompositionWindow.Init(selected, name, caller);
                        }
                        GUI.backgroundColor = preBG;
                    }

                    GUILayout.Space(3f);
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Info, "Click to display info about Field Composition"), EditorStyles.label, GUILayout.Width(16)))
                    {
                        EditorUtility.DisplayDialog("What is FieldSetup Composition?", "FieldSetup composition gives you possibility to customize previously prepared FieldSetup in detailed way.\n\nYou can choose selectively to generate or to not generate choosed Modificators, replace prefabs to spawn, replace variables etc.\n\nWhen you change something in the FieldSetup you need to update composition to keep it up to date", "Ok");
                    }

                }
                else
                {
                    EditorGUILayout.HelpBox("Null Field Setup", MessageType.None);
                }

            }
            else if (selected.GenType == EPGGGenType.ModPack)
            {

                if (selected.JustModPack != null)
                {

                    GUILayout.Space(3f);
                    string name = "";
                    if (!planner) name = selected.JustModPack.name; else name = planner.name;//.Substring(0, Mathf.Min(10, planner.name.Length));

                    if (selected.Prepared == false)
                    {
                        if (GUILayout.Button("Create Composition"))
                        {
                            //selected.ResetSetup(null);
                            selected.PrepareWith(caller, planner, selected.JustModPack);
                            EditorUtility.SetDirty(caller);
                            FieldCompositionWindow.Init(selected, name, caller);
                        }
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(0.35f, 1f, 0.35f, 1f);
                        if (GUILayout.Button("Open Composition"))
                        {
                            if (selected.Prepared == false)
                            {
                                if (selected.ParentFieldPlanner == null) selected.PrepareWith(caller, planner, selected.Setup);
                            }

                            selected.RefreshWith(caller, planner, selected.Setup);
                            EditorUtility.SetDirty(caller);
                            FieldCompositionWindow.Init(selected, name, caller);
                        }
                        GUI.backgroundColor = preBG;
                    }

                    GUILayout.Space(3f);
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Info, "Click to display info about Field Composition"), EditorStyles.label, GUILayout.Width(16)))
                    {
                        EditorUtility.DisplayDialog("What is FieldSetup Composition?", "FieldSetup composition gives you possibility to customize previously prepared FieldSetup in detailed way.\n\nYou can choose selectively to generate or to not generate choosed Modificators, replace prefabs to spawn, replace variables etc.\n\nWhen you change something in the FieldSetup you need to update composition to keep it up to date", "Ok");
                    }

                }
                else
                {
                    EditorGUILayout.HelpBox("Null Modificator Pack", MessageType.None);
                }

            }
            else if (selected.GenType == EPGGGenType.Modificator)
            {

                if (selected.JustMod != null)
                {

                    GUILayout.Space(3f);
                    string name = "";
                    if (!planner) name = selected.JustMod.name; else name = planner.name;//.Substring(0, Mathf.Min(10, planner.name.Length));

                    if (selected.Prepared == false)
                    {
                        if (GUILayout.Button("Create Composition"))
                        {
                            //selected.ResetSetup(null);
                            selected.PrepareWith(caller, planner, selected.JustMod);
                            EditorUtility.SetDirty(caller);
                            FieldCompositionWindow.Init(selected, name, caller);
                        }
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(0.35f, 1f, 0.35f, 1f);
                        if (GUILayout.Button("Open Composition"))
                        {
                            if (selected.Prepared == false)
                            {
                                if (selected.ParentFieldPlanner == null) selected.PrepareWith(caller, planner, selected.Setup);
                            }

                            selected.RefreshWith(caller, planner, selected.Setup);
                            EditorUtility.SetDirty(caller);
                            FieldCompositionWindow.Init(selected, name, caller);
                        }
                        GUI.backgroundColor = preBG;
                    }

                    GUILayout.Space(3f);
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Info, "Click to display info about Field Composition"), EditorStyles.label, GUILayout.Width(16)))
                    {
                        EditorUtility.DisplayDialog("What is FieldSetup Composition?", "FieldSetup composition gives you possibility to customize previously prepared FieldSetup in detailed way.\n\nYou can choose selectively to generate or to not generate choosed Modificators, replace prefabs to spawn, replace variables etc.\n\nWhen you change something in the FieldSetup you need to update composition to keep it up to date", "Ok");
                    }

                }
                else
                {
                    EditorGUILayout.HelpBox("Null Field Modification", MessageType.None);
                }

            }

            EditorGUILayout.EndHorizontal();
        }

        public static void DrawGenSelectorGUI(FieldSetupComposition composition)
        {
            if (composition.GenType == EPGGGenType.FieldSetup) EditorGUILayout.ObjectField(composition.Setup, typeof(FieldSetup), true);
            else if (composition.GenType == EPGGGenType.ModPack) EditorGUILayout.ObjectField(composition.JustModPack, typeof(ModificatorsPack), true);
            else if (composition.GenType == EPGGGenType.Modificator) EditorGUILayout.ObjectField(composition.JustMod, typeof(FieldModification), true);
        }

        internal void Clear()
        {
            UseComposition = false;
            OverrideEnabled = true;
            Prepared = false;

            subSetup = null;

            FieldSetupVariablesOverrides = new List<FieldVariable>();
            FieldPackagesOverrides = new List<PackOverrideHelper>();
            UtilityModsOverrides = new List<ModOverrideHelper>();

            OverrideCellSize = false;
        }

#endif

    }

}