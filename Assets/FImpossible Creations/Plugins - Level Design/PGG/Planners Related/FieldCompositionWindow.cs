#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;
using System.Collections.Generic;

namespace FIMSpace.Generating
{
    public partial class FieldCompositionWindow : EditorWindow
    {
        public FieldSetupComposition composition;
        Vector2 mainScroll = Vector2.zero;
        Vector2 pfScroll = Vector2.zero;
        Vector2 pfUtilScroll = Vector2.zero;
        bool drawPrefabsOverrides = true;
        bool drawUtilitiesOverrides = false;

        public static List<FieldCompositionWindow> opened = new List<FieldCompositionWindow>();

        private void Awake()
        {
            PGGUtils.CheckForNulls(opened);
            if (opened.Contains(this) == false) opened.Add(this);
        }

        private void OnDestroy()
        {
            if (opened.Contains(this)) opened.Remove(this);
        }


        public static void Init(FieldSetupComposition composition, string targetName = "", Object toDirty = null)
        {
            for (int i = 0; i < opened.Count; i++)
            {
                if (composition == opened[i].composition)
                {
                    opened[i].Focus();
                    return;
                }
            }

            FieldCompositionWindow window = GetWindow<FieldCompositionWindow>(true);
            if (window == null)
            {
#if UNITY_2019_4_OR_NEWER
                window = CreateWindow<FieldCompositionWindow>();
#else
                return;
#endif
            }

            string title = "Composition";
            if (string.IsNullOrEmpty(targetName))
            {
                if (composition.Setup != null) title = composition.Setup.name;
            }
            else
                title = targetName;

            window.titleContent = new GUIContent(title, Resources.Load<Texture>("SPR_Compos"));
            window.composition = composition;
            window.Show();
        }


        void OnGUI()
        {
            //bool preWideMode = EditorGUIUtility.wideMode;
            //bool preHierarchyMode = EditorGUIUtility.hierarchyMode;
            //EditorGUIUtility.hierarchyMode = true;
            //EditorGUIUtility.wideMode = true;

            Color preBGCol = GUI.backgroundColor;
            Color preCol = GUI.color;
            bool preEn = GUI.enabled;

            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);
            EditorGUI.BeginChangeCheck();

            GUILayout.Space(9);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("  Customize ", EditorStyles.boldLabel, GUILayout.Width(81));

            GUI.enabled = false;

            FieldSetupComposition.DrawGenSelectorGUI(composition);
            //EditorGUILayout.ObjectField("Parent Planner:", composition.ParentFieldPlanner, typeof(FieldPlanner), true);
            GUILayout.Space(5);
            GUI.enabled = preEn;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5f);

            bool preWide = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
            GUILayout.BeginHorizontal();
            composition.OverrideCellSize = GUILayout.Toggle(composition.OverrideCellSize, GUIContent.none, GUILayout.Width(18));
            if (composition.OverrideCellSize == false)
            {
                GUI.enabled = false;
                if (composition.Setup) composition.OverridingCellSize = composition.Setup.GetCellUnitSize();
            }
            EditorGUIUtility.labelWidth = 120;
            composition.OverridingCellSize = EditorGUILayout.Vector3Field("Override Cell Size:", composition.OverridingCellSize);
            EditorGUIUtility.labelWidth = 0;
            if (composition.OverrideCellSize == false) GUI.enabled = true;
            GUILayout.EndHorizontal();
            EditorGUIUtility.wideMode = preWide;


            GUILayout.Space(6);

            FieldSetup setp = composition.GetSetup;
            ModificatorsPack pack = null;
            FieldModification mod = null;
            FieldSpawner spawner = null;

