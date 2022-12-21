using FIMSpace.FEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating.Planning
{
    /// <summary>
    /// FM: Editor class component to enchance controll over component from inspector window
    /// </summary>
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(BuildPlanPreset))]
    public class BuildPlanPresetEditor : UnityEditor.Editor
    {
        public BuildPlanPreset Get { get { if (_get == null) _get = (BuildPlanPreset)target; return _get; } }
        private BuildPlanPreset _get;
        bool preE = true;

        void ResetRootChunkSettings()
        {
            Get.RootChunkSetup.DoorConnectionsCount = new MinMax(10000, 10000);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("You can use building plan as group of setted up elements for other generators", MessageType.Info);
            //EditorGUILayout.HelpBox("Building Plan is not needed to use Procedural Generation Grid\nUse building plan if you really need it\nYour main focus thing to use this plugin should be 'Field Setups'", MessageType.Info);
            GUILayout.Space(4);

            preE = GUI.enabled;
            serializedObject.Update();
            Get.RootChunkSetup.InternalSetup.GenerationMode = GenerationShape.EGenerationMode.RandomTunnels;

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), GUIContent.none);
            GUI.enabled = preE;
            if (GUILayout.Button("Select Asset")) EditorGUIUtility.PingObject(Get);
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(4f);
            DrawPresetList(Get.Settings, FGUI_Resources.BGInBoxStyle, "Presets", ref Get._editorFoldout, ref Get._editorSelected, true, true, Get);
            if (Get._editorFoldout) DrawSettings(Get, Get._editorSelected, false, false);

            if (Get._editorSelected != -1) GUILayout.Space(4);

            GUILayout.Space(8);
            Get.RootChunkSetup.DoorConnectionsCount = new MinMax(10000, 10000);
            SingleInteriorSettingsEditor.DrawGUIChunkSetup("Root Corridor", Get.RootChunkSetup, serializedObject, serializedObject.FindProperty("RootChunkSetup"), ref Get._editorRootFoldout);

            GUILayout.Space(8);


            Color preC = GUI.color;

            GUI.color = new Color(0.65f, 0.65f, 1f, 1f);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            GUI.color = preC;

            string fold = Get._EditorAdvancedBuildingFoldout ? " ▼" : " ►";
            if (GUILayout.Button(fold + "  More Guide Parameters", EditorStyles.label, GUILayout.Width(200))) Get._EditorAdvancedBuildingFoldout = !Get._EditorAdvancedBuildingFoldout;

            if (Get._EditorAdvancedBuildingFoldout)
            {

                EditorGUI.indentLevel++;
                GUILayout.Space(4);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("NeightbourWallsCellsRestrictions"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CounterWallsCellsRestrictions"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OutsideWallsCellsRestrictions"));
                GUILayout.Space(4);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(4);
            EditorGUILayout.HelpBox("Add 'Door Hole' and 'Counter Door' cell Instructions in your Field Setups to create door connections between rooms", MessageType.Info);

            if (GUILayout.Button("Open Build Plan in designer window", GUILayout.Height(38))) AssetDatabase.OpenAsset(Get);

            serializedObject.ApplyModifiedProperties();
        }


        public static void DrawSettings(BuildPlanPreset preset, int selected, bool drawField, bool multiRectNotSupp = false, SerializedObject so_preset = null)
        {
            if (selected == -1) return;
            if (selected >= preset.Settings.Count) return;

            var sett = preset.Settings[selected];
            if (sett == null) return;
            if (sett.FieldSetup == null) return;

            SerializedObject so = so_preset;
            if ( so_preset == null) so = new SerializedObject(preset);
            SerializedProperty prp = so.FindProperty("Settings");

            prp = prp.GetArrayElementAtIndex(selected);
            if (prp != null)
            {
                var settingsPreset = preset.Settings[selected];
                SingleInteriorSettingsEditor.DrawGUI(settingsPreset, so, prp, drawField, selected, multiRectNotSupp);
            }
        }



        public static void DrawPresetList(List<SingleInteriorSettings> toDraw, GUIStyle style, string title, ref bool foldout, ref int selected, bool newButton = false, bool moveButtons = false, UnityEngine.Object toDirty = null, bool drawNames = false)
        {
            if (toDraw == null) return;

            Color bgc = GUI.backgroundColor;

            EditorGUILayout.BeginVertical(style);

            EditorGUILayout.BeginHorizontal();
            string fold = foldout ? " ▼" : " ►";
            if (GUILayout.Button(fold + "  " + title + " (" + toDraw.Count + ")", EditorStyles.label, GUILayout.Width(200))) foldout = !foldout;

            if (foldout)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+"))
                {
                    if (toDraw.Count == 0)
                        toDraw.Add(null);
                    else
                    {
                        var inter = toDraw[toDraw.Count - 1];
                        toDraw.Add(inter.Copy());
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                GUILayout.Space(4);

                if (toDraw.Count > 0)
                    for (int i = 0; i < toDraw.Count; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();

                        GUIContent lbl = new GUIContent("[" + i + "]");
                        float wdth = EditorStyles.label.CalcSize(lbl).x;

                        if (selected == i) GUI.backgroundColor = Color.green;
                        if (GUILayout.Button(lbl, GUILayout.Width(wdth + 8))) { if (selected == i) selected = -1; else selected = i; }
                        GUI.backgroundColor = bgc;

                        if (toDraw[i] == null)
                        {
                            FieldSetup tempPreset = (FieldSetup)EditorGUILayout.ObjectField(null, typeof(FieldSetup), false);
                            if (tempPreset != null)
                            {
                                var sett = new SingleInteriorSettings();
                                sett.FieldSetup = tempPreset;
                                toDraw[i] = sett;
                            }
                        }
                        else
                            toDraw[i].FieldSetup = (FieldSetup)EditorGUILayout.ObjectField(toDraw[i].FieldSetup, typeof(FieldSetup), false);

                        if (newButton)
                            if (toDraw[i] == null)
                                if (GUILayout.Button(new GUIContent("New"), GUILayout.Width(44)))
                                {
                                    FieldSetup tempPreset = (FieldSetup)FGenerators.GenerateScriptable(ScriptableObject.CreateInstance<FieldSetup>(), "FS_");
                                    if (tempPreset != null)
                                    {
                                        var sett = new SingleInteriorSettings();
                                        sett.FieldSetup = tempPreset;
                                        toDraw[i] = sett;
                                    }
                                }

                        if (drawNames == false)
                            if (moveButtons)
                            {
                                if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp, "Move this element to be executed before above one"), GUILayout.Width(24))) { SingleInteriorSettings temp = toDraw[i - 1]; toDraw[i - 1] = toDraw[i]; toDraw[i] = temp; }
                                if (i < toDraw.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown, "Move this element to be executed after below one"), GUILayout.Width(24))) { SingleInteriorSettings temp = toDraw[i + 1]; toDraw[i + 1] = toDraw[i]; toDraw[i] = temp; }
                            }


                        if (toDraw[i] != null)
                            if (newButton == false && drawNames)
                            {
                                GUIContent nlbl = new GUIContent(toDraw[i].GetName());
                                float wid = EditorStyles.centeredGreyMiniLabel.CalcSize(nlbl).x;
                                if (wid > 120) wid = 120;

                                EditorGUILayout.LabelField(toDraw[i].GetName(), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(wid));
                            }

                        if (drawNames)
                            if (moveButtons)
                            {
                                if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp, "Move this element to be executed before above one"), GUILayout.Width(24))) { SingleInteriorSettings temp = toDraw[i - 1]; toDraw[i - 1] = toDraw[i]; toDraw[i] = temp; }
                                if (i < toDraw.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown, "Move this element to be executed after below one"), GUILayout.Width(24))) { SingleInteriorSettings temp = toDraw[i + 1]; toDraw[i + 1] = toDraw[i]; toDraw[i] = temp; }
                            }

                        if (GUILayout.Button("X", GUILayout.Width(24))) { toDraw.RemoveAt(i); break; }

                        EditorGUILayout.EndHorizontal();
                        if (toDirty != null) if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(toDirty); }
                    }
                else
                {
                    EditorGUILayout.LabelField("No object in list", EditorStyles.centeredGreyMiniLabel);
                }
            }

            EditorGUILayout.EndVertical();
        }

    }
}