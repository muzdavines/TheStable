using FIMSpace.Generating;
using UnityEngine;

namespace FIMSpace.Graph
{
    // Drawer class required in editor directories "NodePort_DrawerBase"
    // It will require [CustomPropertyDrawer(typeof(FloatPort))]
    [System.Serializable]
    public class FloatPort : NodePortBase
    {
        public float Value = 0f;
        public override System.Type GetPortValueType { get { return typeof(float); } }
        public override object DefaultValue { get { return Value; } }

        /// <summary>
        /// If null then returning zero
        /// </summary>
        public float GetInputValue
        {
            get
            {
                object val = GetPortValueSafe;
                if (FGenerators.CheckIfIsNull(val)) return 0f;
                if (val is int) return (float)((int)val);
                if (val is float) return (float)val;
                return 0f;
            }
        }

        public override Color GetColor()
        {
            return new Color(0.4f, 0.4f, .9f, 1f);
        }

        public override void InitialValueRefresh(object initialValue)
        {
            if (initValueSet) return;
            base.InitialValueRefresh(initialValue);
            if (initialValue is float) Value = (float)initialValue;
        }
    }
}