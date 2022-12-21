using FIMSpace.FEditor;
using FIMSpace.Generating.Rules;
using FIMSpace.Generating.Rules.QuickSolutions;
using FIMSpace.Graph;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FieldModification))]
    public partial class FieldModificationEditor : Editor
    {
        static Vector2 scroll = Vector2.zero;
#pragma warning disable IDE0052 // Remove unread private members
        static Vector2 scroll2 = Vector2.zero;
#pragma warning restore IDE0052 // Remove unread private members
        SerializedProperty sp_Enabled;
        SerializedProperty sp_DrawSetupFor;
        SerializedProperty sp_subSpwn;
        SerializedProperty sp_Spwn;
        SerializedProperty sp_ParentField;
        SerializedProperty sp_ParentPack;

        static int selected = 0;
        static GUIContent[] spawnersC;
        static string[] spawnersM;
        static int[] spawnersIds;
        static bool spawnersOptionsRefresh = true;

        static public SpawnRuleBase.EProcedureType? drawProcedures = null;
        static MonoScript mono;
        static Texture2D monoIcon;
        static bool displayCustomHeader = true;

        private void OnEnable()
        {
            mono = MonoScript.FromScriptableObject(Get);
            monoIcon = Resources.Load<Texture2D>("SPR_Modification");

            sp_Enabled = serializedObject.FindProperty("Enabled");
            sp_DrawSetupFor = serializedObject.FindProperty("DrawSetupFor");
            sp_Spwn = serializedObject.FindProperty("Spawners");
            sp_subSpwn = serializedObject.FindProperty("SubSpawners");
            sp_ParentField = serializedObject.FindProperty("ParentPreset");
            sp_ParentPack = serializedObject.FindProperty("ParentPack");

            RefreshSpawnersList(Get);
        }

        public static void OnSelectNewMod()
        {
            selected = 0;
        }

        private static string FoldSimbol(bool foldout) { if (foldout) return "▼"; else return "▲"; }


        public static void DrawHeaderGUI(SerializedObject serializedObject, FieldModification Get, SerializedProperty sp_Enabled = null, SerializedProperty sp_ParentField = null, SerializedProperty sp_ParentPack = null, bool displayCustomHeaderSwitch = false)
        {
            if (monoIcon == null) { monoIcon = Resources.Load<Texture2D>("SPR_Modification"); }
            if (sp_Enabled == null) sp_Enabled = serializedObject.FindProperty("Enabled");
            if (sp_ParentField == null) sp_ParentField = serializedObject.FindProperty("ParentPreset");
            if (sp_ParentPack == null) sp_ParentPack = serializedObject.FindProperty("ParentPack");

            EditorGUILayout.BeginHorizontal();
            int scale = 46;
            SerializedProperty spEn = sp_Enabled.Copy();

            GUILayout.Space(3);
            if (GUILayout.Button(new GUIContent(monoIcon, "Click to go to FieldMod script directory"), FGUI_Resources.ButtonStyle, GUILayout.Width(scale), GUILayout.Height(scale))) EditorGUIUtility.PingObject(mono);

            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(); GUILayout.Space(5);


            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 4;
            EditorGUILayout.PropertyField(spEn, new GUIContent(" ", "Making Field Modificator enabled or disabled"), GUILayout.Width(24));
            EditorGUIUtility.labelWidth = 0;
            if (GUILayout.Button(new GUIContent(Get.name), EditorStyles.boldLabel, GUILayout.MaxWidth(300))) EditorGUIUtility.PingObject(Get);
            if (GUILayout.Button(new GUIContent(" (Field Modificator)"), EditorStyles.label, GUILayout.MaxWidth(140))) EditorGUIUtility.PingObject(Get);

            if (Get.ParentPack != null)
            {
                GUIContent buttonC = new GUIContent(FGUI_Resources.Tex_AB);
                Color c = GUI.color;

                if (Get.hideFlags == HideFlags.None)
                {
                    GUI.color = c;
                    buttonC.tooltip = "Hide FieldModificator from search and project browser";
                }
                else
                {
                    GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.5f);
                    buttonC.tooltip = "Expose FieldModificator from search and project browser";
                }

                if (GUILayout.Button(buttonC, EditorStyles.label, GUILayout.MaxWidth(24)))
                {
                    if (Get.hideFlags == HideFlags.None) Get.hideFlags = HideFlags.HideInHierarchy;
                    else Get.hideFlags = HideFlags.None;
                    EditorUtility.SetDirty(Get);
                    AssetDatabase.SaveAssets();
                }

                GUI.color = c;
            }

            EditorGUILayout.EndHorizontal();

            //GUILayout.Space(-19);
            //GUILayout.Label("asd");
            //GUILayout.Space(-2);
            //GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal(); GUILayout.Space(11);
            EditorGUIUtility.labelWidth = 64;
            spEn.NextVisible(false);

            float targetWidth = EditorStyles.label.CalcSize(new GUIContent(spEn.stringValue)).x;
            if (targetWidth < 16) targetWidth = 16; if (targetWidth > 90) targetWidth = 90;

            EditorGUILayout.PropertyField(spEn, GUILayout.Width(targetWidth + 64 + 10)); EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.labelWidth = 72;
            GUILayout.Space(10);
            spEn.NextVisible(false); EditorGUILayout.PropertyField(spEn, new GUIContent("Box Gizmo", "Turn this on if your models are back-face invisible, drawed box will try to fit to the model representation")); EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(10);
            EditorGUIUtility.labelWidth = 57;
            spEn.NextVisible(false); EditorGUILayout.PropertyField(spEn/*, GUILayout.Width(100)*/); EditorGUIUtility.labelWidth = 0;


            GUILayout.FlexibleSpace();

            bool preE = GUI.enabled;
            GUI.enabled = false;
            EditorGUIUtility.labelWidth = 4;

            bool parentpacked = false;

            if (Get.ParentPack)
                if (Get.ParentPack.ParentPreset)
                    parentpacked = true;

            if (!parentpacked)
            {
                if (Get.ParentPreset)
                    EditorGUILayout.PropertyField(sp_ParentField, new GUIContent(" ", "Parent FieldSetup of this modificator"), GUILayout.Width(40));
            }
            else
            {
                EditorGUILayout.ObjectField(new GUIContent(" ", "Parent FieldSetup of this modificator"), Get.ParentPack.ParentPreset, typeof(FieldSetup), false, GUILayout.Width(40));
            }

            if (Get.ParentPack)
                EditorGUILayout.PropertyField(sp_ParentPack, new GUIContent(" ", "Parent FieldPackage of this modificator"), GUILayout.Width(40));

            EditorGUIUtility.labelWidth = 0;
            GUI.enabled = preE;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if (displayCustomHeaderSwitch)
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Default, "Click to view default header for Field Modificator"), FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20))) displayCustomHeader = !displayCustomHeader;
            GUILayout.Space(8);

            EditorGUILayout.EndHorizontal();
            FGUI_Inspector.DrawUILine(new Color(0f, 0f, 0f, 0.7f), 1, 5);

            serializedObject.ApplyModifiedProperties();
        }

        public static void RefreshSpawnersList(FieldModification mod)
        {
            if (spawnersOptionsRefresh) spawnersOptionsRefresh = false;

            if (string.IsNullOrEmpty(mod.name)) spawnersOptionsRefresh = true;

            spawnersC = mod.GetToSpawnNames();
            spawnersM = mod.GetMultiSpawnNames();
            spawnersIds = mod.GetToSpawnIndexes();
        }

        static int[] _subSp = new int[] { 0, 1 };
        static string[] _subSpnames = new string[] { "Spawners", "Sub-Spawners" };

        public static void DrawInspectorGUI(SerializedObject so, FieldModification Get, SerializedProperty sp_DrawSetupFor = null, SerializedProperty sp_Enabled = null, SerializedProperty sp_Spwn = null, bool displayCustomHeaderSwitch = false)
        {
            if (sp_Enabled == null) sp_Enabled = so.FindProperty("Enabled");
            if (sp_DrawSetupFor == null) sp_DrawSetupFor = so.FindProperty("DrawSetupFor");
            if (sp_Spwn == null)
            {
                sp_Spwn = so.FindProperty(FieldModification._subDraw == 0 ? "Spawners" : "SubSpawners");
            }

            if (Event.current != null) if (Event.current.type == EventType.Layout) FieldModification.UseEditorEvents();

            so.Update();

            Color bg = GUI.backgroundColor;
            bool ge = GUI.enabled;
            bool runRefresh = false;

            bool isvariant = false;
            if (Get.VariantOf)
            {
                GUILayout.Space(3);
                GUI.color = new Color(0.2f, 1f, 0.3f, 0.7f);
                EditorGUILayout.BeginVertical(FGUI_Resources.HeaderBoxStyle);
                GUI.color = bg;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Variant Mod of:", EditorStyles.boldLabel, GUILayout.Width(110));
                Get.VariantOf = (FieldModification)EditorGUILayout.ObjectField(GUIContent.none, Get.VariantOf, typeof(FieldModification), false);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(3);
                EditorGUILayout.HelpBox("With variant pack you're free to change prefabs in list, but changing rules will change them in root FieldModification", MessageType.None);
                GUILayout.Space(2);
                isvariant = true;
            }

            EditorGUI.BeginChangeCheck();
            SerializedProperty spEn = sp_Enabled.Copy();

            if (displayCustomHeaderSwitch)
            {
                if (displayCustomHeader == false) displayCustomHeader = EditorGUILayout.Toggle("Custom Header", displayCustomHeader);
                if (displayCustomHeader == false) EditorGUILayout.PropertyField(spEn);
            }

            spEn.NextVisible(false);

            if (Get.Enabled == false) GUI.enabled = false;

            if (displayCustomHeaderSwitch)
                if (displayCustomHeader == false) EditorGUILayout.PropertyField(spEn);
            spEn.NextVisible(false);
            if (displayCustomHeaderSwitch)
                if (displayCustomHeader == false) EditorGUILayout.PropertyField(spEn);
            GUILayout.Space(4);

            if (EditorGUI.EndChangeCheck()) runRefresh = true;

            // Stamp Fields ---------------------------------
            string foldS = FoldSimbol(Get._editor_drawStamp);
            EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxStyleH);
            if (GUILayout.Button(foldS + "   PREFABS TO SPAWN WITH MOD   " + foldS, FGUI_Resources.HeaderStyle))
            {
                Get._editor_drawStamp = !Get._editor_drawStamp;
                RefreshSpawnersList(Get);
            }

            EditorGUILayout.EndHorizontal();

            if (Get._editor_drawStamp)
            {
                GUILayout.Space(1);
                scroll2 = EditorGUILayout.BeginScrollView(scroll, FGUI_Resources.BGInBoxStyle);
                if (!isvariant) EditorGUILayout.PropertyField(sp_DrawSetupFor);

                SerializedProperty sp = sp_DrawSetupFor.Copy(); sp.NextVisible(false);


                if (Get.DrawSetupFor == FieldModification.EModificationMode.CustomPrefabs)
                {
                    EditorGUI.BeginChangeCheck();
                    int selReq = -1; bool drawThReq = true;
                    int preSel = selReq;
                    PrefabReference.DrawPrefabsList(Get.PrefabsList, ref Get.DrawObjectStamps, ref selReq, ref drawThReq, Color.gray, Color.green, EditorGUIUtility.currentViewWidth - 48, 64, false, Get, !isvariant);
                    if (preSel != selReq)
                    {
                        for (int i = 0; i < Get.Spawners.Count; i++)
                        {
                            if (selected == i) continue;
                            if (Get.Spawners[i].StampPrefabID == selReq)
                            {
                                selected = i;
                                break;
                            }
                        }
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(Get);
                        so.ApplyModifiedProperties();
                        so.Update();
                        RefreshSpawnersList(Get);
                    }
                }
                else if (Get.DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp)
                {
                    //sp.NextVisible(false);
                    //EditorGUILayout.PropertyField(sp);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(sp);
                    if (GUILayout.Button("Create New", GUILayout.Width(94))) Get.OStamp = (OStamperSet)FGenerators.GenerateScriptable(CreateInstance<OStamperSet>(), "OS_");
                    EditorGUILayout.EndHorizontal();

                    if (Get.OStamp != null)
                    {
                        if (Get.OStamp.Prefabs == null) Get.OStamp.Prefabs = new System.Collections.Generic.List<OSPrefabReference>();

                        EditorGUI.BeginChangeCheck();
                        int selReq = -1; bool drawThReq = true;
                        PrefabReference.DrawPrefabsList(Get.OStamp.Prefabs, ref Get.DrawObjectStamps, ref selReq, ref drawThReq, Color.gray, Color.green, EditorGUIUtility.currentViewWidth - 48, 64, false, Get.OStamp);
                        if (EditorGUI.EndChangeCheck())
                        {
                            RefreshSpawnersList(Get);
                            so.ApplyModifiedProperties();
                            so.Update();
                            EditorUtility.SetDirty(Get);
                        }
                    }
                }
                else if (Get.DrawSetupFor == FieldModification.EModificationMode.ObjectMultiEmitter)
                {
                    sp.NextVisible(false); //sp.NextVisible(false);
                    //EditorGUILayout.PropertyField(sp);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(sp);
                    if (GUILayout.Button("Create New", GUILayout.Width(94))) Get.OMultiStamp = (OStamperMultiSet)FGenerators.GenerateScriptable(CreateInstance<OStamperMultiSet>(), "OSM_");
                    EditorGUILayout.EndHorizontal();

                    if (Get.OMultiStamp != null)
                    {
                        FGenerators.DrawScriptableModificatorList<OStamperSet>(Get.OMultiStamp.PrefabsSets, FGUI_Resources.BGInBoxStyle, "Prefab Sets", ref Get.DrawMultiObjectStamps, true, false, Get, "[0]");
                        int reqInt = -1;
                        OStamperMultiSetEditor.DrawPrefabPreviewButtons(Get.OMultiStamp.PrefabSetSettings, ref reqInt, true, Get.OMultiStamp);
                    }
                }

                EditorGUILayout.EndScrollView();

            }

            GUILayout.Space(2);

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);
            EditorGUILayout.BeginHorizontal();
            foldS = FoldSimbol(Get._editor_drawSpawners);



            if (GUILayout.Button(" " + foldS + "   PREFABS SPAWNERS", EditorStyles.boldLabel, new GUILayoutOption[] { GUILayout.Height(24), GUILayout.Width(170) })) Get._editor_drawSpawners = !Get._editor_drawSpawners;

            if (isvariant == false)
            {
                if (GUILayout.Button("+ Add Spawner +", FGUI_Resources.ButtonStyle))
                {
                    GenericMenu menu = new GenericMenu();
                    AddPrefabsContextMenuItems(menu, Get);
                    menu.DropDown(new Rect(Event.current.mousePosition + Vector2.left * 100, Vector2.zero));
                }
            }

            if (Get.SubSpawners.Count > 0)
            {
                if (FieldModification._subDraw == 1) GUI.backgroundColor = new Color(0.75f, 0.8f, 1f, 1f);
                FieldModification._subDraw = EditorGUILayout.IntPopup(FieldModification._subDraw, _subSpnames, _subSp, GUILayout.Height(20), GUILayout.MaxWidth(86));
                GUI.backgroundColor = bg;
            }
            else
            {
                FieldModification._subDraw = 0;
            }

            EditorGUILayout.EndHorizontal();

            if (Get._editor_drawSpawners)
                if (FieldModification._subDraw == 1)
                {
                    EditorGUILayout.HelpBox("Sub-Spawners are not executed on the cell check.\nSub-spawners can be called using 'Call Sub Spawner' node from MAIN SPAWNERS!", MessageType.None);
                }
            GUILayout.Space(4);

            FieldSpawner spToRemove = null;

            EditorGUI.BeginChangeCheck();

            if (Get._editor_drawSpawners)
            {


                scroll = EditorGUILayout.BeginScrollView(scroll);

                var spawnersList = Get.Spawners;
                if (isvariant) spawnersList = Get.VariantOf.Spawners;

                if (FieldModification._subDraw == 1)
                {
                    if (Get.SubSpawners != null)
                        if (Get.SubSpawners.Count > 0)
                        {
                            spawnersList = Get.SubSpawners;
                        }
                }

                int tgtCount = Get.GetPRSpawnOptionsCount() + 1;
                if (Get.DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp) tgtCount += 1;

                #region Refreshing lists for enum popups

                if (spawnersList != null)
                {
                    if (spawnersC == null || spawnersC.Length != tgtCount || spawnersOptionsRefresh)
                    {
                        RefreshSpawnersList(Get);
                    }
                }

                #endregion



                if (isvariant == false)
                {
                    if (spawnersList == null || spawnersList.Count == 0)
                    {

                        GUILayout.Space(4);

                        Color prebC = GUI.backgroundColor;
                        GUI.backgroundColor = Color.green;
                        if (GUILayout.Button("Add First Spawning Execution Block"))
                        {
                            GenericMenu menu = new GenericMenu();
                            AddPrefabsContextMenuItems(menu, Get);
                            menu.DropDown(new Rect(Event.current.mousePosition + Vector2.left * 100, Vector2.zero));
                        }
                        GUI.backgroundColor = prebC;

                        EditorGUILayout.LabelField("This Execution Block is called\n'Field Spawner'", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30));

                        GUILayout.Space(4);

                    }
                }


                if (spawnersList != null)
                {
                    // Drawing spawner

                    if (spawnersList.Count > 0)
                    {
                        if (selected < spawnersList.Count)
                        {
                            if (selected < 0) selected = 0;
                            var spawner = spawnersList[selected];

                            if (spawnersList[selected] != null)
                            {
                                SerializedProperty sp_sp = null;

                                sp_sp = null;
                                if (selected < sp_Spwn.arraySize)
                                {
                                    sp_sp = sp_Spwn.GetArrayElementAtIndex(selected);
                                }
                                else
                                {
                                    if (selected < 0) selected = 0;
                                }

                                //try
                                //{

                                if (sp_sp != null)
                                {
                                    EditorGUILayout.BeginHorizontal();

                                    #region All Spawners buttons

                                    Color preGC = GUI.color;

                                    float lineWidth = 0;
                                    for (int i = 0; i < spawnersList.Count; i++)
                                    {
                                        if (selected == i) { GUI.backgroundColor = Color.green; Get._editor_shareSelected = i; }
                                        GUIContent nm = new GUIContent(spawnersList[i].Name);
                                        float width = EditorStyles.miniButton.CalcSize(nm).x;

                                        if (spawnersList[i].Enabled == false) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                                        if (string.IsNullOrEmpty(spawnersList[i].Name)) GUI.color = new Color(1f, 0.6f, 0.6f, GUI.color.a);

                                        if (GUILayout.Button(spawnersList[i].Name, EditorStyles.miniButton, GUILayout.Width(width)))
                                        {
                                            bool rmb = false;
                                            if (FGenerators.IsRightMouseButton())
                                            {
                                                GenericMenu menu = new GenericMenu();
                                                menu.AddItem(new GUIContent("Duplicate Spawner"), false, () => { FieldModification.AddEditorEvent(() => { Get.DuplicateSpawner(spawner); }); });
                                                menu.AddItem(new GUIContent(""), false, () => { });
                                                menu.AddItem(new GUIContent("Remove Spawner"), false, () => { selected -= 1; FieldModification.AddEditorEvent(() => { Get.RemoveSpawner(spawner); }); });
                                                FGenerators.DropDownMenu(menu);

                                                rmb = true;
                                            }

                                            if (!rmb)
                                            {
                                                selected = i;
                                                GUI.FocusControl(null);
                                            }
                                        }

                                        if (spawnersList[i].Enabled == false) GUI.color = preGC;
                                        if (selected == i) { GUI.backgroundColor = bg; Get._editor_shareSelected = i; }
                                        lineWidth += width;

                                        if (i < spawnersList.Count && i > 0)
                                        {
                                            if (lineWidth > EditorGUIUtility.currentViewWidth - 120)
                                            {
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.BeginHorizontal();
                                                lineWidth = 0;
                                            }
                                        }
                                    }


                                    #endregion

                                    EditorGUILayout.EndHorizontal();


                                    EditorGUI.BeginChangeCheck();
                                    GUILayout.Space(4);

                                    EditorGUILayout.BeginHorizontal();
                                    spawner.Enabled = EditorGUILayout.Toggle(spawner.Enabled, GUILayout.Width(24));
                                    spawnersList[selected].Name = EditorGUILayout.TextField(spawnersList[selected].Name);
                                    EditorGUIUtility.labelWidth = 28;
                                    float targetWidth = EditorStyles.label.CalcSize(new GUIContent(spawnersList[selected].SpawnerTag)).x + 40;
                                    if (targetWidth < 78) targetWidth = 78; if (targetWidth > 160) targetWidth = 160;
                                    spawnersList[selected].SpawnerTag = EditorGUILayout.TextField("Tag:", spawnersList[selected].SpawnerTag, GUILayout.Width(targetWidth));
                                    EditorGUIUtility.labelWidth = 0;

                                    if (selected < spawnersList.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowRight), GUILayout.Width(24))) { FGenerators.SwapElements(spawnersList, selected, selected + 1); selected++; if (selected > spawnersList.Count - 1) selected = 0; }
                                    if (selected > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowLeft), GUILayout.Width(24))) { FGenerators.SwapElements(spawnersList, selected, selected - 1); selected--; if (selected < 0) selected = 0; }

                                    if (GUILayout.Button(EditorGUIUtility.IconContent(spawner.DisplayPreviewGUI ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff"), FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.Width(24), GUILayout.Height(22) })) spawner.DisplayPreviewGUI = !spawner.DisplayPreviewGUI;

                                    Color prebc = GUI.backgroundColor;
                                    GUI.backgroundColor = new Color(1f, 0.635f, 0.635f, 1f);
                                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove), FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.Width(24), GUILayout.Height(22) })) { spToRemove = spawner; }
                                    GUI.backgroundColor = prebc;

                                    EditorGUILayout.EndHorizontal();


                                    EditorGUILayout.BeginHorizontal();

                                    if (selected < spawnersList.Count)
                                    {
                                        if (spawnersList[selected].MultipleToSpawn == false)
                                            spawnersList[selected].StampPrefabID = EditorGUILayout.IntPopup(new GUIContent("To Spawn"), spawnersList[selected].StampPrefabID, spawnersC, spawnersIds);
                                        else
                                        {
                                            spawnersList[selected].StampPrefabID = EditorGUILayout.MaskField(new GUIContent("To Spawn"), spawnersList[selected].StampPrefabID, spawnersM);
                                        }
                                    }

                                    EditorGUIUtility.labelWidth = 6; EditorGUIUtility.fieldWidth = 32 - 6;
                                    spawner.MultipleToSpawn = EditorGUILayout.Toggle(new GUIContent(" ", "Switch this toggle to be able for selecting multiple prefabs for spawning which will be randomly choosed"), spawner.MultipleToSpawn, GUILayout.Width(32));
                                    EditorGUIUtility.fieldWidth = 0; EditorGUIUtility.labelWidth = 0;
                                    EditorGUILayout.EndHorizontal();


                                    //EditorGUILayout.PropertyField(sp_sp.FindPropertyRelative("OnConditionsMet"));
                                    SerializedProperty sp_CellCheckMode = sp_sp.FindPropertyRelative("CellCheckMode");
                                    var sp_next = sp_CellCheckMode.Copy();
                                    sp_next.NextVisible(false);

                                    if (FieldModification._subDraw == 0)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(sp_CellCheckMode, GUILayout.ExpandWidth(true));
                                        EditorGUIUtility.labelWidth = 6; EditorGUIUtility.fieldWidth = 32 - 6;


                                        GUILayout.Space(5);
                                        if (GUILayout.Button(sp_next.boolValue ? FGUI_Resources.GUIC_FoldGrayDown : FGUI_Resources.GUIC_FoldGrayLeft, EditorStyles.label, GUILayout.Height(18), GUILayout.Width(22)))
                                        { sp_next.boolValue = !sp_next.boolValue; }//EditorGUILayout.PropertyField(sp_next, new GUIContent(" ", "Display Advanced option like grid scaling"), GUILayout.Width(32));
                                        EditorGUIUtility.fieldWidth = 0; EditorGUIUtility.labelWidth = 0;
                                        GUILayout.Space(5);
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    else
                                    {
                                        GUILayout.Space(8);
                                        FGUI_Inspector.DrawUILine(0.35f, 0.4f, 1, 1);
                                    }

                                    if (spawner._Editor_SpawnerAdvancedOptionsFoldout)
                                    {
                                        if (spawner.CellCheckMode != FieldSpawner.ESR_CellOrder.Ordered && spawner.CellCheckMode != FieldSpawner.ESR_CellOrder.TotalRandom)
                                            spawner.CellCheckMode = FieldSpawner.ESR_CellOrder.TotalRandom;
                                    }

                                    sp_next.NextVisible(false);

                                    if (spawner._Editor_SpawnerAdvancedOptionsFoldout)
                                    {
                                        if (spawner.OnScalledGrid < 0) spawner.OnScalledGrid = 1;

                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUIUtility.fieldWidth = 20;
                                        EditorGUILayout.PropertyField(sp_next);
                                        EditorGUIUtility.fieldWidth = 0;

                                        if (spawner.OnScalledGrid != 0)
                                        {
                                            if (spawner.ScaleAccess == FieldSpawner.ESR_CellHierarchyAccess.SameScale)
                                                spawner.ScaleAccess = (FieldSpawner.ESR_CellHierarchyAccess)EditorGUILayout.EnumPopup(spawner.ScaleAccess, GUILayout.Width(52));
                                            else
                                                spawner.ScaleAccess = (FieldSpawner.ESR_CellHierarchyAccess)EditorGUILayout.EnumPopup(spawner.ScaleAccess, GUILayout.Width(59));
                                        }
                                        EditorGUILayout.EndHorizontal();

                                        sp_next.NextVisible(false);
                                        if (sp_next.intValue < 1) sp_next.intValue = 1;
                                        EditorGUILayout.PropertyField(sp_next);

                                        GUILayout.BeginHorizontal();
                                        sp_next.NextVisible(false);
                                        EditorGUILayout.PropertyField(sp_next);
                                        sp_next.NextVisible(false);
                                        EditorGUILayout.PropertyField(sp_next);
                                        GUILayout.EndHorizontal();
                                    }
                                    //else // Now it's foldout -> not enable/disable extra options!
                                    //{
                                    //    spawner.OnScalledGrid = 1;
                                    //    spawner.ScaleAccess = FieldSpawner.ESR_CellHierarchyAccess.SameScale;
                                    //}


                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        EditorUtility.SetDirty(Get);
                                    }


                                    #region Rule Category Buttons

                                    GUILayout.Space(4);

                                    // Selecting spawners to draw by category
                                    // Drawing spawner procedures category buttons
                                    if (spawner.Rules.Count >= 2)
                                    {
                                        int countRl = 0;
                                        int countEv = 0;
                                        int countOncond = 0;
                                        int countOth = 0;

                                        for (int i = 0; i < spawner.Rules.Count; i++)
                                        {
                                            ISpawnProcedureType spawnerProc = spawner.Rules[i] as ISpawnProcedureType;
                                            if (spawnerProc != null)
                                            {
                                                if (spawnerProc.Type == SpawnRuleBase.EProcedureType.Rule) countRl++;
                                                else if (spawnerProc.Type == SpawnRuleBase.EProcedureType.Event) countEv++;
                                                else if (spawnerProc.Type == SpawnRuleBase.EProcedureType.OnConditionsMet) countOncond++;
                                                else countOth++;
                                            }
                                        }

                                        int diffCats = 0;
                                        if (countRl > 0) diffCats++;
                                        if (countEv > 0) diffCats++;
                                        if (countOncond > 0) diffCats++;
                                        if (countOth > 0) diffCats++;

                                        if (diffCats > 2)
                                        {

                                            EditorGUILayout.BeginHorizontal();
                                            float al = 0.75f;

                                            if (countRl > 0)
                                            {
                                                if (drawProcedures == SpawnRuleBase.EProcedureType.Rule) GUI.backgroundColor = Color.green; else GUI.backgroundColor = SpawnRuleBase.GetProcedureColor(SpawnRuleBase.EProcedureType.Rule, al);
                                                if (GUILayout.Button("Rules", FGUI_Resources.ButtonStyle)) drawProcedures = SpawnRuleBase.EProcedureType.Rule;
                                                GUI.backgroundColor = bg;
                                            }

                                            if (countEv > 0)
                                            {
                                                if (drawProcedures == SpawnRuleBase.EProcedureType.Event) GUI.backgroundColor = Color.green; else GUI.backgroundColor = SpawnRuleBase.GetProcedureColor(SpawnRuleBase.EProcedureType.Event, al);
                                                if (GUILayout.Button("Events", FGUI_Resources.ButtonStyle)) drawProcedures = SpawnRuleBase.EProcedureType.Event;
                                                GUI.backgroundColor = bg;
                                            }

                                            if (countOncond > 0)
                                            {
                                                if (drawProcedures == SpawnRuleBase.EProcedureType.OnConditionsMet) GUI.backgroundColor = Color.green; else GUI.backgroundColor = SpawnRuleBase.GetProcedureColor(SpawnRuleBase.EProcedureType.OnConditionsMet, al);
                                                if (GUILayout.Button("On Conditions", FGUI_Resources.ButtonStyle)) drawProcedures = SpawnRuleBase.EProcedureType.OnConditionsMet;
                                                GUI.backgroundColor = bg;
                                            }

                                            if (countOth > 0)
                                            {
                                                if (drawProcedures == SpawnRuleBase.EProcedureType.Coded) GUI.backgroundColor = Color.green; else GUI.backgroundColor = SpawnRuleBase.GetProcedureColor(SpawnRuleBase.EProcedureType.Coded, al);
                                                if (GUILayout.Button("Other", FGUI_Resources.ButtonStyle)) drawProcedures = SpawnRuleBase.EProcedureType.Coded;
                                                GUI.backgroundColor = bg;
                                            }

                                            if (drawProcedures == null) GUI.backgroundColor = Color.green;
                                            if (GUILayout.Button("All", FGUI_Resources.ButtonStyle)) drawProcedures = null;
                                            GUI.backgroundColor = bg;

                                            EditorGUILayout.EndHorizontal();
                                        }
                                    }
                                    else
                                    {
                                        drawProcedures = null;
                                    }

                                    #endregion

                                    #region Spawner Draw

                                    GUILayout.Space(8);

                                    bool preE = GUI.enabled;

                                    if (spawner.Enabled == false)
                                    {
                                        GUI.enabled = false;
                                    }


                                    // Call Spawner node Support
                                    if (FieldSpawner._editorLastDrawedSubCall)
                                    {
                                        EditorGUILayout.HelpBox("Other rule is calling this spawner", MessageType.None);
                                        if (GUI.enabled == false)
                                        {
                                            GUILayout.Space(3);
                                            GUI.enabled = true;
                                            bg = new Color(0.8f, 0.9f, 1f, 0.9f);
                                            GUI.backgroundColor = bg;
                                        }
                                    }


                                    for (int i = 0; i < spawner.Rules.Count; i++)
                                    {
                                        if (spawner.Rules[i] != null)
                                        {
                                            ISpawnProcedureType spawnerProc = spawner.Rules[i] as ISpawnProcedureType;

                                            if (spawnerProc != null)
                                            {
                                                if (SpawnRuleBase.DrawingCategory(drawProcedures, spawnerProc.Type))
                                                {
                                                    FieldSpawner.DrawRule(Get, selected, spawner, i);

                                                    #region Mod Graph

                                                    if (spawner.Rules[i].Enabled)
                                                        if (spawner.Rules[i]._editor_drawRule)
                                                            if (spawner.Rules[i].GetType() == typeof(Rules.QuickSolutions.SR_ModGraph))
                                                            {
                                                                Rules.QuickSolutions.SR_ModGraph srModG = spawner.Rules[i] as Rules.QuickSolutions.SR_ModGraph;
                                                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                                                                Color preBg = GUI.backgroundColor;

                                                                GUILayout.Space(-14);
                                                                EditorGUILayout.BeginHorizontal();
                                                                GUILayout.Space(2);

                                                                //if (SeparatedModWindow.Get != null)
                                                                //{
                                                                //    if (srModG._editor_DrawGraph) GUI.backgroundColor = Color.green;

                                                                //    string foldStr = FGUI_Resources.GetFoldSimbol(srModG._editor_DrawGraph, false);
                                                                //    if (GUILayout.Button(foldStr + "   Show Graph   " + foldStr, GUILayout.Height(22)))
                                                                //    {
                                                                //        srModG._editor_DrawGraph = !srModG._editor_DrawGraph;
                                                                //    }

                                                                //    if (srModG._editor_DrawGraph) GUI.backgroundColor = preBg;
                                                                //}

                                                                GUILayout.Space(6);

                                                                if (srModG.ExternalModGraph == srModG)
                                                                {
                                                                    srModG.ExternalModGraph = null;
                                                                    EditorUtility.SetDirty(srModG);
                                                                }

                                                                if (GUILayout.Button(new GUIContent("  Graph In Separated Window", PGGUtils.TEX_ModGraphIcon), GUILayout.Height(22)))
                                                                {
                                                                    if (srModG.ExternalModGraph == null)
                                                                        ModGraphWindow.Init(srModG);
                                                                    else
                                                                        ModGraphWindow.Init(srModG.ExternalModGraph);
                                                                }

                                                                if (srModG.ExternalModGraph != null)
                                                                {
                                                                    GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                                                                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Default, "Don't use external mod graph (clears field on the right)"), FGUI_Resources.ButtonStyle, GUILayout.Width(23), GUILayout.Height(20)))
                                                                    {
                                                                        srModG.ExternalModGraph = null;
                                                                        EditorUtility.SetDirty(srModG);
                                                                    }
                                                                }


                                                                if (srModG.ExternalModGraph == null)
                                                                    GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                                                                else
                                                                    GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1f);


                                                                int wdth = srModG.ExternalModGraph == null ? 32 : 158;

                                                                EditorGUILayout.BeginVertical(FGUI_Resources.ButtonStyle, GUILayout.Width(wdth), GUILayout.Height(19));
                                                                GUILayout.Space(-2);
                                                                if (srModG.ExternalModGraph == null)
                                                                {
                                                                    srModG.ExternalModGraph = (Rules.QuickSolutions.SR_ModGraph)EditorGUILayout.ObjectField(GUIContent.none, srModG.ExternalModGraph, typeof(Rules.QuickSolutions.SR_ModGraph), false, GUILayout.Width(wdth), GUILayout.Height(18));
                                                                }
                                                                else
                                                                {
                                                                    EditorGUIUtility.labelWidth = 122;
                                                                    EditorGUI.BeginChangeCheck();
                                                                    srModG.ExternalModGraph = (Rules.QuickSolutions.SR_ModGraph)EditorGUILayout.ObjectField("External Mod Graph:", srModG.ExternalModGraph, typeof(Rules.QuickSolutions.SR_ModGraph), false, GUILayout.Width(wdth), GUILayout.Height(18));
                                                                    if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(srModG); }
                                                                    EditorGUIUtility.labelWidth = 0;
                                                                }

                                                                EditorGUILayout.EndVertical();

                                                                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1f);

                                                                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Save, "Save Mod Graph as separated file inside project, to be able to call it's logics in different spawners"), FGUI_Resources.ButtonStyle, GUILayout.Width(23), GUILayout.Height(20)))
                                                                {
                                                                    var generated = ExportModGraph(srModG);

                                                                    if (generated)
                                                                    {
                                                                        EditorGUIUtility.PingObject(generated);
                                                                        srModG.ExternalModGraph = generated;
                                                                        EditorUtility.SetDirty(srModG);
                                                                    }
                                                                }

                                                                GUI.backgroundColor = Color.white;

                                                                GUILayout.Space(5);
                                                                EditorGUILayout.EndHorizontal();

                                                                //if (srModG._editor_DrawGraph)
                                                                //{
                                                                //    GUILayout.Space(-18);
                                                                //    ModGraphDrawer graphDraw = DrawGraph(srModG, so);
                                                                //    if (graphDraw != null) graphDraw.RefreshTitle();
                                                                //}

                                                                GUILayout.Space(5);

                                                                if (!srModG._editor_DrawGraph)
                                                                {
                                                                    if (srModG.ExternalModGraph == null)
                                                                        EditorGUILayout.LabelField("Nodes: " + srModG.Nodes.Count, EditorStyles.centeredGreyMiniLabel);
                                                                    else
                                                                        EditorGUILayout.LabelField("(External Graph) Nodes: " + srModG.ExternalModGraph.Nodes.Count, EditorStyles.centeredGreyMiniLabel);

                                                                }

                                                                GUILayout.Space(3);

                                                                GUILayout.EndVertical();
                                                                GUILayout.Space(11);

                                                            }


                                                    #endregion

                                                }
                                            }
                                            else
                                            {
                                                if (drawProcedures == null)
                                                {
                                                    FieldSpawner.DrawRule(Get, selected, spawner, i);
                                                }
                                            }
                                        }
                                    }

                                    if (spawner.Enabled == false) GUI.enabled = preE;

                                    spawner.Parent = Get;
                                    spawner.DrawInspector();

                                    #endregion


                                }


                                //}
                                //catch (System.Exception exc)
                                //{
                                //    if (PGGInspectorUtilities.LogPGGWarnings)
                                //        UnityEngine.Debug.Log("There was issue when drawing Field Modificator window, probably harmless: " + exc);
                                //}


                            }
                            else
                            {
                                Color prebc = GUI.backgroundColor;
                                GUI.backgroundColor = new Color(1f, 0.635f, 0.635f, 1f);
                                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove), FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.Width(24), GUILayout.Height(22) })) Get.RemoveSpawner(spawner);
                                GUI.backgroundColor = prebc;
                            }

                        }
                        else
                        {
                            selected = 0;
                        }
                    }
                }


                EditorGUILayout.EndScrollView();

            }

            if (FGenerators.CheckIfExist_NOTNULL(spToRemove))
            {
                selected -= 1;
                if (selected < 0) selected = 0;

                if (FieldModification._subDraw == 0)
                    Get.Spawners.Remove(spToRemove);
                else
                    Get.SubSpawners.Remove(spToRemove);
            }

            EditorGUILayout.EndVertical();




            if (FieldModification._subDraw == 1)
            {
                GUILayout.Space(10);
                if (GUILayout.Button("Back to Spawners", FGUI_Resources.ButtonStyle))
                {
                    FieldModification._subDraw = 0;
                }
            }



            #region Global Rules Display

            bool gEnbl = GUI.enabled;

            bool anyGlobal = false;
            bool anyGlobalR = false;

            if (Get.ParentPack)
            {
                if (Get.ParentPack.CallOnAllSpawners != null)
                    if (Get.ParentPack.CallOnAllSpawners.Rules != null)
                        if (Get.ParentPack.CallOnAllSpawners.Rules.Count > 0)
                            anyGlobal = true;
            }

            for (int i = 0; i < Get.Spawners.Count; i++)
                for (int r = 0; r < Get.Spawners[i].Rules.Count; r++)
                {
                    if (Get.Spawners[i].Rules == null) continue;
                    if (Get.Spawners[i].Rules[r] == null) { Get.Spawners[i].Rules.RemoveAt(r); break; }
                    if (Get.Spawners[i].Rules[r].Global)
                    {
                        anyGlobalR = true;
                        anyGlobal = true;
                        break;
                    }
                }

            if (anyGlobal)
            {
                foldS = FoldSimbol(Get._editor_drawGlobalRules);
                GUILayout.Space(14);

                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                if (GUILayout.Button(" " + foldS + "    GLOBAL RULES FOR MOD    " + foldS + " ", FGUI_Resources.HeaderStyle, new GUILayoutOption[] { GUILayout.Height(24) })) Get._editor_drawGlobalRules = !Get._editor_drawGlobalRules;

                if (Get._editor_drawGlobalRules)
                {

                    if (anyGlobalR)
                    {
                        GUILayout.Space(4);
                        GUI.backgroundColor = Color.black;
                        EditorGUILayout.BeginVertical(FGUI_Resources.BGBoxStyle);
                        GUI.backgroundColor = bg;
                        EditorGUILayout.BeginHorizontal();
                        Get.Spawners[selected].UseGlobalRules = EditorGUILayout.Toggle(Get.Spawners[selected].UseGlobalRules, GUILayout.Width(18));
                        string addStr = Get.Spawners[selected].UseGlobalRules ? "" : " (Disabled for Spawner)";
                        EditorGUILayout.LabelField("Global Rules" + addStr, FGUI_Resources.HeaderStyle);
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(4);

                        for (int i = 0; i < Get.Spawners.Count; i++)
                            for (int r = 0; r < Get.Spawners[i].Rules.Count; r++)
                            {
                                if (Get.Spawners[i].Rules == null) continue;
                                if (Get.Spawners[i].Rules[r].Global)
                                {
                                    FieldSpawner.DrawRule(Get, selected, Get.Spawners[i], r, false);
                                }
                            }

                        EditorGUILayout.EndVertical();
                        GUILayout.Space(4);
                    }
                }

                EditorGUILayout.EndVertical();



                if (Get.ParentPack != null)
                    if (Get.ParentPack.CallOnAllSpawners != null)
                        if (Get.ParentPack.CallOnAllSpawners.Rules != null)
                            if (Get.Spawners.Count > 0)
                            //if (Get.ParentPack.CallOnAllSpawners.Rules.Count > 0)
                            {
                                foldS = FoldSimbol(Get._editor_drawModPackRules);
                                GUILayout.Space(14);

                                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);

                                if (GUILayout.Button(" " + foldS + "    MOD PACK GLOBAL RULES    " + foldS + " ", FGUI_Resources.HeaderStyle, new GUILayoutOption[] { GUILayout.Height(24) })) Get._editor_drawModPackRules = !Get._editor_drawModPackRules;

                                if (Get._editor_drawModPackRules)
                                {

                                    GUILayout.Space(4);
                                    GUI.backgroundColor = Color.white;

                                    EditorGUILayout.BeginVertical(FGUI_Resources.HeaderBoxStyle);
                                    GUI.backgroundColor = bg;
                                    sp_callOnAll = new SerializedObject(Get.ParentPack).FindProperty("CallOnAllSpawners");

                                    EditorGUILayout.BeginHorizontal();
                                    //UnityEngine.Debug.Log("is nan? " + Get.Spawners[selected].DontUseParentPackageRules );
                                    Get.Spawners[selected].UseParentPackageRules = EditorGUILayout.Toggle(Get.Spawners[selected].UseParentPackageRules, GUILayout.Width(18));
                                    string addStr = Get.Spawners[selected].UseParentPackageRules ? "Mod Pack 'Call on every Spawner'" : " Disabled for this Spawner";
                                    EditorGUILayout.LabelField(addStr, FGUI_Resources.HeaderStyle);

                                    GUI.enabled = false; EditorGUILayout.ObjectField(Get.ParentPack, typeof(ModificatorsPack), true, GUILayout.Width(35)); GUI.enabled = gEnbl;

                                    EditorGUILayout.EndHorizontal();

                                    GUILayout.Space(4);
                                    Get.ParentPack.CallOnAllSpawners.DrawSpawnerGUI(sp_callOnAll, new GUIContent[0], new int[0], false, false, false);
                                    EditorGUILayout.EndVertical();

                                    GUILayout.Space(4);
                                }

                                EditorGUILayout.EndVertical();

                            }


            }

            #endregion


            so.ApplyModifiedProperties();


            if (EditorGUI.EndChangeCheck()) runRefresh = true;
            if (!runRefresh) runRefresh = CheckCellsSelectorWindow.GetChanged();

            if (runRefresh)
            {
                if (FieldDesignWindow.Get)
                    if (FieldDesignWindow.Get.AutoRefreshPreview)
                    {
                        if (FieldDesignWindow.Get.UsePainterGenerating == false)
                        {
                            FieldDesignWindow.Get.RunFieldCellsRules();
                            if (FieldDesignWindow.Get.PreviewAutoSpawn) FieldDesignWindow.Get.RunFieldCellsSpawningGameObjects();
                        }
                        else
                        {
                            FieldDesignWindow.Get.TriggerRefresh();
                        }

                        SceneView.RepaintAll();

                        so.Update();
                        RefreshSpawnersList(Get);
                    }
            }

            if (isvariant) EditorGUILayout.EndVertical();

            GUI.enabled = ge;
        }

        private static SR_ModGraph ExportModGraph(SR_ModGraph source)
        {
            string savePath = FGenerators.GetPathPopup("Save Mod Graph", "ModGraph_");

            if (savePath == "") return null;

            SR_ModGraph newMod = Instantiate(source);
            AssetDatabase.CreateAsset(newMod, savePath);
            newMod = AssetDatabase.LoadAssetAtPath<SR_ModGraph>(savePath);

            if (newMod == null) return null;
            AssetDatabase.SaveAssets();

            if (AssetDatabase.Contains(newMod) == false) return null;

            savePath = AssetDatabase.GetAssetPath(newMod);

            newMod.Procedures.Clear();
            for (int p = 0; p < source.Procedures.Count; p++) // Checking and copying all nodes
            {
                if (source.Procedures[p] == null) continue;

                var nNode = GameObject.Instantiate(source.Procedures[p]); // Creating copy
                AssetDatabase.AddObjectToAsset(nNode, savePath);
                newMod.Procedures.Add(nNode);
                nNode.hideFlags = HideFlags.HideInHierarchy;
            }

            AssetDatabase.SaveAssets();

            return newMod;
        }

        static SerializedProperty sp_callOnAll;

        protected override void OnHeaderGUI()
        {
            if (displayCustomHeader == false)
            {
                base.OnHeaderGUI();
                return;
            }

            DrawHeaderGUI(serializedObject, Get, sp_Enabled, sp_ParentField, sp_ParentPack, true);
        }


        public override void OnInspectorGUI()
        {
            //UnityEditor.EditorUtility.SetDirty(Get);
            DrawInspectorGUI(serializedObject, Get, sp_DrawSetupFor, sp_Enabled, FieldModification._subDraw == 0 ? sp_Spwn : sp_subSpwn, true);
        }



        private static SerializedObject so_currentSetup = null;
        private static ModGraphDrawer BuildPlannerGraphDraw = null;
        internal static bool drawGraph = true;
        //public static Rules.QuickSolutions.SR_ModGraph LatestFieldPlanner;
        internal static ModGraphDrawer DrawGraph(Rules.QuickSolutions.SR_ModGraph target, SerializedObject so)
        {
            if (so != null) so_currentSetup = so;

            if (target != null)
            {
                if (so_currentSetup == null) so_currentSetup = new SerializedObject(target);

                if (BuildPlannerGraphDraw == null || BuildPlannerGraphDraw.currentSetup.ScrObj != target)
                {
                    BuildPlannerGraphDraw = new ModGraphDrawer(SeparatedModWindow.Get, target);
                }
            }

            if (BuildPlannerGraphDraw != null)
            {
                GUILayout.Space(1);
                var rect = GUILayoutUtility.GetLastRect();
                rect.height = 300;

                GUI.BeginGroup(rect);
                //BuildPlannerGraphDraw.AdjustTopOffset = rect.y;
                BuildPlannerGraphDraw.DrawedInsideInspector = true;
                BuildPlannerGraphDraw.displayPadding = new Vector4(8, 0, 0, 0);
                BuildPlannerGraphDraw.Tex_Net = SeparatedModWindow.Get.Tex_Net;
                BuildPlannerGraphDraw.Parent = SeparatedModWindow.Get;
                BuildPlannerGraphDraw.DrawGraph();

                if (so_currentSetup != null)
                    if (BuildPlannerGraphDraw.AsksForSerializedPropertyApply)
                    {
                        so_currentSetup.ApplyModifiedProperties();
                        so_currentSetup.Update();
                        BuildPlannerGraphDraw.AsksForSerializedPropertyApply = false;
                    }

                GUI.EndGroup();
                GUILayout.Space(380);
            }

            return BuildPlannerGraphDraw;
        }

    }
}