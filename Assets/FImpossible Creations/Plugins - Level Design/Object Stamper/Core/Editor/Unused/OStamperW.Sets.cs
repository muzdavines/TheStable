using UnityEngine;
using UnityEditor;

namespace FIMSpace.Generating
{
    //public partial class ObjectsStamperWindow : EditorWindow
    //{
    //    int selectedPrefab = 0;
    //    Vector2 pfListScroll = Vector2.zero;
    //    bool drawPfThumbnails = true;
    //    bool drawPrefabsList = true;
    //    void DrawObjectsSet(OStamperSet set)
    //    {
    //        if (selectedPreset == null) return;

    //        Color bg = GUI.backgroundColor;
    //        SerializedObject so = new SerializedObject(selectedPreset);

    //        EditorGUILayout.BeginHorizontal();
    //        EditorGUIUtility.labelWidth = 60;
    //        projectPreset = (OStamperSet)EditorGUILayout.ObjectField("Preset:", projectPreset, typeof(OStamperSet), false);
    //        if (GUILayout.Button("Create New", GUILayout.Width(94))) projectPreset = (OStamperSet)GenerateScriptable(Instantiate(selectedPreset), "OP_");
    //        EditorGUILayout.EndHorizontal();

    //        so.Update();
    //        SerializedProperty iter = so.GetIterator();

    //        if (iter != null)
    //        {
    //            iter.Next(true);
    //            iter.NextVisible(false);


    //            // Drawing prefabs list
    //            if (selectedPreset.Prefabs != null)
    //            {
    //                GUILayout.Space(3);
    //                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

    //                EditorGUI.BeginChangeCheck();
    //                PrefabReference.DrawPrefabsList(selectedPreset.Prefabs, ref drawPrefabsList, ref selectedPrefab, ref drawPfThumbnails, Color.gray, Color.green, EditorGUIUtility.currentViewWidth, 72, true, selectedPreset);
    //                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(selectedPreset);

    //                EditorGUILayout.EndVertical();
    //            }

    //        }

    //        so.ApplyModifiedProperties();
    //        EditorGUIUtility.labelWidth = 0;
    //    }


    //    public static string lastPath = "";
    //    public static ScriptableObject GenerateScriptable(ScriptableObject reference, string exampleFilename = "")
    //    {
    //        if (lastPath == "")
    //        {
    //            if (EditorPrefs.HasKey("LastSaveOStampDir"))
    //            {
    //                lastPath = EditorPrefs.GetString("LastSaveOStampDir");
    //                if (!System.IO.File.Exists(lastPath)) lastPath = Application.dataPath;
    //            }
    //            else lastPath = Application.dataPath;
    //        }

    //        string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Generate Preset File", exampleFilename, "asset", "Enter name of file", lastPath);

    //        try
    //        {
    //            if (path != "")
    //            {
    //                lastPath = System.IO.Path.GetDirectoryName(path);
    //                EditorPrefs.SetString("LastSaveOStampDir", lastPath);
    //                UnityEditor.AssetDatabase.CreateAsset(reference, path);
    //            }
    //        }
    //        catch (System.Exception)
    //        {
    //            Debug.LogError("Something went wrong when creating scriptable file in your project.");
    //        }

    //        return reference;
    //    }

    //}
}