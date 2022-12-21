using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(FieldSetup))]
    public class FieldSetupEditor : UnityEditor.Editor
    {
        public FieldSetup Get { get { if (_get == null) _get = (FieldSetup)target; return _get; } }
        private FieldSetup _get;

        private void OnEnable()
        {
            FieldModificationEditor.CleanupAndGetUnused(Get);
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Field Setup in designer window", GUILayout.Height(38))) AssetDatabase.OpenAsset(Get);
            EditorGUILayout.HelpBox("Field Setup should be edited through FieldDesigner window", MessageType.Info);

            GUILayout.Space(4f);

            DrawDefaultInspector();
            GUILayout.Space(4f);

            if (GUILayout.Button("Rename"))
            {
                string filename = EditorUtility.SaveFilePanelInProject("Type your title (no file will be created)", Get.name, "", "Type your title (no file will be created)");
                if (!string.IsNullOrEmpty(filename))
                {
                    filename = System.IO.Path.GetFileNameWithoutExtension(filename);
                    if (!string.IsNullOrEmpty(filename))
                    {
                        Get.name = filename;
                        string path = AssetDatabase.GetAssetPath(Get);
                        string noExt = path.Replace(".asset", "");
                        noExt += filename + ".asset";

                        AssetDatabase.RenameAsset(path, noExt);
                        EditorUtility.SetDirty(Get);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Get));
                    }
                }
            }

            GUILayout.Space(4f);

            if (GUILayout.Button("Scan for unparented modificators"))
            {

                var modsIn = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(this));
                for (int m = 0; m < modsIn.Length; m++)
                {
                    var file = modsIn[m];
                    if ( file is FieldModification)
                    {
                        FieldModification mod = file as FieldModification;

                        for (int c = 0; c < Get.CellsCommands.Count; c++)
                        {
                            if(Get.CellsCommands[c].TargetModification == mod || Get.CellsCommands[c].extraMod == mod)
                            {
                                if (Get.UtilityModificators.Contains(mod) == false)
                                {
                                    Get.UtilityModificators.Add(mod);
                                }
                            }
                        }
                    }
                }
            }


            if (GUILayout.Button("Search for unused files in FielSetup and remove them"))
            {
                var unuses = FieldModificationEditor.CleanupAndGetUnused(Get, true);

                if (unuses.Count > 0)
                {
                    string fileNames = "";

                    for (int u = 0; u < unuses.Count; u++)
                    {
                        if (string.IsNullOrEmpty(unuses[u].name) == false) fileNames += unuses[u].name + "   ";
                    }

                    if (EditorUtility.DisplayDialog("Attempt to remove " + unuses.Count + " Files", "Remove Pernamently Field Modificators and Spawn Rules found inside FieldSetup file which seems to belong nowhere?\n\nFound " + unuses.Count + " unused files.\n" + fileNames, "Yes", "No"))
                    {
                        for (int i = unuses.Count - 1; i >= 0; i--)
                        {
                            GameObject.DestroyImmediate(unuses[i], true);
                        }

                        UnityEditor.EditorUtility.SetDirty(Get);
                        UnityEditor.AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("No Unusuded Files", "Not found any unused files inside FieldSetup file", "Ok");
                }
            }

            GUILayout.Space(4f);

        }
    }
}