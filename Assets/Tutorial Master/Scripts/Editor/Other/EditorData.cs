using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <summary>
    /// Stores data such as currently selected stages and tutorials.
    /// </summary>
    [Serializable]
    public class EditorData : ScriptableObject, ISerializationCallbackReceiver
    {
        private const string EditorDataDir = "/Scripts/Editor/editordata.asset";
        private static EditorData _editorData;

        public Vector2 ScrollViewStagesList;
        public Vector2 ScrollViewTutorialsList;

        public int SelectedLanguageIndex = 0;
        public int SelectedStageIndex = 0;
        public int SelectedTutorialIndex = 0;

        public bool ShowAllModules = false;
        public int SelectedModuleTabIndex = 0;

        public Dictionary<string, bool> SectionToggles = new Dictionary<string, bool>();

        [SerializeField]
        private List<string> sectionTogglesKeys = new List<string>();

        [SerializeField]
        private List<bool> sectionTogglesValues = new List<bool>();

        /// <summary>
        /// Gets the Editor Data. If it doesn't exist, then a new one is created.
        /// </summary>
        public static EditorData Data
        {
            get
            {
                if (_editorData == null)
                {
                    FileInfo dataDir = new FileInfo(TMEditorUtils.DirectoryPath + "/Scripts/Editor/");
                    if (dataDir.Directory != null) dataDir.Directory.Create();

                    _editorData = (EditorData)AssetDatabase.LoadAssetAtPath(
                        TMEditorUtils.DirectoryPath + EditorDataDir,
                        typeof(EditorData));

                    if (_editorData == null)
                    {
                        _editorData = CreateInstance<EditorData>();
                        AssetDatabase.CreateAsset(
                            _editorData,
                            TMEditorUtils.DirectoryPath + EditorDataDir);
                        AssetDatabase.SaveAssets();
                    }
                }

                return _editorData;
            }
        }

        public void OnAfterDeserialize() 
        {
            SectionToggles = new Dictionary<string, bool>();

            for (int i = 0; i != Math.Min(sectionTogglesKeys.Count, sectionTogglesValues.Count); i++)
            {
                SectionToggles.Add(sectionTogglesKeys[i], sectionTogglesValues[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            sectionTogglesKeys.Clear();
            sectionTogglesValues.Clear();

            foreach (KeyValuePair<string, bool> pair in SectionToggles)
            {
                sectionTogglesKeys.Add(pair.Key);
                sectionTogglesValues.Add(pair.Value);
            }
        }
    }
}