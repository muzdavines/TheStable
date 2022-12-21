using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraph_NodeBase
    {
        private bool forceRefreshPorts = false;
        public bool isCulled { get; set; } = false;
        public bool RefreshedPorts { get; private set; } = false;


        private void OnEnable()
        {
            RefreshedPorts = false;
            OnValidate();
        }

        protected virtual void OnValidate()
        {
            if (!RefreshedPorts)
            {
                RefreshPorts();
                RefreshedPorts = true;
            }
        }

#if UNITY_2019_4_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        void ResetAfterCompile()
        {
            RefreshedPorts = false;
            OnValidate();
        }

        public void RemoveAllPortConnections()
        {
            for (int o = 0; o < outputPorts.Count; o++)
            {
                var port = outputPorts[o];
                for (int c = 0; c < port.Connections.Count; c++)
                {
                    var oPort = outputPorts[o].Connections[c].PortReference;
                    if (oPort != null)
                    {
                        NodePortBase nPort = oPort as NodePortBase;
                        nPort.DisconnectWith(this, port);
                    }
                }
            }

            for (int o = 0; o < inputPorts.Count; o++)
            {
                var port = inputPorts[o];
                for (int c = 0; c < port.Connections.Count; c++)
                {
                    var oPort = inputPorts[o].Connections[c].PortReference;
                    if (oPort != null)
                    {
                        NodePortBase nPort = oPort as NodePortBase;
                        nPort.DisconnectWith(this, port);
                    }
                }
            }
        }


        public void RefreshPorts()
        {
            #region Preparing ports in editor

#if UNITY_EDITOR
            UnityEditor.SerializedObject so = baseSerializedObject;
            so.Update();
#endif

            // Collect lists of port variables
            inputPorts.Clear();
            outputPorts.Clear();

            System.Type nodeType = GetType();
            List<FieldInfo> fieldInfo = new List<FieldInfo>(nodeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance));

            System.Type tempType = nodeType;
            while ((tempType = tempType.BaseType) != typeof(FGraph_NodeBase))
            { fieldInfo.AddRange(tempType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)); }

            for (int i = 0; i < fieldInfo.Count; i++)
            {
                FieldInfo info = fieldInfo[i];

                if (info.FieldType.IsSubclassOf(typeof(NodePortBase)))
                {
                    IFGraphPort iport = (IFGraphPort)info.GetValue(this);

                    if (iport != null)
                    {
                        NodePortBase port = iport as NodePortBase;
                        if (port != null)
                        {
                            object[] attribs = fieldInfo[i].GetCustomAttributes(true);
                            PortAttribute portAttribute = attribs.FirstOrDefault(x => x is PortAttribute) as PortAttribute;

                            if (portAttribute != null)
                            {
                                port.PortType = portAttribute.PinType;
                                port.NameDisplayMode = portAttribute.NameDisplay;
                                if (!string.IsNullOrEmpty(portAttribute.CustomName)) port.DisplayName = portAttribute.CustomName;

                                port.InitialValueRefresh(portAttribute.InitialValue);
                            }

                            port.Refresh(this);

                            if (port.IsOutput)
                                outputPorts.Add(port);
                            else
                                inputPorts.Add(port);
                        }
                    }
                }
            }

            // When all node lists are filled then refresh them
            for (int i = 0; i < inputPorts.Count; i++)
            {
                NodePortBase port = inputPorts[i] as NodePortBase;
                if (port != null)
                {
                    port.Refresh(this);
                }
            }

            for (int i = 0; i < outputPorts.Count; i++)
            {
                NodePortBase port = outputPorts[i] as NodePortBase;
                if (port != null)
                {
                    port.Refresh(this);
                }
            }

#if UNITY_EDITOR
            so.ApplyModifiedProperties();
#endif

            #endregion

        }





#if UNITY_EDITOR
        public void ClearAfterPaste()
        {
            IndividualID = -1;
            ClearPorts();
            InputConnections.Clear();
            OutputConnections.Clear();
            forceRefreshPorts = true;
            _editorForceChanged = true;
            RequestsConnectionsRefresh = true;
        }
#endif
    }
}
