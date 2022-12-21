using FIMSpace.Generating;
using UnityEngine;

namespace FIMSpace.Graph
{
    // Drawer class required in editor directories "NodePort_DrawerBase"
    // It will require [CustomPropertyDrawer(typeof(BoolPort))]
    [System.Serializable]
    public class BoolPort : NodePortBase
    {
        /// <summary> Backing value if no port is connected </summary>
        public bool Value = false;
        public override System.Type GetPortValueType { get { return typeof(bool); } }
        public override object DefaultValue { get { return Value; } }

        /// <summary>
        /// If null then returning false
        /// </summary>
        public bool GetInputValue
        {
            get
            {
                object val = GetPortValueSafe;
                if (FGenerators.CheckIfIsNull(val)) { return false; }
                if (val is string) if (string.IsNullOrEmpty((string)val)) { return false; }
                if (val is bool) return (bool)val;
                return true;
            }
        }

        public override Color GetColor()
        {
            return new Color(0.9f, 0.4f, .4f, 1f);
        }

        public override void InitialValueRefresh(object initialValue)
        {
            if (initValueSet) return;
            base.InitialValueRefresh(initialValue);
            if (initialValue is bool) Value = (bool)initialValue;
        }

    }
}