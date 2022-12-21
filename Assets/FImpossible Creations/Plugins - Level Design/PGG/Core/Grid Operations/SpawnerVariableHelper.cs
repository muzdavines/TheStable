using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public class SpawnerVariableHelper
    {
        public string name = "";
        [NonSerialized] public FieldVariable reference = null;
        [HideInInspector] public FieldVariable.EVarType requiredType = FieldVariable.EVarType.None;
        [HideInInspector] public string Tooltip = "";

        public SpawnerVariableHelper(FieldVariable.EVarType type = FieldVariable.EVarType.None)
        {
            requiredType = type;
        }

        public float GetValue(float defaultVal)
        {
            if (string.IsNullOrEmpty(name)) { return defaultVal; }
            if (FGenerators.CheckIfIsNull(reference)) { return defaultVal; }
            if (string.IsNullOrEmpty(reference.Name)) { return defaultVal; }
            return reference.Float;
        }

        public Material GetMatValue()
        {
            var refr = GetVariableReference();
            if (FGenerators.CheckIfIsNull(refr)) return null;
            return refr.GetMaterialRef();
        }

        public GameObject GetGameObjValue()
        {
            var refr = GetVariableReference();
            if (FGenerators.CheckIfIsNull(refr)) return null;
            return refr.GetGameObjRef();
        }

        public SpawnerVariableHelper GetVariable()
        {
            if (string.IsNullOrEmpty(name) == false) return this;
            return null;
        }

        public FieldVariable GetVariableReference()
        {
            if (FGenerators.CheckIfIsNull(GetVariable())) return null;
            return GetVariable().reference;
        }

        public bool IsType(FieldVariable.EVarType type)
        {
            if (FGenerators.CheckIfIsNull(GetVariable())) return false;
            if (FGenerators.CheckIfIsNull(GetVariable().reference)) return false;
            return GetVariable().reference.ValueType == type;
        }

        public Vector3 GetVector3(Vector3 defaultVal)
        {
            if (string.IsNullOrEmpty(name)) { return defaultVal; }
            if (FGenerators.CheckIfIsNull(reference)) { return defaultVal; }
            if (string.IsNullOrEmpty(reference.Name)) { return defaultVal; }
            return reference.GetVector3Value();
        }

        public List<SpawnerVariableHelper> GetListedVariable()
        {
            if (string.IsNullOrEmpty(name) == false) return new List<SpawnerVariableHelper>() { this };
            return null;
        }
    }


}
