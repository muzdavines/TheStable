using System.IO;
using UnityEditor;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <summary>
    /// Some common functions used by Tutorial Master
    /// </summary>
    public static class TMEditorUtils
    {
        /// <summary>
        /// Directory path in which Tutorial Master resides in
        /// </summary>
        private static string _directoryPath;

        /// <summary>
        /// Returns a directory where Tutorial Master resides in
        /// </summary>
        public static string DirectoryPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_directoryPath)) return _directoryPath;

                string[] results = AssetDatabase.FindAssets("Tutorial Master");

                foreach (string result in results)
                {
                    string path = AssetDatabase.GUIDToAssetPath(result);

                    if (Path.GetFileName(path) != "Tutorial Master") continue;
                    _directoryPath = path;
                    break;
                }

                return _directoryPath;
            }
        }
    }
}