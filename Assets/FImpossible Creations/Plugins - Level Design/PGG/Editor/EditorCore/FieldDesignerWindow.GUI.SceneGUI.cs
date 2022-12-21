using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace FIMSpace.Generating
{
    public partial class FieldDesignWindow
    {
        public bool AutoRefreshPreview = true;
        public bool PreviewAutoSpawn = true;
        public bool ColorizePreview = true;
        public bool DrawGrid = true;
        public bool DrawEmptys = true;
        public bool AutoDestroy = true;
        public bool DrawWhenFocused = false;
        public bool DrawScreenGUI = true;
        private bool Repose = false;
        [Range(0f, 1f)] public float PreviewAlpha = 0.0f;


        #region Base preparation for Scene GUI Draw


        void OnFocus()
        {
#if UNITY_2019_4_OR_NEWER
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#endif
            FGeneratorsGizmosDrawer.AddEvent(OnDrawGizmos);
        }

        bool isVisible = true;
        void OnBecameVisible()
        {
            isVisible = true;
        }
        void OnBecameInvisible()
        {
            isVisible = false;
            FGeneratorsGizmosDrawer.TemporaryRemove();
        }


        void OnDestroy()
        {
            viewed = 0;

#if UNITY_2019_4_OR_NEWER
            SceneView.duringSceneGui -= this.OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#endif

            if (FGeneratorsGizmosDrawer.Instance)
            {
                if (AutoDestroy) ClearAllGeneratedGameObjects();
            }

            FGeneratorsGizmosDrawer.TemporaryRemove();
        }


        public Matrix4x4 gridMatrix = Matrix4x4.identity;
        void OnSceneGUI(SceneView sceneView)
        {
            if (SceneView.currentDrawingSceneView == null) return;
            if (SceneView.currentDrawingSceneView.camera == null) return;
            if (!isVisible) return;
            if (gridMode != EDesignerGridMode.Paint) if (DrawWhenFocused)
                    if (focusedWindow != Get && (focusedWindow is SceneView == false)) return;

            Handles.BeginGUI();
            DrawPaintGUI();
            Handles.EndGUI();

            Handles.BeginGUI();
            Handles.SetCamera(SceneView.currentDrawingSceneView.camera);
            Handles.matrix = gridMatrix;
            if (grid != null) DrawHandles(SceneView.currentDrawingSceneView.camera);
            Handles.matrix = Matrix4x4.identity;
            Handles.EndGUI();

        }

        GUIStyle centeredWhiteMiniLabel = null;

        #endregion


        void OnDrawGizmos()
        {
            DrawGridAndPreview();
        }


        void DrawHandles(Camera c)
        {
            Color cl = GUI.color;
            Matrix4x4 mx = gridMatrix;

            /*if (drawTestGenSetts) */
            if (DrawScreenGUI && !isSelectedPainter)
                if (gridMode == EDesignerGridMode.Paint)
                    ProcessInputEvents();

            DrawDebugCommandsHandles();

            GUI.color = cl;
            Repaint();
        }


        void DrawGridAndPreview()
        {
            if (grid == null) return;
            if (grid.AllCells == null) return;
            if (projectPreset == null) return;
            if (!isVisible) return;
            if (gridMode != EDesignerGridMode.Paint) if (DrawWhenFocused)
                {
                    if (focusedWindow != Get
                        && (focusedWindow is SceneView == false)) return;
                }

            Color preC = Gizmos.color;
            Color prehC = Handles.color;

            Vector3 cellSize = projectPreset.GetCellUnitSize(); cellSize.y *= 0.1f;
            Vector3 cellSize2 = projectPreset.GetCellUnitSize() * 0.85f; cellSize.y *= 0.02f;
            Vector3 cSize = projectPreset.GetCellUnitSize();
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);

            if (centeredWhiteMiniLabel == null)
            {
                centeredWhiteMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                centeredWhiteMiniLabel.normal.textColor = new Color(1f, 1f, 1f, 0.75f);
            }

            FieldModification selectedMod = null;
            if (Selection.activeObject) selectedMod = Selection.activeObject as FieldModification;
            if (selectedMod == null) if (SeparatedModWindow.Get) { selectedMod = SeparatedModWindow.Get.latestMod as FieldModification; }

            int scale = 1;
            if (selectedMod != null)
            {
                if (selectedMod.Spawners == null) selectedMod.Spawners = new List<FieldSpawner>();
                if (selectedMod.Spawners.Count > 0)
                    if (selectedMod._editor_shareSelected < selectedMod.Spawners.Count)
                        scale = selectedMod.Spawners[selectedMod._editor_shareSelected].OnScalledGrid;
            }

            if (scale < 1) scale = 1;


            // Draw grid helper axis
            Vector3Int min = grid.GetMin();

            Handles.color = new Color(0f, 0f, 1f, 0.5f);
            Vector3 leftSide = new Vector3((min.x - 1) * (cellSize.x), 0, (min.z - 0) * (cellSize.z));
            leftSide += cellSize.x * Vector3.left * 0.3f;

            FEditor.FGUI_Handles.DrawArrow(leftSide, Quaternion.identity, cellSize.x * 0.5f);
            Handles.Label(leftSide + cellSize.x * Vector3.left * 0.3f + Vector3.back * cellSize.x * 0.2f, new GUIContent("Forward"), centeredWhiteMiniLabel);

            Handles.color = new Color(1f, .4f, .4f, 0.5f);
            Vector3 rightSide = new Vector3((min.x - 0) * (cellSize.x), 0, (min.z - 1) * (cellSize.z));
            rightSide += cellSize.z * Vector3.back * 0.3f;
            FEditor.FGUI_Handles.DrawArrow(rightSide, Quaternion.Euler(0, 90, 0), cellSize.x * 0.5f);
            Handles.Label(rightSide + cellSize.z * Vector3.back * 0.3f, new GUIContent("Right"), centeredWhiteMiniLabel);


            for (int p = 0; p < projectPreset.ModificatorPacks.Count; p++)
            {
                var mods = projectPreset.ModificatorPacks[p];

                for (int i = 0; i < grid.AllCells.Count; i++)
                {
                    FieldCell cell = grid.AllCells[i];

                    Vector3 pos = new Vector3(cell.Pos.x, cell.Pos.y, cell.Pos.z);
                    pos = projectPreset.TransformCellPosition(pos);

                    if (cell != null && cell.InTargetGridArea)
                    {
                        var spawns = cell.GetSpawnsJustInsideCell();

                        // Supporting gizmos draw for spawned rules
                        for (int s = 0; s < spawns.Count; s++)
                        {
                            var sp = spawns[s];
                            if (sp.Spawner != null)
                                if (sp.Spawner.Rules != null)
                                    for (int r = 0; r < sp.Spawner.Rules.Count; r++)
                                    {
                                        var rl = sp.Spawner.Rules[r];
                                        if (rl == null) continue;
                                        if (rl.Enabled == false || rl.Ignore) continue;
                                        if (rl._EditorDebug) rl.OnDrawDebugGizmos(projectPreset, sp, cell, grid);
                                    }
                        }

                        if (DrawEmptys)
                        {
                            // Drawing emptys
                            Gizmos.color = new Color(0.3f, 1f, 0.2f, 0.9f);
                            for (int m = 0; m < spawns.Count; m++)
                            {
                                if (spawns[m].idInStampObjects == -2 && spawns[m].WasTemporaryPrefab == false)
                                {
                                    Quaternion rotation = Quaternion.Euler(spawns[m].RotationOffset);
                                    Gizmos.matrix = Matrix4x4.TRS(pos + spawns[m].Offset + rotation * spawns[m].DirectionalOffset, rotation, spawns[m].LocalScaleMul);
                                    Gizmos.DrawWireSphere(Vector3.zero, cellSize.x * 0.1f);
                                    Gizmos.matrix = Matrix4x4.identity;
                                }
                            }
                            Gizmos.color = Color.white;
                        }

                        if (ColorizePreview == false) Gizmos.color = new Color(1f, 1f, 1f, PreviewAlpha);

                        // Draw spawner objects previews
                        if (PreviewAlpha > 0f)
                            for (int m = 0; m < spawns.Count; m++)
                            {
                                if (ColorizePreview)
                                {
                                    Color tgtCol = Color.HSVToRGB(CountModForColor(spawns[m].OwnerMod), 0.6f, 0.75f);
                                    tgtCol.a = PreviewAlpha; Gizmos.color = tgtCol;
                                }

                                spawns[m].OwnerMod.OnSceneGUI(cell, projectPreset, grid, spawns[m]);

                                if (spawns[m].Spawner.DisplayPreviewGUI)
                                {
                                    if (spawns[m].Prefab != null)
                                    {
                                        Quaternion rotation = spawns[m].Prefab.transform.rotation * Quaternion.Euler(spawns[m].RotationOffset);

                                        if (spawns[m].PreviewMesh != null)
                                        {
                                            Vector3 posp = pos + spawns[m].Offset + rotation * spawns[m].DirectionalOffset;
                                            Gizmos.DrawMesh(spawns[m].PreviewMesh, posp, rotation, Vector3.Scale(spawns[m].Prefab.transform.localScale, spawns[m].LocalScaleMul));

                                            if (spawns[m].OwnerMod.DrawMeshAndBox)
                                            {
                                                if (spawns[m].PreviewMesh != null)
                                                {
                                                    Gizmos.DrawCube((pos + spawns[m].PreviewMesh.bounds.center) + spawns[m].Offset + rotation * (spawns[m].DirectionalOffset * 1.175f), rotation * Vector3.Scale(spawns[m].PreviewMesh.bounds.size, spawns[m].LocalScaleMul));
                                                }
                                                else
                                                {
                                                    Gizmos.DrawCube(pos + spawns[m].Offset + rotation * (spawns[m].DirectionalOffset * 1.175f), Vector3.Scale(new Vector3(0.16f, 0.1f, 0.16f), spawns[m].LocalScaleMul));
                                                }

                                                Gizmos.DrawCube(posp, Vector3.Scale(new Vector3(0.3f, 0.1f, 0.3f), spawns[m].LocalScaleMul));
                                            }
                                        }
                                        else
                                        {
                                            Gizmos.matrix = Matrix4x4.TRS(pos + spawns[m].Offset + rotation * spawns[m].DirectionalOffset, rotation, spawns[m].LocalScaleMul);
                                            Gizmos.DrawCube(Vector3.zero, Vector3.Scale(new Vector3(0.5f, 0.1f, 0.5f), spawns[m].LocalScaleMul));
                                            Gizmos.matrix = Matrix4x4.identity;
                                        }
                                    }
                                }
                            }



                        if (scale == 1)
                            if (DrawGrid && !usedPainter)
                            {
                                if (cell.Pos.y == focusOnY)
                                {
                                    Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
                                    Gizmos.DrawWireCube(pos, cellSize);

#if UNITY_2020_1_OR_NEWER
                                    if (ColorizePreview)
                                    {
                                        string displayS = "";
                                        if (spawns.Count > 0) displayS = ((pos.x / cSize.x).ToString() + ", " + (pos.z / cSize.z).ToString()) + "\n" + ("Spawns: " + (spawns.Count).ToString());
                                        else displayS = (pos.x / cSize.x).ToString() + ", " + (pos.z / cSize.z).ToString();

                                        // I have no idea why, but unity 2020 Handles.Label are positioned in some strange way
                                        Handles.Label(pos, displayS, centeredWhiteMiniLabel);
                                    }
#else
                                    if (spawns.Count > 0)
                                        Handles.Label(pos, ((pos.x / cSize.x).ToString() + ", " + (pos.z / cSize.z).ToString()) + "\n" + ("Spawns: " + (spawns.Count).ToString()), centeredWhiteMiniLabel);
                                    else
                                        Handles.Label(pos, (pos.x / cSize.x).ToString() + ", " + (pos.z / cSize.z).ToString(), centeredWhiteMiniLabel);
#endif

                                    if (cell.ChildCells != null)
                                    {
                                        Vector3 u = Vector3.up * cSize.y * 0.2f;
                                        Gizmos.DrawCube(pos + u, cellSize * 0.3f);

                                        for (int ch = 0; ch < cell.ChildCells.Count; ch++)
                                        {
                                            if (FGenerators.CheckIfIsNull(cell.ChildCells[ch])) continue;
                                            Vector3 chPos = projectPreset.TransformCellPosition((Vector3)cell.ChildCells[ch].Pos);
                                            //Vector3 chPos = (Vector3)cell.ChildCells[ch].Pos * projectPreset.CellSize;
                                            Gizmos.DrawLine(pos + u, chPos + u);
                                            Gizmos.DrawCube(chPos + u, cellSize * 0.15f);
                                        }
                                    }
                                    else if (cell.ParentCell != null)
                                    {
                                        Gizmos.color = Color.cyan * 0.5f;
                                        Vector3 u = Vector3.up * cSize.y * 0.3f;
                                        Gizmos.DrawCube(pos + u, cellSize * 0.3f);

                                        Vector3 chPos = projectPreset.TransformCellPosition((Vector3)cell.ParentCell.Pos);
                                        //Vector3 chPos = (Vector3)cell.ParentCell.Pos * projectPreset.CellSize;
                                        Gizmos.DrawLine(pos + u, chPos + u);
                                        Gizmos.DrawCube(chPos + u, cellSize * 0.15f);
                                    }
                                }
                                else
                                {
                                    Gizmos.color = new Color(1f, 1f, 1f, 0.01f);
                                    Gizmos.DrawWireCube(pos, cellSize2);
                                }
                            }

                    }
                    else
                    {
                        if (scale == 1)
                        {
                            if (DrawGrid && !usedPainter)
                            {
                                if (cell.Pos.y == focusOnY)
                                {
                                    Gizmos.color = new Color(0f, 0f, 0f, 0.125f);
                                    Gizmos.DrawWireCube(pos, cellSize);
                                }
                                else
                                {
                                    Gizmos.color = new Color(1f, 1f, 1f, 0.025f);
                                    Gizmos.DrawWireCube(pos, cellSize2);
                                }
                            }
                        }

                    }
                }

            }

            if (scale > 1)
            {
                Vector3 scaledSize = cSize * scale;
                cellSize = new Vector3(scaledSize.x, cSize.y * 0.1f * scale, scaledSize.z);
                Gizmos.color = new Color(1f, 1f, 1f, 0.1f);

                Vector3 cellOffset = new Vector3(1f, 0f, 1f) * (scale * 1f) - new Vector3(1f, 0f, 1f) * (projectPreset.CellSize * 0.5f);

                int modulo = scale;

                for (int x = grid.MinX.Pos.x; x < grid.MaxX.Pos.x; x += 1)
                {
                    if (x % modulo != 0) continue;
                    for (int z = grid.MinZ.Pos.z; z < grid.MaxZ.Pos.z; z += 1)
                    {
                        if (z % modulo != 0) continue;
                        var cell = grid.GetCell(x, focusOnY, z, false);
                        if (FGenerators.CheckIfIsNull(cell)) continue;

                        Vector3 pos = cell.WorldPos(cSize);
                        pos += cellOffset;

                        if (grid.CountCellsAround(cell, scale) <= 1)
                        {
                            float al = cell.Pos.y == focusOnY ? 1f : 0.4f;
                            Gizmos.color = new Color(0f, 0f, 0f, 0.2f * al);
                            Gizmos.DrawWireCube(pos, cellSize);
                            Gizmos.color = new Color(1f, 1f, 1f, 0.1f * al);
                        }
                        else
                            Gizmos.DrawWireCube(pos, cellSize);
                    }
                }

            }


            Handles.color = prehC;
            Gizmos.color = preC;
        }



        #region Utilities


        private float CountModForColor(FieldModification mod)
        {
            if (mod == null) return 0f;
            if (projectPreset == null) return 0f;
            if (projectPreset.ModificatorPacks == null) return 0f;

            for (int p = 0; p < projectPreset.ModificatorPacks.Count; p++)
            {
                int i = 0;

                float modColStep = 0.1f;
                if (projectPreset.ModificatorPacks != null) modColStep = 1f / (float)projectPreset.ModificatorPacks.Count;

                for (int m = 0; m < projectPreset.ModificatorPacks[p].FieldModificators.Count; m++)
                {
                    if (mod == projectPreset.ModificatorPacks[p].FieldModificators[m]) return (float)i * modColStep;
                    i++;
                }
            }

            return 0f;
        }


        #endregion



        #region Debug Draw commands


        List<DrawCommand> drawCommands;
        int drawCommandDuration = 0;
        public void DrawCellPosition(FieldCell cell, int duration, float size, Color col)
        {
            Vector3 pos = new Vector3(cell.Pos.x, cell.Pos.y, cell.Pos.z);
            pos *= projectPreset.CellSize;
            DrawPosition(pos, duration, size, col);
        }


        public void DrawPosition(Vector3 position, int duration, float size, Color col)
        {
            if (drawCommands == null) drawCommands = new List<DrawCommand>();
            drawCommands.Add(new DrawCommand() { pos = position, duration = duration, size = size, c = col });
            if (drawCommandDuration <= 0) drawCommandDuration = duration;
        }


        void DrawDebugCommandsHandles()
        {
            if (drawCommands != null)
                if (drawCommands.Count > 0)
                {
                    drawCommandDuration -= 1;
                    Handles.color = drawCommands[0].c;
                    Handles.SphereHandleCap(0, drawCommands[0].pos, Quaternion.identity, projectPreset.CellSize * 0.3f * drawCommands[0].size, EventType.Repaint);

                    if (drawCommandDuration <= 0)
                    {
                        drawCommands.RemoveAt(0);
                        if (drawCommands.Count > 0) drawCommandDuration = drawCommands[0].duration + 1;
                    }
                }
        }


        public struct DrawCommand
        {
            public Vector3 pos;
            public int duration;
            public float size;
            public Color c;
        }


        #endregion


    }
}