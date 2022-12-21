using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;

#if UNITY_EDITOR && !UNITY_CLOUD_BUILD
namespace FIMSpace.Hidden
{
    public sealed class PGGStartupWindow : EditorWindow
    {
        public static PGGStartupWindow Get;
        public PGGStartupReferences StartupRefs;
        
        [MenuItem("Window/FImpossible Creations/Level Design/Display PGG Startup Window", false, 1151)]
        static void Init()
        {
            PGGStartupWindow window = (PGGStartupWindow)GetWindow<PGGStartupWindow>(true, "PGG Startup", true);
            Get = window;

            if (Get.StartupRefs == null)
            {
                UnityEngine.Debug.Log("[PGG] Not found startup window references file!");
                return;
            }


            window.Show();
            window.position = new Rect(window.position.x + 60, window.position.y + 60, Screen.currentResolution.width * 0.2f, Screen.currentResolution.height * 0.425f);
            window.maxSize = new Vector2(800, 1000);
            EditorPrefs.SetInt("PGGStart", EditorPrefs.GetInt("PGGStart", 0) + 1);
        }

        static int framesWait = 61;
        [InitializeOnLoadMethod]
        private static void OnReload()
        {
            if (EditorPrefs.GetInt("PGGStart", 0) > 0) return;
            framesWait = 61;
            EditorApplication.update += ReloadStartup;
        }

        private static void ReloadStartup()
        {
            if (framesWait > 0)
            {
                --framesWait;
            }
            else
            {
                EditorApplication.update -= ReloadStartup;
                Init();
            }
        }

        Vector2 scroll = Vector2.zero;
        private void OnGUI()
        {
            //EditorPrefs.SetInt("PGGStart", 0);
            if (Get == null) Init();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            Texture2D headerImage = StartupRefs.StartupImage;
            if (headerImage)
            {
                float titleScaledWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.standardVerticalSpacing * 4;
                float titleScaledHeight = titleScaledWidth * ((float)headerImage.height / (float)headerImage.width);
                Rect titleRect = EditorGUILayout.GetControlRect();
                titleRect.width = titleScaledWidth;
                titleRect.height = titleScaledHeight;
                GUI.DrawTexture(titleRect, headerImage, ScaleMode.ScaleToFit);
                GUILayout.Label("", GUILayout.Height(titleScaledHeight - 20));
            }

            FGUI_Inspector.DrawUILine(0.65f, 0.2f, 2, 7);
            EditorGUILayout.LabelField("PGG STARTUP WINDOW", FGUI_Resources.HeaderStyleBig);
            FGUI_Inspector.DrawUILine(0.65f, 0.2f, 2, 7);

            if (GUILayout.Button("Check Quick Start.pdf File")) { AssetDatabase.OpenAsset(StartupRefs.QuickStartFile); }
            if (GUILayout.Button("Check User Manual.pdf File")) { AssetDatabase.OpenAsset(StartupRefs.ManualFile); }
            if (GUILayout.Button("Check Tutorial Videos")) { PGGMenuItems.OpenWebsiteTutorials(); }
            if (GUILayout.Button("Go to PGG Asset Store Page")) { PGGMenuItems.OpenPGGAssetStorePage(); }

            FGUI_Inspector.DrawUILine(0.65f, 0.2f, 2, 7);

            if (GUILayout.Button("Import PGG DEMOS Examples")) 
            {
#if UNITY_2019_1_OR_NEWER
                AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(StartupRefs.DemosPackage), true);
#else
                if (EditorUtility.DisplayDialog("Unity Back Compability Fails", "Unfortunatelly Unity does not support back compatibility for prefabs on unity 2018 or lower.\nPlease consider using Unity 2019.4 + for demo scenes as learning version.", "Import", "Ok, I will use newer Unity Version to check Demos"))
                {
                    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(StartupRefs.DemosPackage), true);
                }
#endif
            }

            if (StartupRefs.PGGdirectory != null)
                if (GUILayout.Button("Go to PGG Directory")) { EditorGUIUtility.PingObject(StartupRefs.PGGdirectory); }

            FGUI_Inspector.DrawUILine(0.65f, 0.2f, 2, 7);

#if UNITY_2018_2_OR_NEWER
#else
            if (GUILayout.Button("Info for Unity 2017-2018.1 Users"))
            {
                EditorUtility.DisplayDialog("Dear Unity 2017-2018 Users", "Unfortunatelly Unity does not support back compatibility for prefabs on this versions.\nSince examples database is so big (258 prefabs) I can't afford another weeks of making example assets,\nso please consider using Unity 2019.4 + for demo scenes as learning version.", "Ok");
            }
#endif


            EditorGUILayout.HelpBox("You can dispaly this window again by going to: Window/Fimpossible Creations/Level Design/Display PGG Startup Window", MessageType.Info);
            EditorGUILayout.HelpBox("You will find PGG and other Level Design plugins under: Assets/Fimpossible Creations/Plugins - Level Design", MessageType.Info);

            EditorGUILayout.EndScrollView();
        }

    }
}
#endif