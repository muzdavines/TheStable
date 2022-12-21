using FIMSpace.Generating;
using System;
using UnityEngine;

namespace FIMSpace.Graph
{
    // Drawer class required in editor directories "NodePort_DrawerBase"
    // It will require [CustomPropertyDrawer(typeof(IntPort))]
    [System.Serializable]
    public class IntPort : NodePortBase
    {
        public int Value = 0;
        public override System.Type GetPortValueType { get { return typeof(int); } }
        public override object DefaultValue { get { return Value; } }

        /// <summary>
        /// If null then returning zero
        /// </summary>
        public int GetInputValue
        {
            get
            {
                object val = GetPortValueSafe;
                if (FGenerators.CheckIfIsNull(val)) return 0;
                if (val is int) return (int)val;
                if (val is float) return Mathf.RoundToInt((float)val);
                return 0;
            }
        }

        public override Color GetColor()
        {
            return new Color(0.2f, 0.6f, .9f, 1f);
        }

        public override void InitialValueRefresh(object initialValue)
        {
            if (initValueSet) return;
            base.InitialValueRefresh(initialValue);
            if (initialValue is int) Value = (int)initialValue;
        }

    }
}