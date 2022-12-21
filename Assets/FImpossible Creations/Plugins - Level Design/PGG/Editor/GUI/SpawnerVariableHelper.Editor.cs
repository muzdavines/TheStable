using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{
    [CustomPropertyDrawer(typeof(SpawnerVariableHelper))]
    public class SpawnerVariableHelperDrawer : PropertyDrawer
    {
        Rect previousPropRect;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            previousPropRect = GUILayoutUtility.GetLastRect();
            return 1;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect drawPos = previousPropRect;
            drawPos.position -= Vector2.right * (16);
            drawPos.width = 15;
            drawPos.height = 18;

            SerializedProperty pName = property.FindPropertyRelative("name");

            var gLabel = new GUIStyle(EditorStyles.label);

            if (string.IsNullOrEmpty(pName.stringValue) == false)
                gLabel.normal.textColor = new Color(0.1f, 0.75f, 0.15f, 1f);
            else
                gLabel.normal.textColor = new Color(.8f, .8f, .8f, .75f);


            string tooltip;
            if (string.IsNullOrEmpty(label.tooltip) == false) tooltip = label.tooltip; else tooltip = "Set FieldSetup's variable multiplier for this property";

            if (GUI.Button(drawPos, new GUIContent("●", tooltip), gLabel))
            {
                SpawnRuleBase sr = (SpawnRuleBase)property.serializedObject.targetObject;

                SpawnerVariableHelper helper = null;
                var targetObject = property.serializedObject.targetObject;
                var targetObjectClassType = targetObject.GetType();
                var field = targetObjectClassType.GetField(property.propertyPath);
                if (field != null) helper = (SpawnerVariableHelper)field.GetValue(targetObject);

                bool anyType = true;
                if (helper != null) if (helper.requiredType != FieldVariable.EVarType.None) anyType = false;

                FieldSetup isFieldSet = null;
                ModificatorsPack isPack = null;

                if (sr != null)
                {
                    isFieldSet = sr.TryGetParentFieldSetup();
                    isPack = sr.TryGetParentModPack();

                    if (isFieldSet)
                        if (isPack)
                            if (isFieldSet.RootPack == isPack) isPack = null;
                }

                if (isFieldSet || isPack)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("None"), pName.stringValue == "", () => { pName.stringValue = ""; property.serializedObject.ApplyModifiedProperties(); EditorUtility.SetDirty(property.serializedObject.targetObject); });

                    bool header = false;

                    if (isFieldSet != null)
                        for (int i = 0; i < isFieldSet.Variables.Count; i++)
                        {
                            string nam = isFieldSet.Variables[i].Name;

                            if (anyType)
                            {
                                if (!header) { menu.AddItem(new GUIContent("_______  Field Variables  _______"), false, () => { }); header = true; }
                                menu.AddItem(new GUIContent(nam), pName.stringValue == nam, () => { pName.stringValue = nam; property.serializedObject.ApplyModifiedProperties(); EditorUtility.SetDirty(property.serializedObject.targetObject); });
                            }
                            else
                            {
                                if (isFieldSet.Variables[i].ValueType == helper.requiredType)
                                {
                                    if (!header) { menu.AddItem(new GUIContent("_______  Field Variables  _______"), false, () => { }); header = true; }
                                    menu.AddItem(new GUIContent(nam), pName.stringValue == nam, () => { pName.stringValue = nam; property.serializedObject.ApplyModifiedProperties(); EditorUtility.SetDirty(property.serializedObject.targetObject); });
                                }
                            }
                        }


                    if (isFieldSet)
                        if (helper != null)
                        {
                            menu.AddItem(new GUIContent("+ Add Variable to FieldSetup"), false, () =>
                            {
                                FieldVariable fVar = new FieldVariable("New", 1f);
                                fVar.ValueType = helper.requiredType;
                                fVar.Name = EditorUtility.SaveFilePanelInProject("Type new variable name (no file will be created)", "NewVariableName", "", "Type new variable name (no file will be created)");
                                fVar.Name = System.IO.Path.GetFileNameWithoutExtension(fVar.Name);

                                if (!string.IsNullOrEmpty(fVar.Name))
                                {
                                    isFieldSet.Variables.Add(fVar);
                                    pName.stringValue = fVar.Name;
                                    property.serializedObject.ApplyModifiedProperties();
                                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                                    EditorUtility.SetDirty(isPack);
                                }

                            });
                        }

                    if (isPack)
                    {
                        header = false;

                        for (int i = 0; i < isPack.Variables.Count; i++)
                        {
                            string nam = isPack.Variables[i].Name;

                            if (anyType)
                            {
                                if (!header) { menu.AddItem(new GUIContent("_______  Pack Variables  _______"), false, () => { }); header = true; }
                                menu.AddItem(new GUIContent(nam), pName.stringValue == nam, () => { pName.stringValue = nam; property.serializedObject.ApplyModifiedProperties(); EditorUtility.SetDirty(property.serializedObject.targetObject); });
                            }
                            else
                            {
                                if (isPack.Variables[i].ValueType == helper.requiredType)
                                {
                                    if (!header) { menu.AddItem(new GUIContent("_______  Pack Variables  _______"), false, () => { }); header = true; }
                                    menu.AddItem(new GUIContent(nam), pName.stringValue == nam, () => { pName.stringValue = nam; property.serializedObject.ApplyModifiedProperties(); EditorUtility.SetDirty(property.serializedObject.targetObject); });
                                }
                            }
                        }


                        if (helper != null)
                        {
                            menu.AddItem(new GUIContent("+ Add Variable to Pack"), false, () =>
                            {
                                FieldVariable fVar = new FieldVariable("New", 1f);
                                fVar.ValueType = helper.requiredType;
                                fVar.Name = EditorUtility.SaveFilePanelInProject("Type new variable name (no file will be created)", "NewVariableName", "", "Type new variable name (no file will be created)");
                                fVar.Name = System.IO.Path.GetFileNameWithoutExtension(fVar.Name);

                                if (!string.IsNullOrEmpty(fVar.Name))
                                {
                                    isPack.Variables.Add(fVar);
                                    pName.stringValue = fVar.Name;
                                    property.serializedObject.ApplyModifiedProperties();
                                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                                    EditorUtility.SetDirty(isPack);
                                }

                            });
                        }
                    }

                    menu.DropDown(new Rect(Event.current.mousePosition + Vector2.left * 100, Vector2.zero));
                }
                else
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Can't get parent FieldSetup/Pack"), false, () => { });
                    menu.DropDown(new Rect(Event.current.mousePosition + Vector2.left * 100, Vector2.zero));
                }

            }

            EditorGUI.EndProperty();
        }
    }
}