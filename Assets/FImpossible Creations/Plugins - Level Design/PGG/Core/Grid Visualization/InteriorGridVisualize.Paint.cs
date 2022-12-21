using UnityEngine;

namespace FIMSpace.Generating
{
    public static partial class GridVisualize
    {
        public enum EPaintMode { None, Paint_Erase, Edit_Cells }

        public static void DrawPaintGUI(ref bool paintNow, int screenOffset = 0)
        {
            Rect bRect = new Rect(15, screenOffset - 78, 120, 24);
            Color preC = GUI.backgroundColor;

            if (paintNow) GUI.backgroundColor = Color.green;

            if (GUI.Button(bRect, "Paint " + (paintNow ? "ON" : "OFF") + " (shift+x)"))
            {
                paintNow = !paintNow;
#if UNITY_EDITOR
                RefreshPaintTimer();
#endif
            }

            if (paintNow)
            {
                bRect = new Rect(15, screenOffset - 114, 240, 34);
                GUI.Label(bRect, "LMB - Paint, RMB - Erase\nUse middle mouse to move camera");
            }

            GUI.backgroundColor = preC;
        }


        public static Vector3 GetMouseWorldPosition(Vector3 normal, Event e, Camera sceneCam, int yLevel = 0, float cellWorldSize = 2f, Vector3? posOffset = null)
        {
            Vector3 yLevelOff = new Vector3(0, yLevel * cellWorldSize, 0);
            Plane floor = new Plane(normal, yLevelOff + (posOffset == null ? Vector3.zero : posOffset.Value));
            float dpi = FGenerators.EditorUIScale;
            Ray camRay = sceneCam.ScreenPointToRay(new Vector3(e.mousePosition.x * dpi, (sceneCam.scaledPixelHeight - e.mousePosition.y * dpi)) + yLevelOff);
            float dist;
            floor.Raycast(camRay, out dist);
            return camRay.GetPoint(dist);
        }

        public static Vector3 GetMouseWorldPosition2D(Vector3 normal, Event e, Camera sceneCam, int zLevel = 0, float cellWorldSize = 2f, Vector3? posOffset = null)
        {
            Vector3 zLevelOff = new Vector3(0, 0, zLevel * cellWorldSize);
            Plane floor = new Plane(normal, zLevelOff + (posOffset == null ? Vector3.zero : posOffset.Value));
            float dpi = FGenerators.EditorUIScale;
            Ray camRay = sceneCam.ScreenPointToRay(new Vector3(e.mousePosition.x * dpi, (sceneCam.scaledPixelHeight - e.mousePosition.y * dpi)) + zLevelOff);
            float dist;
            floor.Raycast(camRay, out dist);
            return camRay.GetPoint(dist);
        }

        public static FieldCell PaintGrid(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, Event e, Camera sceneCam, bool? erase, Transform root, int yLevel, float cellWorldSize, Vector3? posOff = null, bool is2D = false)
        {
            Vector3 clickPos;

            if (is2D)
            {
                //if (posOff != null) posOff = new Vector3(posOff.Value.x, posOff.Value.z, posOff.Value.y);
                clickPos = GetMouseWorldPosition2D(Vector3.back, e, sceneCam, yLevel, cellWorldSize, posOff);
            }
            else
                clickPos = GetMouseWorldPosition(Vector3.up, e, sceneCam, yLevel, cellWorldSize, posOff);

            if (root != null)
            {
                clickPos = root.InverseTransformPoint(clickPos);
            }

            Vector3Int gridPos;
            float unitSize = preset.GetCellUnitSize().x;

            if (is2D)
            {
                float x = clickPos.x / unitSize;
                float y = (clickPos.y - cellWorldSize * 0.5f) / unitSize;
                gridPos = new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), yLevel);
            }
            else
            {
                float x = (clickPos.x) / unitSize;
                float z = (clickPos.z) / unitSize;
                gridPos = new Vector3Int(Mathf.RoundToInt(x), yLevel, Mathf.RoundToInt(z));
            }

            if (erase == null)
            {

                var cell = grid.GetCell(gridPos);
                return cell;
            }
            else
            if (erase == false)
            {
                return grid.AddCell(gridPos);
            }
            else
            {
                var cell = grid.GetCell(gridPos);
                cell.InTargetGridArea = false;
                if (grid.AllApprovedCells.Contains(cell)) grid.AllApprovedCells.Remove(cell);
                return cell;
            }
        }

    }
}

