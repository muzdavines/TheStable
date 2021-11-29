using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class BlueManaEditor : EditorWindow
{
    [MenuItem("Blue Mana/Utils")]
    static void Init() {
        BlueManaEditor window = (BlueManaEditor)EditorWindow.GetWindow(typeof(BlueManaEditor));
        window.Show();
    }

    private void OnGUI() {
        if (GUILayout.Button("Create Saved Characters")) {
            for (int i = 0; i < 50; i++) {
                var example = ScriptableObject.CreateInstance<Character>();
                string path = "Assets/_Characters/Resources/SavedCharacters/Char" + i + ".asset";
                AssetDatabase.CreateAsset(example, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = example;
            }
        }
    }
}
