using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;
using FIMSpace.Generating.Planning;

namespace FIMSpace.Generating
{
    public partial class BuildPlannerWindow : EditorWindow
    {
        public static BuildPlannerPreset _DrawPlannerPresetBar_Assigned = null;
        public static void DrawPlannerPresetBar(ref BuildPlannerPreset projectPreset, ref ScriptableObject wasChecked, Object draftsDirectory, bool isInDefaultDirectory)
        {
            _DrawPlannerPresetBar_Assigned = null;

            #region Preset Field

            Color preBGCol = GUI.backgroundColor;

            // Build plan preset field
            EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxStyle);
            EditorGUIUtility.labelWidth = 42;
            EditorGUIUtility.labelWidth = 60;
            projectPreset = (BuildPlannerPreset)EditorGUILayout.ObjectField("Preset:", projectPreset, typeof(BuildPlannerPreset), false);
            bool wasAutoFileButtons = false;

                if (draftsDirectory)
                {
                    wasAutoFileButtons = true;

                    #region Button to display menu of draft field setup files

                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Display quick selection menu for FieldSetups contained in the drafts directory"), EditorStyles.label, GUILayout.Width(18), GUILayout.Height(16)))
                    {
                        string path = AssetDatabase.GetAssetPath(draftsDirectory);
                        var files = System.IO.Directory.GetFiles(path, "*.asset");
                        if (files != null)
                        {
                            GenericMenu draftsMenu = new GenericMenu();

                            draftsMenu.AddItem(new GUIContent("None"), projectPreset == null, () => { _DrawPlannerPresetBar_Assigned = null; });

                            for (int i = 0; i < files.Length; i++)
                            {
                                BuildPlannerPreset fs = AssetDatabase.LoadAssetAtPath<BuildPlannerPreset>(files[i]);
                                if (fs) draftsMenu.AddItem(new GUIContent(fs.name), projectPreset == fs, () => { _DrawPlannerPresetBar_Assigned = fs; });
                            }

                            draftsMenu.ShowAsContext();
                        }
                    }


                    #endregion

                    if (projectPreset != null)
                        ModificatorsPackEditor.DrawRenameScriptableButton(projectPreset);

                    if (projectPreset == null) GUI.backgroundColor = Color.green;
                    else GUI.backgroundColor = new Color(0.75f, 1f, 0.75f, 1f);

                    if (GUILayout.Button(new GUIContent("New", "Generates new Planner Setup file in the PGG drafts directory, after that you can move draft file into project directories with 'Move' button which will appear"), GUILayout.Width(44)))
                    {
                        string path;

                        path = AssetDatabase.GetAssetPath(draftsDirectory);
                        var files = System.IO.Directory.GetFiles(path, "*.asset");
                        path += "/FS_NewFieldSetup" + (files.Length + 1) + ".asset";

                        BuildPlannerPreset scrInstance = CreateInstance<BuildPlannerPreset>();

                        if (string.IsNullOrEmpty(path))
                            path = FGenerators.GenerateScriptablePath(scrInstance, "FS_NewFieldSetup");

                        if (!string.IsNullOrEmpty(path))
                        {
                            UnityEditor.AssetDatabase.CreateAsset(scrInstance, path);
                            AssetDatabase.SaveAssets();
                            projectPreset = scrInstance;
                        }

                        //projectPreset = (FieldSetup)FGenerators.GenerateScriptable(CreateInstance<FieldSetup>(), "FS_");
                    }

                    GUI.backgroundColor = preBGCol;



                    if (isInDefaultDirectory)
                    {
                        if (projectPreset != null)
                            if (GUILayout.Button(new GUIContent(" Move", PGGUtils.TEX_FolderDir, "Move 'Field Setup' file to choosed project directory"), GUILayout.Height(20), GUILayout.Width(74)))
                            {
                                string path = FGenerators.GetPathPopup("Move 'Field Setup' file to new directory in project", "FS_Your FieldSetup");
                                if (!string.IsNullOrEmpty(path))
                                {
                                    UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(projectPreset), path);
                                    AssetDatabase.SaveAssets();
                                }

                                wasChecked = null;
                            }
                    }
                    else
                    {
                        if (draftsDirectory != null)
                            if (projectPreset != null)
                                if (GUILayout.Button(new GUIContent(" Back", PGGUtils.TEX_FolderDir, "Move 'Field Setup' file from project to default PGG Drafts directory"), GUILayout.Height(20), GUILayout.Width(56)))
                                {
                                    string path = AssetDatabase.GetAssetPath(draftsDirectory);

                                    if (!string.IsNullOrEmpty(path))
                                    {
                                        path += "/" + projectPreset.name + ".asset";
                                        UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(projectPreset), path);
                                        AssetDatabase.SaveAssets();
                                    }

                                    wasChecked = null;
                                }
                    }
                }


            if (projectPreset)
            {
                if (wasAutoFileButtons)
                {
                    if (GUILayout.Button(new GUIContent("+", "Create new Build Planner Preset in selected project directory"), GUILayout.Width(24))) projectPreset = (BuildPlannerPreset)FGenerators.GenerateScriptable(Instantiate(projectPreset), "BP_");
                }
                else
                    if (GUILayout.Button("Create New", GUILayout.Width(94))) projectPreset = (BuildPlannerPreset)FGenerators.GenerateScriptable(Instantiate(projectPreset), "BP_");
            }
            else
            {
                if (wasAutoFileButtons)
                {
                    if (GUILayout.Button(new GUIContent("+", "Create new Build Planner Preset in selected project directory"), GUILayout.Width(24))) projectPreset = (BuildPlannerPreset)FGenerators.GenerateScriptable(CreateInstance<BuildPlannerPreset>(), "BP_");
                }
                else
                    if (GUILayout.Button("Create New", GUILayout.Width(94))) projectPreset = (BuildPlannerPreset)FGenerators.GenerateScriptable(CreateInstance<BuildPlannerPreset>(), "BP_");
            }

            EditorGUILayout.EndHorizontal();

            #endregion

        }
    }
}