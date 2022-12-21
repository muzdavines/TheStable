#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Hidden
{
    public static class PGGMenuItems
    {

        [MenuItem("Assets/Create/FImpossible Creations/Procedural Generation/Create Build Planner Coded Node", false, 1100)]
        static void CreateNewCodedPlannerRule()
        {
            TextAsset text = Resources.Load("Templates/PR_PlannerCodedNodeTemplate.cs") as TextAsset;
            string path = AssetDatabase.GetAssetPath(text);
            string fullPath = Application.dataPath + path.Replace("Assets", "");
            CreateScriptAsset(path, "CustomCodedNode.cs", fullPath);
        }


        [MenuItem("Assets/Create/FImpossible Creations/Procedural Generation/Scripting Templates/Create Spawn Rule Script Template", false, 400)]
        static void CreateNewSpawnRule()
        {
            //string path = "Assets/FImpossible Creations/Plugins - Level Design/PGG - Procedural Generation Grid/Rules Logics/Utilities/Templates/SR_SpawnRuleTemplate.cs.txt";
            //string fullPath = Application.dataPath + path.Replace("Assets", "");
            TextAsset text = Resources.Load("Templates/SR_SpawnRuleTemplate.cs") as TextAsset;
            string path = AssetDatabase.GetAssetPath(text);
            string fullPath = Application.dataPath + path.Replace("Assets", "");
            CreateScriptAsset(path, "CustomSpawnRule.cs", fullPath);
        }


        [MenuItem("Assets/Create/FImpossible Creations/Procedural Generation/Scripting Templates/Create Spawn Rule Script Template (Empty)", false, 401)]
        static void CreateNewSpawnRuleEmpty()
        {
            //string path = "Assets/FImpossible Creations/Plugins - Level Design/PGG - Procedural Generation Grid/Rules Logics/Utilities/Templates/SR_SpawnRuleTemplateEmpty.cs.txt";
            TextAsset text = Resources.Load("Templates/SR_SpawnRuleTemplateEmpty.cs") as TextAsset;
            string path = AssetDatabase.GetAssetPath(text);
            string fullPath = Application.dataPath + path.Replace("Assets", "");
            CreateScriptAsset(path, "CustomSpawnRule.cs", fullPath);
        }


        [MenuItem("Assets/Create/FImpossible Creations/Procedural Generation/Scripting Templates/Create Custom Generator Script", false, 402)]
        static void CreateNewGeneratorScript()
        {
            //string path = "Assets/FImpossible Creations/Plugins - Level Design/PGG - Procedural Generation Grid/Rules Logics/Utilities/Templates/SR_SpawnRuleTemplateEmpty.cs.txt";
            TextAsset text = Resources.Load("Templates/PGGGeneratorScriptTemplate.cs") as TextAsset;
            string path = AssetDatabase.GetAssetPath(text);
            string fullPath = Application.dataPath + path.Replace("Assets", "");
            CreateScriptAsset(path, "CustomGeneratorScript.cs", fullPath);
        }


        [MenuItem("Window/FImpossible Creations/Level Design/PGG Video Tutorials...", false, 403)]
        public static void OpenWebsiteTutorials()
        {
            Application.OpenURL("https://www.youtube.com/watch?v=MjdGnLfh3O4");
        }


        [MenuItem("Window/FImpossible Creations/Level Design/PGG Manual...", false, 404)]
        public static void OpenWebsiteManual()
        {
            Application.OpenURL("http://filipmoeglich.pl/download/Procedural%20Generation%20Grid%20-%20User%20Manual.pdf");
        }

        //[MenuItem("Window/FImpossible Creations/Level Design/Where are Demos?", false, 1005)]
        //public static void ShowDemosInfoDialog()
        //{
        //    EditorUtility.DisplayDialog("Where are Demos?", "Demos are inside unitypackage under 'Assets/Fimpossible Creations/Plugins - Level Design/PGG' and useful components are in the same directory/Components", "Ok");
        //}

        //[MenuItem("Window/FImpossible Creations/Level Design/PGG Asset Store Page", false, 1006)]
        public static void OpenPGGAssetStorePage()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/procedural-generation-grid-beta-195535");
        }


        static void CreateScriptAsset(string templatePath, string targetName, string fullPath)
        {
            if (File.Exists(fullPath))
            {
#if UNITY_2019_1_OR_NEWER
                UnityEditor.ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, targetName);
#else
                typeof(UnityEditor.ProjectWindowUtil)
                    .GetMethod("CreateScriptAsset", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(null, new object[] { templatePath, targetName });
#endif
            }
            else
                Debug.LogError("File under path '" + fullPath + "' doesn't exist, directory probably was moved");
        }

    }
}

#endif