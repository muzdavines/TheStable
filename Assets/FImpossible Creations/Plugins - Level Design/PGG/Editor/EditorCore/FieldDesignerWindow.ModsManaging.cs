using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FIMSpace.FEditor;
using System.Linq;

namespace FIMSpace.Generating
{
    public partial class FieldDesignWindow
    {
        /// <summary>
        /// Drawing Modificators Packs
        /// </summary>
        void DrawMods()
        {
            EditorGUI.BeginChangeCheck();

            if (!projectPreset.ModificatorPacks.Contains(projectPreset.RootPack)) projectPreset.ModificatorPacks.Add(projectPreset.RootPack);

            if (projectPreset == null) return;
            if (projectPreset.RootPack == null) return;
            projectPreset.RootPack.ParentPreset = projectPreset;

            if (advancedMode)
                DrawModificatorPackFieldSetupList(projectPreset.RootPack, projectPreset.ModificatorPacks, FGUI_Resources.BGInBoxStyle, "FieldSetup's Modification Packs", ref drawPacks, ref selectedPackIndex, true, true, projectPreset, true);

            // Selection check
            for (int i = 0; i < projectPreset.ModificatorPacks.Count; i++)
            {
                if (Selection.objects.Contains(projectPreset.ModificatorPacks[i]))
                {
                    if (selectedPackIndex != i) if (projectPreset.ModificatorPacks[i].FieldModificators.Count > 0) AssetDatabase.OpenAsset(projectPreset.ModificatorPacks[i].FieldModificators[0]);

                    selectedPackIndex = i;
                    break;
                }
            }

            if (selectedPackIndex >= 0)
            {
                // Drawing Selected Modificator Pack -----------------------------------
                if (selectedPackIndex >= projectPreset.ModificatorPacks.Count) selectedPackIndex = 0;

                var modPack = projectPreset.ModificatorPacks[selectedPackIndex];

                if (modPack != null)
                {
                    GUILayout.Space(3);
                    if (advancedMode)
                        ModificatorsPackEditor.DrawFieldModList(modPack, modPack == projectPreset.RootPack ? "Base Field's Mods" : modPack.name, ref drawPack, modPack, true, projectPreset);
                    else
                        ModificatorsPackEditor.DrawFieldModList(modPack, modPack == projectPreset.RootPack ? "Sets Of Objects to Spawn on Grid" : modPack.name, ref drawPack, modPack, true, projectPreset, true);
                }

                if (projectPreset.UtilityModificators.Count > 0)
                {
                    GUILayout.Space(3);
                    ModificatorsPackEditor.DrawUtilityModsList(projectPreset.UtilityModificators, ref drawUtilMods, "Utility Modificators", projectPreset);
                }
            }

            if (EditorGUI.EndChangeCheck() || CheckCellsSelectorWindow.GetChanged()) TriggerRefresh(false);

            if (!advancedMode)
                if (projectPreset.ModificatorPacks.Count > 1)
                {
                    FGenerators.CheckForNulls(projectPreset.ModificatorPacks);

                    if (projectPreset.ModificatorPacks.Count > 1)
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.HelpBox("WARNING: This FieldSetup file contains multiple packs of FieldModificators.\nIN THE BEGINNER MODE you see ONLY ONE OF THE PACKS!", MessageType.Info);
                        GUILayout.Space(3);
                    }
                }
        }

        static GUIContent _GUICCombine { get { if (_guicComb == null) _guicComb = new GUIContent(EditorGUIUtility.IconContent("_Popup").image, "Mesh Combine Mode"); return _guicComb; } }
        static GUIContent _guicComb = null;

