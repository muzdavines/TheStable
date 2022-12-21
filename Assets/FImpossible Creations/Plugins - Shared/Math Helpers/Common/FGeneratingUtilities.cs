#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// FM: Class which contains many helpful methods for generating files in project mainly for editor use
/// </summary>
public static class FGeneratingUtilities
{

    #region Instantiating Related

    public static GameObject InstantiateObject(GameObject obj)
    {
#if UNITY_EDITOR
        GameObject newObj = null;

        if (Application.isPlaying == false && (PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.Regular || PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.Variant))
            newObj = (GameObject)PrefabUtility.InstantiatePrefab(obj);

        if (newObj == null) newObj = GameObject.Instantiate(obj);
        return newObj;
#else
                return GameObject.Instantiate(obj);
#endif
    }


    public static void DestroyObject(GameObject obj)
    {
        if (obj == null) return;

#if UNITY_EDITOR
        if (Application.isPlaying == false)
            GameObject.DestroyImmediate(obj);
        else
            GameObject.Destroy(obj);
#else
                GameObject.Destroy(obj);
#endif
    }

    #endregion


    #region Defined Seed Random Handling

    static Dictionary<int, System.Random> randoms = new Dictionary<int, System.Random>();
    static System.Random GetR(int randomId)
    {
        System.Random r;

        if (randoms.ContainsKey(randomId) == false)
        {
            r = new System.Random(randomId);
            randoms.Add(randomId, r);
        }
        else
        {
            r = randoms[randomId];
        }

        return r;
    }


    static System.Random random = new System.Random();

    public static void SetSeed(int seed)
    {
        random = new System.Random(seed);
    }

    public static void SetSeed(int seed, int randomId)
    {
        if (randoms.ContainsKey(randomId) == false) randoms.Add(randomId, new System.Random(randomId));
        else randoms[randomId] = new System.Random(randomId);
    }

    public static float GetRandom()
    {
        return (float)random.NextDouble();
    }

    public static float GetRandom(int randomId)
    {
        return (float)GetR(randomId).NextDouble();
    }

    public static float GetRandom(float from, float to)
    {
        return from + (float)random.NextDouble() * (to - from);
    }

    public static float GetRandom(float from, float to, int randomId)
    {
        return from + (float)GetR(randomId).NextDouble() * (to - from);
    }

    public static int GetRandom(int from, int to)
    {
        return random.Next(from, to);
    }

    public static int GetRandom(int from, int to, int randomId)
    {
        return GetR(randomId).Next(from, to);
    }

    public static int GetRandom(MinMax minMax)
    {
        return (int)(minMax.Min + (float)random.NextDouble() * ((minMax.Max + 1) - minMax.Min));
    }

    public static int GetRandom(MinMax minMax, int randomId)
    {
        return (int)(minMax.Min + (float)GetR(randomId).NextDouble() * ((minMax.Max + 1) - minMax.Min));
    }

    #endregion


    #region GUI or Editor GUI Related


#if UNITY_EDITOR

    public static void DrawGenerateScriptableField<T>(ref T selected, string exampleFilename = "") where T : ScriptableObject
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUIUtility.labelWidth = 60;
        selected = (T)EditorGUILayout.ObjectField("Preset:", selected, typeof(T), false);
        DrawGenerateScriptableButton<T>(ref selected, exampleFilename);

        EditorGUILayout.EndHorizontal();
    }

    public static void DrawGenerateScriptableButton<T>(ref T selected, string exampleFilename = "") where T : ScriptableObject
    {
        if (GUILayout.Button("Create New", GUILayout.Width(94))) selected = (T)GenerateScriptable(GameObject.Instantiate(selected), exampleFilename);
    }

    public static void DrawObjectList<T>(List<T> toDraw, GUIStyle style, string title, ref bool foldout, bool moveButtons = false, UnityEngine.Object toDirty = null) where T : UnityEngine.Object
    {
        if (toDraw == null) return;

        EditorGUILayout.BeginVertical(style);

        EditorGUILayout.BeginHorizontal();
        string fold = foldout ? " ▼" : " ►";
        if (GUILayout.Button(fold + "  " + title + " (" + toDraw.Count + ")", EditorStyles.label, GUILayout.Width(200))) foldout = !foldout;

        if (foldout)
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+")) toDraw.Add(null);
        }

        EditorGUILayout.EndHorizontal();

        if (foldout)
        {
            GUILayout.Space(4);

            if (toDraw.Count > 0)
                for (int i = 0; i < toDraw.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.BeginHorizontal();

                    GUIContent lbl = new GUIContent("[" + i + "]");
                    float wdth = EditorStyles.label.CalcSize(lbl).x;

                    EditorGUILayout.LabelField(lbl, GUILayout.Width(wdth + 2));

                    toDraw[i] = (T)EditorGUILayout.ObjectField(toDraw[i], typeof(T), false);

                    if (moveButtons)
                    {
                        if (i > 0) if (GUILayout.Button("˄", GUILayout.Width(24))) { T temp = toDraw[i - 1]; toDraw[i - 1] = toDraw[i]; toDraw[i] = temp; }
                        if (i < toDraw.Count - 1) if (GUILayout.Button("˅", GUILayout.Width(24))) { T temp = toDraw[i + 1]; toDraw[i + 1] = toDraw[i]; toDraw[i] = temp; }
                    }

                    if (GUILayout.Button("X", GUILayout.Width(24))) { toDraw.RemoveAt(i); break; }

                    EditorGUILayout.EndHorizontal();
                    if (toDirty != null) if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(toDirty); }
                }
            else
            {
                EditorGUILayout.LabelField("No object in list", EditorStyles.centeredGreyMiniLabel);
            }
        }

        EditorGUILayout.EndVertical();
    }

