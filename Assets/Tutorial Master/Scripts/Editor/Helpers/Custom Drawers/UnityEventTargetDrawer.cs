using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HardCodeLab.TutorialMaster.EditorUI.Drawers
{
    /// <inheritdoc />
    /// <summary>
    /// Property Drawer for rendering UnityEvent Target. Also allows user to selected a target UnityEvent.
    /// </summary>
    /// <seealso cref="T:UnityEditor.PropertyDrawer" />
    [CustomPropertyDrawer(typeof(UnityEventTarget))]
    public class UnityEventTargetDrawer : PropertyDrawer
    {
        /// <summary>
        /// A selected UnityEvent ItemPrefab.
        /// </summary>
        private UnityEventMetadata _selectedMenuItem;

        /// <summary>
        /// A list of all UnityEvent items.
        /// </summary>
        private List<UnityEventMetadata> _menuItems;

        /// <summary>
        /// Used to check if the source has been checked.
        /// </summary>
        private bool _sourceChecked;

        /// <summary>
        /// The current path of serialized property
        /// </summary>
        private string _currentPath;

        /// <summary>
        /// Gets the name of the selected UnityEvent Field.
        /// </summary>
        private string SelectedUnityEventFieldName
        {
            get
            {
                return _selectedMenuItem != null && !string.IsNullOrEmpty(_selectedMenuItem.Name)
                    ? _selectedMenuItem.Name + "()"
                    : "None";
            }
        }

        /// <summary>
        /// Validates the source. Ensures that 
        /// </summary>
        /// <param name="property">Serialized property of <seealso cref="UnityEventTarget"/> which will be validated.</param>
        private void ValidateSource(SerializedProperty property)
        {
            if (_sourceChecked)
                return;

            var source = property.FindPropertyRelative("Source").objectReferenceValue as GameObject;

            if (source == null)
                return;

            var unityEventName = property.FindPropertyRelative("_unityEventName").stringValue;
            var componentName = property.FindPropertyRelative("_componentName").stringValue;
            var targetComponent = source.GetComponent(componentName);

            if (targetComponent == null)
                return;

            var unityEventInfo = targetComponent
                .GetType()
                .GetFields(UnityEventTarget.Flags)
                .FirstOrDefault(UnityEventTarget.GetPredicate(unityEventName));

            if (unityEventInfo == null)
                SetUnityEvent(property, null);

            _sourceChecked = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 34;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CheckEditorChanges(property);

            EditorGUI.BeginProperty(position, label, property);

            ValidateSource(property);

            GetSelectedUnityEvent(property);

            var sourceProperty = property.FindPropertyRelative("Source");

            GUI.changed = false;

            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, 16), sourceProperty, new GUIContent("Source"));

            if (GUI.changed)
            {
                SetUnityEvent(property, null);
            }

            GUI.enabled = sourceProperty.objectReferenceValue != null;

            if (sourceProperty.objectReferenceValue == null)
            {
                _selectedMenuItem = null;
            }

            if (GUI.Button(new Rect(position.x, position.y + 18, position.width, 16), SelectedUnityEventFieldName,
                EditorResources.Styles.DropdownStyle))
            {
                GetUnityEvents(property);
                ShowMenu(property);
            }

            GUI.enabled = true;

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Shows a context menu of all UnityEvent items.
        /// </summary>
        /// <param name="property">Serialized property of <seealso cref="UnityEventTarget"/> which will be affected.</param>
        private void ShowMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("None"), false, () =>
            {
                SetUnityEvent(property, null);
            });

            if (_menuItems.Count == 0)
                return;

            menu.AddSeparator("");

            foreach (var item in _menuItems)
            {
                var targetItem = item;
                menu.AddItem(new GUIContent(item.MenuPath), item.Equals(_selectedMenuItem), () =>
                {
                    SetUnityEvent(property, targetItem);
                });
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// Gets a currently selected UnityEvent.
        /// </summary>
        /// <param name="property">Serialized property of <seealso cref="UnityEventTarget"/> from which data will be fetched.</param>
        private void GetSelectedUnityEvent(SerializedProperty property, bool forceUpdate = false)
        {
            if (!forceUpdate && _selectedMenuItem != null)
                return;

            _selectedMenuItem = new UnityEventMetadata
            (
                property.FindPropertyRelative("_unityEventName").stringValue,
                property.FindPropertyRelative("_componentName").stringValue
            );
        }

        /// <summary>
        /// Checks if serialized property has been changed.
        /// </summary>
        /// <param name="property">The property that will be checked.</param>
        private void CheckEditorChanges(SerializedProperty property)
        {
            if (_currentPath != property.propertyPath)
            {
                _sourceChecked = false;
                GetSelectedUnityEvent(property, true);
                _currentPath = property.propertyPath;
            }
        }

        /// <summary>
        /// Sets the UnityEvent ItemPrefab.
        /// </summary>
        /// <param name="property">Serialized property of <seealso cref="UnityEventTarget"/> that will be updated.</param>
        /// <param name="unityEvent">New UnityEvent ItemPrefab.</param>
        private void SetUnityEvent(SerializedProperty property, UnityEventMetadata unityEvent)
        {
            _selectedMenuItem = unityEvent;

            if (_selectedMenuItem != null)
            {
                property.FindPropertyRelative("_componentName").stringValue = _selectedMenuItem.SourceComponentName;
                property.FindPropertyRelative("_unityEventName").stringValue = _selectedMenuItem.Name;
            }
            else
            {
                property.FindPropertyRelative("_componentName").stringValue = "";
                property.FindPropertyRelative("_unityEventName").stringValue = "";
            }


            property.serializedObject.ApplyModifiedProperties();
            EditorSceneManager.MarkAllScenesDirty();
        }

        /// <summary>
        /// Retrieves all public UnityEvents
        /// </summary>
        /// <param name="property">Serialized property of <seealso cref="UnityEventTarget"/> from which event will be evaluated.</param>
        private void GetUnityEvents(SerializedProperty property)
        {
            _menuItems = new List<UnityEventMetadata>();

            var obj = property.FindPropertyRelative("Source").objectReferenceValue as GameObject;
            if (obj == null)
                return;

            var components = obj.GetComponents<Component>();

            foreach (var component in components)
            {
                if (component == null)
                    continue;

                var unityEvents = component.GetType()
                    .GetFields(UnityEventTarget.Flags)
                    .Where(UnityEventTarget.GetPredicate());

                foreach (var unityEvent in unityEvents)
                {
                    _menuItems.Add
                    (
                        new UnityEventMetadata(unityEvent.Name, component.GetType().Name)
                    );
                }
            }
        }
    }
}