        /// <summary>
        /// Modificator packs are always outside project files, except built-in packages in room preset
        /// </summary>
        public static void DrawModificatorPackFieldSetupList(ModificatorsPack basePack, List<ModificatorsPack> toDraw, GUIStyle style, string title, ref bool foldout, ref int selected, bool newButton = false, bool moveButtons = false, UnityEngine.Object toDirty = null, bool drawEnableDisablePackSwitch = false, bool draftsButtons = true)
        {
            if (toDraw == null) return;

            Color bgc = GUI.backgroundColor;
            Color preC = GUI.color;

            GUI.color = Color.green;
            EditorGUILayout.BeginVertical(style);
            GUI.color = bgc;

            EditorGUILayout.BeginHorizontal();
            string fold = foldout ? "  ▼" : "  ►";
            if (GUILayout.Button(new GUIContent(fold + "  " + title + " (" + toDraw.Count + ")", PGGUtils._Tex_ModPackSmall), EditorStyles.label, GUILayout.Height(19))) foldout = !foldout;

            if (foldout)
            {
                GUILayout.FlexibleSpace();

                if (draftsButtons)
                {

                    #region Draft Button

                    if (Get)
                        if (Get.StartupRefs)
                            if (Get.StartupRefs.FSModPackDraftsDirectory)
                            {
                                Color prebg = GUI.backgroundColor;
                                GUI.backgroundColor = new Color(0.75f, 1f, 0.75f, 1f);

                                if (GUILayout.Button(new GUIContent("New", "Generates new Field Mod Pack file in the PGG drafts directory"), GUILayout.Width(42)))
                                {
                                    string path;

                                    path = AssetDatabase.GetAssetPath(Get.StartupRefs.FSModPackDraftsDirectory);
                                    var files = System.IO.Directory.GetFiles(path, "*.asset");
                                    path += "/MP_ModPack" + (files.Length + 1) + ".asset";

                                    ModificatorsPack scrInstance = CreateInstance<ModificatorsPack>();

                                    if (string.IsNullOrEmpty(path))
                                        path = FGenerators.GenerateScriptablePath(scrInstance, "MP_NewModPack");

                                    if (!string.IsNullOrEmpty(path))
                                    {
                                        UnityEditor.AssetDatabase.CreateAsset(scrInstance, path);
                                        AssetDatabase.SaveAssets();
                                        toDraw.Add(scrInstance);
                                    }
                                }

                                GUI.backgroundColor = prebg;
                            }


                    #endregion

                }

                if (GUILayout.Button("+[ ]"))
                {
                    toDraw.Add(null);
                    EditorUtility.SetDirty(toDirty);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                GUILayout.Space(5);

                if (toDraw.Count > 0)
                    for (int i = 0; i < toDraw.Count; i++)
                    {

                        bool isBasePackage = false;
                        if (toDraw[i] == basePack) isBasePackage = true;

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();

                        if (toDraw[i] != null)
                        {
                            EditorGUI.BeginChangeCheck();
                            toDraw[i].DisableWholePackage = !EditorGUILayout.Toggle(!toDraw[i].DisableWholePackage, GUILayout.Width(16));
                            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(toDraw[i]);
                            if (toDraw[i].DisableWholePackage) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                        }

                        GUIContent lbl = new GUIContent(isBasePackage ? "Base" : i.ToString());
                        float wdth = EditorStyles.label.CalcSize(lbl).x;

                        //EditorGUILayout.LabelField(lbl, );
                        if (selected == i) GUI.backgroundColor = Color.green;

                        if (GUILayout.Button(lbl, GUILayout.Width(wdth + 8)))
                        {
                            if (selected == i) selected = -1;
                            else
                            {
                                selected = i;
                                if (toDraw[i]) if (toDraw[i].FieldModificators.Count > 0) AssetDatabase.OpenAsset(toDraw[i].FieldModificators[0]);
                            }

                            Selection.activeObject = toDraw[i];
                        }

                        GUI.backgroundColor = preC;

                        bool preE = GUI.enabled;
                        if (isBasePackage) GUI.enabled = false; else EditorGUI.BeginChangeCheck();
                        toDraw[i] = (ModificatorsPack)EditorGUILayout.ObjectField(toDraw[i], typeof(ModificatorsPack), false);

                        if (toDraw[i] != null)
                        {
                            GUI.enabled = preE;
                            if (toDraw[i].CombineSpawns == ModificatorsPack.EPackCombine.None) GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                            EditorGUIUtility.labelWidth = 18;
                            toDraw[i].CombineSpawns = (ModificatorsPack.EPackCombine)EditorGUILayout.EnumPopup(_GUICCombine, toDraw[i].CombineSpawns, GUILayout.Width(74));
                            EditorGUIUtility.labelWidth = 0;
                            if (toDraw[i].CombineSpawns == ModificatorsPack.EPackCombine.None) GUI.backgroundColor = Color.white;
                        }

                        if (isBasePackage)
                        {
                            if (ModificatorsPackEditor.DrawRenameScriptableButton(toDraw[i], "Modificator Pack", true))
                            {
                                if (toDraw[i].ParentPreset)
                                {
                                    toDraw[i].name = (toDraw[i].ParentPreset.name + "_" + i + "_Pack").Replace("FS_", "");
                                    EditorUtility.SetDirty(toDirty);
                                }
                            }
                        }
                        else if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(toDirty);


                        if (!isBasePackage)
                            if (newButton)
                                if (toDraw[i] == null)
                                    if (GUILayout.Button(new GUIContent("New", "Generate new ModificatorsPack file in project assets"), GUILayout.Width(52)))
                                    {
                                        ModificatorsPack tempPreset = (ModificatorsPack)FGenerators.GenerateScriptable(ScriptableObject.CreateInstance<ModificatorsPack>(), "ModPack_");
                                        AssetDatabase.SaveAssets();
                                        if (AssetDatabase.Contains(tempPreset)) if (tempPreset != null) toDraw[i] = tempPreset;
                                        EditorUtility.SetDirty(toDirty);
                                    }

                        if (moveButtons)
                        {
                            EditorGUI.BeginChangeCheck();
                            if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp, "Move this element to be executed before above one"), GUILayout.Width(24))) { ModificatorsPack temp = toDraw[i - 1]; toDraw[i - 1] = toDraw[i]; toDraw[i] = temp; }
                            if (i < toDraw.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown, "Move this element to be executed after below one"), GUILayout.Width(24))) { ModificatorsPack temp = toDraw[i + 1]; toDraw[i + 1] = toDraw[i]; toDraw[i] = temp; }

                            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(toDirty);
                        }

                        if (!isBasePackage)
                            if (GUILayout.Button("X", GUILayout.Width(24))) { toDraw.RemoveAt(i); EditorUtility.SetDirty(toDirty); break; }

                        GUI.color = preC;
                        EditorGUILayout.EndHorizontal();
                        //if (toDirty != null) if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(toDirty); }
                    }
                else
                {
                    EditorGUILayout.LabelField("No object in list", EditorStyles.centeredGreyMiniLabel);
                }
            }

            EditorGUILayout.EndVertical();
        }


        public static readonly int varPerPages = 8;
        public static void DrawCellInstructionDefinitionsList(ref int page, List<InstructionDefinition> toDraw, GUIStyle style, string title, ref bool foldout, bool moveButtons = false, FieldSetup toDirty = null)
        {
            if (toDraw == null) return;

            Color bgc = GUI.backgroundColor;

            int pagesCount = Mathf.FloorToInt(toDraw.Count / varPerPages);

            EditorGUI.BeginChangeCheck();

            GUI.color = Color.green;
            EditorGUILayout.BeginVertical(style);
            GUI.color = bgc;

            EditorGUILayout.BeginHorizontal();
            string fold = foldout ? "   ▼" : "   ►";
            if (GUILayout.Button(new GUIContent(fold + "  " + title + " (" + toDraw.Count + ")", PGGUtils.TEX_CellInstr), EditorStyles.label)) foldout = !foldout;

            if (foldout)
            {
                GUILayout.FlexibleSpace();

                if (pagesCount > 0)
                {
                    GUILayout.Space(2);
                    if (GUILayout.Button("◄", EditorStyles.label, GUILayout.Width(20))) { page -= 1; }
                    GUILayout.Space(5);
                    GUILayout.BeginVertical();
                    GUILayout.Space(2); GUILayout.Label((page + 1) + "/" + (pagesCount + 1), FGUI_Resources.HeaderStyle);
                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                    if (GUILayout.Button("►", EditorStyles.label, GUILayout.Width(20))) { page += 1; }

                    if (page < 0) page = pagesCount;
                    if (page > pagesCount) page = 0;
                    GUILayout.Space(3);
                }

                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    toDraw.Add(null);
                    EditorUtility.SetDirty(toDirty);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                GUILayout.Space(4);

                int startIndex = page * varPerPages;

                if (toDraw.Count > 0)
                    for (int i = startIndex; i < Mathf.Min(startIndex + varPerPages, toDraw.Count); i++)
                    {
                        if (toDraw[i] == null) continue;

                        GUI.backgroundColor = bgc;

                        bool preE = GUI.enabled;
                        EditorGUILayout.BeginHorizontal();

                        if (toDraw[i].Foldout) GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 1f);
                        if (GUILayout.Button("[" + i + "]", FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(18)))
                        {

                            if (toDirty)
                            {
                                if (FGenerators.IsRightMouseButton())
                                {
                                    GenericMenu menu = new GenericMenu();
                                    FieldModification modd = toDraw[i].TargetModification;

                                    if (modd)
                                        menu.AddItem(new GUIContent("Export Copy"), false, () => { ModificatorsPackEditor.ExportCopyPopup(modd); });
                                    if (modd)
                                        menu.AddItem(new GUIContent("Export Variant"), false, () => { ModificatorsPackEditor.ExportVariantPopup(modd); });

                                    menu.AddItem(new GUIContent(""), false, () => { });

                                    if (modd)
                                        menu.AddItem(new GUIContent("Prepare for Copy"), false, () => { ModificatorsPackEditor.PrepareForCopy(modd); });

                                    if (ModificatorsPackEditor.PreparedToCopyModReference != null)
                                    {
                                        InstructionDefinition src = toDraw[i];
                                        menu.AddItem(new GUIContent("Replace with Copy"), false, () =>
                                        {
                                            var duplicate = ModificatorsPackEditor.GetDuplicateOfPreparedToCopy();
                                            FGenerators.AddScriptableTo(duplicate, toDirty, true, true);
                                            src.TargetModification = duplicate;
                                            EditorUtility.SetDirty(toDirty);
                                            AssetDatabase.SaveAssets();
                                        });
                                    }

                                    FGenerators.DropDownMenu(menu);
                                }
                                else
                                    toDraw[i].Foldout = !toDraw[i].Foldout;
                            }
                            else
                                toDraw[i].Foldout = !toDraw[i].Foldout;

                        }
                        GUI.backgroundColor = bgc;

                        toDraw[i].Title = EditorGUILayout.TextField(toDraw[i].Title);
                        toDraw[i].InstructionType = (InstructionDefinition.EInstruction)EditorGUILayout.EnumPopup(toDraw[i].InstructionType);

                        if (moveButtons)
                        {
                            if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp), GUILayout.Width(24))) { var temp = toDraw[i - 1]; toDraw[i - 1] = toDraw[i]; toDraw[i] = temp; }
                            if (i < toDraw.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown), GUILayout.Width(24))) { var temp = toDraw[i + 1]; toDraw[i + 1] = toDraw[i]; toDraw[i] = temp; }
                        }

                        //

                        if (GUILayout.Button("X", GUILayout.Width(24))) { toDraw.RemoveAt(i); EditorUtility.SetDirty(toDirty); break; }

                        EditorGUILayout.EndHorizontal();

                        if (toDraw[i].Foldout)
                        {
                            InstructionDefinitionEditor.DrawGUI(toDraw[i], toDirty);
                        }

                        FGUI_Inspector.DrawUILine(0.3f, 0.6f, 1, 5);

                        //GUILayout.Space(7);

                        //if (toDirty != null) if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(toDirty); }
                    }
                else
                {
                    EditorGUILayout.LabelField("No object in list", EditorStyles.centeredGreyMiniLabel);
                }
            }

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck()) if (toDirty != null) EditorUtility.SetDirty(toDirty);
        }



        public static List<FieldVariable> _CopyVarsFrom = null;

        public static void DrawFieldVariablesList(List<FieldVariable> toDraw, GUIStyle style, string title, ref int selected, ref EDrawVarMode mode, UnityEngine.Object toDirty = null, bool selfInjections = true)
        {
            if (toDraw == null) return;

            Color bgc = GUI.backgroundColor;

            EditorGUI.BeginChangeCheck();

            GUI.color = Color.blue;
            EditorGUILayout.BeginVertical(style);
            GUI.color = bgc;

            EditorGUILayout.BeginHorizontal();
            string fold = (selected != -1) ? "   ▼" : "   ►";

            if (GUILayout.Button(new GUIContent(fold + "  " + title + " (" + toDraw.Count + ")", FGUI_Resources.Tex_Variables), EditorStyles.label, GUILayout.Height(20), GUILayout.Width(208)))
            {
                if (FGenerators.IsRightMouseButton())
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Copy Variables"), false, () => { _CopyVarsFrom = toDraw; });
                    if (_CopyVarsFrom != null && _CopyVarsFrom != toDraw)
                    {
                        menu.AddItem(new GUIContent("Paste Variables"), false, () =>
                        {
                            List<FieldVariable> toPaste = new List<FieldVariable>();
                            for (int i = 0; i < _CopyVarsFrom.Count; i++) toPaste.Add(_CopyVarsFrom[i]);

                            #region Pasting values of variables with the same names

                            for (int i = 0; i < toDraw.Count; i++)
                            {
                                for (int p = 0; p < toPaste.Count; p++)
                                {
                                    bool assigned = false;
                                    if (toDraw[i].Name == toPaste[p].Name)
                                    {
                                        assigned = true;
                                        toDraw[i].SetValue(toPaste[p].GetValue());
                                        break;
                                    }

                                    if (assigned)
                                    {
                                        toPaste.RemoveAt(p);
                                        break;
                                    }
                                }
                            }

                            #endregion

                            for (int p = 0; p < toPaste.Count; p++)
                            {
                                toDraw.Add(new FieldVariable(toPaste[p].Name, toPaste[p].GetValue()));
                            }

                            if (Get) if (Get.projectPreset) EditorUtility.SetDirty(Get.projectPreset);
                        });
                    }

                    menu.ShowAsContext();
                }
                else
                {
                    if (selected != -1) selected = -1; else selected = -2;
                }
            }

            if (selected != -1)
            {
                GUILayout.FlexibleSpace();

                mode = (EDrawVarMode)EditorGUILayout.EnumPopup(mode, GUILayout.Width(80));

                if (GUILayout.Button("+"))
                {
                    toDraw.Add(new FieldVariable("Variable " + toDraw.Count, 1f));
                    if (toDirty != null) EditorUtility.SetDirty(toDirty);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (selected != -1)
            {
                GUILayout.Space(4);

                if (toDraw.Count > 0)
                {
                    for (int i = 0; i < toDraw.Count; i++)
                    {
                        if (toDraw[i] == null) continue;

                        if (mode != EDrawVarMode.All)
                        {
                            if (mode == EDrawVarMode.GameObjects)
                            {
                                if (toDraw[i].ValueType != FieldVariable.EVarType.GameObject) continue;
                            }
                            else if (mode == EDrawVarMode.Materials)
                            {
                                if (toDraw[i].ValueType != FieldVariable.EVarType.Material) continue;
                            }
                            else if (mode == EDrawVarMode.Variables)
                            {
                                if (toDraw[i].ValueType == FieldVariable.EVarType.GameObject || toDraw[i].ValueType == FieldVariable.EVarType.Material) continue;
                            }
                        }

                        var v = toDraw[i];
                        EditorGUILayout.BeginHorizontal();
                        if (selected == i) GUI.backgroundColor = Color.green;
                        if (GUILayout.Button("[" + i + "]", GUILayout.Width(26))) { if (selected == i) selected = -2; else selected = i; }
                        if (selected == i) GUI.backgroundColor = Color.white;
                        GUILayout.Space(6);

                        GUIContent cName = new GUIContent(v.Name);
                        float width = EditorStyles.textField.CalcSize(cName).x + 6;
                        if (width > 220) width = 220;

                        v.Name = EditorGUILayout.TextField(v.Name, GUILayout.Width(width));
                        GUILayout.Space(6);

                        if (v.ValueType == FieldVariable.EVarType.Number)
                        {
                            EditorGUIUtility.labelWidth = 10;

                            if (v.helpForFieldCommand == false)
                            {
                                if (v.FloatSwitch == FieldVariable.EVarFloatingSwitch.Float)
                                {
                                    if (v.helper == Vector3.zero) v.Float = EditorGUILayout.FloatField(" ", v.Float);
                                    else
                                    {
                                        v.Float = FieldVariable.DrawSliderFor(v.Float, v.helper.x, v.helper.y);
                                        //v.Float = EditorGUILayout.Slider(" ", v.Float, v.helper.x, v.helper.y);
                                    }
                                }
                                else
                                {
                                    if (v.helper == Vector3.zero) v.IntV = EditorGUILayout.IntField(" ", v.IntV);
                                    else v.IntV = FieldVariable.DrawSliderForInt(v.Float, v.helper.x, v.helper.y);
                                }
                            }
                            else
                            {
                                v.IntV = EditorGUILayout.IntField(" ", v.IntV);
                            }
                        }
                        else if (v.ValueType == FieldVariable.EVarType.Bool)
                        {
                            EditorGUIUtility.labelWidth = 70;
                            v.SetValue(EditorGUILayout.Toggle("Default:", v.GetBoolValue()));
                        }
                        else if (v.ValueType == FieldVariable.EVarType.Material)
                        {
                            EditorGUIUtility.labelWidth = 70;
                            v.SetValue((Material)EditorGUILayout.ObjectField("Material:", v.GetMaterialRef(), typeof(Material), false));
                        }
                        else if (v.ValueType == FieldVariable.EVarType.GameObject)
                        {
                            EditorGUIUtility.labelWidth = 70;
                            v.SetValue((GameObject)EditorGUILayout.ObjectField("Object:", v.GetGameObjRef(), typeof(GameObject), false));
                        }
                        else if (v.ValueType == FieldVariable.EVarType.Vector3)
                        {
                            EditorGUIUtility.labelWidth = 70;
                            if (v.FloatSwitch == FieldVariable.EVarFloatingSwitch.Float)
                                v.SetValue(EditorGUILayout.Vector3Field("", v.GetVector3Value()));
                            else
                                v.SetValue(EditorGUILayout.Vector3IntField("", v.GetVector3IntValue()));
                        }
                        else if (v.ValueType == FieldVariable.EVarType.Vector2)
                        {
                            EditorGUIUtility.labelWidth = 70;
                            if (v.FloatSwitch == FieldVariable.EVarFloatingSwitch.Float)
                                v.SetValue(EditorGUILayout.Vector2Field("", v.GetVector2Value()));
                            else
                                v.SetValue(EditorGUILayout.Vector2IntField("", v.GetVector2IntValue()));
                        }
                        else if (v.ValueType == FieldVariable.EVarType.ProjectObject)
                        {
                            EditorGUIUtility.labelWidth = 70;
                            v.SetValue(EditorGUILayout.ObjectField("", v.GetUnityObjRef(), typeof(UnityEngine.Object), true));
                        }
                        else if (v.ValueType == FieldVariable.EVarType.String)
                        {
                            EditorGUIUtility.labelWidth = 70;
                            v.SetValue(EditorGUILayout.TextField("", v.GetStringValue()));
                        }

                        EditorGUIUtility.labelWidth = 0;

                        GUILayout.Space(6);
                        if (GUILayout.Button("X", GUILayout.Width(24))) { toDraw.RemoveAt(i); if (toDirty != null) EditorUtility.SetDirty(toDirty); break; }

                        EditorGUILayout.EndHorizontal();

                        if (selected == i)
                        {
                            EditorGUILayout.BeginHorizontal();
                            v.ValueType = (FieldVariable.EVarType)EditorGUILayout.EnumPopup(v.ValueType, GUILayout.Width(66));

                            GUILayout.Width(6);

                            if (v.ValueType == FieldVariable.EVarType.Number || v.ValueType == FieldVariable.EVarType.Vector2 || v.ValueType == FieldVariable.EVarType.Vector3)
                            {
                                v.FloatSwitch = (FieldVariable.EVarFloatingSwitch)EditorGUILayout.EnumPopup(v.FloatSwitch, GUILayout.Width(50));
                                GUILayout.Space(6);
                            }

                            if (v.ValueType == FieldVariable.EVarType.Vector3)
                            {
                                v.displayOnScene = GUILayout.Toggle(v.displayOnScene, new GUIContent(FGUI_Resources.Tex_Gizmos, "Display in scene gizmos when using executor"), GUILayout.Height(17), GUILayout.Width(48));
                                v.allowTransformFollow = GUILayout.Toggle(v.allowTransformFollow, new GUIContent(FGUI_Resources.Tex_Movement, "Allow following transform position"), GUILayout.Height(17), GUILayout.Width(48));
                            }

                            if (v.ValueType == FieldVariable.EVarType.Number)
                                if (v.FloatSwitch == FieldVariable.EVarFloatingSwitch.Int)
                                {
                                    v.helpForFieldCommand = EditorGUILayout.Toggle(v.helpForFieldCommand, GUILayout.Width(26));
                                }

                            if (!v.helpForFieldCommand)
                            {
                                if (v.ValueType == FieldVariable.EVarType.Number)
                                {
                                    EditorGUIUtility.labelWidth = 44;
                                    v.helper.x = EditorGUILayout.FloatField("Min:", v.helper.x);
                                    GUILayout.Space(8);
                                    v.helper.y = EditorGUILayout.FloatField("Max:", v.helper.y);
                                    EditorGUIUtility.labelWidth = 0;
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField(new GUIContent("  Display 'Commands' selector in Executor", PGGUtils.TEX_CellInstr, "Displaying commands selector in the executor component instead of displaying just numbers"));
                            }

                            //else if ( v.ValueType == FieldVariable.EVarType.Bool)
                            //{
                            //    v.SetValue( EditorGUILayout.Toggle("Default:", v.GetBoolValue()));
                            //}

                            EditorGUILayout.EndHorizontal();

                        }

                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No object in list", EditorStyles.centeredGreyMiniLabel);
                }


                GUILayout.Space(5);

                if (selfInjections)
                {
                    //Get.drawSelfInj = EditorGUILayout.Foldout(Get.drawSelfInj, "Self Injections", true);
                    //if (Get.drawSelfInj)
                    //{
                    EditorGUI.indentLevel++;

                    GUILayout.Space(4);
                    EditorGUILayout.PropertyField(Get.so_preset.FindProperty("SelfInjections"));
                    GUILayout.Space(4);

                    EditorGUI.indentLevel--;
                    //}
                }

            }


            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck()) if (toDirty != null) EditorUtility.SetDirty(toDirty);
        }


    }
}