            if (!composition.IsSettedUp)
            {
                EditorGUILayout.LabelField("No Setup Reference in composition!", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                if (composition.GenType == EPGGGenType.FieldSetup)
                {
                    if (composition.Setup) pack = composition.Setup.RootPack;
                }
                else if (composition.GenType == EPGGGenType.Modificator) mod = composition.JustMod;
                else if (composition.GenType == EPGGGenType.ModPack) pack = composition.JustModPack;


                #region Selecting References

                if (composition.GenType != EPGGGenType.Modificator)
                {

                    if (pack)
                    {

                        if (pack.FieldModificators.Count == 0)
                        {
                            // Just root pack
                            EditorGUILayout.LabelField("Root Mods Pack", EditorStyles.centeredGreyMiniLabel);
                        }
                        else // Root pack and other packs
                        {

                            if (setp)
                            {

                                if (composition.GenType == EPGGGenType.FieldSetup)
                                {
                                    if (selectedPack < composition.Setup.ModificatorPacks.Count) pack = composition.Setup.ModificatorPacks[selectedPack];
                                    else selectedPack = 0;
                                }

                                if (pack.FieldModificators.Count == 0)
                                {
                                    EditorGUILayout.LabelField("No Modificators in Pack!", EditorStyles.centeredGreyMiniLabel);
                                }
                                else
                                {
                                    if (selectedMod < pack.FieldModificators.Count) mod = pack.FieldModificators[selectedMod];
                                    else selectedMod = 0;

                                    if (mod)
                                    {
                                        if (mod.Spawners.Count > 0)
                                        {
                                            if (selectedSpawner < mod.Spawners.Count) spawner = mod.Spawners[selectedSpawner];
                                            else selectedSpawner = 0;
                                        }
                                    }
                                }

                            }

                        }

                    }
                    else
                    {
                        EditorGUILayout.LabelField("Wrong Modificators Pack!", EditorStyles.centeredGreyMiniLabel);
                    }
                }

                #endregion


                bool validation = composition.ValidateComposition(selectedPack);

                if (validation)
                {
                    FieldSetup tgtSetup = composition.GetSetup;

                    if (tgtSetup && composition.FieldSetupVariablesOverrides != null && composition.FieldSetupVariablesOverrides.Count > 0)
                    {
                        GUILayout.Space(4f);

                        #region Field Setup Variables


                        if (tgtSetup.Variables.Count > 0)
                        {
                            string varTitle = "Override FieldSetup Variables";
                            if (composition.GenType != EPGGGenType.FieldSetup) varTitle = "Override Parent FieldSetup Variables";

                            FGUI_Inspector.FoldHeaderStart(ref drawSetupVars, varTitle, FGUI_Resources.BGInBoxStyle);

                            if (drawSetupVars)
                            {
                                GUILayout.Space(5f);

                                for (int i = 0; i < composition.FieldSetupVariablesOverrides.Count; i++)
                                {
                                    composition.FieldSetupVariablesOverrides[i].UpdateVariableWith(tgtSetup.Variables[i], true);
                                    FieldVariable.Editor_DrawTweakableVariable(composition.FieldSetupVariablesOverrides[i]);
                                }

                                GUILayout.Space(5f);
                            }

                            EditorGUILayout.EndVertical();
                        }

                        GUILayout.Space(10f);

                        #endregion
                    }


                    if (composition.GenType == EPGGGenType.Modificator)
                    {
                        if (composition.UtilityModsOverrides == null) composition.UtilityModsOverrides = new List<FieldSetupComposition.ModOverrideHelper>();
                        if (composition.UtilityModsOverrides.Count == 0)
                        {
                            composition.UtilityModsOverrides.Add(new FieldSetupComposition.ModOverrideHelper());
                            composition.RefreshModTo(mod, composition.UtilityModsOverrides[0]);
                        }

                        DisplayModOverridesGUI(mod, composition.UtilityModsOverrides[0], ref pfUtilScroll);
                    }
                    else
                    {
                        if (pack && mod/* && spawner != null*/)
                        {

                            FGUI_Inspector.FoldHeaderStart(ref drawPrefabsOverrides, "Override Prefabs to Spawn", FGUI_Resources.BGInBoxStyle);
                            bool packEnabled = true;

                            #region Package Overrides

                            if (drawPrefabsOverrides)
                            {
                                GUILayout.Space(7);
                                var packOverrides = composition.GetOverridesFor(pack);
                                bool refreshNeeded = false;


                                if (composition.GenType == EPGGGenType.FieldSetup)
                                {

                                    if (composition.Setup.ModificatorPacks.Count > 1)
                                    {
                                        if (selectedPack > 0) GUI.color = new Color(0.7f, 1f, 0.7f, 1f);
                                        EditorGUILayout.BeginVertical();
                                        GUI.color = preCol;
                                    }

                                    if (composition.Setup.ModificatorPacks.Count > 1)
                                    {
                                        EditorGUILayout.BeginVertical(FGUI_Resources.HeaderBoxStyle);

                                        EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

                                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_LeftFold), GUILayout.Height(20))) { pfScroll = Vector2.zero; selectedPack -= 1; if (selectedPack < 0) selectedPack = composition.Setup.ModificatorPacks.Count - 1; }

                                        EditorGUILayout.LabelField((selectedPack + 1) + " / " + composition.Setup.ModificatorPacks.Count, FGUI_Resources.HeaderStyle, GUILayout.Width(64));


                                        if (packOverrides != null)
                                        {
                                            packOverrides.SetEnabled =
                                                EditorGUILayout.Toggle(packOverrides.SetEnabled, GUILayout.Width(24));

                                            packEnabled = packOverrides.SetEnabled;
                                        }


                                        GUI.enabled = false;
                                        EditorGUILayout.ObjectField(pack, typeof(ModificatorsPack), true, GUILayout.MinWidth(120));
                                        GUI.enabled = preEn;

                                        //EditorGUILayout.ObjectField(pack, typeof(ModificatorsPack), true, GUILayout.MinWidth(120));
                                        //EditorGUILayout.LabelField(" Mod Pack: " + (selectedPack + 1) + " / " + composition.Setup.ModificatorPacks.Count, FGUI_Resources.HeaderStyle);
                                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_RightFold), GUILayout.Height(20))) { pfScroll = Vector2.zero; selectedPack += 1; if (selectedPack >= composition.Setup.ModificatorPacks.Count) selectedPack = 0; }
                                        EditorGUILayout.EndHorizontal();

                                        GUILayout.Space(2f);

                                        if (pack.Variables.Count > 0)
                                        {
                                            GUILayout.Space(4);
                                            //EditorGUILayout.LabelField("Override Package Variables", EditorStyles.centeredGreyMiniLabel);

                                            FGUI_Inspector.FoldHeaderStart(ref drawPackVars, "Override Package Variables", FGUI_Resources.BGInBoxStyle);

                                            if (drawPackVars)
                                            {

                                                for (int i = 0; i < packOverrides.PackVariablesOverrides.Count; i++)
                                                {
                                                    packOverrides.PackVariablesOverrides[i].UpdateVariableWith(packOverrides.ParentPack.Variables[i], true);
                                                    FieldVariable.Editor_DrawTweakableVariable(packOverrides.PackVariablesOverrides[i]);
                                                }

                                            }

                                            EditorGUILayout.EndVertical();
                                        }

                                        EditorGUILayout.EndVertical();
                                    }

                                }

                                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

