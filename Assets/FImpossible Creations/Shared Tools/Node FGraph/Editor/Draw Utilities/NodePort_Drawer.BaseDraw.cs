using FIMSpace.Generating;
using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{
    public partial class NodePort_DrawerBase : PropertyDrawer
    {
        protected GUIContent displayContent = null;
        protected float displayWidth = 0f;

        protected Rect _E_InitRect = new Rect();
        [NonSerialized] public Rect _E_LatestInteractionRect = new Rect();
        protected Rect _E_GUIPropertyRect;
        Vector2 latestpropertyPosition;
        Vector2 latestpropertySize;
        private Texture2D coloredTex = null;


        private float _preLabelWdth = 0;
        /// <summary> Label width computed with displayContent </summary>
        protected void SetLabelWidth()
        {
            _preLabelWdth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = DisplayContentWidth() + 6;
        }

        protected float DisplayContentWidth()
        {
            float xS = EditorStyles.label.CalcSize(displayContent).x;
            return xS;
        }

        protected void RestoreLabelWidth()
        {
            EditorGUIUtility.labelWidth = _preLabelWdth;
        }

        public virtual void PrepareAndDrawFieldGUI(Rect rect, bool setRectsRefs)
        {
            if (FGenerators.RefIsNull(port)) return;

            _E_InitRect = rect;

            if (displayContent == null || displayContent.text.Length == 0)
            {
                displayContent = new GUIContent(port.DisplayName, baseTooltip);
                displayWidth = EditorStyles.label.CalcSize(displayContent).x;
            }

            if (string.IsNullOrEmpty(port.OverwriteName))
            {
                if (displayContent.text.Length != port.DisplayName.Length)
                {
                    displayContent.text = (port.DisplayName);
                    displayContent.tooltip = (baseTooltip);
                }
            }
            else
            {
                if (displayContent.text.Length != port.OverwriteName.Length)
                {
                    displayContent.text = (port.OverwriteName);
                    displayContent.tooltip = (baseTooltip);
                    displayWidth = EditorStyles.label.CalcSize(displayContent).x;
                }
            }


            Rect dRect;

            dRect = new Rect(rect);

            Rect propertyRect = new Rect(dRect);
            int drawSpace = 20;

            EPortPinState state = port.PortState();

            if (port.PortType == EPortPinType.Input)
            {
                propertyRect.position += new Vector2(drawSpace, 0);
                propertyRect.width -= drawSpace;

                if (state == EPortPinState.Connected)
                    coloredTex = FGraphStyles.TEX_jackIn;
                else
                    coloredTex = FGraphStyles.TEX_typeCircle;
            }
            else
            {
                propertyRect.position -= new Vector2(0, 0);
                propertyRect.width -= drawSpace;

                if (state == EPortPinState.Connected)
                    coloredTex = FGraphStyles.TEX_outputConnected;
                else
                    coloredTex = FGraphStyles.TEX_typeCircle;
            }


            _E_GUIPropertyRect = propertyRect;
            if (propertyRect.position.x != 0) latestpropertyPosition.x = propertyRect.position.x;
            if (propertyRect.position.y != 0) latestpropertyPosition.y = propertyRect.position.y;
            if (propertyRect.size.x != 0) latestpropertySize.x = propertyRect.size.x;
            if (propertyRect.size.y != 0) latestpropertySize.y = propertyRect.size.y;

            EditorGUIUtility.labelWidth = displayWidth + 6;
            DrawPortField(_E_GUIPropertyRect);
            EditorGUIUtility.labelWidth = 0;

            DisplayPortGUI(setRectsRefs);

        }


        public virtual void DisplayPortGUI(bool setRectsRefs)
        {
            if (FGenerators.RefIsNull(port)) return;

            Rect portRect = new Rect(_E_GUIPropertyRect);
            portRect.size = Vector2.one * 19;

            string tooltip = "";

            if (port.PortType == EPortPinType.Output)
            {
                float offset = _E_GUIPropertyRect.width;
                portRect.position += new Vector2(offset, 0);
                tooltip = OutputTooltipText;
            }
            else if (port.PortType == EPortPinType.Input)
            {
                portRect.position += new Vector2(-21, -1);
                tooltip = InputTooltipText;
            }

            if (port.SlotMode != EPortSlotDisplay.HidePort)
                if (port.BaseConnection != null)
                {
                    if (port.BaseConnection.NodeReference != null)
                    {
                        tooltip += "\nConnected With: " + port.BaseConnection.NodeReference.GetDisplayName();
                    }
                }

            if (setRectsRefs)
            {
                port._E_LatestPortRect = portRect;
            }

            if (port.SlotMode != EPortSlotDisplay.HidePort)
            {
                GUI.Label(portRect, new GUIContent(FGraphStyles.TEX_freeInput, tooltip)); // Input BG
                Color preC = GUI.color; GUI.color = port.GetColor();
                GUI.Label(portRect, new GUIContent(coloredTex, tooltip)); // Input port overlay

                GUI.color = preC;

                if (port.IsSender)
                {
                    if (port.PortState() == EPortPinState.Connected) GUI.color = new Color(1f, 1f, 1f, 0.8f);
                    else GUI.color = new Color(1f, 1f, 1f, 0.65f);

                    Rect sRect = new Rect(portRect.position, Vector2.one * 14f);
                    sRect.position += new Vector2(8, 2);

                    GUI.Label(sRect, new GUIContent(FGraphStyles.TEX_PointerRight, "This port is sending execution signal to the connected node")); // Sender overlay

                    GUI.color = preC;
                }
            }

            //Rect debugRect = new Rect(portRect.position - new Vector2(8,-14), new Vector2(100, 20));
            //GUI.Label(debugRect, "ID: " + port.PortID);

            port._E_LatestPortRect.position += port._EditorCustomOffset;
        }





        protected void DrawPortField(Rect fieldRect)
        {

            Rect labelRect = new Rect(fieldRect);
            labelRect.position += new Vector2(1, 0);
            labelRect.width = displayWidth;

            Rect fieldDrawRect = new Rect(fieldRect);
            fieldDrawRect.width -= (labelRect.width + 4);


            if (port.SlotMode == EPortSlotDisplay.Default) // Offset for port
            {
                if (port.IsOutput)
                {
                    fieldDrawRect.width -= 12;
                    fieldDrawRect.position += new Vector2(labelRect.width + 10, 0);
                }
                else // Input
                {
                    fieldDrawRect.width -= 2;
                    fieldDrawRect.position += new Vector2(labelRect.width + 6, 0);
                }
            }

            if (fieldDrawRect.position.x <= 2) fieldDrawRect.position = new Vector2(2, fieldDrawRect.position.y);
            if (fieldDrawRect.position.y <= 2) fieldDrawRect.position = new Vector2(fieldDrawRect.position.x, 2);

            if (port.SlotMode == EPortSlotDisplay.HidePort)
            {
                labelRect = new Rect(labelRect.x - 19, labelRect.y, labelRect.width + 18, labelRect.height);
            }

            Rect labelAndFieldRect = new Rect(labelRect);

            labelAndFieldRect.width += fieldDrawRect.width;

            bool drawLabel = true;
            bool drawValue = true;
            bool editableValue = true;

            if (port.PortState() == EPortPinState.Connected)
            {
                if (port.NameDisplayMode == EPortNameDisplay.HideName) drawLabel = false;
                else if (port.NameDisplayMode == EPortNameDisplay.HideOnConnected) drawLabel = false;

                if (port.ValueDisplayMode == EPortValueDisplay.HideValue) drawValue = false;
                else if (port.ValueDisplayMode == EPortValueDisplay.HideOnConnected) drawValue = false;
                else
                {
                    if (port.ValueDisplayMode == EPortValueDisplay.Default) editableValue = false;
                    else if (port.ValueDisplayMode == EPortValueDisplay.NotEditable) editableValue = false;
                }

                {
                    //if (drawLabel && drawValue && editableValue)
                    //{
                    //    DrawValueWithLabelField(labelRect, fieldDrawRect, cRect);
                    //}
                    //else if (drawLabel && drawValue)
                    //{
                    //    DrawLabel(labelRect);
                    //    DrawValueFieldNoEditable(fieldDrawRect);
                    //}
                    //else if (drawLabel)
                    //{
                    //    DrawLabel(labelRect);
                    //}
                    //else if (drawValue && editableValue)
                    //{
                    //    DrawValueField(fieldDrawRect);
                    //}
                    //else if (drawValue)
                    //{
                    //    DrawValueFieldNoEditable(fieldDrawRect);
                    //}
                }
            }
            else // Disconnected
            {
                if (port.NameDisplayMode == EPortNameDisplay.HideName) drawLabel = false;
                if (port.ValueDisplayMode == EPortValueDisplay.HideValue) drawValue = false;
                else
                {
                    if (port.ValueDisplayMode == EPortValueDisplay.NotEditable)
                        editableValue = false;
                }

            }


            if (drawLabel && drawValue && editableValue)
            {
                DrawValueWithLabelField(labelRect, fieldDrawRect, labelAndFieldRect);
            }
            else if (drawLabel && drawValue)
            {
                DrawLabel(labelRect);
                DrawValueFieldNoEditable(drawLabel ? fieldDrawRect : labelAndFieldRect);
            }
            else if (drawLabel)
            {
                DrawLabel(labelRect);
            }
            else if (drawValue && editableValue)
            {
                DrawValueField(labelAndFieldRect);
            }
            else if (drawValue)
            {
                DrawValueFieldNoEditable(drawLabel ? fieldDrawRect : labelAndFieldRect);
            }

        }

    }
}
