#if UNITY_EDITOR
using FIMSpace.FEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FIMSpace.Generating.TileMeshSetup;

namespace FIMSpace.Generating
{
    public partial class TileDesignerWindow
    {


        /// <summary> returns true if clicked remove </summary>
        bool GUI_DisplayAttachable(List<MonoScript> attachables, int index)
        {
            bool clicked = false;

            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 70;
            attachables[index] = (MonoScript)EditorGUILayout.ObjectField("To Attach:", attachables[index], typeof(MonoScript), false);
            if (GUILayout.Button(FGUI_Resources.Tex_Remove, FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(19))) { clicked = true; }
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            return clicked;
        }

        /// <summary> returns true if clicked remove </summary>
        bool GUI_DisplaySendMessageHelper(List<TileDesign.SendMessageHelper> helpers, int index)
        {
            bool clicked = false;
            var helper = helpers[index];

            GUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 70;
            helper.Message = EditorGUILayout.TextField("Message:", helper.Message);

            GUILayout.Space(8);
            EditorGUIUtility.labelWidth = 58;
            //helper.SendOn = (TileDesign.SendMessageHelper.EMessageSend)EditorGUILayout.EnumPopup("Send On:", helper.SendOn, GUILayout.Width(140));

            GUILayout.Space(8);
            helper.SendValue = EditorGUILayout.Toggle(helper.SendValue, GUILayout.Width(18));
            EditorGUIUtility.labelWidth = 98;
            if (helper.SendValue == false) GUI.enabled = false;
            helper.MessageValue = EditorGUILayout.FloatField("Message Value:", helper.MessageValue, GUILayout.Width(128));
            EditorGUIUtility.labelWidth = 0;
            if (helper.SendValue == false) GUI.enabled = true;
            if (GUILayout.Button(FGUI_Resources.Tex_Remove, FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(19))) { clicked = true; }
            GUILayout.EndHorizontal();

            return clicked;
        }


        void SetDisplayRect(Rect r)
        {
            if (Event.current == null) return;
            if (Event.current.type == EventType.Repaint) _editorDisplayRect1 = r;
        }

        void SetDisplayRect2(Rect r)
        {
            if (Event.current == null) return;
            if (Event.current.type == EventType.Repaint) _editorDisplayRect2 = r;
        }

        private Vector2 GetRPos(Rect r, float xOff, float yOff, bool transform)
        {
            Vector2 pos = new Vector2(r.position.x + xOff, r.position.y + yOff);
            if (transform) pos -= _latestEditorDisplayRect.position;
            return pos;
        }

        private Vector2 GetRPos(Rect r, Vector2 pos, bool transform)
        {
            return GetRPos(r, pos.x, pos.y, transform);
        }

        float FlattenVal(float val, float to)
        {
            return Mathf.Round(val / to) * to;
        }


        GUIDragInfo _dragInfo = null;
        private class GUIDragInfo
        {
            public CurvePoint owner;
            public EDrag draggin;
            public Vector2 startMousePos;

            public GUIDragInfo(CurvePoint p, EDrag t, Vector2 s)
            {
                owner = p;
                draggin = t;
                startMousePos = s;
            }

            /// <summary> Returns true if something changed </summary>
            internal bool UpdateDragPos(Rect display, Vector2 vector2)
            {
                if (draggin == EDrag.InTan)
                {
                    Vector2 pre = owner.inTan;

                    Vector3 tan = owner.rPos - vector2 + owner.rect.size / 2f;
                    owner.inTan = -tan;

                    if (pre != owner.inTan) return true;
                }
                else
                {
                    if (owner.next != null)
                    {
                        Vector2 pre = owner.nextTan;

                        Vector3 tan = owner.next.rPos - vector2 + owner.rect.size / 2f;
                        owner.nextTan = -tan;

                        if (pre != owner.nextTan) return true;
                    }
                }

                return false;
            }

            public enum EDrag
            {
                InTan, NextTan
            }
        }


    }
}
#endif