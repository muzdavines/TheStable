/*
* Copyright (c) Mad Pixel Machine
* http://www.madpixelmachine.com/
*/

using EnergyBarToolkit;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof (EnergyBar))]
public class EnergyBarInspector : Editor {

    #region Fields

    private SerializedProperty valueCurrent;
    private SerializedProperty valueMin;
    private SerializedProperty valueMax;

    #endregion

    #region Methods

    void OnEnable() {
        valueCurrent = serializedObject.FindProperty("_valueCurrent");
        valueMin = serializedObject.FindProperty("valueMin");
        valueMax = serializedObject.FindProperty("valueMax");
    }

    public override void OnInspectorGUI() {
#if UNITY_5_6_OR_NEWER
        serializedObject.UpdateIfRequiredOrScript();
#else
        serializedObject.UpdateIfDirtyOrScript();
#endif

        EditorGUILayout.IntSlider(valueCurrent, valueMin.intValue, valueMax.intValue, "Value Current");

        EditorGUILayout.Space();

        MadGUI.PropertyField(valueMin, "Value Min");
        MadGUI.PropertyField(valueMax, "Value Max");

        if (valueMin.intValue >= valueMax.intValue) {
            MadGUI.Error("Value Min should be lower than Value Max.");
        }

        serializedObject.ApplyModifiedProperties();
        
    }

    #endregion
}