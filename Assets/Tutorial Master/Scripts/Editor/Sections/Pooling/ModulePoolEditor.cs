using UnityEditor;

using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <summary>
    /// Responsible for rendering a Module Pool settings for a specific Module.
    /// </summary>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    /// <seealso cref="Section" />
    public sealed class ModulePoolEditor<TModule> : Section
        where TModule : Module
    {
        private readonly string _basePropertyName;

        public ModulePoolEditor(ref TutorialMasterEditor editor, string title, string basePropertyName)
            : base(ref editor, title, canExpand: false)
        {
            _basePropertyName = basePropertyName;
        }

        /// <summary>
        /// Renders the body of the section GUI
        /// </summary>
        protected override void OnSectionGUI()
        {
            var modulePrefabProperty =
                Editor.serializedObject.FindProperty(string.Format("{0}.Prefab", _basePropertyName));
            var customPoolSizeEnabledProperty =
                Editor.serializedObject.FindProperty(string.Format("{0}.OverridePoolSize", _basePropertyName));
            var poolSizeProperty =
                Editor.serializedObject.FindProperty(string.Format("{0}.CustomPoolSize", _basePropertyName));

            EditorField.Field(modulePrefabProperty, new GUIContent("Prefab"));
            EditorField.Field(customPoolSizeEnabledProperty, new GUIContent("Override Pool Size"));

            EvaluateProperty(ref modulePrefabProperty);

            if (customPoolSizeEnabledProperty.boolValue)
            {
                EditorField.Field(poolSizeProperty, new GUIContent("Size"));
            }
        }

        /// <summary>
        /// Evaluates the property to determine whether supplied value is a prefab and nothing else. The field gets nullified if it's not a prefab.
        /// </summary>
        private void EvaluateProperty(ref SerializedProperty property)
        {
            if (property.objectReferenceValue == null)
                return;

#if UNITY_2018_3_OR_NEWER
            var prefabType = PrefabUtility.GetPrefabAssetType(property.objectReferenceValue);
            var isPrefab = prefabType != PrefabAssetType.NotAPrefab;
#else
            var prefabType = PrefabUtility.GetPrefabType(property.objectReferenceValue);
            var isPrefab = prefabType == PrefabType.Prefab || prefabType == PrefabType.PrefabInstance;
#endif
            var gameObject = property.objectReferenceValue as GameObject;
            var isGameObject = gameObject != null;
            var hasModuleComponent = isGameObject && gameObject.GetComponent<TModule>() != null;

            // determine whether or not should the property field object reference be deleted
            if (!isPrefab || !isGameObject || !hasModuleComponent)
            {
                property.objectReferenceValue = null;
            }
        }
    }
}