#endif

    #endregion


    #region Helper Class for Minimum and Maximum

    [System.Serializable]
    public struct MinMax
    {
        public int Min;
        public int Max;

        public MinMax(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public int GetRandom()
        {
            return (int)(Min + (float)GetRandom() * ((Max + 1) - Min));
        }

#if UNITY_EDITOR

        public static MinMax DrawGUI(MinMax target, GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();
            float width = EditorStyles.label.CalcSize(label).x;
            EditorGUIUtility.labelWidth = width + 4;
            EditorGUILayout.LabelField(label, GUILayout.Width(width));

            GUILayout.Space(28);
            EditorGUIUtility.labelWidth = 28;
            target.Min = EditorGUILayout.IntField("Min", target.Min, GUILayout.Width(70));

            //GUILayout.FlexibleSpace();
            GUILayout.Space(24);
            EditorGUIUtility.labelWidth = 32;
            target.Max = EditorGUILayout.IntField("Max", target.Max, GUILayout.Width(74));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            if (target.Min < 0) target.Min = 0;
            if (target.Max < 0) target.Max = 0;
            if (target.Min > target.Max) target.Max = target.Min;
            if (target.Max < target.Min) target.Min = target.Max;

            return target;
        }

#endif

    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MinMax))]
    public class MinMaxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect srcPos = position;
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUIUtility.labelWidth = 30;
            float labelW = EditorStyles.label.CalcSize(label).x;
            var vRect = new Rect(srcPos.width - 60 * 2f - 10, position.y, 60, position.height);

            int preInd = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            SerializedProperty sp_min = property.FindPropertyRelative("Min");
            EditorGUI.PropertyField(vRect, sp_min, new GUIContent(sp_min.displayName));

            vRect = new Rect(srcPos.width - 60, position.y, 60, position.height);
            SerializedProperty sp_max = property.FindPropertyRelative("Max");
            EditorGUI.PropertyField(vRect, sp_max, new GUIContent(sp_max.displayName));

            if (sp_min.intValue < 0) sp_min.intValue = 0;
            if (sp_max.intValue < 0) sp_max.intValue = 0;
            if (sp_min.intValue > sp_max.intValue) sp_max.intValue = sp_min.intValue;
            if (sp_max.intValue < sp_min.intValue) sp_min.intValue = sp_max.intValue;
            EditorGUI.indentLevel = preInd;

            EditorGUIUtility.labelWidth = 0;
            EditorGUI.EndProperty();
        }
    }
#endif


    [System.Serializable]
    public struct MinMaxF
    {
        public float Min;
        public float Max;

        public MinMaxF(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MinMaxF))]
    public class MinMaxFDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect srcPos = position;
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUIUtility.labelWidth = 30;
            float labelW = EditorStyles.label.CalcSize(label).x;
            var vRect = new Rect(srcPos.width - 60 * 2f - 10, position.y, 60, position.height);

            SerializedProperty sp_min = property.FindPropertyRelative("Min");
            EditorGUI.PropertyField(vRect, sp_min, new GUIContent(sp_min.displayName));

            vRect = new Rect(srcPos.width - 60, position.y, 60, position.height);
            SerializedProperty sp_max = property.FindPropertyRelative("Max");
            EditorGUI.PropertyField(vRect, sp_max, new GUIContent(sp_max.displayName));

            if (sp_min.floatValue < 0) sp_min.floatValue = 0;
            if (sp_max.floatValue < 0) sp_max.floatValue = 0;
            if (sp_min.floatValue > sp_max.floatValue) sp_max.floatValue = sp_min.floatValue;
            if (sp_max.floatValue < sp_min.floatValue) sp_min.floatValue = sp_max.floatValue;

            EditorGUIUtility.labelWidth = 0;
            EditorGUI.EndProperty();
        }
    }
#endif


    #endregion


    #region Scriptable Related

    public static string lastPath = "";
    public static ScriptableObject GenerateScriptable(ScriptableObject reference, string exampleFilename = "", string playerPrefId = "")
    {
#if UNITY_EDITOR
        if (lastPath == "")
        {
            if (EditorPrefs.HasKey(playerPrefId) && string.IsNullOrEmpty(playerPrefId) == false)
            {
                lastPath = EditorPrefs.GetString(playerPrefId);
                if (!System.IO.File.Exists(lastPath)) lastPath = Application.dataPath;
            }
            else lastPath = Application.dataPath;
        }

        string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Generate Preset File", exampleFilename, "asset", "Enter name of file", lastPath);

        try
        {
            if (path != "")
            {
                lastPath = System.IO.Path.GetDirectoryName(path);
                EditorPrefs.SetString(playerPrefId, lastPath);
                UnityEditor.AssetDatabase.CreateAsset(reference, path);
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("Something went wrong when creating scriptable file in your project.");
        }

#endif
        return reference;
    }

    #endregion


    #region Final Utilities

    public static void CheckForNulls<T>(List<T> elements)
    {
        for (int i = elements.Count - 1; i >= 0; i--)
        {
            if (elements[i] == null) elements.RemoveAt(i);
        }
    }

    public static T GetRandomElement<T>(this List<T> list, bool unityRandom = false) 
    {
        if (list == null) return default(T);
        if (list.Count == 1) return list[0];
        if (unityRandom) return list[UnityEngine.Random.Range(0, list.Count)];
        return list[GetRandom(0, list.Count)];
    }

    #endregion

}


