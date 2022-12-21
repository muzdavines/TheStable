#if UNITY_EDITOR
// Inside #if UnityEditor for monobehvaiours to use this classes

using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{
    public static partial class GridVisualize
    {


        #region Handling Input

        public static float TimeOfEnable = 0f;
        public static void RefreshPaintTimer() { TimeOfEnable = Time.realtimeSinceStartup; }

        public static FieldCell ProcessInputEvents(ref bool paintNow, FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, ref int yLevel, Transform root = null, bool modifyGrid = true, float cellWorldSize = 2, bool is2D = false)
        {
            if (SceneView.lastActiveSceneView == null) return null;
            if (Selection.activeGameObject != null) if (Selection.activeGameObject.scene.rootCount == 1) return null;
            if (Event.current == null) return null;

            Camera sceneCam = SceneView.lastActiveSceneView.camera;
            if (sceneCam == null) return null;

            Event e = Event.current;

            if (((e.shift) && (e.keyCode == KeyCode.X)) && e.type == EventType.KeyDown)
            {
                paintNow = !paintNow;
            }
            else
            {
                if (Time.realtimeSinceStartup - TimeOfEnable < 0.25f) return null;

                if (e.type == EventType.Used) return null;
                //if (e.type == EventType.ContextClick) return;

                if (paintNow)
                {

                    if (((e.shift) && (e.keyCode == KeyCode.Z)) && e.type == EventType.KeyDown)
                    {
                        yLevel -= 1;
                    }
                    else if (((e.shift) && (e.keyCode == KeyCode.A)) && e.type == EventType.KeyDown)
                    {
                        yLevel += 1;
                    }
                    else if (((e.shift) && (e.keyCode == KeyCode.Q)) && e.type == EventType.KeyDown)
                    {
                        yLevel = 0;
                    }
                    else
                    {

                        // Removing rectangle selection event when dragging mouse with lmb
                        int controlId = GUIUtility.GetControlID(FocusType.Passive);
                        if (e.type == EventType.MouseDown)
                        {
                            if (e.button == 2) return null;

                            GUIUtility.hotControl = controlId;
                            Event.current.Use();
                        }

                        // Perform painting in world space from scene camera
                        if (e.isMouse || e.type == EventType.Used)
                        {
                            Vector3? rootOff = null;
                            if (root != null)
                            {
                                if ( is2D)
                                rootOff = Vector3.forward * root.position.z;
                                else
                                rootOff = Vector3.up * root.position.y;
                            }

                            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                            {
                                if (e.button < 2)
                                {
                                    FieldCell c;
                                    if (modifyGrid) c = PaintGrid(grid, preset, e, sceneCam, e.button == 1, root, yLevel, cellWorldSize, rootOff, is2D);
                                    else c = PaintGrid(grid, preset, e, sceneCam, null, root, yLevel, cellWorldSize, rootOff, is2D);

                                    ClearEvent(e, paintNow);
                                    return c;
                                }
                            }
                            else if (e.type == EventType.Used)
                                if (e.button < 1)
                                {
                                    FieldCell c;
                                    if (modifyGrid) c = PaintGrid(grid, preset, e, sceneCam, false, root, yLevel, cellWorldSize, rootOff, is2D);
                                    else c = PaintGrid(grid, preset, e, sceneCam, null, root, yLevel, cellWorldSize, rootOff, is2D);

                                    ClearEvent(e, paintNow);
                                    return c;
                                }
                        }
                    }

                }

            }

            return null;
        }

        public static void ClearEvent(Event e, bool paintingNow)
        {
            if (!paintingNow) return;

            e.Use();
            e = null;
        }

        #endregion

    }
}

#endif
