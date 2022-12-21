#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif

using UnityEngine;
using System.Collections.Generic;
using System;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Work In Progress/Flexible Painter", 9)]
    public class FlexiblePainter : PGGFlexibleGeneratorBase
    {

        #region Add Header Options

#if UNITY_EDITOR

        [MenuItem("CONTEXT/FlexibleGenerator/Clear whole Grid and Instructions")]
        private static void ClearWholeGrid(MenuCommand menuCommand)
        {
            FlexiblePainter painter = menuCommand.context as FlexiblePainter;
            if (painter) painter.Cells.ClearAll();
        }

#endif

        #endregion


        #region Painting Related

        public enum EPaintMode { Cells, Instructions }
        [HideInInspector] [SerializeField] public int PaintingID = -1;
        public bool Painting
        {
            get { return _Editor_Paint; }
            set { _Editor_Paint = value; }
        }

        #endregion


        [HideInInspector]
        [PGG_SingleLineTwoProperties("AllowOverlapInstructions", 0, 0, 10, 0, 3)]
        [Tooltip("If adding instructions on certain positions should automatically add cell to grid in same position. Useful for inject painting.")]
        public bool AddCellsOnInstructions = false;
        [Tooltip("If you want to add two instructions into single cell with painting")]
        [HideInInspector] public bool AllowOverlapInstructions = false;

        public enum EDebug { None, DrawGrid, DrawGridDetails }
        //[PGG_SingleLineTwoProperties("Transprent", 60, 70, 10, -45, 3)]
        public EDebug Debug = EDebug.DrawGrid;
        [HideInInspector] [Tooltip("Just making cell data rectanles transparent on gizmos")] public bool Transprent = true;

        #region Editor Related

        [HideInInspector] public bool _EditorGUI_DrawExtra = false;
        [HideInInspector] public bool _EditorGUI_DrawIgnoring = false;
        [HideInInspector] public bool _EditorGUI_DrawVars = false;
        [HideInInspector] public bool _EditorGUI_DrawPackVars = false;
        [HideInInspector] public bool _ModifyVars = false;
        [HideInInspector] public bool _ModifyPackVars = false;
        [HideInInspector] public int _EditorGUI_SelectedId = -1;

        [HideInInspector] public bool _Editor_Paint = false;
        [HideInInspector] public int _Editor_RadiusY = 1;
        [HideInInspector] public int _Editor_PaintRadius = 1;
        [HideInInspector] public int _Editor_YLevel = 0;
        [HideInInspector] public int _Editor_CommandsPage = 0;

        [HideInInspector] public GridPainter.EPaintSpaceMode _Editor_PaintSpaceMode = GridPainter.EPaintSpaceMode.XZ;
        public bool IsPainting2D { get { return _Editor_PaintSpaceMode == GridPainter.EPaintSpaceMode.XY_2D; } }
        [HideInInspector] public string _Editor_Instruction = "0";
        [HideInInspector] public bool _Editor_RotOrMovTool = false;
        [HideInInspector] public bool _Editor_ContinousMode = false;

        #endregion


        public override void GenerateObjects()
        {
            Prepare();

            // Run rules
            Cells.Generate(true);

            // Post events
            base.GenerateObjects();
        }


        public void PaintPosition(Vector3Int pos, bool erase = false, bool generate = false)
        {
            if (erase == false)
                Cells.AddCell(pos);
            else
                Cells.RemoveCell(pos);

            if (generate) GenerateObjects();
        }

        public override void ClearGenerated(bool destroy = true)
        {
            if (destroy) InstantiatedInfo.Clear(true);
            Cells.ClearPhantomSpawnsInAllCells();
            Cells.SetAllDirty();
        }

        public void HardClear(bool destroy)
        {
            base.ClearGenerated(destroy);
        }


        #region Debugging Help Gizmos

#if UNITY_EDITOR


        private void OnDrawGizmosSelected()
        {
            if (FieldSetup == null) return;
            if (DataSetup != null) DataSetup.RefreshReferences(this);

            Cells.CheckIfGridPrepared();
            var grid = Cells.Grid;

            // Preparation for drawing
            Color preColor = GUI.color;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Handles.matrix = Gizmos.matrix;
            Gizmos.color = new Color(1f, 1f, 1f, 0.1f);

            Vector3 cSize = FieldSetup.GetCellUnitSize();
            bool is2D = _Editor_PaintSpaceMode == GridPainter.EPaintSpaceMode.XY_2D;

            // Below just drawing debug grid, instructions, draw position - no events or paint triggers - it's done in OnSceneGUI in editor script component

            #region Drawing grid

            if (Debug != EDebug.None)
                for (int i = 0; i < grid.AllApprovedCells.Count; i++)
                {
                    var cell = grid.AllApprovedCells[i];
                    Vector3 genPosition = cell.WorldPos(cSize);

                    if ((!is2D && cell.Pos.y == _Editor_YLevel) || (is2D && cell.Pos.z == _Editor_YLevel))
                    {
                        if (is2D)
                            Gizmos.DrawWireCube(genPosition + new Vector3(0, cSize.y * 0.5f, 0), new Vector3(cSize.x, cSize.y, cSize.z * 0.2f));
                        else
                            Gizmos.DrawWireCube(genPosition, new Vector3(cSize.x, cSize.y * 0.2f, cSize.z));

                        //if (cell.InTargetGridArea) Gizmos.DrawCube(genPosition, new Vector3(cSize.x, cSize.y * 0.2f, cSize.z) * 0.5f);

                        //Color preC = Gizmos.color;
                        //Gizmos.color = Color.white;
                        //if (GenController.GridCellsSave.Contains(cell)) Gizmos.DrawCube(genPosition + Vector3.right * cSize.x * 0.25f, new Vector3(cSize.x, cSize.y * 0.2f, cSize.z) * 0.3f);
                        //Gizmos.color = preC;

                        if (Debug == EDebug.DrawGridDetails)
                        {
                            Handles.Label(genPosition, new GUIContent(cell.Pos.x + ", " + cell.Pos.z + "\nSpawns: " + cell.GetJustCellSpawnCount()), EditorStyles.centeredGreyMiniLabel);

                            if (FGenerators.CheckIfExist_NOTNULL(cell.ParentCell))
                            {
                                Handles.color = Color.cyan;
                                Handles.DrawLine(genPosition, genPosition + Vector3.up * cSize.y * 0.25f);
                                Handles.DrawLine(genPosition, cell.ParentCell.WorldPos(cSize));
                                Handles.color = Color.white;
                            }
                        }
                    }
                    else
                    {
                        Gizmos.color = new Color(1f, 1f, 1f, 0.05f);

                        if (is2D)
                            Gizmos.DrawWireCube(genPosition + new Vector3(0, cSize.y * 0.5f, 0), new Vector3(cSize.x * 0.9f, cSize.y * 0.9f, cSize.z * 0.05f));
                        else
                            Gizmos.DrawWireCube(genPosition, new Vector3(cSize.x * 0.9f, cSize.y * 0.05f, cSize.z * 0.9f));

                        Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
                    }
                }

            #endregion


            #region Drawing cell instructions

            var instr = Preparation.CellInstructions;
            if (instr != null)
                for (int i = 0; i < instr.Count; i++)
                {
                    Color nCol = Color.HSVToRGB((float)instr[i].HelperID * 0.1f % 1f, 0.7f, 0.8f);
                    nCol.a = Transprent ? 0.185f : 1f;
                    Gizmos.color = nCol;
                    Vector3 pos = FieldSetup.TransformCellPosition((Vector3)instr[i].gridPosition);
                    Gizmos.DrawCube(pos, new Vector3(cSize.x, cSize.y * 0.2f, cSize.z));
                }

            #endregion


            #region Drawing Entered Cells when painting + radius

            Gizmos.color = new Color(1f, 1f, 1f, 0.1f);

            if (Painting)
            {
                if (FieldSetup != null)
                {
                    if (SceneView.currentDrawingSceneView != null)
                        if (SceneView.currentDrawingSceneView.camera != null)
                        {
                            if (PaintingID == -1) Gizmos.color = Color.white;
                            else Gizmos.color = Color.yellow;

                            if (is2D == false)
                            {
                                // Line Guide
                                Vector3 mousePos = transform.InverseTransformPoint(GridVisualize.GetMouseWorldPosition(transform.up, Event.current, SceneView.currentDrawingSceneView.camera, _Editor_YLevel, cSize.y, transform.position.y * Vector3.up));
                                Gizmos.DrawLine(mousePos, mousePos + Vector3.up * 1);

                                // Cell Guide
                                Vector3 gridPos = FVectorMethods.FlattenVector(mousePos, cSize);
                                Gizmos.DrawWireCube(gridPos, new Vector3(cSize.x, cSize.y * 0.2f, cSize.z));

                                // Cells Radius
                                if (_Editor_PaintRadius > 1 || _Editor_RadiusY > 1)
                                {
                                    int rad = Mathf.RoundToInt((float)_Editor_PaintRadius * 0.7f);

                                    for (int x = -rad; x < rad; x++)
                                        for (int ry = 0; ry < _Editor_RadiusY; ry++)
                                            for (int z = -rad; z < rad; z++)
                                            {
                                                if (x == 0 && ry == 0 && z == 0) continue; // Ignore zero cell (drawn in lines above)
                                                Gizmos.DrawWireCube(gridPos + new Vector3(cSize.x * x, cSize.y * ry, cSize.z * z), new Vector3(cSize.x, cSize.y * 0.2f, cSize.z));
                                            }
                                }
                            }
                            else // 2D
                            {
                                // Line Guide
                                Vector3 mousePos = transform.InverseTransformPoint(GridVisualize.GetMouseWorldPosition2D(-transform.forward, Event.current, SceneView.currentDrawingSceneView.camera, _Editor_YLevel, cSize.y, transform.position.z * Vector3.forward));
                                Gizmos.DrawLine(mousePos, mousePos + Vector3.back * 1);

                                // Cell Guide
                                Vector3 gridPos = FVectorMethods.FlattenVector(mousePos - new Vector3(0f, cSize.y * 0.5f, 0f), cSize);
                                Gizmos.DrawWireCube(gridPos + new Vector3(0, cSize.y * 0.5f, 0f), new Vector3(cSize.x, cSize.y, cSize.z * 0.2f));

                                // Cells Radius
                                if (_Editor_PaintRadius > 1 || _Editor_RadiusY > 1)
                                {
                                    int rad = Mathf.RoundToInt((float)_Editor_PaintRadius * 0.7f);

                                    for (int x = -rad; x < rad; x++)
                                        for (int ry = -rad; ry < rad; ry++)
                                            for (int z = 0; z < _Editor_RadiusY; z++)
                                            {
                                                if (x == 0 && ry == 0 && z == 0) continue; // Ignore zero cell (drawn in lines above)
                                                Gizmos.DrawWireCube(gridPos + new Vector3(0f, cSize.y * 0.5f, 0) + new Vector3(cSize.x * x, cSize.y * ry, cSize.z * z), new Vector3(cSize.x, cSize.y, cSize.z * 0.2f));
                                            }
                                }
                            }
                        } // If field exist and scene view END
                }
            } // If painting END

            #endregion


            // Restore Gizmos
            Gizmos.color = preColor;
            Gizmos.matrix = Matrix4x4.identity;
            Handles.matrix = Matrix4x4.identity;

        }

#endif

        #endregion


        internal void OnChange()
        {
            if (AutoRefresh) GenerateObjects();
        }


        #region Grid related

        /// <summary>
        /// World Position to Grid Cell number Position
        /// </summary>
        public Vector3Int WorldToGridCellPosition(Vector3 worldPosition, bool is2D = false, Camera camera = null)
        {
            if (FieldSetup == null) return Vector3Int.zero;

            Vector3 cSize = FieldSetup.GetCellUnitSize();

            if (is2D == false) // Flat top down
            {
                Vector3 mousePos = transform.InverseTransformPoint(worldPosition);
                Vector3 gridPos = FVectorMethods.FlattenVector(mousePos, cSize);
                return (gridPos).V3toV3Int();
            }
            else // 2D - Flat Side
            {
                Vector3 mousePos = transform.InverseTransformPoint(GridVisualize.GetMouseWorldPosition2D(-transform.forward, Event.current, camera, _Editor_YLevel, cSize.y, transform.position.z * Vector3.forward));
                Vector3 gridPos = FVectorMethods.FlattenVector(mousePos - new Vector3(0f, cSize.y * 0.5f, 0f), cSize);
                return (gridPos).V3toV3Int();
            }
        }

        /// <summary>
        /// From grid cell position to world position
        /// </summary>
        public Vector3 GridCellToWorldPosition(Vector3Int gridCell)
        {
            Vector3 cSize = FieldSetup.GetCellUnitSize();
            return transform.TransformPoint(Vector3.Scale(gridCell, cSize));
        }

        /// <summary>
        /// From world position to cell position in world
        /// </summary>
        public Vector3 GetWorldPositionOfCellAt(Vector3 worldPosition, bool is2D = false)
        {
            return GridCellToWorldPosition(WorldToGridCellPosition(worldPosition, is2D));
        }

        internal static List<SpawnInstruction> CellInstructionsToSpawnInstructions(List<SpawnInstructionGuide> guides, FieldSetup setup, Transform generator)
        {
            List<SpawnInstruction> spwns = new List<SpawnInstruction>();
            
            for (int i = 0; i < guides.Count; i++)
            {
                var guide = guides[i];
                SpawnInstruction instr = new SpawnInstruction();


                Vector3 dir;

                if (guide.WorldRot)
                    dir = (Quaternion.Inverse(generator.rotation) * guide.rot) * Vector3.forward;
                else
                    dir = guide.rot * Vector3.forward;

                instr.desiredDirection = new Vector3Int(Mathf.RoundToInt(dir.x), 0, Mathf.RoundToInt(dir.z));
                instr.gridPosition = new Vector3Int(guide.pos.x, guide.pos.y, guide.pos.z);
                instr.useDirection = guide.UseDirection;

                if (guide.CustomDefinition == null)
                {
                    if (guide.Id < setup.CellsInstructions.Count)
                    {
                        instr.definition = setup.CellsInstructions[guide.Id];
                    }
                }
                else
                {
                    if (guide.CustomDefinition.InstructionType != InstructionDefinition.EInstruction.None)
                        instr.definition = guide.CustomDefinition;
                    else
                        if (guide.Id < setup.CellsInstructions.Count)
                        instr.definition = setup.CellsInstructions[guide.Id];
                }
                
                spwns.Add(instr);
            }

            return spwns;
        }

        #endregion

    }




