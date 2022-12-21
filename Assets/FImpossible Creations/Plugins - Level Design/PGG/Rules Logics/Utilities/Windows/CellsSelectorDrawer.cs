#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System;
using FIMSpace.Generating.Checker;

namespace FIMSpace.Generating
{

    public class CellsSelectorDrawer
    {
        static GUIStyle boxStyle = null;
        static GUIStyle boxStyleSel = null;

        static private Vector2 scroller = Vector2.zero;
        static private Dictionary<Vector3Int, bool> pressed = new Dictionary<Vector3Int, bool>();

        public int drawSize = 30;
        public int depthLevel = 0;
        public int selectedManualShape = 0;

        public void DrawCellsSelectorGUI(ShapeCellGroup drawing, bool repaintScene = false)
        {
            DrawCellsSelector( drawing, ref drawSize, ref depthLevel, repaintScene);
        }

        public static void DrawCellsSelector(ShapeCellGroup drawing, ref int drawSize, ref int depthLevel, bool repaintScene = false)
        {
#if UNITY_EDITOR
            Event e = Event.current;

            EditorGUILayout.BeginVertical(GUILayout.Height(280));

            Color bc = GUI.backgroundColor;

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(FGUI_Resources.BGInBoxStyle);
                boxStyle.alignment = TextAnchor.MiddleCenter;
                boxStyle.fontStyle = FontStyle.Bold;
                boxStyle.fontSize = Mathf.RoundToInt(boxStyle.fontSize * 1.2f);

                boxStyleSel = new GUIStyle(boxStyle);
                boxStyleSel.normal.background = FGUI_Resources.HeaderBoxStyleH.normal.background;
            }

            drawSize = EditorGUILayout.IntSlider("Cells Draw Size", drawSize, 12, 40);
            GUILayout.Space(3);

            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.fieldWidth = 0;
            EditorGUIUtility.labelWidth = 0;

            depthLevel = EditorGUILayout.IntField(new GUIContent("Y Level", "If you want to check cells placement above or below main cell"), depthLevel);
            EditorGUIUtility.fieldWidth = 0;
            EditorGUIUtility.labelWidth = 0;
            if (GUILayout.Button("▲", FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(18))) depthLevel++;
            if (GUILayout.Button("▼", FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(18))) depthLevel--;

            GUILayout.Space(6);

            EditorGUILayout.EndHorizontal();

            FGUI_Inspector.DrawUILine(0.25f, 0.4f, 2, 6);

            var position = GUILayoutUtility.GetLastRect();
            scroller = EditorGUILayout.BeginScrollView(scroller);

            float centerX = position.size.x / 2f;
            float centerY = position.size.y / 2f - (drawSize + 12);

            Rect refRect = new Rect(0, 0, drawSize, drawSize);
            float offset = refRect.width / 4f;

            int gridDrawW = Mathf.RoundToInt(position.width / drawSize);
            int gridDrawH = Mathf.RoundToInt(300 / drawSize);

            int centerCellOnGridX = gridDrawW / 2;
            if (gridDrawW % 2 == 0) centerCellOnGridX -= 1;

            int centerCellOnGridY = gridDrawH / 2 - 1;
            if (gridDrawH % 2 == 0) centerCellOnGridY -= 1;

            bool changed = false;

            bool mouseDownE = false;

            if (e.isMouse)
                if (e.type == EventType.MouseDrag || e.type == EventType.MouseDown)
                    mouseDownE = true;

