using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using FIMSpace.Generating.Planning;

namespace FIMSpace.Generating
{
    [CustomPropertyDrawer(typeof(BuildPlannerPreset))]
    public class CPD_BuildPlannerPreset : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int preI = EditorGUI.indentLevel;
            if (EditorGUI.indentLevel > 1) EditorGUI.indentLevel = 1;

            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            Rect pos = new Rect(position);
            pos.width -= 42;

            EditorGUI.PropertyField(pos, property, GUIContent.none);

            pos.x += pos.width + 2;
            pos.width = 40;

            if (property.objectReferenceValue == null)
            {
                if (GUI.Button(pos, "New"))
                {
                    BuildPlannerPreset gen = (BuildPlannerPreset)FGenerators.GenerateScriptable(BuildPlannerPreset.CreateInstance<BuildPlannerPreset>(), "BuildPlanner_");
                    if (gen) property.objectReferenceValue = gen;
                }
            }
            else
            {
                if (GUI.Button(pos, "Open"))
                {
                    BuildPlannerPreset gen = (BuildPlannerPreset)property.objectReferenceValue;
                    AssetDatabase.OpenAsset(gen);
                }
            }



            //SerializedProperty spinj = property.FindPropertyRelative("Inject");
            //SerializedProperty spmod = spinj.Copy(); spmod.Next(false);
            //SerializedProperty spmodp = spmod.Copy(); spmodp.Next(false);

            //Rect p = position;
            //p.height = EditorGUIUtility.singleLineHeight;
            //p.width = 88;
            //EditorGUI.PropertyField(p, spinj, new GUIContent(""));

            //p = position;
            //p.position += Vector2.right * 82;
            //p.height = EditorGUIUtility.singleLineHeight;
            //p.width -= 88 + 80;

            //if (spinj.intValue == 0 || spinj.intValue == 2)
            //    EditorGUI.PropertyField(p, spmod, new GUIContent(""));
            //else
            //    EditorGUI.PropertyField(p, spmodp, new GUIContent(""));

            //SerializedProperty sp = spmodp.Copy();
            //sp.Next(false);

            //p.position += Vector2.right * (p.width - 8);
            //p.width = 68;
            //EditorGUI.PropertyField(p, sp, new GUIContent(""));

            //sp.Next(false);
            //p.position += Vector2.right * (p.width - 0);
            //p.width = 28;
            //EditorGUIUtility.labelWidth = 6;
            //EditorGUI.PropertyField(p, sp, new GUIContent(" ", "Enable toggle to override variables present in rules of this Field Modificator"));
            //EditorGUIUtility.labelWidth = 0;

            //if (sp.boolValue) // Overrides enabled
            //{
            //    FieldModification mod = (FieldModification)spmod.objectReferenceValue;
            //    ModificatorsPack modp = (ModificatorsPack)spmodp.objectReferenceValue;
            //    bool allow = false;

            //    if (spinj.intValue == 0 || spinj.intValue == 2) // Single Modificator
            //        if (mod != null) allow = true;
            //        else // Mod pack
            //        if (modp != null) allow = true;

            //    if (allow) // Modificator selected
            //    {
            //        sp.Next(false);

            //        List<FieldVariable> toOverride;

            //        if (spinj.intValue == 0) // Single Modificator
            //            toOverride = mod.TryGetFieldVariablesList();
            //        else if (spinj.intValue == 1)  // Mod pack
            //            toOverride = ModificatorsPackEditor.TryGetFieldVariablesList(modp);
            //        else // Mod pack as reference to variables
            //            toOverride = mod.TryGetFieldVariablesList();

            //        if (toOverride == null || toOverride.Count == 0)
            //        {
            //            allow = false;
            //        }
            //        else
            //        {
            //            List<FieldVariable> propList = (List<FieldVariable>)GetTargetObjectOfProperty(sp);

            //            if (propList != null)
            //            {
            //                #region Validating list count

            //                int targetCount = toOverride.Count;
            //                for (int i = propList.Count - 1; i >= 0; i--) if (propList[i] == null) propList.RemoveAt(i);
            //                if (propList.Count != targetCount)
            //                {
            //                    if (propList.Count < targetCount)
            //                    {
            //                        for (int i = 0; i < targetCount - propList.Count; i++)
            //                        {
            //                            var nQuest = new FieldVariable("", 1f);
            //                            propList.Add(nQuest);
            //                        }
            //                    }
            //                    else if (propList.Count > targetCount)
            //                    {
            //                        propList.RemoveRange(targetCount, propList.Count - targetCount);
            //                    }
            //                }

            //                #endregion

            //                if (propList.Count == toOverride.Count)
            //                    for (int i = 0; i < toOverride.Count; i++)
            //                    {
            //                        propList[i].Name = toOverride[i].Name;

            //                        p = position; p.height = EditorGUIUtility.singleLineHeight;
            //                        p.position += Vector2.up * EditorGUIUtility.singleLineHeight * (1 + i);

            //                        if (toOverride[i].ValueType == FieldVariable.EVarType.Material)
            //                        {
            //                            propList[i].SetValue((Material)EditorGUI.ObjectField(p, toOverride[i].Name, propList[i].GetMaterialRef(), typeof(Material), false));
            //                        }      
            //                        else if (toOverride[i].ValueType == FieldVariable.EVarType.GameObject)
            //                        {
            //                            propList[i].SetValue((GameObject)EditorGUI.ObjectField(p, toOverride[i].Name, propList[i].GetGameObjRef(), typeof(GameObject), false));
            //                        }
            //                        else if(toOverride[i].ValueType == FieldVariable.EVarType.Bool)
            //                        {
            //                            propList[i].SetValue(EditorGUI.Toggle(p, toOverride[i].Name, propList[i].GetBoolValue()));
            //                        }
            //                        else
            //                        {
            //                            if (toOverride[i].helper != Vector3.zero)
            //                                propList[i].Float = EditorGUI.Slider(p, toOverride[i].Name, propList[i].Float, toOverride[i].helper.x, toOverride[i].helper.y);
            //                            else
            //                                propList[i].Float = EditorGUI.FloatField(p, toOverride[i].Name, propList[i].Float);
            //                        }
            //                    }
            //            }
            //        }
            //    }

            //    if (!allow)
            //    {
            //        p = position; p.height = EditorGUIUtility.singleLineHeight;
            //        p.position += Vector2.up * EditorGUIUtility.singleLineHeight;
            //        EditorGUI.LabelField(p, "Nothing to override", EditorStyles.centeredGreyMiniLabel);
            //    }
            //}

            //sp.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
            EditorGUI.indentLevel = preI;
        }

        #region Editor get helper variables

        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        #endregion

    }
}