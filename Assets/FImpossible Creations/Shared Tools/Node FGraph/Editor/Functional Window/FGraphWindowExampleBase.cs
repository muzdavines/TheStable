using FIMSpace.FEditor;
using FIMSpace.Generating;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphWindowExampleBase : EditorWindow
    {
        /// <summary> Assign through script inspector window </summary>
        public Texture2D Tex_Net;
        /// <summary> Assign through script inspector window </summary>
        public UnityEngine.Object BaseDirectory;


        //[MenuItem("Window/FGraph", false, 151)]
        #region Initialize and show window
        //public static void Init()
        //{
        //    YourWindowClass window = (YourWindowClass)GetWindow(typeof(YourWindowClass));
        //    window.titleContent = new GUIContent("AI Graph", EditorGUIUtility.IconContent("d_BlendTree Icon").image, "AI Graph Window");
        //    // other Icons: "BuildSettings.Android" "Grid.MoveTool@2x" "Preset.Context@2x" "AnimatorState Icon" "BlendTree Icon" d_BlendTree Icon "d_NavMeshAgent Icon" "Animator Icon"
        //    window.Show();

        //    Rect p = window.position;
        //    if (p.size.x < 500) { p.size += new Vector2(200, 0); }
        //    if (p.size.y < 350) p.size += new Vector2(0, 120);
        //    window.position = p;

        //    Get = window;
        //}
        #endregion

        #region Open File with double click - open graph window

        //[OnOpenAssetAttribute(1)]
        //public static bool OpenBuildPlannerScriptableFile(int instanceID, int line)
        //{
        //    Object obj = EditorUtility.InstanceIDToObject(instanceID);
        //    if (obj is YourScriptable)
        //    {
        //        if (Get == null) Init(); else Get.Focus();

        //        Get.SetSetup(obj as YourScriptable);
        //        Get.Repaint();
        //        return true;
        //    }

        //    return false;
        //}

        //internal void SetSetup(YourScriptable setup)
        //{
        //    CurrentSetup = setup;
        //}

        #endregion

        #region Main Utility

        int latestFilesInDraft = 0;
        public bool IsVisible { get; protected set; }
        void OnBecameVisible() { IsVisible = true; }
        void OnBecameInvisible() { IsVisible = false; }

        protected Color preBG;
        protected Color preGuiC;

        private void OnEnable()
        {
            if (BaseDirectory == null) return;
            string path = AssetDatabase.GetAssetPath(BaseDirectory);
            var files = System.IO.Directory.GetFiles(path, "*.asset");
            if (files != null) latestFilesInDraft = files.Length;
        }


        private void OnInspectorUpdate()
        {
            if (FGenerators.CheckIfIsNull(GraphDrawer)) return;
            GraphDrawer.OnInspectorUpdate();
        }

        #endregion


        public static Texture _FolderDir { get { if (__folderdir != null) return __folderdir; __folderdir = EditorGUIUtility.IconContent("d_Folder Icon").image; return __folderdir; } }
        private static Texture __folderdir = null;

        private SerializedObject so_currentSetup = null;

        private ScriptableObject wasChecked = null;
        private bool isInDefaultDirectory = false;

        /// <summary> Used for automatically generating right scriptable file to contain nodes in </summary>
        public abstract Type GetNodesContainerFileType { get; }


        #region Interface

        protected abstract ScriptableObject ProjectFileGraphPreset { get; set; }
        /// <summary>
        /// It can be just: return ScriptableObject.CreateInstance<YourPresetScriptableClass>();
        /// </summary>
        protected virtual ScriptableObject CreateNewScriptablePresetToContainNodesIn()
        {
            return CreateInstance(GetNodesContainerFileType);
        }

        /// <summary>
        /// It can be just: return new YourGraphDrawer();
        /// </summary>
        protected abstract FGraphDrawerBase CreateGraphDrawer();
        protected abstract FGraphDrawerBase GraphDrawer { get; set; }
        public virtual bool ShouldGenerateNewGraphDrawerForDebugView { get { return false; } }

        #endregion


        protected virtual void GUI_DrawPresetField()
        {
            /*ScriptableGraphPreset = (YourPresetScriptableClass)*/
            EditorGUILayout.ObjectField(ProjectFileGraphPreset, typeof(ScriptableObject/*YourPresetScriptableClass*/), false);
        }


        private void OnGUI()
        {
            preGuiC = GUI.color;
            preBG = GUI.backgroundColor;

            GUILayout.Space(5);

            #region Lower Header

            EditorGUILayout.BeginHorizontal();

            if (ProjectFileGraphPreset != null)
            {
                if (GUILayout.Button(new GUIContent(FGUI_Resources.TexTargetingIcon, "Reposition graph display to nodes and reset zoom"), EditorStyles.label, GUILayout.Width(24), GUILayout.Height(18))) GraphDrawer.ResetGraphPosition();
            }

            GUI_DrawPresetField();

            GUILayout.Space(4);


            #region Button to display menu of draft setup files

            if (BaseDirectory)
                if (latestFilesInDraft > 0)
                {
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Display quick selection menu for FieldSetups contained in the drafts directory"), EditorStyles.label, GUILayout.Width(16), GUILayout.Height(16)))
                    {
                        string path = AssetDatabase.GetAssetPath(BaseDirectory);
                        var files = System.IO.Directory.GetFiles(path, "*.asset");
                        if (files != null)
                        {
                            latestFilesInDraft = files.Length;

                            GenericMenu draftsMenu = new GenericMenu();

                            for (int i = 0; i < files.Length; i++)
                            {
                                ScriptableObject fs = AssetDatabase.LoadAssetAtPath<ScriptableObject>(files[i]);
                                if (fs)
                                {
                                    draftsMenu.AddItem(new GUIContent(fs.name), ProjectFileGraphPreset == fs, () => { ProjectFileGraphPreset = fs; });
                                }
                            }

                            draftsMenu.ShowAsContext();
                        }
                    }

                    GUILayout.Space(3);
                }


            #endregion


            if (ProjectFileGraphPreset != null)
            {
                if (GUILayout.Button(new GUIContent(FGUI_Resources.TexTargetingIcon, "Select Graph Setup for inspector window"), FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(19))) Selection.activeObject = ProjectFileGraphPreset;
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rename, "Opens popup for renaming Graph Setup filename"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(19))) FGenerators.RenamePopup(ProjectFileGraphPreset);
                GUILayout.Space(4);
            }


            if (GUILayout.Button(new GUIContent("New", "Generating new Graph Setup file in default directory.\n\n(setted through window script's inspector window - 'BaseDirectory' field, after assigning wait to compile, close graph window and open it again).\n\nYou can easily move setup file to selected directory with button which appear next to this button."), GUILayout.Width(44)))
            {
                string path = "";

                if (BaseDirectory != null)
                {
                    path = AssetDatabase.GetAssetPath(BaseDirectory);
                    var files = Directory.GetFiles(path, "*.asset");
                    path += "/AI_Setup" + (files.Length + 1) + ".asset";
                }

                var scrInstance = CreateNewScriptablePresetToContainNodesIn();

                if (string.IsNullOrEmpty(path))
                    path = FGenerators.GenerateScriptablePath(scrInstance, "GraphSetup");

                if (!string.IsNullOrEmpty(path))
                {
                    UnityEditor.AssetDatabase.CreateAsset(scrInstance, path);
                    AssetDatabase.SaveAssets();
                    ProjectFileGraphPreset = scrInstance;
                }
            }


            if (ProjectFileGraphPreset)
            {

                #region Preset File Check

                if (wasChecked != ProjectFileGraphPreset)
                {
                    so_currentSetup = null;
                    isInDefaultDirectory = false;

                    wasChecked = ProjectFileGraphPreset;
                    if (BaseDirectory)
                    {
                        string qPath = AssetDatabase.GetAssetPath(BaseDirectory);
                        string sPath = AssetDatabase.GetAssetPath(ProjectFileGraphPreset);
                        qPath = Path.GetFileName(qPath);
                        sPath = Path.GetFileName(Path.GetDirectoryName(sPath));
                        if (sPath.Contains(qPath)) isInDefaultDirectory = true;
                    }
                }

                #endregion


                if (isInDefaultDirectory)
                {
                    if (GUILayout.Button(new GUIContent(" Move", _FolderDir, "Move Graph Setup file in project directory"), GUILayout.Height(20), GUILayout.Width(74)))
                    {
                        string path = FGenerators.GetPathPopup("Move Graph Setup file to new directory in project", "GraphSetup");
                        if (!string.IsNullOrEmpty(path))
                        {
                            UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(ProjectFileGraphPreset), path);
                            AssetDatabase.SaveAssets();
                        }

                        wasChecked = null;
                    }
                }
                else
                {
                    if (BaseDirectory != null)
                        if (GUILayout.Button(new GUIContent(" Back", _FolderDir, "Move Graph Setup file in project to default Setups directory"), GUILayout.Height(20), GUILayout.Width(56)))
                        {
                            string path = AssetDatabase.GetAssetPath(BaseDirectory);

                            if (!string.IsNullOrEmpty(path))
                            {
                                path += "/" + ProjectFileGraphPreset.name + ".asset";
                                UnityEditor.AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(ProjectFileGraphPreset), path);
                                AssetDatabase.SaveAssets();
                            }

                            wasChecked = null;
                        }
                }

            }
            else
            {
                //wasChecked = null;
            }

            GUILayout.Space(2);

            //s

            EditorGUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(4);

            if (ProjectFileGraphPreset == null)
            {
                EditorGUILayout.HelpBox("Preset file is required to draw something", MessageType.Info);
            }
            else
            {
                bool newSo = false;
                if (so_currentSetup == null) { so_currentSetup = new SerializedObject(ProjectFileGraphPreset); newSo = true; }

                #region Drawing Graph

                GUI.backgroundColor = preBG;

                //bool gen = false;
                if (GraphDrawer == null || ShouldGenerateNewGraphDrawerForDebugView || newSo)
                {
                    //gen = true;
                    GraphDrawer = CreateGraphDrawer();
                }

                GraphDrawer.Parent = this;
                GraphDrawer.Tex_Net = Tex_Net;

                GraphDrawer.DrawGraph();

                if (so_currentSetup != null)
                    if (GraphDrawer.AsksForSerializedPropertyApply)
                    {
                        so_currentSetup.ApplyModifiedProperties();
                        so_currentSetup.Update();
                        GraphDrawer.AsksForSerializedPropertyApply = false;
                    }


                #endregion

            }

            GUILayout.Space(1);

        }


        private void Update()
        {
            if (GraphDrawer != null)
            {
                GraphDrawer.Update();
            }
        }


    }
}