#if UNITY_EDITOR

    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(FlexiblePainter))]
    public class FlexiblePainterEditor : PGGFlexibleGeneratorBaseEditor
    {
        public FlexiblePainter Get { get { if (_get == null) _get = (FlexiblePainter)target; return _get; } }
        private FlexiblePainter _get;

        public override bool RequiresConstantRepaint()
        {
            return Get.Painting;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _editor_drawRandomNumber = false;
            Get.Seed = 0;
        }

        protected override void DrawGUIFooter()
        {
            DrawGeneratingButtons(false, true);
            GUILayout.Space(3);
        }


        protected override void DrawGeneratingButtons(bool drawPreview = true, bool drawClear = true)
        {
            GUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate")) Get.GenerateObjects();

            if (bGet)
                if (bGet.DataSetup.CellsController != null)
                    if (bGet.DataSetup.CellsController.InstantiatedInfo != null)
                        if (bGet.DataSetup.CellsController.InstantiatedInfo.Instantiated != null)
                            if (bGet.DataSetup.CellsController.InstantiatedInfo.Instantiated.Count > 0)
                                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                                    Get.ClearGenerated(true);

            if (GUILayout.Button("Erase Grid", GUILayout.Width(70)))
            {
                Get.HardClear(true);
            }

            EditorGUILayout.EndHorizontal();
        }


        #region Scene GUI

        private void OnSceneGUI()
        {
            if (SceneView.currentDrawingSceneView == null) return;
            if (SceneView.currentDrawingSceneView.camera == null) return;
            if (Selection.activeGameObject != Get.gameObject) return;
            if (Get.FieldSetup == null) return;

            Get.Cells.CheckIfGridPrepared();
            var grid = Get.Cells.Grid;

            Undo.RecordObject(Get, "PGGGridP");

            Vector3 cSize = Get.FieldSetup.GetCellUnitSize();
            Color preH = Handles.color;


            {
                //if (grid != null)

                //    if (Get._Editor_Paint == false)
                //        if (Get.GetAllPainterCells.Count > 0)
                //        {
                //            Handles.color = new Color(0.2f, 1f, 0.2f, 0.8f);

                //            for (int i = Get.CellsInstructions.Count - 1; i >= 0; i--)
                //            {
                //                if (Get.CellsInstructions[i] == null)
                //                {
                //                    Get.CellsInstructions.RemoveAt(i);
                //                    continue;
                //                }

                //                var instr = Get.CellsInstructions[i];
                //                float size = cSize.x;
                //                Vector3 genPosition = Get.transform.TransformPoint(Get.FieldPreset.TransformCellPosition((Vector3)instr.pos));
                //                //Vector3 genPosition = Get.transform.TransformPoint((Vector3)instr.pos * size);

                //                if (Handles.Button(genPosition, Quaternion.identity, size * 0.2f, size * 0.2f, Handles.SphereHandleCap))
                //                {
                //                    if (Get.Selected == instr) Get.Selected = null; else Get.Selected = instr;
                //                }

                //                if (Get.Selected == instr)
                //                {
                //                    if (DrawButton(EditorGUIUtility.IconContent(Get._Editor_RotOrMovTool ? "MoveTool" : "RotateTool"), genPosition - Get.transform.forward * size * 0.4f - Get.transform.right * size * 0.4f, size * 0.9f))
                //                    {
                //                        Get._Editor_RotOrMovTool = !Get._Editor_RotOrMovTool;
                //                    }

                //                    if (Get._Editor_RotOrMovTool)
                //                    {
                //                        Handles.Label(genPosition - Get.transform.forward * size * 0.44f - Get.transform.right * size * 0.57f, new GUIContent(FGUI_Resources.Tex_Info, "Switch back to cell rotation tool if you want to use directional mode. If you leave this guide in movement mode then rotation will not be used!"));
                //                    }

                //                    instr.UseDirection = !Get._Editor_RotOrMovTool;

                //                    //if (Get.CellsInstructions.Count > 1)
                //                    //{
                //                    //    if (DrawButton(new GUIContent(instr.Id.ToString()), genPosition + Get.transform.forward * size * 0.4f , size * 0.9f)) instr.Id += 1;
                //                    //    if (DrawButton(new GUIContent("+"), genPosition + Get.transform.forward * size * 0.4f + Get.transform.right * size * 0.4f, size * 0.9f)) instr.Id += 1;
                //                    //    if (DrawButton(new GUIContent("-"), genPosition + Get.transform.forward * size * 0.3f + Get.transform.right * size * 0.5f, size * 0.9f)) instr.Id -= 1;
                //                    //    if (instr.Id < 0) instr.Id = Get.CellsInstructions.Count - 1;
                //                    //    if (instr.Id > Get.CellsInstructions.Count - 1) instr.Id = 0;
                //                    //}

                //                    if (Get._Editor_RotOrMovTool == false)
                //                    {
                //                        Get.Selected.rot = FEditor_TransformHandles.RotationHandle(Get.Selected.rot, genPosition, size * 0.75f);
                //                        Get.Selected.rot = FVectorMethods.FlattenRotation(Get.Selected.rot, 45f);
                //                    }
                //                    else
                //                    {
                //                        Get.Selected.pos = PGGUtils.V3toV3Int(Get.transform.InverseTransformPoint(FEditor_TransformHandles.PositionHandle(genPosition, Get.transform.rotation * instr.rot, size * 0.75f)) / size);
                //                        //Get.Selected.pos = FVectorMethods.FlattenVector(Get.Selected.pos, Get.RoomPreset.CellSize);
                //                    }

                //                    Quaternion r = Get.transform.rotation * Get.Selected.rot;
                //                    FGUI_Handles.DrawArrow(genPosition, Quaternion.Euler(r.eulerAngles), 0.5f, 0f, 1f);

                //                    if (instr != null)
                //                    {
                //                        if (instr.CustomDefinition.InstructionType != InstructionDefinition.EInstruction.None)
                //                            Handles.Label(genPosition + Get.transform.forward * size * 0.3f, new GUIContent(instr.CustomDefinition.Title));
                //                        else
                //                            Handles.Label(genPosition + Get.transform.forward * size * 0.3f, new GUIContent(Get.FieldPreset.CellsInstructions[instr.Id].Title));
                //                    }
                //                }
                //            }

                //        }
            }


            Handles.color = preH;
            Handles.BeginGUI();

            float dpiH = FGenerators.EditorUIScale;
            int hOffset = (int)(Screen.height / dpiH);


            #region Scene View GUI Buttons

            GridVisualize.DrawPaintGUI(ref Get._Editor_Paint, hOffset);

            if (Get._Editor_Paint)
            {
                //if (Get._Editor_PaintSpaceMode == GridPainter.EPaintSpaceMode.XZ)
                {
                    Rect radRect = new Rect(15, hOffset - 140, 140, 20);
                    Get._Editor_PaintRadius = Mathf.RoundToInt(GUI.HorizontalSlider(radRect, Get._Editor_PaintRadius, 1, 4));
                    radRect = new Rect(radRect);
                    radRect.y -= 18;
                    GUI.Label(radRect, new GUIContent("Radius"));

                    radRect = new Rect(15, hOffset - 179, 140, 20);
                    Get._Editor_RadiusY = Mathf.RoundToInt(GUI.HorizontalSlider(radRect, Get._Editor_RadiusY, 1, 4));
                    radRect = new Rect(radRect);
                    radRect.y -= 18;
                    GUI.Label(radRect, new GUIContent("Radius Y"));
                }

                Rect refreshRect = new Rect(160, hOffset - 75, 120, 20);
                Get.AutoRefresh = GUI.Toggle(refreshRect, Get.AutoRefresh, "Auto Refresh");

                if (!Get.AutoRefresh)
                {
                    refreshRect.position += new Vector2(116, 0);

                    if (GUI.Button(refreshRect, "Generate"))
                    {
                        Get.GenerateObjects();
                    }
                }
            }
            else
            {
                Rect refreshRect = new Rect(160, hOffset - 75, 120, 20);
                if (GUI.Button(refreshRect, "Generate"))
                {
                    //Get.ClearGenerated();
                    Get.GenerateObjects();
                }
            }


            Color preC = GUI.backgroundColor;
            Rect bRect;

            int x = 160;
            int y = (int)(22 * dpiH);
            bRect = new Rect(x, y, 160, 40);

            string focusTxt = Get._Editor_PaintSpaceMode == GridPainter.EPaintSpaceMode.XZ ? "Focus On Y Level: " : "Focus On Z Depth: ";

            GUI.Label(bRect, focusTxt + Get._Editor_YLevel, FGUI_Resources.HeaderStyle);
            bRect = new Rect(x + 150, y, 20, 20);
            if (GUI.Button(bRect, "▲")) Get._Editor_YLevel++;


            if (Get._Editor_Paint)
            {
                bRect = new Rect(x + 185, y, 55, 20);
                GUI.Label(bRect, "shift + A", FGUI_Resources.HeaderStyle);
            }

            bRect = new Rect(x + 150, y + 22 * dpiH, 20, 20);
            if (GUI.Button(bRect, "▼")) Get._Editor_YLevel--;

            if (Get._Editor_Paint)
            {
                bRect = new Rect(x + 185, y + 21 * dpiH, 55, 20);
                GUI.Label(bRect, "shift + Z", FGUI_Resources.HeaderStyle);
            }

            bRect = new Rect(x + 20 * dpiH, y + 31 * dpiH, 120, 20);
            bool is2D = Get._Editor_PaintSpaceMode == GridPainter.EPaintSpaceMode.XY_2D;
            is2D = GUI.Toggle(bRect, is2D, " Paint 2D");
            Get._Editor_PaintSpaceMode = is2D ? GridPainter.EPaintSpaceMode.XY_2D : GridPainter.EPaintSpaceMode.XZ;

            //bRect = new Rect(x + 200, y - 7 * dpiH, 300, 100);
            //GUI.Label(bRect, "If you don't see 'Paint' button on the bottom of\nscene view then you must change dpi settings\nof unity editor and restart it.");


            if (string.IsNullOrEmpty(Get.FieldSetup.InfoText) == false)
            {
                GUIContent cont = new GUIContent("Info:\n" + Get.FieldSetup.InfoText);
                Vector2 sz = EditorStyles.label.CalcSize(cont); sz = new Vector2(sz.x * 1.04f + 8, sz.y * 1.1f);
                bRect = new Rect((Screen.width / dpiH) - sz.x, hOffset - 45 * dpiH - sz.y, sz.x, sz.y);
                GUI.Label(bRect, cont);
            }

            #endregion


            DrawCellTools();

            Handles.EndGUI();

            if (Get.FieldSetup != null)
                if (Event.current.type == EventType.MouseMove)
                    SceneView.RepaintAll();


            #region Triggering Cell Painting etc.


            int eButton = Event.current.button;
            CellsController generating = Get.Cells;

            if (eButton < 2)
                if (Get.PaintingID == -1) // Painting simple grid cells
                {
                    var cell = GridVisualize.ProcessInputEvents(ref Get._Editor_Paint, grid, Get.FieldSetup, ref Get._Editor_YLevel, Get.transform, false, cSize.y, is2D);

                    if (cell != null)
                    {
                        if (eButton == 0)
                        {
                            Get.PaintPosition(cell.Pos);
                        }
                        else if (eButton == 1)
                        {
                            Get.PaintPosition(cell.Pos, true);
                        }

                        if (Get._Editor_PaintRadius > 1 || Get._Editor_RadiusY > 1)
                        {
                            int rad = Mathf.RoundToInt((float)Get._Editor_PaintRadius * 0.7f);

                            if (!is2D)
                            {
                                for (int rx = -rad; rx < rad; rx++)
                                    for (int ry = 0; ry < Get._Editor_RadiusY; ry++)
                                        for (int rz = -rad; rz < rad; rz++)
                                        {
                                            if (rx == 0 && ry == 0 && rz == 0) continue;
                                            Vector3Int radPos = cell.Pos + new Vector3Int(rx, ry, rz);

                                            if (cell.InTargetGridArea)
                                            {
                                                Get.PaintPosition(radPos);
                                            }
                                            else
                                                Get.PaintPosition(radPos, true);
                                        }
                            }
                            else
                            {

                                for (int rx = -rad; rx < rad; rx++)
                                    for (int ry = -rad; ry < rad; ry++)
                                        for (int rz = 0; rz < Get._Editor_RadiusY; rz++)
                                        {
                                            if (rx == 0 && ry == 0 && rz == 0) continue;
                                            Vector3Int radPos = cell.Pos + new Vector3Int(rx, ry, rz);

                                            if (cell.InTargetGridArea)
                                            {
                                                Get.PaintPosition(radPos);
                                            }
                                            else
                                                Get.PaintPosition(radPos, true);
                                        }
                            }


                        }

                        Get.OnChange();
                    }
                }
                else // Painting cell commands
                {
                    //bool change = false;
                    //bool rem = false;

                    //var e = Event.current;
                    //if (e != null) if (e.isMouse) if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) if (Event.current.button == 1) rem = true;

                    //var cell = GridVisualize.ProcessInputEvents(ref Get._Editor_Paint, grid, Get.FieldPreset, ref Get._Editor_YLevel, Get.transform, false, cSize.y, is2D);

                    //if (cell != null)
                    //{
                    //    int already = -1;
                    //    for (int i = 0; i < Get.CellsInstructions.Count; i++)
                    //    {
                    //        if (Get.CellsInstructions[i] == null) continue;
                    //        if (Get.CellsInstructions[i].pos == cell.Pos)
                    //        {
                    //            already = i;
                    //            if (Get.AllowOverlapInstructions)
                    //            {
                    //                if (Get.CellsInstructions[i].Id == Get.PaintingID)
                    //                {
                    //                    break;
                    //                }
                    //                else
                    //                    if (eButton == 0) already = -1;
                    //            }
                    //            else
                    //                break;
                    //        }
                    //    }

                    //    if (already == -1)
                    //    {

                    //        if (eButton == 0)
                    //        {
                    //            SpawnInstructionGuide instr = new SpawnInstructionGuide();
                    //            instr.pos = cell.Pos;
                    //            instr.Id = Get.PaintingID;

                    //            Get.CellsInstructions.Add(instr);

                    //            if (Get.AddCellsOnInstructions) grid.AddCell(cell.Pos);

                    //            change = true;
                    //            new SerializedObject(target).Update();
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (rem)
                    //        {
                    //            Get.CellsInstructions.RemoveAt(already);
                    //            change = true;
                    //            new SerializedObject(target).Update();
                    //        }
                    //    }

                    //    if (change) Get.OnChange();
                    //}
                }


            #endregion

        }


        /// <summary>
        /// Draw paint cell instructions pages etc.
        /// </summary>
        public void DrawCellTools()
        {
            if (Get.FieldSetup == null) return;

            Rect bRect = new Rect(15, 15, 132, 24);
            Color preC = GUI.backgroundColor;

            if (Get.PaintingID == -1) GUI.backgroundColor = Color.green;
            if (GUI.Button(bRect, "Paint Cells "))
            {
                Get.PaintingID = -1;
            }

            GUI.backgroundColor = preC;

            //int commandsPerPage = 7;
            //int totalPages = ((Get.FieldSetup.CellsInstructions.Count + (commandsPerPage - 1)) / commandsPerPage);

            //bRect.y += 10;

            //if (totalPages > 1)
            //{
            //    float yy = bRect.y + FGenerators.EditorUIScale * 24;
            //    Rect btRect = new Rect(bRect.x, yy, 20, 18);
            //    if (GUI.Button(btRect, "<")) { Get._Editor_CommandsPage -= 1; if (Get._Editor_CommandsPage < 0) Get._Editor_CommandsPage = totalPages - 1; }
            //    btRect = new Rect(bRect.x + 24, yy, 86, 18);
            //    GUI.Label(btRect, (Get._Editor_CommandsPage + 1) + " / " + totalPages, FGUI_Resources.HeaderStyle);
            //    btRect = new Rect(bRect.x + 100, yy, 20, 18);
            //    if (GUI.Button(btRect, ">")) { Get._Editor_CommandsPage += 1; if (Get._Editor_CommandsPage >= totalPages) Get._Editor_CommandsPage = 0; }
            //    bRect.y += 20;
            //}

            //bRect.y += 4 * FGenerators.EditorUIScale;
            //bRect.height -= 4;
            //bRect.width += 6;
            //float padding = 23 * FGenerators.EditorUIScale;

            //if (Get._Editor_CommandsPage >= totalPages) Get._Editor_CommandsPage = 0;
            //int startI = Get._Editor_CommandsPage * commandsPerPage;

            //for (int i = startI; i < startI + commandsPerPage; i++)
            //{
            //    if (i >= Get.FieldSetup.CellsInstructions.Count) break;

            //    bRect.y += padding;
            //    if (Get.PaintingID == i) GUI.backgroundColor = Color.green;

            //    if (FGenerators.CheckIfExist_NOTNULL(Get.FieldSetup.CellsInstructions[i]))
            //        if (GUI.Button(bRect, Get.FieldSetup.CellsInstructions[i].Title))
            //            Get.PaintingID = i;

            //    if (Get.PaintingID == i) GUI.backgroundColor = preC;
            //}

            GUI.backgroundColor = preC;
        }


        /// <summary>
        /// Drawing button visible in scene view
        /// </summary>
        bool DrawButton(GUIContent content, Vector3 pos, float size)
        {
            float sc = HandleUtility.GetHandleSize(pos);
            float hSize = Mathf.Sqrt(size) * 32 - sc * 16;

            if (hSize > 0f)
            {
                Handles.BeginGUI();
                Vector2 pos2D = HandleUtility.WorldToGUIPoint(pos);
                float hhSize = hSize / 2f;
                if (GUI.Button(new Rect(pos2D.x - hhSize, pos2D.y - hhSize, hSize, hSize), content))
                {
                    Handles.EndGUI();
                    return true;
                }

                Handles.EndGUI();
            }

            return false;
        }


        #endregion

    }


#endif
}