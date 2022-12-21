using System.Collections.Generic;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileDesignPreset : ScriptableObject
    {
        public TileDesign BaseDesign
        {
            get
            {
                if (Designs.Count > 0) return Designs[0];
                Designs.Add(new TileDesign());
                return Designs[0];
            }
        }

        public List<TileDesign> Designs = new List<TileDesign>();

    }


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(TileDesignPreset))]
    public class TileDesignPresetEditor : UnityEditor.Editor
    {
        public TileDesignPreset Get { get { if (_get == null) _get = (TileDesignPreset)target; return _get; } }
        private TileDesignPreset _get;

        #region Open window on double-click on TileDesign File

        [OnOpenAssetAttribute(1)]
        public static bool OpenFieldScriptableFile(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);

            if (obj as TileDesignPreset != null)
            {
                TileDesignPreset pres = obj as TileDesignPreset;
                TileDesignerWindow.Init(pres.Designs[0], pres);
                return true;
            }

            return false;
        }

        #endregion

        int toOpen = 0;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();


            GUILayout.Space(8f);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(FGUI_Resources.Tex_ArrowLeft, GUILayout.Width(22)))
            {
                toOpen += 1;
            }

            GUILayout.Label((toOpen + 1) + "/" + Get.Designs.Count, EditorStyles.centeredGreyMiniLabel);

            if (GUILayout.Button(FGUI_Resources.Tex_ArrowRight, GUILayout.Width(22)))
            {
                toOpen -= 1;
            }

            if (toOpen < 0) toOpen = Get.Designs.Count - 1;
            if (toOpen >= Get.Designs.Count) toOpen = 0;

            TileDesign targetDes = Get.BaseDesign;
            if ( FLogicMethods.ContainsIndex(Get.Designs, toOpen))
            {
                targetDes =Get.Designs[toOpen];
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4f);

            if (GUILayout.Button("Open '" + targetDes.DesignName + "' In Tile Designer Window"))
            {
                TileDesignerWindow.Init(Get.BaseDesign, Get);
            }
        }
    }
#endif

}