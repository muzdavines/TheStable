#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class FieldVariable
    {

        public static float DrawSliderFor(float val, float min, float max)
        {
            if ((min > 0 && max == 0f) /*|| (min == 0 && max > 0)*/)
                val = EditorGUILayout.FloatField(" ", val);
            else
                val = EditorGUILayout.Slider(" ", val, min, max);

            if (min > 0 && max == 0f)
            { if (val < min) val = min; }

            return val;
        }

        public static int DrawSliderForInt(float valf, float minf, float maxf)
        {
            int val = Mathf.RoundToInt(valf);
            int min = Mathf.RoundToInt(minf);
            int max = Mathf.RoundToInt(maxf);

            if ((min > 0 && max == 0f) /*|| (min == 0 && max > 0)*/)
                val = EditorGUILayout.IntField(" ", val);
            else
                val = EditorGUILayout.IntSlider(" ", val, min, max);

            if (min > 0 && max == 0f)
            { if (val < min) val = min; }

            return val;
        }

        public static object Editor_DrawTweakableVariable(FieldVariable toDraw, object helper = null)
        {
            if (toDraw == null) return null;
            object ret = null;
            var v = toDraw;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(3);

            GUIContent cName = new GUIContent(v.Name);
            float width = EditorStyles.boldLabel.CalcSize(cName).x + 6;
            if (width > 220) width = 220;

            EditorGUILayout.LabelField(v.Name, EditorStyles.boldLabel, GUILayout.Width(width));
            GUILayout.Space(6);

            if (v.ValueType == FieldVariable.EVarType.Number)
            {
                EditorGUIUtility.labelWidth = 10;

                if (v.FloatSwitch == EVarFloatingSwitch.Float)
                {
                    if (v.helper == Vector3.zero) v.Float = EditorGUILayout.FloatField(" ", v.Float);
                    //else v.Float = EditorGUILayout.Slider(" ", v.Float, v.helper.x, v.helper.y);
                    else v.Float = DrawSliderFor(v.Float, v.helper.x, v.helper.y);
                }
                else
                {
                    bool drawed = false;
                    if (toDraw.helpForFieldCommand && toDraw.helpForFieldCommandRef != null)
                    {
                        var intIds = toDraw.GetVariablesIDList();
                        if (intIds != null)
                        {
                            drawed = true;
                            v.IntV = EditorGUILayout.IntPopup(v.IntV, v.GetVariablesNameList(), v.GetVariablesIDList());
                        }
                    }

                    if (drawed == false)
                    {
                        if (v.helper == Vector3.zero) v.IntV = EditorGUILayout.IntField(" ", v.GetIntValue());
                        else
                        {
                            //v.Float = EditorGUILayout.Slider(" ", v.GetIntValue(), v.helper.x, v.helper.y);
                            v.IntV = DrawSliderForInt(v.Float, v.helper.x, v.helper.y);
                        }
                    }
                }
            }
            else if (v.ValueType == FieldVariable.EVarType.Bool)
            {
                EditorGUIUtility.labelWidth = 70;
                v.SetValue(EditorGUILayout.Toggle(GUIContent.none, v.GetBoolValue()));
            }
            else if (v.ValueType == FieldVariable.EVarType.Material)
            {
                EditorGUIUtility.labelWidth = 70;
                v.SetValue((Material)EditorGUILayout.ObjectField("Material:", v.GetMaterialRef(), typeof(Material), false));
            }
            else if (v.ValueType == FieldVariable.EVarType.GameObject)
            {
                EditorGUIUtility.labelWidth = 70;
                v.SetValue((GameObject)EditorGUILayout.ObjectField("Object:", v.GetGameObjRef(), typeof(GameObject), false));
            }
            else if (v.ValueType == FieldVariable.EVarType.Vector3)
            {
                bool drawV3 = true;
                Transform pre = null;
                if (helper != null) if (helper is Transform) pre = helper as Transform;

                if (v.allowTransformFollow)
                {
                    /*if (pre) */
                    drawV3 = false;
                }

                Color preC = GUI.color;
                if (!drawV3) GUI.color = new Color(1f, 1f, 1f, 0.45f);

                EditorGUIUtility.labelWidth = 70;
                if (v.FloatSwitch == EVarFloatingSwitch.Float)
                    v.SetValue(EditorGUILayout.Vector3Field("", v.GetVector3Value()));
                else
                    v.SetValue(EditorGUILayout.Vector3IntField("", v.GetVector3IntValue()));

                if (!drawV3) GUI.color = preC;

                if (!drawV3)
                {
                    ret = (Transform)EditorGUILayout.ObjectField(pre, typeof(Transform), true, GUILayout.Width(54));
                }
            }
            else if (v.ValueType == FieldVariable.EVarType.Vector2)
            {
                EditorGUIUtility.labelWidth = 70;

                if (v.FloatSwitch == EVarFloatingSwitch.Float)
                    v.SetValue(EditorGUILayout.Vector2Field("", v.GetVector2Value()));
                else
                    v.SetValue(EditorGUILayout.Vector2IntField("", v.GetVector2IntValue()));
            }
            else if (v.ValueType == EVarType.String)
            {
                EditorGUIUtility.labelWidth = 70;
                v.SetValue(EditorGUILayout.TextField("", v.GetStringValue()));
            }

            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(3);

            EditorGUILayout.EndHorizontal();
            return ret;
        }

    }
}
#endif