                                bool drawSwitchers = pack.FieldModificators.Count > 1;
                                EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

                                if (drawSwitchers) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_LeftFold), GUILayout.Height(20))) { pfScroll = Vector2.zero; selectedMod -= 1; if (selectedMod < 0) selectedMod = pack.FieldModificators.Count - 1; }
                                if (drawSwitchers) EditorGUILayout.LabelField((selectedMod + 1) + " / " + pack.FieldModificators.Count, FGUI_Resources.HeaderStyle, GUILayout.Width(64));

                                bool modEnabled = true;

                                if (packOverrides != null)
                                    if (packOverrides.PackModsOverrides[selectedMod] != null)
                                    {
                                        packOverrides.PackModsOverrides[selectedMod].SetEnabled =
                                            EditorGUILayout.Toggle(packOverrides.PackModsOverrides[selectedMod].SetEnabled, GUILayout.Width(24));

                                        modEnabled = packOverrides.PackModsOverrides[selectedMod].SetEnabled;
                                    }

                                if (composition.GenType == EPGGGenType.ModPack)
                                {
                                    packOverrides = composition.FieldPackagesOverrides[0];
                                }

                                GUI.enabled = false;
                                EditorGUILayout.ObjectField(mod, typeof(FieldModification), true, GUILayout.MinWidth(120));
                                GUI.enabled = preEn;
                                GUILayout.Space(4f);

                                if (drawSwitchers) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_RightFold), GUILayout.Height(20))) { pfScroll = Vector2.zero; selectedMod += 1; if (selectedMod >= pack.FieldModificators.Count) selectedMod = 0; }
                                EditorGUILayout.EndHorizontal();


                                GUILayout.Space(2);
                                EditorGUILayout.LabelField("Alternate spawned prefabs with other ones", EditorStyles.centeredGreyMiniLabel);
                                GUILayout.Space(2);


                                if (spawner == null)
                                {
                                    EditorGUILayout.LabelField("No Spawners In Modificator!", EditorStyles.centeredGreyMiniLabel);
                                }

                                GUILayout.Space(4f);

                                FieldSetupComposition.ModOverrideHelper modOverrides = null;

                                if (composition.GenType == EPGGGenType.FieldSetup || composition.GenType == EPGGGenType.ModPack)
                                {
                                    if (packOverrides == null)
                                    {
                                        refreshNeeded = true;
                                    }
                                    else
                                    if (packOverrides.PackVariablesOverrides.Count != pack.Variables.Count)
                                    {
                                        refreshNeeded = true;
                                    }
                                    else
                                    {
                                        if (packOverrides.PackModsOverrides.Count != pack.FieldModificators.Count)
                                        {
                                            refreshNeeded = true;
                                        }
                                    }

                                    if (refreshNeeded)
                                        if (GUILayout.Button("Refresh Overrides Reference"))
                                        {
                                            if (composition.GenType == EPGGGenType.FieldSetup)
                                                composition.RefreshWith(this, composition.ParentFieldPlanner, composition.Setup, true);
                                            else
                                                composition.RefreshWith(composition.JustModPack);
                                        }

                                    if (!refreshNeeded)
                                        if (packOverrides != null)
                                        {
                                            modOverrides = packOverrides.GetOverridesFor(mod);

                                            if (modOverrides == null)
                                                refreshNeeded = true;
                                            else
                                                if (modOverrides.OverridePrefabs.Count != mod.PrefabsList.Count)
                                                refreshNeeded = true;

                                            if (refreshNeeded)
                                                if (GUILayout.Button("Refresh Overrides Reference"))
                                                {
                                                    if (composition.GenType == EPGGGenType.FieldSetup)
                                                        composition.RefreshWith(this, composition.ParentFieldPlanner, composition.Setup, true);
                                                    else
                                                        composition.RefreshWith(composition.JustModPack);
                                                }
                                        }

                                }


                                if (modEnabled == false || packEnabled == false) GUI.enabled = false;
                                DisplayModOverridesGUI(mod, modOverrides, ref pfScroll);
                                if (modEnabled == false || packEnabled == false) GUI.enabled = true;


                                GUILayout.Space(4f);
                                EditorGUILayout.EndVertical();


                                if (composition.GenType == EPGGGenType.FieldSetup)
                                    if (composition.Setup.ModificatorPacks.Count > 1)
                                    {
                                        EditorGUILayout.EndVertical();
                                    }
                            }

                            #endregion

                            EditorGUILayout.EndVertical();

                            if (composition.GenType == EPGGGenType.FieldSetup)
                            {

                                if (composition.Setup.UtilityModificators.Count > 0)
                                {

                                    GUILayout.Space(10);
                                    FGUI_Inspector.FoldHeaderStart(ref drawUtilitiesOverrides, "Override Utility Modificators", FGUI_Resources.BGInBoxStyle);

                                    if (drawUtilitiesOverrides)
                                    {
                                        GUILayout.Space(4);

                                        EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

                                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_LeftFold), GUILayout.Height(20))) { pfScroll = Vector2.zero; selectedCommand -= 1; if (selectedCommand < 0) selectedCommand = composition.Setup.UtilityModificators.Count - 1; }

                                        EditorGUILayout.LabelField((selectedCommand + 1) + " / " + composition.Setup.UtilityModificators.Count, FGUI_Resources.HeaderStyle, GUILayout.Width(64));

                                        GUI.enabled = false;
                                        EditorGUILayout.ObjectField(composition.Setup.UtilityModificators[selectedCommand], typeof(FieldModification), true, GUILayout.MinWidth(120));
                                        GUI.enabled = preEn;

                                        if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_RightFold), GUILayout.Height(20))) { pfScroll = Vector2.zero; selectedCommand += 1; if (selectedCommand >= composition.Setup.UtilityModificators.Count) selectedCommand = 0; }
                                        EditorGUILayout.EndHorizontal();

                                        GUILayout.Space(2);

                                        DisplayModOverridesGUI(composition.Setup.UtilityModificators[selectedCommand], composition.UtilityModsOverrides[selectedCommand], ref pfUtilScroll);

                                        GUILayout.Space(2);


                                    }

                                    EditorGUILayout.EndVertical();

                                }

                            }

                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Composition must be refreshed!", MessageType.Info);
                }
            }


            GUILayout.Space(9);
            GUI.backgroundColor = new Color(1f, 0.65f, 0.65f, 1f);
            if (GUILayout.Button(new GUIContent(" Don't Use Composition", FGUI_Resources.Tex_Remove), FGUI_Resources.ButtonStyle, GUILayout.Height(22))) { composition.Prepared = false; Close(); }
            GUI.backgroundColor = preCol;
            GUILayout.Space(6);

            if (EditorGUI.EndChangeCheck())
            {
                if (composition.Owner)
                {
                    EditorUtility.SetDirty(composition.Owner);
                }
            }

            EditorGUILayout.EndScrollView();

            //EditorGUIUtility.wideMode = preWideMode;
            //EditorGUIUtility.hierarchyMode = preHierarchyMode;
        }



        void DisplayModOverridesGUI(FieldModification mod, FieldSetupComposition.ModOverrideHelper modOverrides, ref Vector2 pfScroll)
        {
            Color preCol = GUI.color;

            if (mod.DrawSetupFor == FieldModification.EModificationMode.CustomPrefabs)
            {
                var pList = mod.PrefabsList;
                var wdth = GUILayout.Width(54);

                pfScroll = EditorGUILayout.BeginScrollView(pfScroll, false, false);

                EditorGUILayout.BeginHorizontal();

                if (pList.Count == 0) EditorGUILayout.HelpBox("No Prefabs in Modificator", MessageType.Info);

                for (int p = 0; p < pList.Count; p++)
                {
                    bool overriding = false;

                    if (modOverrides != null)
                        if (modOverrides.OverridePrefabs != null)
                            if (p < modOverrides.OverridePrefabs.Count)
                                if (modOverrides.OverridePrefabs[p] != null)
                                {
                                    if (modOverrides.OverridePrefabs[p].CoreGameObject != null)
                                    {
                                        GUI.color = new Color(1f, 1f, 1f, 0.3f);
                                        overriding = true;
                                    }
                                }

                    EditorGUILayout.BeginVertical(wdth);
                    PrefabReference.DrawPrefabField(pList[p], Color.gray, "Default", 54, () => { EditorGUIUtility.PingObject(pList[p].CoreGameObject); }, null, true, null, false, false);

                    if (overriding) GUI.color = preCol;

                    if (overriding) GUI.backgroundColor = Color.green; else GUI.backgroundColor = new Color(0.7f, 1f, 0.7f, 0.9f);
                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH, wdth);
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.LabelField("Override:", wdth);
                    PrefabReference prref = null;

                    if (modOverrides != null)
                        if (modOverrides.OverridePrefabs != null)
                            if (p < modOverrides.OverridePrefabs.Count)
                            {
                                prref = modOverrides.OverridePrefabs[p];
                                modOverrides.OverridePrefabs[p].SetPrefab( (GameObject)EditorGUILayout.ObjectField(modOverrides.OverridePrefabs[p].GameObject, typeof(GameObject), false, wdth));
                            }

                    if (prref != null)
                    {
                        if (prref.CoreGameObject == null)
                            PrefabReference.DrawPrefabField(prref, Color.gray, "_", 54, () => { EditorGUIUtility.PingObject(prref.CoreGameObject); }, null, true, null, false);
                        else
                            PrefabReference.DrawPrefabField(prref, Color.gray, "_", 54, () => { EditorGUIUtility.PingObject(prref.CoreGameObject); }, () => { modOverrides.OverridePrefabs[p].SetPrefab ( null); }, true, null, false);
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();
            }
            else if (mod.DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp)
            {
                if (modOverrides != null)
                    if (modOverrides.OverrideStampSet != null)
                        GUI.color = new Color(1f, 1f, 1f, 0.3f);

                EditorGUILayout.ObjectField("Modificator's Stamp:", mod.OStamp, typeof(OStamperSet), false);
                GUILayout.Space(4f);

                GUI.color = preCol;

                if (modOverrides != null)
                    modOverrides.OverrideStampSet = (OStamperSet)EditorGUILayout.ObjectField("Override With:", modOverrides.OverrideStampSet, typeof(OStamperSet), false);

                //EditorGUILayout.ObjectField("Override With:", null, typeof(OStamperSet), false);
            }
            else if (mod.DrawSetupFor == FieldModification.EModificationMode.ObjectMultiEmitter)
            {
                if (modOverrides != null)
                    if (modOverrides.OverrideMultiSet != null)
                        GUI.color = new Color(1f, 1f, 1f, 0.3f);

                EditorGUILayout.ObjectField("Modificator's MultiStamp:", mod.OMultiStamp, typeof(OStamperMultiSet), false);
                GUI.color = preCol;

                GUILayout.Space(4f);

                if (modOverrides != null)
                    modOverrides.OverrideMultiSet = (OStamperMultiSet)EditorGUILayout.ObjectField("Override With:", modOverrides.OverrideMultiSet, typeof(OStamperMultiSet), false);


                //EditorGUILayout.ObjectField("Override With:", null, typeof(OStamperMultiSet), false);
            }
        }

        public int selectedPack = 0;
        public int selectedCommand = 0;
        public int selectedMod = 0;
        public int selectedSpawner = 0;

        public bool drawPackVars = false;
        public bool drawSetupVars = true;
    }
}

#endif
