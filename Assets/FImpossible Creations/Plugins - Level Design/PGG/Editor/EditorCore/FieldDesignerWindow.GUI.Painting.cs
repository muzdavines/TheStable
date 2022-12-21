#if UNITY_EDITOR
using UnityEngine;
using FIMSpace.FEditor;
using UnityEditor;

namespace FIMSpace.Generating
{
    public partial class FieldDesignWindow
    {
        void DrawPaintGUI()
        {
            float dpiH = FGenerators.EditorUIScale;
            int hOffset = (int)(Screen.height / dpiH);

            if (DrawScreenGUI && !isSelectedPainter)
            {
                if (gridMode == EDesignerGridMode.Paint)
                {
                    Rect bRect = new Rect(15, hOffset - 78, 120, 24);
                    Color preC = GUI.backgroundColor;
                    if (paintNow) GUI.backgroundColor = Color.green;

                    if (GUI.Button(bRect, "Paint " + (paintNow ? "ON" : "OFF") + " (shift+x)"))
                    {
                        paintNow = !paintNow;
                    }

                    GUI.backgroundColor = preC;
                }
            }

            if (grid != null)
            {
                //if (grid.GetMin().y != 0 || grid.GetMax().y != 0)
                {
                    Color preC = GUI.backgroundColor;
                    Rect bRect;

                    if (DrawScreenGUI && !isSelectedPainter)
                    {
                        bRect = new Rect(5, 15, 160, 40);
                        GUI.Label(bRect, "Focus On Y Level: " + focusOnY, FGUI_Resources.HeaderStyle);
                        bRect = new Rect(155, 15, 20, 20);
                        if (GUI.Button(bRect, "▲")) focusOnY++;

                        if (paintNow)
                        {
                            bRect = new Rect(185, 14, 55, 20);
                            GUI.Label(bRect, "shift + A", FGUI_Resources.HeaderStyle);
                        }

                        bRect = new Rect(155, 15 + 22, 20, 20);
                        if (GUI.Button(bRect, "▼")) focusOnY--;

                        if (paintNow)
                        {
                            bRect = new Rect(185, 36, 55, 20);
                            GUI.Label(bRect, "shift + Z", FGUI_Resources.HeaderStyle);
                        }
                    }

                    if (projectPreset != null)
                        if (string.IsNullOrEmpty(projectPreset.InfoText) == false)
                        {
                            bool draw = true;
                            if (!advancedMode) if (projectPreset.InfoText.StartsWith("Here write custom")) draw = false;

                            if (draw)
                            {
                                GUIContent cont = new GUIContent("Info:\n" + projectPreset.InfoText);
                                Vector2 sz = EditorStyles.label.CalcSize(cont); sz = new Vector2(sz.x * 1.04f + 8, sz.y * 1.1f);
                                bRect = new Rect((Screen.width / dpiH) - sz.x, hOffset - 45 - sz.y, sz.x, sz.y);
                                GUI.Label(bRect, cont);
                            }
                        }

                    //if (focusOnY < grid.GetMin().y) focusOnY = grid.GetMax().y;
                    //if (focusOnY > grid.GetMax().y) focusOnY = grid.GetMin().y;

                    GUI.backgroundColor = preC;
                }
                //else
                //{
                //    focusOnY = 0;
                //}
            }
        }


        #region Handling Input

        bool paintNow = true;
        int focusOnY = 0;

        void ProcessInputEvents()
        {
            if (SceneView.lastActiveSceneView == null) return;
            if (Selection.activeGameObject != null) if (Selection.activeGameObject.scene.rootCount == 1) return;
            if (Event.current == null) return;

            Camera sceneCam = SceneView.lastActiveSceneView.camera;
            if (sceneCam == null) return;

            Event e = Event.current;

            if (e.type == EventType.Used) return;

            bool switched = false;
            if (((e.shift) && (e.keyCode == KeyCode.X)) && e.type == EventType.KeyDown)
            {
                paintNow = !paintNow;
                switched = true;
            }

            if (!switched)
                if (paintNow)
                {

                    if (((e.shift) && (e.keyCode == KeyCode.Z)) && e.type == EventType.KeyDown)
                    {
                        focusOnY -= 1;
                    }
                    else if (((e.shift) && (e.keyCode == KeyCode.A)) && e.type == EventType.KeyDown)
                    {
                        focusOnY += 1;
                    }
                    else if (((e.shift) && (e.keyCode == KeyCode.Q)) && e.type == EventType.KeyDown)
                    {
                        focusOnY = 0;
                    }
                    else
                    {

                        // Removing rectangle selection event when dragging mouse with lmb
                        int controlId = GUIUtility.GetControlID(FocusType.Passive);
                        if (e.type == EventType.MouseDown)
                        {
                            if (e.button == 2) return;

                            GUIUtility.hotControl = controlId;
                            Event.current.Use();
                        }

                        // Perform painting in world space from scene camera
                        if (e.isMouse || e.type == EventType.Used)
                        {
                            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                            {
                                if (e.button < 2)
                                {
                                    PaintGrid(e, sceneCam, e.button == 1, focusOnY, Get.projectPreset.CellSize);
                                    ClearEvent(e);
                                    return;
                                }
                            }
                            else if (e.type == EventType.Used)
                                if (e.button < 1)
                                {
                                    PaintGrid(e, sceneCam, false, focusOnY, Get.projectPreset.CellSize);
                                    ClearEvent(e);
                                    return;
                                }
                        }
                    }
                }


        }

        void ClearEvent(Event e)
        {
            if (!paintNow) return;

            e.Use();
            e = null;
        }

        #endregion


        void PaintGrid(Event e, Camera sceneCam, bool erase = false, int yLevel = 0, float cellWorldSize = 2f)
        {
            float dpiH = FGenerators.EditorUIScale;
            int hOffset = (int)(Screen.height / dpiH);

            Plane floor = new Plane(Vector3.up, new Vector3(0, yLevel * cellWorldSize, 0));
            float dist;
            float dpi = FGenerators.EditorUIScale;
            Ray camRay = sceneCam.ScreenPointToRay(new Vector3(e.mousePosition.x * dpi, sceneCam.pixelHeight - e.mousePosition.y * dpi));
            floor.Raycast(camRay, out dist);

            Vector3 clickPos = camRay.GetPoint(dist);

            Vector3Int gridPos;
            float x = clickPos.x / projectPreset.CellSize;
            //float y = clickPos.y / projectPreset.CellSize;
            float z = clickPos.z / projectPreset.CellSize;
            gridPos = new Vector3Int(Mathf.RoundToInt(x), yLevel, Mathf.RoundToInt(z));


            if (erase == false)
            {
                grid.AddCell(gridPos);
            }
            else
            {
                var cell = grid.GetCell(gridPos);
                cell.InTargetGridArea = false;
                if (grid.AllApprovedCells.Contains(cell)) grid.AllApprovedCells.Remove(cell);
            }

            TriggerRefresh();
        }


    }
}

#endif