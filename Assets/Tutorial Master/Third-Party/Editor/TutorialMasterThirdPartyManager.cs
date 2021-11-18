using HardCodeLab.TutorialMaster.EditorUI;
using UnityEditor;

namespace HardCodeLab.TutorialMaster.Utils
{
    public class TutorialMasterThirdPartyManager : Editor
    {
        private static string _packagesDir;

        private static string PackagesDir
        {
            get
            {
                if (string.IsNullOrEmpty(_packagesDir))
                {
                    _packagesDir = TMEditorUtils.DirectoryPath + "/Third-Party/Compressed/";
                }

                return _packagesDir;
            }
        }

        private const string BaseMenuItem = "Window/Tutorial Master/Import Third-Party/";

        [MenuItem(BaseMenuItem + "TextMesh Pro", false, 0)]
        public static void ImportTMP()
        {
            ExtractPackage("textmeshpro");
        }

        [MenuItem(BaseMenuItem + "PlayMaker", false, 0)]
        public static void ImportPlayMaker()
        {
            ExtractPackage("playmaker");

        }

        [MenuItem(BaseMenuItem + "Import All", false, 51)]
        public static void ImportAll()
        {
            ImportPlayMaker();
            ImportTMP();
        }

        private static void ExtractPackage(string packageName)
        {
            AssetDatabase.ImportPackage(string.Format("{0}{1}.unitypackage", PackagesDir, packageName), false);
        }
    }
}