using System;
using com.ootii.Helpers;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.ootii.Demos
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DemoProperties), true)]
    public class DemoPropertiesEditor : UnityEditor.Editor
    {
        private const float ListPadding = 12;
        private const float ColumnPadding = 6;

        private ReorderableList mList = null;
        private SerializedProperty mInputItemsProperty = null;

        private void OnEnable()
        {
            mInputItemsProperty = serializedObject.FindProperty("InputItems");
            SetupList();
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "Inspector");
            serializedObject.Update();

            GUILayout.Space(5);

            EditorHelper.DrawInspectorTitle("ootii Demo Properties");

            EditorHelper.DrawInspectorDescription("These properties will be displayed in the appropriate fields in the 'Instructions' panel "
                + "in the demo UI.", MessageType.None);

            GUILayout.Space(5);

            try
            {
                GUILayout.BeginVertical(EditorHelper.Box);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Title"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));
            }
            finally
            {
                GUILayout.EndVertical();
            }

            GUILayout.Space(5);

            EditorGUILayout.LabelField("Input Settings", EditorStyles.boldLabel, GUILayout.Height(16f));
            try
            {
                GUILayout.BeginVertical(EditorHelper.Box);
                mList.DoLayoutList();
            }
            finally
            {
                GUILayout.EndVertical();
            }

            GUILayout.Space(5);

            serializedObject.ApplyModifiedProperties();
        }


        private void SetupList()
        {
            mList = new ReorderableList(serializedObject, mInputItemsProperty, true, true, true, true);
            mList.drawHeaderCallback += OnDrawHeader;
            mList.drawElementCallback += OnDrawElement;
            mList.onAddCallback += OnAddItem;
        }

        /// <summary>
        /// Clear the InputAlias and Description fields on a new Input Item when it is added
        /// </summary>
        /// <param name="rList"></param>
        private void OnAddItem(ReorderableList rList)
        {
            int lIndex = rList.serializedProperty.arraySize;
            rList.serializedProperty.arraySize++;
            rList.index = lIndex;

            try
            {
                SerializedProperty lInputItem = rList.serializedProperty.GetArrayElementAtIndex(lIndex);
                lInputItem.FindPropertyRelative("InputAlias").stringValue = string.Empty;
                lInputItem.FindPropertyRelative("ActionDescription").stringValue = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }
        }


        private void OnDrawHeader(Rect rRect)
        {
            float lFieldWidth = (rRect.width - ListPadding - ColumnPadding) / 2;

            EditorGUI.LabelField(new Rect(ListPadding + rRect.x, rRect.y, lFieldWidth, rRect.height), "Input Alias");
            EditorGUI.LabelField(new Rect(ListPadding + rRect.x + lFieldWidth + ColumnPadding, rRect.y, lFieldWidth, rRect.height), "Action Description");
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (mInputItemsProperty == null || !(0 <= index && index < mInputItemsProperty.arraySize))                
            {
                return;
            }

            SerializedProperty lItemProperty = mInputItemsProperty.GetArrayElementAtIndex(index);
            if (lItemProperty == null) { return; }

            SerializedProperty lAliasProperty = lItemProperty.FindPropertyRelative("InputAlias");
            SerializedProperty lDescriptionProperty = lItemProperty.FindPropertyRelative("ActionDescription");
            if (lAliasProperty == null || lDescriptionProperty == null) { return; }

            float rFieldWidth = (rect.width - ColumnPadding) / 2;

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rFieldWidth, EditorGUIUtility.singleLineHeight),
                 lAliasProperty, GUIContent.none, true);
            EditorGUI.PropertyField(new Rect(rect.x + rFieldWidth + ColumnPadding, rect.y, rFieldWidth, EditorGUIUtility.singleLineHeight),
                 lDescriptionProperty, GUIContent.none, true);
        }
    }

}


