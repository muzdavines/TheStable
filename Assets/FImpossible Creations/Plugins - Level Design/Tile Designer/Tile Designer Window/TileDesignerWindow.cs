#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;

namespace FIMSpace.Generating
{
    public partial class TileDesignerWindow : EditorWindow
    {
        private enum EMode { Setup, MeshEditor, Combine }
        private EMode EditorMode = EMode.Setup;

        private bool _foldout_meshSetup = true;
        private bool _foldout_gameObjectSetup = false;
        private bool _foldout_colliderSetup = false;
        private bool _foldout_finalizeSetup = false;

        Vector2 scroll = Vector2.zero;
        Vector2 windowMousePos = Vector2.zero;
        private bool repaintRequest = false;
        private int repaintRequestCounter = 0;

        double _doubleClickTimeMark = -1000;

        private static Rect _latestEditorDisplayRectSelected = new Rect(0, 0, 100, 100);
        private static Rect _editorDisplayRect1 = new Rect(0, 0, 100, 100);
        private static Rect _editorDisplayRect2 = new Rect(0, 0, 100, 100);

        /// <summary> Light gray and a bit transparent </summary>
        Color _curveCol = new Color(0.7f, 0.7f, 0.7f, 0.9f);

        #region Meshes design

        private TileDesign ProjectDesign = null;
        private TileDesign _editorWindowDesignTemp = null;

        private TileDesign EditedDesign
        {
            get
            {
                if (ProjectDesign != null) return ProjectDesign;
                if (_editorWindowDesignTemp == null) _editorWindowDesignTemp = new TileDesign();
                return _editorWindowDesignTemp;
            }
        }

        bool editingTempDesign { get { return ProjectDesign == null; } }

        #endregion

        private TileMeshSetup EditedTileSetup = null;
        UnityEngine.Object ToDirty = null;

        [MenuItem("Window/FImpossible Creations/Level Design/Tile Designer Window", false, 251)]
        static void Init()
        {
            TileDesignerWindow window = (TileDesignerWindow)EditorWindow.GetWindow(typeof(TileDesignerWindow), true);
            window.titleContent = new GUIContent("Tile Designer");
            window.Show();
            _maximizedCurve = null;

            if (EditorGUIUtility.isProSkin == false)
            {
                window._curveCol = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            }
        }

        public static void Init(TileDesign setup, UnityEngine.Object toDirty, bool quickEdit = false)
        {
            TileDesignerWindow window = (TileDesignerWindow)EditorWindow.GetWindow(typeof(TileDesignerWindow), true);
            window.titleContent = new GUIContent("Tile Designer");
            window.ProjectDesign = setup;
            window.ToDirty = toDirty;
            window.Show();
            _maximizedCurve = null;

            window.EditorMode = EMode.Setup;

            if (setup.TileMeshes != null)
                if (quickEdit && setup.TileMeshes.Count > 0)
                {
                    window.EditedTileSetup = setup.TileMeshes[0];
                    window.EditorMode = EMode.MeshEditor;
                }

            if (EditorGUIUtility.isProSkin == false)
            {
                window._curveCol = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            }

        }


        private void OnDestroy()
        {
            ClearScenePreview();
        }

        void OnBecomeInvisible()
        {
            ClearScenePreview();
            _maximizedCurve = null;
        }

        //private void OnDisable()
        //{
        //    //UnityEngine.Debug.Log("ondisable");
        //    Close();
        //}

        void OnGUI()
        {
            EditorGUIUtility.wideMode = true;

            scroll = GUILayout.BeginScrollView(scroll);

            if (Event.current != null)
            {
                windowMousePos = Event.current.mousePosition;

                if (Event.current.type == EventType.MouseDrag)
                {
                    _input_global_wasDrag += 1;
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    _input_global_wasDrag = 0;
                }

            }

            #region GUI Header

            GUILayout.BeginHorizontal();
            if (EditorMode == EMode.Setup) GUI.backgroundColor = Color.green;
            if (GUILayout.Button("", FGUI_Resources.ButtonStyle, GUILayout.Height(3))) { EditorMode = EMode.Setup; }
            if (EditorMode == EMode.MeshEditor) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
            if (GUILayout.Button("", FGUI_Resources.ButtonStyle, GUILayout.Height(3))) { EditorMode = EMode.MeshEditor; }
            if (EditorMode == EMode.Combine) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
            if (GUILayout.Button("", FGUI_Resources.ButtonStyle, GUILayout.Height(3))) { EditorMode = EMode.Combine; }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(7);

            if (EditorMode == EMode.Setup)
            {
                GUISetup();
            }
            else if (EditorMode == EMode.MeshEditor)
            {
                GUIEditor();
            }
            else
            {
                GUICombiner();
            }

            GUILayout.EndScrollView();

            if (repaintRequest || repaintRequestCounter > 0)
            {
                repaintRequest = false;
                repaintRequestCounter -= 1;
                SceneView.RepaintAll();
                Repaint();
            }

            if (ToDirty) EditorUtility.SetDirty(ToDirty);
        }

    }


}

#endif