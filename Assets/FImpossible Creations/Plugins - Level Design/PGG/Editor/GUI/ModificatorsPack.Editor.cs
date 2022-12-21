#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Reflection;


namespace FIMSpace.Generating
{
    public static class ModRemoveTools
    {

        #region Menu Items

        [MenuItem("Assets/FImpossible Creations/Remove PGG Scriptable From Asset", true)]
        private static bool RemoveScriptableCheck(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return false;

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                if (Selection.objects[i] == null) continue;
                var o = Selection.objects[i];

                string assetPath = AssetDatabase.GetAssetPath(Selection.objects[i]);
                if (string.IsNullOrEmpty(assetPath)) continue;

                if (o is FieldModification || o is SpawnRuleBase /*|| o is ModificatorsPack*/)
                    if (AssetDatabase.IsSubAsset(o)) { return true; }

            }
            return false;
        }

        [MenuItem("Assets/FImpossible Creations/Remove PGG Scriptable From Asset")]
        private static void RemoveScriptable(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return;

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                if (Selection.objects[i] == null) continue;

                string assetPath = AssetDatabase.GetAssetPath(Selection.objects[i]);
                if (string.IsNullOrEmpty(assetPath)) continue;

                var main = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (main != null)
                {
#if UNITY_2018_4_OR_NEWER
                    AssetDatabase.RemoveObjectFromAsset(Selection.objects[i]);
#endif
                    EditorUtility.SetDirty(main);
                    GameObject.DestroyImmediate(Selection.objects[i]);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        #endregion

    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ModificatorsPack))]
    public class ModificatorsPackEditor : Editor
    {
        public ModificatorsPack Get { get { if (_get == null) _get = (ModificatorsPack)target; return _get; } }
        private ModificatorsPack _get;

        private bool foldout = true;
        private int drawVariables = -1;
        FieldDesignWindow.EDrawVarMode drawVariablesMode = FieldDesignWindow.EDrawVarMode.All;


        SerializedProperty sp_disWh = null;
        SerializedProperty sp_callOnEvery = null;

        bool drawHelpers = false;

        public override void OnInspectorGUI()
        {
            if (Get.CallOnAllMod == null)
            {
                Get.CallOnAllMod = CreateInstance<FieldModification>();
                ModificatorsPackEditor.AddModTo(Get, Get.CallOnAllMod, true);
                Get.FieldModificators.Remove(Get.CallOnAllMod);
                Get.CallOnAllMod.hideFlags = HideFlags.HideInHierarchy;
            }


            bool preE = GUI.enabled;

            serializedObject.Update();

            if (Get.ModPackType != ModificatorsPack.EModPackType.Base)
            {
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                if (Get.ModPackType == ModificatorsPack.EModPackType.ShallowCopy)
                    EditorGUILayout.LabelField(new GUIContent("This is Shallow Copy Package (i)", "It's shallow copy - means all modificators are references from other package or project files, deleting them from list will not remove them from game project files."), FGUI_Resources.HeaderStyle);
                else if (Get.ModPackType == ModificatorsPack.EModPackType.VariantCopy)
                    EditorGUILayout.LabelField(new GUIContent("This is Variant Copy Package (i)", "It's variant copy - means all modificators are variants of root package field modificators."), FGUI_Resources.HeaderStyle);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();
            if (sp_disWh == null) sp_disWh = serializedObject.FindProperty("DisableWholePackage");
            EditorGUILayout.PropertyField(sp_disWh);

            SerializedProperty sp = sp_disWh.Copy(); sp.Next(false);
            EditorGUIUtility.labelWidth = 72;
            EditorGUILayout.PropertyField(sp);

            if (Get.SeedMode == ModificatorsPack.ESeedMode.Custom)
            {
                EditorGUIUtility.labelWidth = 44;
                EditorGUIUtility.fieldWidth = 16;
                sp.Next(false);
                GUILayout.Space(4);
                EditorGUILayout.PropertyField(sp, new GUIContent("Seed", sp.tooltip));
                EditorGUIUtility.fieldWidth = 0;
            }
            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), GUIContent.none);
            GUI.enabled = preE;
            if (GUILayout.Button("Select Asset")) EditorGUIUtility.PingObject(Get);

            if (GUILayout.Button("Clone"))
            {
                string path = AssetDatabase.GetAssetPath(Get);
                string noExt = path.Replace(".asset", "");
                noExt += "(Clone).asset";
                ModificatorsPackEditor.ExportAsPack(Get.FieldModificators, noExt);

            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            ModificatorsPackEditor.DrawFieldModList(Get, Get.name + " Modificators", ref foldout, Get);


            GUILayout.Space(6);

            FGUI_Inspector.FoldHeaderStart(ref drawHelpers, "File Helpers", FGUI_Resources.BGInBoxStyle);

            if (drawHelpers)
            {
                if (GUILayout.Button("Find and clear unused")) ModificatorsPackEditor.CleanLeftowers(Get);

                if (AssetDatabase.IsSubAsset(Get))
                {
                    if (GUILayout.Button("Switch pack visibility for search"))
                    {
                        if (Get.hideFlags == HideFlags.HideInHierarchy)
                            Get.hideFlags = HideFlags.None;
                        else
                            Get.hideFlags = HideFlags.HideInHierarchy;

                        EditorUtility.SetDirty(Get);
                    }
                }

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(new GUIContent("Hide Mods for Search", "If you want all modificators within pack to be INVISIBLE through Unity's search file window")))
                {
                    for (int i = 0; i < Get.FieldModificators.Count; i++)
                    {
                        if (Get.FieldModificators[i] == null) continue;
                        Get.FieldModificators[i].hideFlags = HideFlags.HideInHierarchy;
                        EditorUtility.SetDirty(Get.FieldModificators[i]);
                    }

                    AssetDatabase.SaveAssets();
                }

                if (GUILayout.Button(new GUIContent("Expose Mods for Search", "If you want all modificators within pack to be VISIBLE through Unity's search file window")))
                {
                    for (int i = 0; i < Get.FieldModificators.Count; i++)
                    {
                        if (Get.FieldModificators[i] == null) continue;
                        Get.FieldModificators[i].hideFlags = HideFlags.None;
                        EditorUtility.SetDirty(Get.FieldModificators[i]);
                    }

                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(new GUIContent("Hide Rules for Search", "If you want all modificators within pack to be INVISIBLE through Unity's search file window")))
                {
                    for (int i = 0; i < Get.FieldModificators.Count; i++)
                    {
                        if (Get.FieldModificators[i] == null) continue;

                        for (int s = 0; s < Get.FieldModificators[i].Spawners.Count; s++)
                        {
                            for (int r = 0; r < Get.FieldModificators[i].Spawners[s].Rules.Count; r++)
                            {
                                var rl = Get.FieldModificators[i].Spawners[s].Rules[r];
                                rl.hideFlags = HideFlags.HideInHierarchy;
                                EditorUtility.SetDirty(rl);
                            }
                        }

                        for (int s = 0; s < Get.FieldModificators[i].SubSpawners.Count; s++)
                        {
                            for (int r = 0; r < Get.FieldModificators[i].SubSpawners[s].Rules.Count; r++)
                            {
                                var rl = Get.FieldModificators[i].SubSpawners[s].Rules[r];
                                rl.hideFlags = HideFlags.HideInHierarchy;
                                EditorUtility.SetDirty(rl);
                            }
                        }

                        EditorUtility.SetDirty(Get.FieldModificators[i]);
                    }

                    AssetDatabase.SaveAssets();
                }

                if (GUILayout.Button(new GUIContent("Expose Rules for Search", "If you want all modificators within pack to be VISIBLE through Unity's search file window")))
                {
                    for (int i = 0; i < Get.FieldModificators.Count; i++)
                    {
                        if (Get.FieldModificators[i] == null) continue;

                        for (int s = 0; s < Get.FieldModificators[i].Spawners.Count; s++)
                        {
                            for (int r = 0; r < Get.FieldModificators[i].Spawners[s].Rules.Count; r++)
                            {
                                var rl = Get.FieldModificators[i].Spawners[s].Rules[r];
                                rl.hideFlags = HideFlags.None;
                                EditorUtility.SetDirty(rl);
                            }
                        }

                        for (int s = 0; s < Get.FieldModificators[i].SubSpawners.Count; s++)
                        {
                            for (int r = 0; r < Get.FieldModificators[i].SubSpawners[s].Rules.Count; r++)
                            {
                                var rl = Get.FieldModificators[i].SubSpawners[s].Rules[r];
                                rl.hideFlags = HideFlags.None;
                                EditorUtility.SetDirty(rl);
                            }
                        }

                        EditorUtility.SetDirty(Get.FieldModificators[i]);
                    }

                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();


            GUILayout.Space(9);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TagForAllSpawners"));
            SerializedProperty sp_comb = serializedObject.FindProperty("CombineSpawns");
            EditorGUILayout.PropertyField(sp_comb);
            //sp_comb.Next(false);
            //if ( Get.CombineSpawns != ModificatorsPack.EPackCombine.None) EditorGUILayout.PropertyField(sp_comb);

            GUILayout.Space(9);
            string callNumbr = "";
            if (Get.CallOnAllMod != null) if (Get.CallOnAllSpawners != null) if (Get.CallOnAllSpawners.Rules != null) if (Get.CallOnAllSpawners.Rules.Count > 0) callNumbr = " (" + Get.CallOnAllSpawners.Rules.Count + ")";
            FGUI_Inspector.FoldHeaderStart(ref Get._EditorDisplayCallOnAll, "Call Logics On Every Spawner (BETA)" + callNumbr, FGUI_Resources.BGInBoxStyle);
            GUILayout.Space(2);

            if (Get._EditorDisplayCallOnAll)
            {
                GUILayout.Space(3);
                if (sp_callOnEvery == null) sp_callOnEvery = serializedObject.FindProperty("CallOnAllSpawners");
                if (FGenerators.CheckIfIsNull(Get.CallOnAllSpawners)) Get.CallOnAllSpawners = new FieldSpawner(-1, FieldModification.EModificationMode.CustomPrefabs, Get.CallOnAllMod);
                Get.CallOnAllSpawners.Enabled = true;
                Get.CallOnAllSpawners._EditorDisplaySpawnerHeader = false;
                Get.CallOnAllSpawners.Parent = Get.CallOnAllMod;

                if (Get.CallOnAllSpawners.Rules != null)
                    for (int i = 0; i < Get.CallOnAllSpawners.Rules.Count; i++)
                        Get.CallOnAllSpawners.Rules[i].DisableDrawingGlobalSwitch = true;

                Get.CallOnAllSpawners.DrawSpawnerGUI(sp_callOnEvery, new GUIContent[0], new int[0], false);
                GUILayout.Space(3);
            }


            EditorGUILayout.EndVertical();


            GUILayout.Space(7);
            FieldDesignWindow.DrawFieldVariablesList(Get.Variables, FGUI_Resources.BGInBoxStyle, "Pack's Variables", ref drawVariables, ref drawVariablesMode, Get, false);


            serializedObject.ApplyModifiedProperties();

        }



        #region Methods


        // Class contains mostly static methods to provide advances GUI & Database handling for prest files -------------------

        /// <summary>
        /// Making copy of 'Field Modificators' and 'Field Modificator Rules' and placing them in one 'ModificatorsPack" file
        /// </summary>
        public static ModificatorsPack ExportAsPack(List<FieldModification> fieldMods, string customPath = "")
        {
            ModificatorsPack pack;

            if (customPath == "")
                pack = (ModificatorsPack)FGenerators.GenerateScriptable(ScriptableObject.CreateInstance<ModificatorsPack>(), "ModPack_");
            else
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ModificatorsPack>(), customPath);
                pack = AssetDatabase.LoadAssetAtPath<ModificatorsPack>(customPath);
            }

            if (pack == null) return null;
            AssetDatabase.SaveAssets();

            if (AssetDatabase.Contains(pack) == false) return null;

            if (fieldMods != null) // If there is field mod data provided we copy it
            {
                for (int rm = 0; rm < fieldMods.Count; rm++) // Checkin and copying all provided field modificators
                {
                    if (fieldMods[rm] == null) continue;

                    FieldModification mod = GameObject.Instantiate(fieldMods[rm]); // Creating copy
                    AddModTo(pack, mod, false); // Adding to target pack
                    mod.ParentPack = pack;

                    CopyRulesTo(fieldMods[rm], mod, false); // And copying mod's rules and applying to copy of field modificator
                }

                AssetDatabase.SaveAssets();
            }

            return pack;
        }

        /// <summary>
        /// Making copy of Package without copying field mods and rules, just references
        /// </summary>
        public static ModificatorsPack ExportAsShallow(List<FieldModification> fieldMods, string customPath = "")
        {
            ModificatorsPack pack;

            if (customPath == "")
                pack = (ModificatorsPack)FGenerators.GenerateScriptable(ScriptableObject.CreateInstance<ModificatorsPack>(), "ModPack_");
            else
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ModificatorsPack>(), customPath);
                pack = AssetDatabase.LoadAssetAtPath<ModificatorsPack>(customPath);
            }

            if (pack == null) return null;
            AssetDatabase.SaveAssets();

            if (AssetDatabase.Contains(pack) == false) return null;

            pack.ModPackType = ModificatorsPack.EModPackType.ShallowCopy;
            for (int i = 0; i < fieldMods.Count; i++) pack.FieldModificators.Add(fieldMods[i]);

            EditorUtility.SetDirty(pack);
            AssetDatabase.SaveAssets();

            return pack;
        }

        /// <summary>
        /// Making copy of Package without copying field mods and rules, just references
        /// </summary>
        public static ModificatorsPack ExportVariantPack(List<FieldModification> fieldMods, string customPath = "")
        {
            ModificatorsPack pack;

            if (customPath == "")
                pack = (ModificatorsPack)FGenerators.GenerateScriptable(ScriptableObject.CreateInstance<ModificatorsPack>(), "ModPack_");
            else
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ModificatorsPack>(), customPath);
                pack = AssetDatabase.LoadAssetAtPath<ModificatorsPack>(customPath);
            }


            if (pack == null) return null;
            AssetDatabase.SaveAssets();

            if (AssetDatabase.Contains(pack) == false) return null;

            pack.ModPackType = ModificatorsPack.EModPackType.VariantCopy;
            for (int i = 0; i < fieldMods.Count; i++)
            {
                FieldModification newR = (FieldModification)GameObject.Instantiate(fieldMods[i]);
                newR.name = fieldMods[i].name + "-Variant";
                newR.VariantOf = fieldMods[i];
                AddModTo(pack, newR, false);
            }

            EditorUtility.SetDirty(pack);
            AssetDatabase.SaveAssets();

            return pack;
        }

        public static T DrawNewScriptableCreateButton<T>(string exmapleName = "FieldMod_", string tooltip = "Generating Field Setup preset file in choosed project directory", T source = null) where T : ScriptableObject
        {
            if (GUILayout.Button(new GUIContent("New", tooltip), GUILayout.Width(40)))
            {
                T src; if (source == null) src = ScriptableObject.CreateInstance<T>(); else src = GameObject.Instantiate(source);
                T temp = (T)FGenerators.GenerateScriptable(src, exmapleName);
                AssetDatabase.SaveAssets();
                if (temp != null) if (AssetDatabase.Contains(temp)) return temp;
            }

            return null;
        }


        public static void MergeOnto(ModificatorsPack from, ModificatorsPack to)
        {
            for (int i = 0; i < from.FieldModificators.Count; i++)
            {
                if (from.FieldModificators[i] == null) continue;
                MergeOnto(from.FieldModificators[i], to);
            }
        }


        public static void MergeOnto(FieldModification from, ModificatorsPack to)
        {
            if (from == null) return;
            if (to == null) return;
            if (FGenerators.IsAssetSaved(from) == false) return;
            if (FGenerators.IsAssetSaved(to) == false) return;

            FieldModification clone = GameObject.Instantiate(from);
            AddModTo(to, clone, true);
            CopyRulesTo(from, clone, true);
        }


        public static void RemoveModFrom(ModificatorsPack pack, FieldModification toRemove, bool delete)
        {
            try
            {
                if (delete) DestroyRules(toRemove);
                if (pack) pack.FieldModificators.Remove(toRemove);
                if (toRemove.ParentPreset) if (toRemove.ParentPreset.UtilityModificators.Contains(toRemove)) toRemove.ParentPreset.UtilityModificators.Remove(toRemove);
#if UNITY_2018_4_OR_NEWER
                AssetDatabase.RemoveObjectFromAsset(toRemove);
#endif
                if (delete) GameObject.DestroyImmediate(toRemove, true);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw;
            }
        }


        public static void AddModTo(ModificatorsPack pack, FieldModification toAdd, bool reload = true)
        {
            if (pack.FieldModificators != null) if (pack.FieldModificators.Contains(toAdd)) return;

            try
            {
                AssetDatabase.AddObjectToAsset(toAdd, pack);

                toAdd.ParentPreset = pack.ParentPreset;
                toAdd.ParentPack = pack;

                pack.FieldModificators.Add(toAdd);

                if (reload) AssetDatabase.SaveAssets();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw;
            }
        }


        /// <summary>
        /// Using instantiated modification (must be contained by database) and source modification to create copy of all rules and place them inside it
        /// </summary>
        internal static void CopyRulesTo(FieldModification sourceMod, FieldModification instaniatedMod, bool reload = true)
        {
            if (sourceMod == null) return;
            if (instaniatedMod == null) return;
            if (!FGenerators.IsAssetSaved(instaniatedMod)) return;

            // All spawners rules copying
            for (int s = 0; s < sourceMod.Spawners.Count; s++)
            {
                var sourceSpawner = sourceMod.Spawners[s];
                if (sourceSpawner == null) continue;

                instaniatedMod.Spawners[s].Parent = instaniatedMod;

                for (int r = 0; r < sourceSpawner.Rules.Count; r++)
                {
                    if (sourceSpawner.Rules[r] == null) continue;

                    SpawnRuleBase ruleCopy = GameObject.Instantiate(sourceSpawner.Rules[r]);
                    ruleCopy.name = sourceSpawner.Rules[r].name;
                    ruleCopy.OwnerSpawner = instaniatedMod.Spawners[s];

                    instaniatedMod.Spawners[s].Rules[r] = ruleCopy;
                    FGenerators.AddScriptableToSimple(instaniatedMod, ruleCopy, false);
                }
            }


            for (int s = 0; s < sourceMod.SubSpawners.Count; s++)
            {
                var sourceSpawner = sourceMod.SubSpawners[s];
                if (sourceSpawner == null) continue;

                instaniatedMod.SubSpawners[s].Parent = instaniatedMod;

                for (int r = 0; r < sourceSpawner.Rules.Count; r++)
                {
                    if (sourceSpawner.Rules[r] == null) continue;

                    SpawnRuleBase ruleCopy = GameObject.Instantiate(sourceSpawner.Rules[r]);
                    ruleCopy.name = sourceSpawner.Rules[r].name;
                    ruleCopy.OwnerSpawner = instaniatedMod.SubSpawners[s];

                    instaniatedMod.SubSpawners[s].Rules[r] = ruleCopy;
                    FGenerators.AddScriptableToSimple(instaniatedMod, ruleCopy, false);
                }
            }


            if (reload) AssetDatabase.SaveAssets();
        }



        public static void DestroyRules(FieldModification target)
        {
            if (!FGenerators.IsAssetSaved(target)) return;

            for (int s = 0; s < target.Spawners.Count; s++)
            {
                var modSpawner = target.Spawners[s]; // Serialized spawner contains list of rules to remove

                for (int r = 0; r < modSpawner.Rules.Count; r++)
                {
                    if (modSpawner.Rules[r] == null) continue;

                    GameObject.DestroyImmediate(modSpawner.Rules[r], true);
                }
            }

            for (int s = 0; s < target.SubSpawners.Count; s++)
            {
                var modSpawner = target.SubSpawners[s]; // Serialized spawner contains list of rules to remove

                for (int r = 0; r < modSpawner.Rules.Count; r++)
                {
                    if (modSpawner.Rules[r] == null) continue;

                    GameObject.DestroyImmediate(modSpawner.Rules[r], true);
                }
            }

            AssetDatabase.SaveAssets();
        }

        static GUIContent _widthTester = null;

        /// <summary>
        /// Drawing modificator pack with additional features
        /// </summary>
        public static void DrawFieldModList(ModificatorsPack toDraw, string title, ref bool foldout, UnityEngine.Object toDirty = null, bool drawLock = false, FieldSetup useOnField = null, bool simpleMode = false)
        {
            if (toDraw.FieldModificators == null) toDraw.FieldModificators = new List<FieldModification>();
            Color preC = GUI.color;
            Color prebC = GUI.backgroundColor;

            GUI.color = Color.blue;
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            GUI.color = preC;

            if (_widthTester == null) _widthTester = new GUIContent();

            #region Header

            EditorGUILayout.BeginHorizontal();

            string fold = foldout ? "  ▼" : "  ►";

            if (!simpleMode)
            {
                if (GUILayout.Button(new GUIContent(fold + "  " + title + " (" + toDraw.FieldModificators.Count + ")", PGGUtils._Tex_ModsSmall), EditorStyles.label, GUILayout.Height(20))) foldout = !foldout;
            }
            else
                if (GUILayout.Button(new GUIContent(fold + "  " + title + " (" + toDraw.FieldModificators.Count + ")", PGGUtils._Tex_ModsSmall), EditorStyles.label, GUILayout.Height(20))) foldout = !foldout;

            if (foldout)
            {
                GUILayout.FlexibleSpace();

                if (!simpleMode)
                {
                    if (GUILayout.Button(FGUI_Resources.GUIC_More, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(18)))
                    {

                        GenericMenu draftsMenu = new GenericMenu();

                        //GUI.backgroundColor = prebC;
                        //GUI.backgroundColor = new Color(0.735f, 0.735f, 1f, 1f);
                        //if (GUILayout.Button(new GUIContent("M", "Merge copy of other package contests into this package"))) { MergeOntoDialog(toDraw); }
                        draftsMenu.AddItem(new GUIContent("Merge copy of other package contests into this package"), false, () =>
                        {
                            MergeOntoDialog(toDraw);
                        });

                        //if (GUILayout.Button(new GUIContent("Copy", "Export copy of this package as project file"))) ModificatorsPackEditor.ExportAsPack(toDraw.FieldModificators);
                        draftsMenu.AddItem(new GUIContent("Export copy of this package as project file"), false, () =>
                        {
                            ModificatorsPackEditor.ExportAsPack(toDraw.FieldModificators);
                        });

                        //if (GUILayout.Button(new GUIContent("Es", "Export shallow copy of this package as project file, shallow means all modificators within pack will not be copies, only references will remain, helpful when you want mod pack with all modificators except one or so"))) ModificatorsPackEditor.ExportAsShallow(toDraw.FieldModificators);

                        //if (GUILayout.Button(new GUIContent("Ev", "Export copy of this package as project file with all modificators as variants"))) ModificatorsPackEditor.ExportVariantPack(toDraw.FieldModificators);
                        draftsMenu.AddItem(new GUIContent("Export copy of this package as project file with all modificators as variants"), false, () =>
                        {
                            ModificatorsPackEditor.ExportVariantPack(toDraw.FieldModificators);
                        });

                        draftsMenu.ShowAsContext();
                    }



                    if (GUILayout.Button(new GUIContent("+[ ]", "Add new null Modificator field to package contests: can be filled by any other field modificator in project (including ones inside other FieldSetups)")))
                    {
                        toDraw.FieldModificators.Add(null);
                        EditorUtility.SetDirty(toDraw);
                    }
                }

                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 1f);
                if (GUILayout.Button(new GUIContent("+", "Generating modificator inside field setup file (or in modificators pack)")))
                {
                    var temp = ScriptableObject.CreateInstance<FieldModification>();
                    temp.name = "New Modificator";
                    temp.ParentPack = toDraw;
                    temp.ParentPreset = toDraw.ParentPreset;
                    FGenerators.AddScriptableTo(temp, toDraw, false, true);
                    toDraw.FieldModificators.Add(temp);
                    EditorUtility.SetDirty(toDraw);
                    AssetDatabase.SaveAssets();
                }


                GUI.backgroundColor = prebC;
            }

            EditorGUILayout.EndHorizontal();

            #endregion

            // Folded out
            if (foldout)
            {
                GUILayout.Space(6);

                if (toDraw.FieldModificators.Count > 0) // Drawing modificators list
                {
                    for (int i = 0; i < toDraw.FieldModificators.Count; i++) // For each modification to draw
                    {
                        bool isNull = toDraw.FieldModificators[i] == null;

                        // Preparing and coloring if mod disabled 
                        Color preCol = GUI.color;

                        // Grey out if mod is not enabled
                        if (!isNull)
                        {
                            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(toDraw)) == false)
                                if (AssetDatabase.GetAssetPath(toDraw.FieldModificators[i]) == AssetDatabase.GetAssetPath(toDraw))
                                    toDraw.FieldModificators[i].ParentPack = toDraw;

                            if (toDraw.FieldModificators[i].Enabled == false) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                        }


                        // Starting horizontal mod field
                        EditorGUILayout.BeginHorizontal();


                        #region Toggle or Label

                        if (!isNull)
                        {
                            EditorGUI.BeginChangeCheck();

                            if (useOnField == null)
                            {
                                toDraw.FieldModificators[i].Enabled = EditorGUILayout.Toggle(toDraw.FieldModificators[i].Enabled, GUILayout.Width(16));
                            }
                            else
                            {
                                bool isEnabled = useOnField.IsEnabled(toDraw.FieldModificators[i]);
                                bool preEn = isEnabled;

                                isEnabled = EditorGUILayout.Toggle(isEnabled, GUILayout.Width(16));

                                if (isEnabled != preEn)
                                {
                                    if (!isEnabled)
                                        useOnField.AddToDisabled(toDraw.FieldModificators[i]);
                                    else
                                        useOnField.RemoveFromDisabled(toDraw.FieldModificators[i]);
                                }

                                if (isEnabled == false) GUI.color = new Color(1f, 1f, 1f, 0.75f);
                            }

                            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(toDraw.FieldModificators[i]);
                        }

                        // Label Text
                        GUIContent lbl = new GUIContent("[" + i + "]");
                        float wdth = EditorStyles.label.CalcSize(lbl).x;

                        if (toDraw.FieldModificators[i] != null)
                        {
                            if (SeparatedModWindow.Get != null)
                            {
                                if (SeparatedModWindow.Get.latestMod == toDraw.FieldModificators[i])
                                    GUI.backgroundColor = Color.green;
                            }
                            else
                            if (Selection.objects.Contains(toDraw.FieldModificators[i]))
                                GUI.backgroundColor = Color.green;

                            if (GUILayout.Button(lbl, GUILayout.Width(wdth + 8)))
                            {
                                bool rmb = false;
                                if (FGenerators.IsRightMouseButton())
                                {
                                    GenericMenu menu = new GenericMenu();
                                    FieldModification modd = toDraw.FieldModificators[i];
                                    menu.AddItem(new GUIContent("Duplicate Mod"), false, () => { DuplicateModificator(useOnField, modd); });
                                    menu.AddItem(new GUIContent("Export Copy"), false, () => { ExportCopyPopup(modd); });
                                    menu.AddItem(new GUIContent("Export Variant"), false, () => { ExportVariantPopup(modd); });

                                    menu.AddItem(new GUIContent(""), false, () => { });
                                    menu.AddItem(new GUIContent("Prepare for Copy"), false, () => { PrepareForCopy(modd); });
                                    menu.AddItem(new GUIContent(""), false, () => { });
                                    menu.AddItem(new GUIContent("Switch Asset Visibility"), false, () => { if (modd.hideFlags == HideFlags.None) modd.hideFlags = HideFlags.HideInHierarchy; else modd.hideFlags = HideFlags.None; EditorUtility.SetDirty(modd); AssetDatabase.SaveAssets(); });

                                    if (toDraw.ParentPreset)
                                    {
                                        if (modd.ParentPreset == null || modd.ParentPreset == toDraw.ParentPreset)
                                            menu.AddItem(new GUIContent("Move Modificator to Utility List"), false, () => { toDraw.ParentPreset.MoveModificatorToUtilityList(modd); });
                                    }

                                    if (PreparedToCopyModReference != null)
                                    {
                                        menu.AddItem(new GUIContent("Paste Prepared Copy as New"), false, () => { AddDuplicateOfPreparedToCopy(modd); });
                                    }

                                    FGenerators.DropDownMenu(menu);
                                    rmb = true;
                                }

                                if (!rmb)
                                {
                                    // Locking project browser for a second to not disturb project browser directory workflow
                                    if (ModificatorsPack._Editor_LockBrowser) LockBrowserWindow(true);

                                    //var preSel = Selection.activeObject;
                                    //if (SeparatedModWindow.Get != null)
                                    {
                                        SeparatedModWindow.SelectMod(toDraw.FieldModificators[i]);
                                    }
                                    //else
                                    //    Selection.activeObject = (toDraw.FieldModificators[i]);
                                }

                                //AssetDatabase.OpenAsset(toDraw.FieldModificators[i]);
                                //EditorGUIUtility.PingObject(dir);

                            }

                            GUI.backgroundColor = preCol;
                        }
                        else
                        {
                            EditorGUILayout.LabelField(lbl, GUILayout.Width(wdth + 2));
                        }

                        #endregion


                        EditorGUI.BeginChangeCheck();


                        if (SeparatedModWindow.Get)
                            if (toDraw.FieldModificators[i] == SeparatedModWindow.Get.latestMod)
                                GUI.backgroundColor = Color.green;

                        // Asset Field
                        if (toDraw.FieldModificators[i] != null)
                        {
                            _widthTester.text = toDraw.FieldModificators[i].name;
                            float width = 18;

                            if (GUI.GetNameOfFocusedControl() == "_PGG" + i)
                                width = Mathf.Min(200f, EditorStyles.textField.CalcSize(_widthTester).x + 2);

                            GUI.SetNextControlName("_PGG" + i);
                            string newName = toDraw.FieldModificators[i].name = EditorGUILayout.TextField(toDraw.FieldModificators[i].name, GUILayout.MaxWidth(width));
                            toDraw.FieldModificators[i] = (FieldModification)EditorGUILayout.ObjectField(toDraw.FieldModificators[i], typeof(FieldModification), false, GUILayout.MinWidth(100));

                            if (newName != toDraw.FieldModificators[i].name)
                            {
                                toDraw.FieldModificators[i].name = newName;
                                EditorUtility.SetDirty(toDraw.FieldModificators[i]);
                            }
                        }
                        else
                            toDraw.FieldModificators[i] = (FieldModification)EditorGUILayout.ObjectField(toDraw.FieldModificators[i], typeof(FieldModification), false);

                        GUI.backgroundColor = prebC;

                        #region Buttons

                        FieldModification nMod = toDraw.FieldModificators[i];
                        DrawModUtilitiesButtons(ref nMod, toDraw, i == 0);
                        toDraw.FieldModificators[i] = nMod;

                        // Move up and down buttons -----------------------------
                        if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp, "Move this element to be executed before above one"), GUILayout.Width(24))) { FieldModification temp = toDraw.FieldModificators[i - 1]; toDraw.FieldModificators[i - 1] = toDraw.FieldModificators[i]; toDraw.FieldModificators[i] = temp; }
                        if (i < toDraw.FieldModificators.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown, "Move this element to be executed after below one"), GUILayout.Width(24))) { FieldModification temp = toDraw.FieldModificators[i + 1]; toDraw.FieldModificators[i + 1] = toDraw.FieldModificators[i]; toDraw.FieldModificators[i] = temp; }


                        // Removing from list buttons -----------------------------
                        if (toDraw.FieldModificators[i] == null)
                        {
                            if (GUILayout.Button(new GUIContent("X", "Removing null preset from list"), GUILayout.Width(24)))
                            {
                                toDraw.FieldModificators.RemoveAt(i);
                                EditorUtility.SetDirty(toDraw);
                                AssetDatabase.SaveAssets();
                                break;
                            }
                        }
                        else
                        {
                            if (toDraw.FieldModificators[i] != null)
                            {

                                // If this Field mod is sub asset we remove it pernamentaly
                                bool remove = false;
                                if (FGenerators.AssetContainsAsset(toDraw.FieldModificators[i], toDraw)) remove = true;
                                else
                                if (toDraw.ParentPreset != null)
                                    if (FGenerators.AssetContainsAsset(toDraw.ParentPreset, toDraw))
                                        remove = true;

                                if (remove)
                                {
                                    GUI.backgroundColor = new Color(1f, 0.635f, 0.635f, 1f);

                                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove, "This modificator is sub-asset of package, clicking on this button will remove modification pernamently"), new GUILayoutOption[] { GUILayout.Height(20), GUILayout.Width(24) }))
                                    {
                                        DestroyRules(toDraw.FieldModificators[i]);
                                        GameObject.DestroyImmediate(toDraw.FieldModificators[i], true);
                                        toDraw.FieldModificators.RemoveAt(i);
                                        EditorUtility.SetDirty(toDraw);
                                        AssetDatabase.SaveAssets();
                                        break;
                                    }

                                    GUI.backgroundColor = prebC;
                                }
                                else // If this Field mod is not sub asset but project file we just remove it from list
                                {
                                    bool infoed = false;
                                    if (toDraw != null) if (toDraw.FieldModificators[i]) if (toDraw.FieldModificators[i].hideFlags != HideFlags.None)
                                            {
                                                if (GUILayout.Button(new GUIContent("?", "This asset is hidden, can't be removed for safety reasons"), GUILayout.Width(20))) EditorUtility.DisplayDialog("It's Hidden Asset", "This asset is hidden, can't be removed for safety reasons.\nYou can unhide it by hitting 'Expose Mods for Search' button inside the ModificatorsPack", "Ok");
                                                infoed = true;
                                            }

                                    if (!infoed)
                                        if (GUILayout.Button(new GUIContent("X", "Removing Mod Preset from list (file will remain inside project)"), GUILayout.Width(24)))
                                        {
                                            toDraw.FieldModificators.RemoveAt(i);
                                            EditorUtility.SetDirty(toDraw);
                                            AssetDatabase.SaveAssets();
                                            break;
                                        }
                                }

                            }
                        }


                        #endregion


                        // Ending one drawed mod horizontal GUI
                        EditorGUILayout.EndHorizontal();


                        #region Editing Finishing

                        // Marking mod as edited
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (toDirty != null)
                            {
                                Undo.RecordObject(toDirty, "Editing " + i + " Element");
                                EditorUtility.SetDirty(toDirty);
                                //if (toDirty != null) EditorUtility.SetDirty(toDirty);
                            }
                        }


                        // Restoring
                        //if (toDraw.FieldModificators[i] != null)
                        //    if (toDraw.FieldModificators[i].Enabled == false)
                        GUI.color = preCol;

                        #endregion

                    }
                }
                else // Zero modifications
                {
                    if (simpleMode)
                    {
                        GUILayout.Space(4);

                        GUI.backgroundColor = Color.green;
                        if (GUILayout.Button("ADD first set of objects to spawn on the grid"))
                        {
                            var temp = ScriptableObject.CreateInstance<FieldModification>();
                            temp.name = "New Modificator";
                            temp.ParentPack = toDraw;
                            temp.ParentPreset = toDraw.ParentPreset;
                            FGenerators.AddScriptableTo(temp, toDraw, false, true);
                            toDraw.FieldModificators.Add(temp);
                            EditorUtility.SetDirty(toDraw);
                            AssetDatabase.SaveAssets();

                            SeparatedModWindow.SelectMod(temp);
                        }
                        GUI.backgroundColor = prebC;

                        EditorGUILayout.LabelField("This set of objects is called\n'Field Modificator'", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(30));

                        GUILayout.Space(4);

                    }
                    else
                        EditorGUILayout.LabelField("Add here field modificators", EditorStyles.centeredGreyMiniLabel);

                }

                if (simpleMode)
                {
                    if (toDraw.FieldModificators.Count > 1)
                        EditorGUILayout.LabelField("The order of Field Modificators matters!", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(20));
                }

            }

            EditorGUILayout.EndVertical();
        }


        static GUIContent _utilIcon = null;
        public static GUIContent _UtilIcon
        { get { if (_utilIcon == null) { _utilIcon = new GUIContent(EditorGUIUtility.IconContent("VerticalLayoutGroup Icon").image, "Click to show menu with more options."); } return _utilIcon; } }

        internal static void DrawUtilityModsList(List<FieldModification> toDraw, ref bool foldout, string title, FieldSetup setup)
        {
            if (toDraw == null) return;
            if (toDraw.Count == 0) return;

            Color preC = GUI.color;
            Color prebC = GUI.backgroundColor;

            GUI.color = Color.yellow;
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            GUI.color = preC;

            #region Header

            EditorGUILayout.BeginHorizontal();

            string fold = foldout ? " ▼" : " ►";

            if (GUILayout.Button(fold + "  " + title + " (" + toDraw.Count + ")", EditorStyles.label, GUILayout.Width(230))) foldout = !foldout;

            if (foldout)
            {
                GUILayout.FlexibleSpace();

                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 1f);
                if (GUILayout.Button(new GUIContent("+", "Generating modificator inside field setup file (or in modificators pack)")))
                {
                    var temp = ScriptableObject.CreateInstance<FieldModification>();
                    temp.name = "New Modificator";
                    temp.ParentPack = null;
                    temp.ParentPreset = setup;
                    FGenerators.AddScriptableTo(temp, setup, false, true);
                    toDraw.Add(temp);
                    EditorUtility.SetDirty(setup);
                    AssetDatabase.SaveAssets();
                }


                GUI.backgroundColor = prebC;
            }

            EditorGUILayout.EndHorizontal();

            #endregion

            // Folded out
            if (foldout)
            {
                GUILayout.Space(4);
                EditorGUILayout.HelpBox("Additional Modificators for use with Cells Instructions Commands.", MessageType.None);
                GUILayout.Space(4);

                for (int i = 0; i < toDraw.Count; i++) // For each modification to draw
                {
                    bool isNull = toDraw[i] == null;

                    // Preparing and coloring if mod disabled 
                    Color preCol = GUI.color;

                    // Grey out if mod is not enabled
                    if (!isNull)
                    {
                        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(setup)) == false)
                            if (AssetDatabase.GetAssetPath(toDraw[i]) == AssetDatabase.GetAssetPath(setup))
                                toDraw[i].ParentPreset = setup;

                        if (toDraw[i].Enabled == false) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }


                    // Starting horizontal mod field
                    EditorGUILayout.BeginHorizontal();


                    #region Toggle or Label

                    if (!isNull)
                    {
                        toDraw[i].Enabled = true;
                    }

                    bool isSelct = false;

                    if (SeparatedModWindow.Get)
                        if (toDraw[i] == SeparatedModWindow.Get.latestMod)
                        {
                            isSelct = true;
                            GUI.backgroundColor = Color.green;
                        }

                    // Label Text
                    GUIContent lbl = new GUIContent(FGUI_Resources.TexTargetingIcon, "Select modificator to display it's settings");
                    float wdth = 20;// EditorStyles.label.CalcSize(lbl).x;

                    if (toDraw[i] != null)
                    {

                        if (GUILayout.Button(_UtilIcon, EditorStyles.label, GUILayout.Width(18), GUILayout.Height(wdth)))
                        {
                            GenericMenu menu = new GenericMenu();
                            FieldModification modd = toDraw[i];
                            menu.AddItem(new GUIContent("Duplicate Mod"), false, () => { DuplicateModificator(setup, modd, true); });
                            menu.AddItem(new GUIContent("Export Copy"), false, () => { ExportCopyPopup(modd); });
                            menu.AddItem(new GUIContent("Export Variant"), false, () => { ExportVariantPopup(modd); });

                            menu.AddItem(new GUIContent(""), false, () => { });
                            menu.AddItem(new GUIContent("Prepare for Copy"), false, () => { PrepareForCopy(modd); });
                            menu.AddItem(new GUIContent(""), false, () => { });

                            if (modd)
                            {
                                if (modd.ParentPreset == setup) menu.AddItem(new GUIContent("Move Modificator to Execution List"), false, () => { setup.MoveModificatorToUtilityList(modd, true); });
                            }

                            if (PreparedToCopyModReference != null)
                            {
                                menu.AddItem(new GUIContent("Paste Prepared Copy as New"), false, () => { AddDuplicateOfPreparedToCopy(modd); });
                            }

                            menu.AddItem(new GUIContent(""), false, () => { });
                            menu.AddItem(new GUIContent("Remove Mod Permanently"), false, () => { RemoveModFrom(null, modd, true); });


                            menu.ShowAsContext();
                        }


                        if (GUILayout.Button(lbl, FGUI_Resources.ButtonStyle, GUILayout.Width(wdth + 2), GUILayout.Height(wdth)))
                        {
                            SeparatedModWindow.SelectMod(toDraw[i]);
                        }


                        GUI.backgroundColor = preCol;
                    }
                    else
                    {
                        EditorGUILayout.LabelField(lbl, GUILayout.Width(wdth + 2));
                    }

                    #endregion


                    EditorGUI.BeginChangeCheck();

                    //if (SeparatedModWindow.Get)
                    //    if (toDraw[i] == SeparatedModWindow.Get.latestMod)
                    //        GUI.backgroundColor = Color.green;

                    // Asset Field
                    toDraw[i] = (FieldModification)EditorGUILayout.ObjectField(toDraw[i], typeof(FieldModification), false, GUILayout.Height(wdth));

                    if (isSelct) GUI.backgroundColor = prebC;


                    #region Buttons

                    FieldModification nMod = toDraw[i];
                    DrawRenameScriptableButton(nMod);

                    #endregion


                    // Ending one drawed mod horizontal GUI
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(2);

                    #region Editing Finishing

                    // Marking mod as edited
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (setup != null)
                        {
                            Undo.RecordObject(setup, "Editing " + i + " Element");
                            EditorUtility.SetDirty(setup);
                        }
                    }


                    GUI.color = preCol;

                    #endregion

                }

            }

            EditorGUILayout.EndVertical();
        }


        internal static FieldModification GetDuplicateOfPreparedToCopy()
        {
            FieldModification newR;

            if (PreparedToCopyModReference == null) return null;
            else
                newR = GameObject.Instantiate(PreparedToCopyModReference);

            FieldModification target = newR;
            ModificatorsPackEditor.CopyRulesTo(PreparedToCopyModReference, target);

            return target;
        }


        internal static void AddDuplicateOfPreparedToCopy(FieldModification modd, bool addToList = true)
        {
            FieldModification newR;
            ModificatorsPack package = modd.ParentPack;

            if (package == null)
            {
                UnityEngine.Debug.Log("Cant duplicate FieldModificator because it's parent pack is null!");
                return;
            }

            if (PreparedToCopyModReference == null)
                newR = CreateInstance<FieldModification>();
            else
                newR = GameObject.Instantiate(PreparedToCopyModReference);

            FieldModification target = newR;
            target.ParentPack = package;
            FGenerators.AddScriptableTo(target, package, false, true);
            ModificatorsPackEditor.CopyRulesTo(PreparedToCopyModReference, target);
            if (target != null) if (addToList) package.FieldModificators.Add(target);

            EditorUtility.SetDirty(package);
            AssetDatabase.SaveAssets();
        }

        public static FieldModification PreparedToCopyModReference;
        internal static void PrepareForCopy(FieldModification modd)
        {
            PreparedToCopyModReference = modd;
        }

        private static void DuplicateModificator(FieldSetup root, FieldModification mod, bool toUtils = false)
        {
            FieldModification newR;
            ModificatorsPack package = mod.ParentPack;

            if (toUtils == false && package == null)
            {
                UnityEngine.Debug.Log("Cant duplicate FieldModificator because it's parent pack is null!");
                return;
            }

            if (mod == null)
                newR = CreateInstance<FieldModification>();
            else
                newR = GameObject.Instantiate(mod);

            FieldModification target = newR;

            if (!toUtils)
            {
                target.ParentPack = package;
                FGenerators.AddScriptableTo(target, package, false, true);
                ModificatorsPackEditor.CopyRulesTo(mod, target);
                if (target != null) package.FieldModificators.Add(target);

                EditorUtility.SetDirty(package);
            }
            else
            {
                target.ParentPreset = root;
                FGenerators.AddScriptableTo(target, root, false, true);
                ModificatorsPackEditor.CopyRulesTo(mod, target);
                if (target != null) root.UtilityModificators.Add(target);
                EditorUtility.SetDirty(root);
            }


            EditorUtility.SetDirty(root);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Return true if click occured
        /// </summary>
        /// <returns></returns>
        public static bool DrawModUtilitiesButtons(ref FieldModification mod, ModificatorsPack pack, bool drawInfo = false)
        {
            Color prebC = GUI.backgroundColor;

            // Modifying mods ---------------------------------
            if (mod != null)
            {
                // If mod exists in database
                if (AssetDatabase.Contains(mod))
                {
                    if (FGenerators.AssetContainsAsset(mod, pack)) // If package contains this modification
                    {
                        DrawRenameScriptableButton(mod);

                        if (drawInfo) DrawVariantExportInfoIcon();
                        //DrawVariantExportButton(mod);
                        //DrawExportModButton(mod, "E", "Export\nMod Pack contains this Field Modificator");

                    }
                    else if (FGenerators.AssetContainsAsset(mod, pack.ParentPreset)) // If Field preset contains this modification
                    {
                        DrawRenameScriptableButton(mod);
                        if (drawInfo) DrawVariantExportInfoIcon();
                        //DrawExportModButton(mod, "E", "Export\nField Setup contains this modificator preset file");

                    }
                    else if (AssetDatabase.IsSubAsset(mod)) // If modification is contained by other preset files
                    {
                        DrawRenameScriptableButton(mod);
                        mod = DrawModInjectButton(mod, pack);
                        if (drawInfo) DrawVariantExportInfoIcon();
                        //DrawExportModButton(mod, "E", "Export\nThis Field Mod file is contained by some other project preset file");

                    }
                    else // If modification is project file
                    {
                        DrawRenameScriptableButton(mod);
                        mod = DrawModInjectButton(mod, pack);
                        if (drawInfo) DrawVariantExportInfoIcon();
                        //DrawExportModButton(mod, "C", "Copy\nThis Field Mod is separated project file in project directory");
                    }
                }
                else
                {
                    if (GUILayout.Button(new GUIContent("???", "Asset is not recognized by Asset Database, click to generate clone of this preset as project file"), GUILayout.Width(34)))
                    {
                        FieldModification newR = (FieldModification)FGenerators.GenerateScriptable(GameObject.Instantiate(mod), mod.name);
                        AssetDatabase.SaveAssets();
                        if (AssetDatabase.Contains(newR)) mod = newR;
                    }

                    mod = DrawModInjectButton(mod, pack);
                    if (drawInfo) DrawVariantExportInfoIcon();
                }

            }


            // Generating new mods -------------------------------
            if (mod == null)
            {
                // New Mod and keeping inside package
                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 1f);
                if (GUILayout.Button(new GUIContent("IN", "Generating modificator inside package file (or in field setup file)"), GUILayout.Width(24)))
                {
                    var temp = ScriptableObject.CreateInstance<FieldModification>();
                    temp.name = "FieldMod_" + pack.name;
                    temp.ParentPack = pack;
                    temp.ParentPreset = pack.ParentPreset;
                    FGenerators.AddScriptableTo(temp, pack, false, true);
                    if (temp != null) if (AssetDatabase.Contains(temp)) mod = temp;

                    FieldModification newR = GameObject.Instantiate(mod);

                    return true;
                }
                GUI.backgroundColor = prebC;

                // New mod in project file
                if (GUILayout.Button(new GUIContent("New", "Generating modificator preset file as separated file in project database"), GUILayout.Width(40)))
                {
                    var temp = (FieldModification)FGenerators.GenerateScriptable(ScriptableObject.CreateInstance<FieldModification>(), "FieldMod_" + pack.name);
                    AssetDatabase.SaveAssets();
                    if (temp != null) if (AssetDatabase.Contains(temp)) mod = temp;

                    return true;
                }
            }

            return false;
        }


        #region Project browser methods

        public static void LockBrowserWindow(bool lockIt)
        {

            Assembly editorAssembly = typeof(Editor).Assembly;
            Type projectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");
            UnityEngine.Object[] projectBrowserInstances = Resources.FindObjectsOfTypeAll(projectBrowserType);

            if (projectBrowserInstances.Length > 0)
            {
                var propertyInfo = projectBrowserType.GetProperty("isLocked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                propertyInfo.SetValue(projectBrowserInstances[0], lockIt, null);
            }

        }

        #endregion


        private static void MergeOntoDialog(ModificatorsPack addTo)
        {
            string lastPath = "";

            if (EditorPrefs.HasKey("LastFGenSaveDir"))
            {
                lastPath = EditorPrefs.GetString("LastFGenSaveDir");
                if (!System.IO.File.Exists(lastPath)) lastPath = Application.dataPath;
            }
            else lastPath = Application.dataPath;

            string path = EditorUtility.OpenFilePanelWithFilters("Select modificator pack file or modificator file", lastPath, new string[] { "'ModPack' (MP_) or 'FieldMod' (FM_)", "asset" });

            try
            {
                if (path != "")
                {
                    ModificatorsPack pack = null;
                    FieldModification mod = null;

                    int l = path.LastIndexOf("Assets/");
                    string projectPath = path.Substring(l, path.Length - l);
                    //projectPath = projectPath.Replace( System.IO.Path.GetExtension(projectPath), "");

                    pack = AssetDatabase.LoadAssetAtPath<ModificatorsPack>(projectPath);
                    if (pack == null) mod = AssetDatabase.LoadAssetAtPath<FieldModification>(projectPath);

                    if (pack != null)
                    {
                        for (int i = 0; i < pack.FieldModificators.Count; i++)
                        {
                            FieldModification copied = GameObject.Instantiate(pack.FieldModificators[i]);
                            AddModTo(addTo, copied, true);
                            CopyRulesTo(pack.FieldModificators[i], copied, true);
                        }
                    }
                    else if (mod != null)
                    {
                        FieldModification copied = GameObject.Instantiate(mod);
                        AddModTo(addTo, copied, true);
                        CopyRulesTo(mod, copied, true);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("[FGenerators] Select 'ModificatorsPack' or 'FieldModification' scriptable file");
                    }
                }
            }
            catch (System.Exception)
            {
                Debug.LogError("Something went wrong when selecting scriptable file in your project.");
            }

        }

        public static FieldModification DrawVariantExportButton(FieldModification mod)
        {
            if (GUILayout.Button(new GUIContent("V", "Export variant of this modificator (Separated file but will use rules references from source modificator)"), new GUILayoutOption[2] { GUILayout.Width(22), GUILayout.Height(19) }))
            {
                return ExportVariantPopup(mod);
            }

            return null;
        }

        internal static FieldModification ExportVariantPopup(FieldModification mod)
        {
            FieldModification previous = mod;
            FieldModification newR = (FieldModification)FGenerators.GenerateScriptable(GameObject.Instantiate(previous), previous.name);
            AssetDatabase.SaveAssets();

            if (AssetDatabase.Contains(newR))
            {
                newR.VariantOf = previous;
                //previous = newR;
                return newR;
            }

            return null;
        }

        /// <summary>
        /// Returns true if pressed Right mouse button on the button
        /// </summary>
        public static bool DrawRenameScriptableButton(ScriptableObject scrptble, string whatToRename = "modificator", bool dontExecuteOnRMB = false)
        {
            bool wasRMB = false;
            if (Event.current != null) if (Event.current.button == 1) wasRMB = true;

            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rename, "Rename " + whatToRename + " display name (Save-File popup window will not create file, it will just take written name)"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(32), GUILayout.Height(20) }))
            {
                bool executeRename = true;

                if (wasRMB)
                {
                    if (Event.current.type == EventType.Used)
                    {
                        if (dontExecuteOnRMB) executeRename = false;
                    }
                    else
                        wasRMB = false;
                }

                if (executeRename)
                {
                    string filename = EditorUtility.SaveFilePanelInProject("Type your title (no file will be created)", scrptble.name, "", "Type your title (no file will be created)");
                    if (!string.IsNullOrEmpty(filename))
                    {
                        filename = System.IO.Path.GetFileNameWithoutExtension(filename);
                        if (!string.IsNullOrEmpty(filename))
                        {
                            bool isSubAsset = AssetDatabase.IsSubAsset(scrptble);
                            scrptble.name = filename;
                            if (!isSubAsset) AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(scrptble), filename);
                            EditorUtility.SetDirty(scrptble);
                            if (!isSubAsset) AssetDatabase.SaveAssets();
                        }
                    }
                }
            }
            else
                wasRMB = false;

            return wasRMB;
        }


        public static FieldModification DrawExportModButton(FieldModification mod, string sing = "E", string addTooltip = "")
        {
            FieldModification previous = mod;

            if (GUILayout.Button(new GUIContent(sing, "Export copy of field modificator to be kept outside field setup (in project file) " + addTooltip), GUILayout.Width(22)))
            {
                previous = ExportCopyPopup(mod);
            }

            return previous;
        }

        public static bool DrawVariantExportInfoIcon()
        {
            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Info, "If you want to export variant / copy / duplicate modificator, then hit right mouse button on '[0]' '[1]' button on the left"), EditorStyles.label, GUILayout.Width(16), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                EditorUtility.DisplayDialog("More options info", "If you want to export variant / copy / duplicate modificator, then hit right mouse button on '[0]' '[1]' button on the left", "OK");
                return true;
            }
            else
                return false;
        }

        internal static FieldModification ExportCopyPopup(FieldModification mod)
        {
            FieldModification previous = mod;

            FieldModification newR = (FieldModification)FGenerators.GenerateScriptable(GameObject.Instantiate(previous), previous.name);
            AssetDatabase.SaveAssets();
            if (AssetDatabase.Contains(newR))
            {
                ModificatorsPackEditor.CopyRulesTo(mod, newR, true);
                previous = newR;
                return newR;
            }

            return null;
        }

        public static FieldModification DrawModInjectButton(FieldModification mod, ModificatorsPack package, FieldSetup setup = null)
        {
            FieldModification target = mod;
            if (mod) if (mod.hideFlags != HideFlags.None) return target;
            if (GUILayout.Button(new GUIContent("IN", "Duplicate field modificator and keep inside field setup"), GUILayout.Width(22)))
            {
                FieldModification newR;

                if (mod == null)
                    newR = CreateInstance<FieldModification>();
                else
                    newR = GameObject.Instantiate(mod);

                if (package != null)
                {
                    target = newR;
                    target.ParentPack = package;
                    if (setup != null) target.ParentPreset = setup;
                    else target.ParentPreset = package.ParentPreset;

                    FGenerators.AddScriptableTo(target, package, false, true);
                    ModificatorsPackEditor.CopyRulesTo(mod, target);
                }
                else if (setup != null)
                {
                    target = newR;
                    target.ParentPreset = setup;
                    target.ParentPack = setup.RootPack;
                    FGenerators.AddScriptableTo(target, setup, false, true);
                    ModificatorsPackEditor.CopyRulesTo(mod, target);
                }

            }

            return target;
        }

        public static void CleanLeftowers(ModificatorsPack pack)
        {
            if (AssetDatabase.Contains(pack) == false) return;

            string path = AssetDatabase.GetAssetPath(pack);
            if (string.IsNullOrEmpty(path)) return;

            List<UnityEngine.Object> used = new List<UnityEngine.Object>();

            if (pack.CallOnAllMod) used.Add(pack.CallOnAllMod);
            used.Add(pack);

            for (int i = 0; i < pack.FieldModificators.Count; i++)
            {
                if (pack.FieldModificators[i] == null) continue;
                used.Add(pack.FieldModificators[i]);

                for (int j = 0; j < pack.FieldModificators[i].Spawners.Count; j++)
                {
                    if (pack.FieldModificators[i].Spawners[j] == null) continue;

                    for (int s = 0; s < pack.FieldModificators[i].Spawners[j].Rules.Count; s++)
                    {
                        if (pack.FieldModificators[i].Spawners[j].Rules[s] == null) continue;
                        used.Add(pack.FieldModificators[i].Spawners[j].Rules[s]);
                    }
                }

                for (int j = 0; j < pack.FieldModificators[i].SubSpawners.Count; j++)
                {
                    if (pack.FieldModificators[i].SubSpawners[j] == null) continue;

                    for (int s = 0; s < pack.FieldModificators[i].SubSpawners[j].Rules.Count; s++)
                    {
                        if (pack.FieldModificators[i].SubSpawners[j].Rules[s] == null) continue;
                        used.Add(pack.FieldModificators[i].SubSpawners[j].Rules[s]);
                    }
                }

            }

            var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            for (int i = 0; i < assets.Length; i++)
            {
                if (used.Contains(assets[i]) == false) GameObject.DestroyImmediate(assets[i], true);
            }

            AssetDatabase.SaveAssets();
        }


        #endregion



        public static List<FieldVariable> TryGetFieldVariablesList(ModificatorsPack pack, bool justPackVars = false, bool allVars = false)
        {
            List<FieldVariable> vars = new List<FieldVariable>();

            if (!justPackVars || allVars)
                for (int i = 0; i < pack.FieldModificators.Count; i++)
                {
                    if (pack.FieldModificators[i] == null) continue;
                    if (pack.FieldModificators[i].Enabled == false) continue;

                    var fmVars = pack.FieldModificators[i].TryGetFieldVariablesList();
                    if (fmVars == null) continue;
                    if (fmVars.Count == 0) continue;

                    for (int v = 0; v < fmVars.Count; v++)
                    {
                        if (!vars.Contains(fmVars[v]))
                            vars.Add(fmVars[v]);
                    }
                }

            //if (vars.Count == 0 || justPackVars)
            {
                for (int i = 0; i < pack.Variables.Count; i++)
                {
                    vars.Add(pack.Variables[i]);
                }
            }

            if (vars.Count == 0) return null;
            return vars;
        }



    }

}