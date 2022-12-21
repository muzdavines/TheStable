using FIMSpace.FEditor;
using FIMSpace.Generating;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public partial class NodePort_DrawerBase : PropertyDrawer
    {
        protected NodePortBase port;
        protected PortAttribute portAttr = null;
        protected GUIContent baseContent = null;
        protected string baseTooltip { get { if (baseContent == null) return ""; else return baseContent.tooltip; } }
        bool triedGetPort = false;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            baseContent = label;
            //Rect r = new Rect(position);
            //GUI.Box(r, new GUIContent("", position.size.ToString()), FGUI_Resources.BGInBoxStyleH);

            #region Try Get Port Attribute

            if (triedGetPort == false)
            {
                var atts = fieldInfo.GetCustomAttributes(false);
                foreach (object atr in atts)
                {
                    if (atr as PortAttribute != null) portAttr = atr as PortAttribute;
                }

                triedGetPort = true;
            }

            #endregion


            #region Try get port class instance

            if (FGenerators.RefIsNull(port))
            {
                port = FGraph_NodeBase.GetValue<NodePortBase>(property);
            }

            #endregion


            if (FGenerators.RefIsNull(port))
            {
                GUI.Label(position, new GUIContent("Can't find port instance (i)", "Maybe you use it in list or something?"));
            }
            else
            {

                if (port.ParentNode == null)
                {
                    FGraph_NodeBase node = property.serializedObject.targetObject as FGraph_NodeBase;
                    if (node != null) port.Refresh(node);
                }

                if ( label.text.StartsWith("P_") == false) port.DisplayName = label.text;

                // Apply attributes
                if (portAttr != null)
                {
                    port.AdditionalAllows = portAttr.AdditionalAllows;
                    port.LimitInConnectionsCount = portAttr.LimitConnectionsCount;

                    if (string.IsNullOrEmpty(portAttr.CustomName) == false) port.DisplayName = portAttr.CustomName;
                    port.PortType = portAttr.PinType;
                    port.NameDisplayMode = portAttr.NameDisplay;
                    port.ValueDisplayMode = portAttr.ValueDisplay;
                }

                EditorGUI.BeginProperty(position, label, property);

                bool rectTrig = false;
                if (port != null) if (port.ParentNode != null)
                    {
                        if (port.ParentNode.baseSerializedObject != null)
                            if (port.ParentNode.IsDrawingGUIInNodeMode)
                                if (position.position != Vector2.zero) // Only way to detect if drawed inside inspector window or in node - node adds Space(1) to make y =1 instead of zero to avoid bug with first port
                                {
                                    PrepareAndDrawFieldGUI(position, property.serializedObject == port.ParentNode.baseSerializedObject);
                                    rectTrig = true;
                                }
                    }

                if (!rectTrig)
                    PrepareAndDrawFieldGUI(position, false);

                EditorGUI.EndProperty();
            }

        }


    }
}
