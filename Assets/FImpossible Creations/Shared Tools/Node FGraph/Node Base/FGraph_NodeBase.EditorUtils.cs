#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraph_NodeBase
    {
        [NonSerialized] public bool _EditorDebugMode = false;

#if UNITY_EDITOR
        /// <summary> For editor use - handling drawing with editor assembly classes </summary>
        public object _editorDrawer = null;

        /// <summary> You can switch it with RMB on node and use it in code for if(_EditorDebugMode) { debugging... }</summary>

        /// <summary>
        /// If auto-generated Generic Menu to choose nodes in graph drawer window should be different than namespace
        /// </summary>
        public virtual string EditorCustomMenuPath() { return ""; }

#endif

        public void _E_SetDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR

        [NonSerialized] public bool _editorForceChanged = false;
        [NonSerialized] public bool _editorForceDrawDefaultTriggerWires = false;


        private UnityEditor.SerializedObject _baseSO = null;

        public UnityEditor.SerializedObject baseSerializedObject
        {
            get
            {
                if (_baseSO == null || _baseSO.targetObject != this)
                {
                    _baseSO = new UnityEditor.SerializedObject(this);
                }

                return _baseSO;
            }
        }

        private UnityEditor.SerializedProperty _baseSp_nodepos = null;
        public UnityEditor.SerializedProperty _sp_nodePosition
        {
            get
            {
                if (_baseSp_nodepos != null) if (_baseSp_nodepos.serializedObject != _baseSO) { _baseSp_nodepos.Dispose(); _baseSp_nodepos = null; }
                if (_baseSp_nodepos == null) _baseSp_nodepos = baseSerializedObject.FindProperty("NodePosition");
                return _baseSp_nodepos;
            }
        }


        static GUIContent _tempC = new GUIContent();

        /// <summary>
        /// Override must be put inside  #if UNITY_EDITOR  !!!
        /// Return true if gui changed and needs to apply serialized object changes (done after input handling of node in drawer class)
        /// </summary>
        public virtual void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {

#if UNITY_EDITOR

            if (baseSerializedObject == null) return;

            if (forceRefreshPorts)
            {
                CheckPortsForNullConnections();
                forceRefreshPorts = false;
            }

            baseSerializedObject.Update();

            UnityEditor.SerializedProperty iterator = baseSerializedObject.GetIterator();
            if (iterator == null) return;

            iterator.Next(true);
            iterator.NextVisible(false);

            bool ch = false;
            while (iterator.NextVisible(false))
            {
                _tempC.text = iterator.displayName;
                UnityEditor.EditorGUIUtility.labelWidth = UnityEditor.EditorStyles.label.CalcSize(_tempC).x + 5;
                UnityEditor.EditorGUI.BeginChangeCheck();
                UnityEditor.EditorGUILayout.PropertyField(iterator, true);
                if (UnityEditor.EditorGUI.EndChangeCheck()) ch = true;
            }

            UnityEditor.EditorGUIUtility.labelWidth = 0;

            if (ch)
            {
                _editorForceChanged = true;
                baseSerializedObject.ApplyModifiedProperties();
            }
#endif

        }

#if UNITY_EDITOR
        /// <summary>
        /// Whole override must be put inside  #if UNITY_EDITOR  !!!
        /// Additional GUI drawn in the inspector window view of node sciptable object
        /// Use baseSerializedObject for serialized object access!
        /// </summary>
        public virtual void Editor_OnAdditionalInspectorGUI()
        {
        }
#endif


        protected Vector2 _Editor_foldableOffset { get { return Vector2.zero; } }

        /// <summary>
        /// Override must be put inside  #if UNITY_EDITOR  !!!
        /// </summary>
        public virtual bool Editor_PreBody()
        {
            bool refresh = false;

#if UNITY_EDITOR
            if (IsFoldable)
            {
                Rect header = new Rect(26, 17, 16, 16);
                header.position += _Editor_foldableOffset;
                string f = FGUI_Resources.GetFoldSimbol(_EditorFoldout, false);

                if (GUI.Button(header, f, EditorStyles.label))
                {
                    SerializedProperty sp = baseSerializedObject.FindProperty("_EditorFoldout");

                    if (IsFoldableFix)
                    {
                        sp.boolValue = !sp.boolValue;
                    }
                    else
                    {
                        sp.serializedObject.ApplyModifiedProperties();
                        _EditorFoldout = !_EditorFoldout;
                        sp.serializedObject.ApplyModifiedProperties();
                    }

                    _E_SetDirty();
                    RefreshNodeParams();
                    refresh = true;
                }
            }
#endif

            return refresh;
        }


        #region Get instance of Serializable class from property field : Author: douduck08: https://github.com/douduck08

        public static T GetValue<T>(UnityEditor.SerializedProperty property) where T : class
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            Regex rgx = new Regex(@"\[\d+\]");

            for (int i = 0; i < fieldStructure.Length; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = System.Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
                    obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
                }
                else
                {
                    obj = GetFieldValue(fieldStructure[i], obj);
                }
            }

            return (T)obj;
        }

        private static object GetFieldValue(string fieldName, object obj, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);

            if (field != null)
            {
                return field.GetValue(obj);
            }

            return default(object);
        }


        private static object GetFieldValueWithIndex(string fieldName, object obj, int index, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);

            if (field != null)
            {
                object list = field.GetValue(obj);

                if (list.GetType().IsArray)
                {
                    return ((object[])list)[index];
                }
                else if (list is IEnumerable)
                {
                    return ((IList)list)[index];
                }
            }

            return default(object);
        }

        #endregion

#endif
    }
}
