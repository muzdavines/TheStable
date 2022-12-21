using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        /// <summary> Executing serializedObject.Apply on the scriptable object preset file after drawing all nodes </summary>
        public bool AsksForSerializedPropertyApply { get; set; } = false;

        /// <summary>
        /// Called when new node / connection added / removed
        /// </summary>
        public virtual void OnGraphStructureChange()
        {
        }


        protected virtual bool IsCursorInGraph()
        {
            if (!graphDisplayRect.Contains(eventMousePos)) return false;
            return true;
        }

        /// <summary> To implement, can be used with cursorOutOfGraphEventForward </summary>
        protected virtual bool IsCursorInAdditionalActionArea()
        {
            return true;
        }

        public virtual void OnCreate()
        {
            refreshRequest = true;
        }
    }
}