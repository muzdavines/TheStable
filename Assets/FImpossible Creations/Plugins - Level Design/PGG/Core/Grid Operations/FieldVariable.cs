using UnityEngine;
using System;
using System.Collections.Generic;
using FIMSpace.Graph;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Class for controlling Field Setup's variables
    /// Probably will be more extensive in future versions
    /// </summary>
    [System.Serializable]
    public partial class FieldVariable
    {
        public string Name = "Variable";
        [SerializeField] private Vector3 v3Val;
        [SerializeField] private string str;
        [SerializeField] private Material mat;
        [SerializeField] private GameObject gameObj;
        [SerializeField] private UnityEngine.Object unityObj;
        internal ModificatorsPack helperPackRef;

        [HideInInspector] public Vector3 helper = Vector3.zero;
        [HideInInspector] public UnityEngine.Object additionalHelperRef;

        private bool returnTempRef = false;
        private object temporaryReference;

        public void SetTemporaryReference(bool forceReturnTemporaryReference, object temp = null)
        {
            returnTempRef = forceReturnTemporaryReference;
            temporaryReference = temp;
        }

        public FieldVariable()
        {
            Name = "None";
            ValueType = EVarType.None;
        }

        public FieldVariable(string name, float value)
        {
            Name = name;
            v3Val.x = value;
            ValueType = EVarType.Number;
        }

        public FieldVariable(string name, object value)
        {
            Name = name;
            SetValue(value);
        }

        public FieldVariable(FieldVariable toCopy)
        {
            Name = toCopy.Name;
            helper = toCopy.helper;
            SetValue(toCopy);
            v3Val = toCopy.v3Val;
            str = toCopy.str;
            mat = toCopy.mat;
            gameObj = toCopy.gameObj;
            unityObj = toCopy.unityObj;
            helpForFieldCommand = toCopy.helpForFieldCommand;
        }


        public static void UpdateVariablesWith(List<FieldVariable> toChange, List<FieldVariable> toAdjustTo)
        {
            PGGUtils.AdjustCount(toChange, toAdjustTo.Count);
            if (toAdjustTo.Count > 0)
                for (int i = 0; i < toChange.Count; i++)
                {
                    FieldVariable target = toAdjustTo[i];
                    bool replace = false;
                    if (toChange[i].ValueType != target.ValueType) replace = true;
                    else if (toChange[i].Name != target.Name) replace = true;
                    if (replace) toChange[i] = target.Copy();
                }
        }

        public float Float { get { return v3Val.x; } set { SetValue(value); } }
        public int IntV { get { return Mathf.RoundToInt(v3Val.x); } set { SetValue(value); } }

        [NonSerialized] public bool Prepared = false;

        public enum EVarType { None, Number, Bool, Material, GameObject, Vector3, Vector2, ProjectObject, String/*, Int, Vector2, String*/ }
        [HideInInspector] public EVarType ValueType = EVarType.None;

        public enum EVarFloatingSwitch { Float, Int }
        [NonSerialized] public FieldSetup helpForFieldCommandRef = null;
        public bool helpForFieldCommand = false;
        public bool displayOnScene = false;
        public bool allowTransformFollow = false;
        [HideInInspector] public EVarFloatingSwitch FloatSwitch = EVarFloatingSwitch.Float;


        public int GetIntValue() { return Mathf.RoundToInt(v3Val.x); }
        public float GetFloatValue() { return v3Val.x; }
        public void SetInternalV3Value(Vector3 value) { v3Val = value; }
        public bool GetBoolValue() { return v3Val.x > 0; }
        public Vector2 GetVector2Value() { return new Vector2(v3Val.x, v3Val.y); }
        public Vector2Int GetVector2IntValue() { return new Vector2(v3Val.x, v3Val.y).V2toV2Int(); }
        public Vector3 GetVector3Value() { return v3Val; }
        public Vector3Int GetVector3IntValue() { return v3Val.V3toV3Int(); }
        public string GetStringValue() { return str; }
        public Material GetMaterialRef() { return mat; }
        public GameObject GetGameObjRef() { return gameObj; }
        public UnityEngine.Object GetUnityObjRef() { return unityObj; }


        //public void SetValue(int value) { v3Val.x = value; ValueType = EVarType.Int; UpdateVariable(); }
        public void SetValue(int value) { v3Val.x = value; ValueType = EVarType.Number; FloatSwitch = EVarFloatingSwitch.Int; UpdateVariable(); }
        public void SetValue(float value) { v3Val.x = value; ValueType = EVarType.Number; FloatSwitch = EVarFloatingSwitch.Float; UpdateVariable(); }
        public void SetValue(bool value) { v3Val.x = value ? 1 : 0; ValueType = EVarType.Bool; UpdateVariable(); }
        public void SetValue(Material value) { mat = value; ValueType = EVarType.Material; UpdateVariable(); }
        public void SetValue(GameObject value) { gameObj = value; ValueType = EVarType.GameObject; UpdateVariable(); }
        //public void SetValue(Vector2 value) { v3Val.x = value.x; v3Val.y = value.y; ValueType = EVarType.Vector2; UpdateVariable(); }
        public void SetValue(Vector3 value) { v3Val = value; ValueType = EVarType.Vector3; FloatSwitch = EVarFloatingSwitch.Float; UpdateVariable(); }
        public void SetValue(Vector3Int value) { v3Val = value; ValueType = EVarType.Vector3; FloatSwitch = EVarFloatingSwitch.Int; UpdateVariable(); }
        public void SetValue(Vector2 value) { v3Val = value; ValueType = EVarType.Vector2; FloatSwitch = EVarFloatingSwitch.Float; UpdateVariable(); }
        public void SetValue(Vector2Int value) { v3Val = new Vector3(value.x, value.y, v3Val.z); ValueType = EVarType.Vector2; FloatSwitch = EVarFloatingSwitch.Int; UpdateVariable(); }
        public void SetValue(string value) { str = value; ValueType = EVarType.String; UpdateVariable(); }
        public void SetValue(UnityEngine.Object value) { unityObj = value; ValueType = EVarType.ProjectObject; UpdateVariable(); }
        public void SetNullGameObjectValue() { gameObj = null; ValueType = EVarType.GameObject; UpdateVariable(); }
        public void SetNullProjectValue() { unityObj = null; ValueType = EVarType.ProjectObject; UpdateVariable(); }


        public void SetValue(object value)
        {
            returnTempRef = false;

            if (value is IFGraphPort)
            {
                temporaryReference = value;
                returnTempRef = true;
                return;
            }

            if (value is PGGCellPort.Data)
            {
                temporaryReference = value;
                returnTempRef = true;
                return;
            }

            if (value is int)
            {
                SetValue(Convert.ToInt32(value));
            }
            else if (value is float)
            {
                SetValue(Convert.ToSingle(value));
            }
            else if (value is bool)
            {
                SetValue(Convert.ToBoolean(value));
            }
            else if (value is Vector2Int)
            {
                SetValue((Vector2Int)value);
            }
            else if (value is Vector3Int)
            {
                SetValue((Vector3Int)value);
            }
            else if (value is Vector2)
            {
                SetValue((Vector2)value);
            }
            else if (value is Vector3)
            {
                SetValue((Vector3)value);
            }
            else if (value is string)
            {
                SetValue((string)value);
            }
            else if (value is Material)
            {
                SetValue((Material)value);
            }
            else if (value is GameObject)
            {
                SetValue((GameObject)value);
            }
            else if (value is UnityEngine.Object)
            {
                SetValue((UnityEngine.Object)value);
            }
            else
            {
                ValueType = EVarType.None;
            }

            UpdateVariable();
        }

        public object GetValue()
        {
            if (returnTempRef)
            {

                if (FGenerators.CheckIfExist_NOTNULL(temporaryReference))
                {
                    if (temporaryReference is NodePortBase)
                    {
                        NodePortBase port = temporaryReference as NodePortBase;
                        if (port.IsOutput)
                            return port.GetPortValueSafe;
                        else
                        {
                            if (port.PortState() == EPortPinState.Connected)
                            {
                                return port.GetPortValueSafe;
                            }
                            else
                                return port.GetPortValueSafe;
                        }
                    }
                    else
                    {
                        return temporaryReference;
                    }
                }

                return null;
            }


            switch (ValueType)
            {
                case EVarType.Number:
                    if (FloatSwitch == EVarFloatingSwitch.Float) return GetFloatValue();
                    else return GetIntValue();

                case EVarType.Bool: return GetBoolValue();

                case EVarType.Material: return GetMaterialRef();

                case EVarType.GameObject: return GetGameObjRef();

                case EVarType.Vector3:
                    if (FloatSwitch == EVarFloatingSwitch.Float) return GetVector3Value();
                    else return GetVector3IntValue();

                case EVarType.Vector2:
                    if (FloatSwitch == EVarFloatingSwitch.Float) return GetVector2Value();
                    else return GetVector2IntValue();

                case EVarType.ProjectObject: return GetUnityObjRef();

                case EVarType.String: return GetStringValue();
            }

            return -1;
        }

        public void SetValue(FieldVariable value)
        {
            if (value == null) return;

            allowTransformFollow = value.allowTransformFollow;
            displayOnScene = value.displayOnScene;
            helpForFieldCommand = value.helpForFieldCommand;

            returnTempRef = false;
            if (value.returnTempRef)
            {
                SetTemporaryReference(true, value.temporaryReference);
                return;
            }

            switch (value.ValueType)
            {
                case EVarType.Number:
                    if (value.FloatSwitch == EVarFloatingSwitch.Float)
                        SetValue(value.GetFloatValue());
                    else
                        SetValue(value.GetIntValue());
                    break;

                case EVarType.Bool: SetValue(value.GetBoolValue()); break;
                case EVarType.Material: SetValue(value.mat); break;
                case EVarType.GameObject: SetValue(value.gameObj); break;
                case EVarType.Vector2: SetValue(value.GetVector2Value()); break;
                case EVarType.Vector3: SetValue(value.GetVector3Value()); break;
                case EVarType.ProjectObject: SetValue(value.GetUnityObjRef()); break;
                case EVarType.String: SetValue(value.GetStringValue()); break;
            }

            UpdateVariable();
        }

        public void UpdateVariable() { }

        public FieldVariable Copy()
        {
            FieldVariable f = (FieldVariable)MemberwiseClone();
            f.ValueType = ValueType;
            f.Name = Name;
            f.str = str;
            f.v3Val = v3Val;
            f.mat = mat;
            f.unityObj = unityObj;
            f.helpForFieldCommand = helpForFieldCommand;
            return f;
        }



        private int[] _VariablesIds = null;
        public int[] GetVariablesIDList(bool forceRefresh = false)
        {
            if (helpForFieldCommandRef == null) return null;

            if (forceRefresh || _VariablesIds == null || _VariablesIds.Length != helpForFieldCommandRef.CellsCommands.Count)
            {
                _VariablesIds = new int[helpForFieldCommandRef.CellsCommands.Count];
                for (int i = 0; i < helpForFieldCommandRef.CellsCommands.Count; i++)
                {
                    _VariablesIds[i] = i;
                }
            }

            return _VariablesIds;
        }

        private GUIContent[] _VariablesNames = null;
        public GUIContent[] GetVariablesNameList(bool forceRefresh = false)
        {
            if (helpForFieldCommandRef == null) return null;

            if (forceRefresh || _VariablesNames == null || _VariablesNames.Length != helpForFieldCommandRef.CellsCommands.Count)
            {
                _VariablesNames = new GUIContent[helpForFieldCommandRef.CellsCommands.Count];
                for (int i = 0; i < helpForFieldCommandRef.CellsCommands.Count; i++)
                {
                    _VariablesNames[i] = new GUIContent(helpForFieldCommandRef.CellsCommands[i].Title);
                }
            }
            return _VariablesNames;
        }


    }
}