            for (int x = 0; x < gridDrawW - 1; x++)
            {
                for (int y = 0; y < gridDrawH - 3; y++)
                {
                    Rect rDraw = new Rect(refRect); rDraw.x = x * drawSize + offset; rDraw.y = y * drawSize + offset;

                    int posX = x - centerCellOnGridX;
                    int posZ = centerCellOnGridY - y;
                    Vector3Int pos = new Vector3Int(posX, depthLevel, posZ);

                    bool contained = false;
                    if (drawing.ContainsPosition(pos)) contained = true;

                    GUIStyle styl = contained ? boxStyleSel : boxStyle;

                    if (pressed.ContainsKey(pos) == false) pressed.Add(pos, false);

                    if (centerCellOnGridX == x && centerCellOnGridY == y && depthLevel == 0)
                    {
                        Color preC = GUI.color;
                        Color prebC = GUI.backgroundColor;

                        if (contained)
                            GUI.backgroundColor = new Color(0.7f, 0.7f, 1f, 1f);
                        else
                            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);

                        GUI.Label(rDraw, new GUIContent("C", "Center cell - means cell from which checking starts!\nPosition Offset = " + pos), boxStyleSel);

                        if (mouseDownE)
                            if (!pressed[pos])
                                if (rDraw.Contains(e.mousePosition))
                                //if (GUI.Buytton(rDraw, new GUIContent("C", "Center cell - means cell from which checking starts!\nPosition Offset = " + pos), boxStyleSel))
                                {
                                    pressed[pos] = true;

                                    if (e.button == 0) { if (!contained) drawing.SwitchOnPosition(pos); }
                                    else if (e.button == 1) { if (contained) drawing.SwitchOnPosition(pos); }
                                    else { drawing.SwitchOnPosition(pos); }

                                    changed = true;
                                }

                        GUI.color = preC;
                        GUI.backgroundColor = prebC;
                    }
                    else
                    {
                        if (contained)
                            GUI.backgroundColor = new Color(0.7f, 0.7f, 1f, 1f);
                        else
                            GUI.backgroundColor = new Color(1f, 1f, 1f, 0.4f);

                        GUI.Label(rDraw, new GUIContent(" ", "Position Offset = " + pos), styl);

                        if (mouseDownE)
                            if (!pressed[pos])
                                if (rDraw.Contains(e.mousePosition))
                                {
                                    pressed[pos] = true;

                                    if (e.button == 0) { if (!contained) drawing.SwitchOnPosition(pos); }
                                    else if (e.button == 1) { if (contained) drawing.SwitchOnPosition(pos); }
                                    else { drawing.SwitchOnPosition(pos); }

                                    changed = true;
                                }
                    }

                    if (e.type == EventType.MouseUp)
                    {
                        pressed[pos] = false;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            if (changed)
            {
                e.Use();
                if (repaintScene) SceneView.RepaintAll();
            }
#endif
        }






    }



    #region Classes for painted cells

    [System.Serializable]
    public class ShapeCellGroup
    {
        public List<Vector3Int> Cells = new List<Vector3Int>();
        public bool ContainsPosition(Vector3Int p) { return Cells.Contains(p); }
        [NonSerialized] public bool Refresh = false;

        public void SwitchOnPosition(Vector3Int pos)
        {
            if (field == null) GetChecker();
            if (Cells == null) Cells = new List<Vector3Int>();
            if (!Cells.Contains(pos))
            {
                Cells.Add(pos);
                field.AddLocal(pos);
                Refresh = true;
            }
            else
            {
                Cells.Remove(pos);
                field.RemoveLocal(pos);
                Refresh = true;
            }
        }

        [NonSerialized] public CheckerField3D field = null;

        public CheckerField3D GetChecker(Transform attach = null, bool recalculateBounds = false)
        {
            if (field == null) Refresh = true;
            else if (field.ChildPositionsCount != Cells.Count) Refresh = true;

            if (Refresh)
            {
                if (field == null)
                {
                    field = new CheckerField3D();
                    field.AttachRootTo = attach;
                    field.UseBounds = recalculateBounds;

                    for (int i = 0; i < Cells.Count; i++)
                    {
                        field.AddLocal(new Vector3(Cells[i].x, Cells[i].y, Cells[i].z));
                    }
                }
                else
                {
                    Cells.Clear();
                    for (int i = 0; i < field.ChildPositionsCount; i++)
                    {
                        Cells.Add(field.ChildPos(i).V3toV3Int());
                    }
                }

                if (recalculateBounds) field.RecalculateMultiBounds();
                Refresh = false;
            }


            return field;
        }

    }

    #endregion

}
