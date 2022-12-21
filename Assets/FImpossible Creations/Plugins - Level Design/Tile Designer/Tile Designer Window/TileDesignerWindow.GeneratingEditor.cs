#if UNITY_EDITOR
using FIMSpace.FEditor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FIMSpace.Generating.TileMeshSetup;

namespace FIMSpace.Generating
{
    public partial class TileDesignerWindow
    {
        static bool _display_normals = true;
        int _input_global_wasDrag = 0;

        private void GenerateTileMeshSetupMesh()
        {
            generatedMesh = EditedTileSetup.FullGenerateMesh();

            #region Scene preview

            if (useScenePreview)
            {
                if (sceneModelPreview == null)
                {
                    sceneModelPreview = new GameObject("Preview");
                    sceneModelPreview.AddComponent<MeshFilter>();
                    sceneModelPreview.AddComponent<MeshRenderer>();
                }

                sceneModelPreview.GetComponent<MeshFilter>().mesh = generatedMesh;
            }
            else
            {
                ClearScenePreview();
            }

            #endregion

            TileMeshUpdatePreview();

        }


        void TileMeshUpdatePreview()
        {
            if (EditedTileSetup == null) return;

            generatedMesh = EditedTileSetup.LatestGeneratedMesh;

            if (generatedMesh != null)
            {
                if (tileMeshPreviewEditor != null)
                {
                    tileMeshPreviewEditor.UpdateMesh(generatedMesh);
                    tileMeshPreviewEditor.SetMaterial(EditedTileSetup.Material);
                }
            }
        }

        void ClearScenePreview()
        {
            if (sceneModelPreview == null) return;
            //FGenerators.DestroyObject(sceneModelPreview);
        }

        bool DrawCopyButton()
        {
            return GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy Parameters Below"), FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(19));
        }

        bool DrawPasteButton()
        {
            return GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Clipboard").image, "Paste Parameters"), FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(19));
        }


        #region Helper Fields

        /// <summary> Edited mesh setup </summary>
        public TileMeshSetup s { get { return EditedTileSetup; } }

        public EMeshGenerator GenTechnique
        {
            get { return EditedTileSetup.GenTechnique; }
            set { EditedTileSetup.GenTechnique = value; }
        }

        public float width { get { return EditedTileSetup.width; } set { EditedTileSetup.width = value; } }

        public float depth
        {
            get
            {
                if (EditedTileSetup.GenTechnique == EMeshGenerator.Loft) return EditedTileSetup._loft_depthDim;
                if (EditedTileSetup.GenTechnique == EMeshGenerator.Sweep) return EditedTileSetup._sweep_radiusMul;
                return EditedTileSetup.depth;
            }
            set
            {
                if (EditedTileSetup.GenTechnique == EMeshGenerator.Loft) { EditedTileSetup._loft_depthDim = value; return; }
                if (EditedTileSetup.GenTechnique == EMeshGenerator.Sweep) { EditedTileSetup._sweep_radiusMul = value; return; }
                EditedTileSetup.depth = value;
            }
        }

        public float height { get { return EditedTileSetup.height; } set { EditedTileSetup.height = value; } }


        #endregion

        private void TileMeshGenericMenu()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Copy tile mesh setup"), false, () => { TileMeshSetup._CopyRef = EditedTileSetup; TileMeshSetup._CopyInstances = false; });
            menu.AddItem(new GUIContent("Copy tile mesh setup & copy instances"), false, () => { TileMeshSetup._CopyRef = EditedTileSetup; TileMeshSetup._CopyInstances = true; });

            if (TileMeshSetup._CopyRef != null)
                if (TileMeshSetup._CopyRef != EditedTileSetup)
                    menu.AddItem(new GUIContent("Paste with " + TileMeshSetup._CopyRef.Name + " parameters"), false, () => { _CopyRef.PasteAllSetupTo(EditedTileSetup, _CopyInstances); _CopyRef = null; });

            menu.ShowAsContext();
        }


        private GUIContent _techInfoContent = null;
        private static TileMeshSetup _TileMesh_CopyFrom = null;
        private void GUIEditor()
        {
            shapeChanged = false;

            if (GUILayout.Button("Mesh Editor View", FGUI_Resources.HeaderStyle)) { TileMeshGenericMenu(); }
            if (GUILayout.Button("Focus on the mesh shape", EditorStyles.centeredGreyMiniLabel)) { TileMeshGenericMenu(); }

            FGUI_Inspector.DrawUILine(0.3f, 0.5f);

            if (EditedTileSetup == null)
            {
                EditorGUILayout.HelpBox("No Tile Mesh to Edit!", MessageType.Error);
                return;
            }


            #region Initial prepare display rect

            Rect editorRect = GUILayoutUtility.GetLastRect();
            editorRect.position += new Vector2(3, EditorGUIUtility.singleLineHeight * 1.5f);
            editorRect.height = position.height - (editorRect.position.y + 10 + 22);
            editorRect.width = position.width - 20;
            editorRect.height -= 24;

            Rect displayRect = editorRect;

            #endregion


            bool drawOptions = true;


            if (_maximizedCurve != null)
            {
                if (Event.current.type == EventType.KeyDown)
                    if (Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.Tab)
                    { _maximizedCurve = null; Event.current.Use(); }

                if (_maximizedCurve != null)
                {
                    DrawMaximizedCurveEditor(editorRect, displayRect);
                    drawOptions = false;
                }
            }


            if (drawOptions)
            {
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);


                #region Upper Panel - Name, Technique


                GUILayout.Space(-3);
                EditorGUILayout.BeginHorizontal();

                DrawTileLeftArrowButton();
                DrawTileRightArrowButton();

                if (GUILayout.Button(new GUIContent("  Back to Setup", FGUI_Resources.Tex_Sliders), FGUI_Resources.ButtonStyle, GUILayout.Height(18))) { EditorMode = EMode.Setup; }
                GUILayout.Space(5);

                if (GUILayout.Button(new GUIContent(" + New & Edit", EditorGUIUtility.IconContent("Mesh Icon").image), FGUI_Resources.ButtonStyle, GUILayout.Width(110), GUILayout.Height(18)))
                {
                    TileMeshSetup nSetup = new TileMeshSetup("Tile Mesh " + (EditedDesign.TileMeshes.Count + 1));
                    EditedDesign.TileMeshes.Add(nSetup); _selectedTileMesh += 1;
                    EditedTileSetup = nSetup;
                }

                if (_comb_switchedFromComb)
                {
                    if (GUILayout.Button(new GUIContent(" To Combiner", FGUI_Resources.Tex_Extension), FGUI_Resources.ButtonStyle, GUILayout.Width(140), GUILayout.Height(18)))
                    {
                        _comb_switchedFromComb = false;
                        EditorMode = EMode.Combine;
                    }
                    GUILayout.Space(5);
                }


                if (DrawCopyButton()) { _TileMesh_CopyFrom = EditedTileSetup; }
                if (_TileMesh_CopyFrom != null) if (_TileMesh_CopyFrom != EditedTileSetup) if (DrawPasteButton()) { EditedTileSetup.PasteSettingsFrom(_TileMesh_CopyFrom); }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                EditedTileSetup.Name = EditorGUILayout.TextField("Name:", EditedTileSetup.Name);
                EditedTileSetup.Material = (Material)EditorGUILayout.ObjectField(EditedTileSetup.Material, typeof(Material), false, GUILayout.MaxWidth(140));
                GUILayout.EndHorizontal();
                //EditorGUILayout.ObjectField("Default Material:", null, typeof(Material), false);
                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                EditedTileSetup.GenTechnique = (EMeshGenerator)EditorGUILayout.EnumPopup("Technique:", EditedTileSetup.GenTechnique);

                if (_techInfoContent == null) _techInfoContent = new GUIContent(" ", FGUI_Resources.Tex_Info, "Click for more info");
                if (_techInfoContent.image == null) _techInfoContent = new GUIContent(" ", FGUI_Resources.Tex_Info, "Click for more info");
                if (EditedTileSetup.GenTechnique == EMeshGenerator.Loft)
                { _techInfoContent.text = " Loft?"; GUILayout.Space(8); if (GUILayout.Button(_techInfoContent, EditorStyles.label, GUILayout.Width(48), GUILayout.Height(16))) { EditorUtility.DisplayDialog("Loft Technique", "Loft technique is dedicated for generating walls,roofs,curved walls,pavements meshes, but you can also find some additional uses for it.", "Ok"); } }
                else if (EditedTileSetup.GenTechnique == EMeshGenerator.Lathe) { GUILayout.Space(8); _techInfoContent.text = " Lathe?"; if (GUILayout.Button(_techInfoContent, EditorStyles.label, GUILayout.Width(58), GUILayout.Height(16))) { EditorUtility.DisplayDialog("Lathe Technique", "Lathe technique is dedicated for generating pillars, columns, rungs or other cylindrical elements.", "Ok"); } }
                else if (EditedTileSetup.GenTechnique == EMeshGenerator.Extrude) { GUILayout.Space(8); _techInfoContent.text = " Extrude?"; if (GUILayout.Button(_techInfoContent, EditorStyles.label, GUILayout.Width(69), GUILayout.Height(16))) { EditorUtility.DisplayDialog("Extrude Technique", "Extrude technique is dedicated for generating cut-shapes, for example to cut doorway shape (in the combine mode) in wall generated with Loft technique.", "Ok"); } }

                GUILayout.EndHorizontal();
                GUILayout.Space(4);


                #endregion


                FGUI_Inspector.DrawUILine(0.4f, 0.5f, 1, 8);
                EditorGUI.BeginChangeCheck();


                #region Tile Mesh Technique Upper Panels 

                if (GenTechnique == EMeshGenerator.Loft)
                    GUI_LoftTopPanel();
                else if (GenTechnique == EMeshGenerator.Lathe)
                    GUI_LatheTopPanel();
                else if (GenTechnique == EMeshGenerator.Extrude)
                    GUI_ExtrudeTopPanel();
                else if (GenTechnique == EMeshGenerator.Sweep)
                    GUI_SweepTopPanel();
                else if (GenTechnique == EMeshGenerator.CustomMeshAndExtras)
                    GUI_CustomMeshTopPanel();
                else if (GenTechnique == EMeshGenerator.Primitive)
                    GUI_PrimitiveTopPanel();

                #endregion


                FGUI_Inspector.DrawUILine(0.4f, 0.5f, 1, 8);



                #region Editing Curves Params Panel


                if (EditedTileSetup.DrawSnappingPX())
                {
                    EditorGUILayout.BeginHorizontal();

                    gridSnapping = EditorGUILayout.IntSlider("Snapping (px):", gridSnapping, 0, 32);

                    //int selectedP = -1;
                    //if (selectedPoint != null) selectedP = selectedPoint.index;
                    //selectedP = EditorGUILayout.IntField("", selectedP);
                    //selectedP = Mathf.Clamp(selectedP, -1, )

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUIUtility.labelWidth = 0;

                #endregion


                if (EditorGUI.EndChangeCheck())
                {
                    shapeChanged = true;
                    repaintRequest = true;
                }


                #region Display Curves Area


                editorRect = GUILayoutUtility.GetLastRect();
                editorRect.position += new Vector2(3, EditorGUIUtility.singleLineHeight * 1.5f);
                editorRect.height = position.height - (editorRect.position.y + 10 + 22);
                editorRect.width = position.width - 20;
                editorRect.height -= 24;

                displayRect = editorRect;

                // Aspect Ratio Compute
                displayRect = GetDisplayRectRatio(displayRect, editorRect);

                EditorGUILayout.BeginVertical(GUILayout.Height(editorRect.height));

                if (Event.current != null) if (Event.current.type == EventType.Repaint) _editorRect = editorRect;

                GUI.BeginGroup(editorRect);

                bool allowDraw = false;
                if (GenTechnique == EMeshGenerator.Extrude)
                {
                    allowDraw = width > 0f && height > 0f && EditedTileSetup._loftDepthCurveWidener > 0f;
                }
                else
                {
                    allowDraw = width > 0f && height > 0f && depth > 0f && EditedTileSetup._loftDepthCurveWidener > 0f;
                }

                if (!_foldout_bigPreview) if (allowDraw)
                    {
                        PrepareCurveDraw();
                        if (GenTechnique == EMeshGenerator.Loft) DrawLoftEditor(editorRect, displayRect);
                        else if (GenTechnique == EMeshGenerator.Lathe) DrawLatheEditor(editorRect, displayRect);
                        else if (GenTechnique == EMeshGenerator.Extrude) DrawExtrudeEditor(editorRect, displayRect);
                        else if (GenTechnique == EMeshGenerator.Sweep) DrawSweepEditor(editorRect, displayRect);
                        else if (GenTechnique == EMeshGenerator.CustomMeshAndExtras) DrawCustomMeshEditor(editorRect, displayRect);
                        else if (GenTechnique == EMeshGenerator.Primitive) DrawPrimitiveEditor(editorRect, displayRect);

                        CheckGeneratorUpdate();
                    }



                GUI.EndGroup();
                EditorGUILayout.EndVertical();


                #endregion


                EditorGUILayout.EndVertical();

            }


            #region Bottom Preview area

            float previewSize = 130f;

            Rect prevRect = position;
            prevRect.position = new Vector2(position.width - previewSize, position.height - (previewSize + 4));
            prevRect.size = new Vector2(previewSize - 3, previewSize);

            if (!_foldout_meshEditPreview)
            {
                prevRect.position = new Vector2(prevRect.position.x - 246, prevRect.position.y - 236);
                prevRect.width += 240;
                prevRect.height += 200;
            }


            Rect prevRectButton = prevRect;
            prevRectButton.position += new Vector2(0, -2);
            prevRectButton.width = 80; prevRectButton.height = 20;

            if (GUI.Button(prevRectButton, new GUIContent("  Preview:"), EditorStyles.label)) _foldout_meshEditPreview = !_foldout_meshEditPreview;

            Rect modelRect = prevRect;

            prevRect.position = new Vector2(position.width - previewSize, position.height - (previewSize + 4));
            prevRect.size = new Vector2(previewSize - 3, 18);


            if (drawOptions)
            {
                if (EditedTileSetup.DrawMeshOptions())
                {

                    GUILayout.FlexibleSpace();



                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);

                    EditorGUIUtility.labelWidth = 110;

                    Color preBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.55f, 1f, 0.75f, 1f);
                    s.NormalsMode = (ENormalsMode)EditorGUILayout.EnumPopup(s.NormalsMode, GUILayout.Width(120));
                    GUI.backgroundColor = preBg;

                    if (s.NormalsMode == ENormalsMode.HardNormals)
                    {
                        GUILayout.Space(5);
                        s.HardNormals = EditorGUILayout.Slider(/*"Hard Normals:", */s.HardNormals, 0f, 1f);
                    }

                    GUILayout.Space(10);

                    EditorGUIUtility.labelWidth = 43;
                    s.Origin = (EOrigin)EditorGUILayout.EnumPopup("Origin:", s.Origin, GUILayout.Width(130));

                    GUILayout.Space(10);

                    EditorGUIUtility.labelWidth = 27;
                    s.UVFit = (EUVFit)EditorGUILayout.EnumPopup("UV:", s.UVFit, GUILayout.Width(87));
                    GUILayout.Space(4);
                    s.UVMul = EditorGUILayout.Vector2Field("", s.UVMul, GUILayout.Width(100));

                    EditorGUIUtility.labelWidth = 0;

                    GUILayout.Space(140);
                    EditorGUILayout.EndHorizontal();
                }


                EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxBlankStyle, GUILayout.Height(24));

                EditorGUIUtility.labelWidth = 88;
                GUILayout.Space(2);
                autoRefresh = EditorGUILayout.Toggle("Auto Refresh:", autoRefresh, GUILayout.Width(110));
                if (autoRefresh) autoRefreshFull = EditorGUILayout.Toggle(GUIContent.none, autoRefreshFull, GUILayout.Width(16));

                EditorGUIUtility.labelWidth = 0;
                GUILayout.Space(10);

                if (GUILayout.Button(new GUIContent("  Refresh", FGUI_Resources.Tex_Refresh), FGUI_Resources.ButtonStyle, GUILayout.Height(20))) { GenerateTileMeshSetupMesh(); }
                GUILayout.Space(4);

                if (autoRefresh)
                {
                    if (GUILayout.Button(new GUIContent(" Scene Mesh", EditorGUIUtility.IconContent("SceneAsset Icon").image), FGUI_Resources.ButtonStyle, GUILayout.Width(100), GUILayout.Height(20)))
                    {
                        GenerateSceneObject();
                    }
                }

                GUILayout.Space(4);

                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Preview, "Click for Big Screen Preview"), EditorStyles.label, GUILayout.Width(26), GUILayout.Height(20))) { _foldout_bigPreview = !_foldout_bigPreview; }
                GUILayout.Space(130);

                EditorGUILayout.EndHorizontal();

            }

            #endregion


            if (_foldout_bigPreview)
            {
                Rect bigRect = displayRect;
                bigRect.width = editorRect.width - 5;
                TilePreviewDisplay(bigRect);
                //if (tileMeshPreviewEditor != null) tileMeshPreviewEditor.OnInteractivePreviewGUI(bigRect, EditorStyles.textArea);
            }
            else
                TilePreviewDisplay(modelRect);


            if (!_foldout_bigPreview)
                GUI.Label(prevRectButton, new GUIContent("  Preview:"), EditorStyles.label);

            if (drawOptions == false)
            {
                prevRectButton = new Rect(8, displayRect.y + displayRect.height + 20, displayRect.width, 28);

                if (_maximizedZoom == 1f)
                    GUI.Label(prevRectButton, new GUIContent("Press ESC to restore view\nUse mouse scroll to zoom"), EditorStyles.centeredGreyMiniLabel);
                else
                    GUI.Label(prevRectButton, new GUIContent("Use WSAD or Arrows to move zoomed view"), EditorStyles.centeredGreyMiniLabel);

                prevRectButton = new Rect(8, displayRect.y + displayRect.height + 32, 80, 20);
                if (GUI.Button(prevRectButton, "Back")) { _maximizedCurve = null; }

                if (_maximizedZoom != 1f)
                {
                    prevRectButton.x += 100;
                    prevRectButton.width = 110;
                    if (GUI.Button(prevRectButton, "Reset View")) { _maximizedZoom = 1f; _maximizedOffset = Vector2.zero; }
                }
            }
        }

        Rect GetDisplayRectRatio(Rect displayRect, Rect editorRect)
        {
            if (GenTechnique == EMeshGenerator.Loft)
            {
                float ratio = (width / depth);
                float unit = editorRect.width;

                if (editorRect.height < unit) unit = editorRect.height;

                if (ratio * unit > editorRect.width) unit = displayRect.width * (depth / width);
                displayRect.width = ratio * unit;

                ratio = (depth / width);
                if (ratio * unit > editorRect.height) unit = displayRect.height * (width / depth);

                displayRect.height = ratio * unit;
                return displayRect;
            }
            else
            {
                float ratio = (width / height);
                float unit = editorRect.width;

                if (editorRect.height < unit) unit = editorRect.height;

                if (ratio * unit > editorRect.width) unit = displayRect.width * (height / width);
                displayRect.width = ratio * unit;

                ratio = (height / width);
                if (ratio * unit > editorRect.height) unit = displayRect.height * (width / height);

                displayRect.height = ratio * unit;
                return displayRect;
            }
        }


        void TilePreviewDisplay(Rect? rect, Mesh setMesh = null, Material setMat = null)
        {
            if (tileMeshPreviewEditor == null)
            {
                tileMeshPreviewEditor = (TilePreviewWindow)Editor.CreateEditor(generatedMesh, typeof(TilePreviewWindow));
            }

            if (rect == null) return;

            if (tileMeshPreviewEditor != null)
            {
                tileMeshPreviewEditor.UseScroll = _maximizedCurve == null;
                if (setMesh != null) tileMeshPreviewEditor.UpdateMesh(setMesh);
                if (setMat != null) tileMeshPreviewEditor.SetMaterial(setMat);
                tileMeshPreviewEditor.OnInteractivePreviewGUI(rect.Value, EditorStyles.textArea);
            }
        }

        void DrawMaximizedCurveEditor(Rect editorRect, Rect displayArea)
        {
            if (_maximizedCurve == null) return;

            Rect lRect = displayArea;
            lRect.position += new Vector2(20, 0);
            lRect.width -= 10;


            #region Zoom keys handling


            if (Event.current.type == EventType.ScrollWheel)
            {
                if (Event.current.delta.y > 0f)
                {
                    _maximizedZoom -= 0.1f;
                    if (_maximizedZoom > 2f) _maximizedZoom -= 0.1f;
                    if (_maximizedZoom > 3f) _maximizedZoom -= 0.2f;
                    Event.current.Use();
                    repaintRequestCounter = 5;
                }
                else if (Event.current.delta.y < 0f)
                {
                    _maximizedZoom += 0.1f;
                    if (_maximizedZoom > 2f) _maximizedZoom += 0.1f;
                    if (_maximizedZoom > 3f) _maximizedZoom += 0.2f;

                    Event.current.Use();
                    repaintRequestCounter = 5;
                }

                if (_maximizedZoom < 0.5f) _maximizedZoom = 0.5f;
                else if (_maximizedZoom > 8f) _maximizedZoom = 8f;
            }

            if (_maximizedZoom != 1f)
            {
                if (Event.current.type == EventType.KeyDown)
                {
                    if (Event.current.keyCode == KeyCode.A || Event.current.keyCode == KeyCode.LeftArrow)
                    {
                        Event.current.Use();
                        _maximizedOffset -= new Vector2(-30 * _maximizedZoom, 0f);
                        repaintRequestCounter = 5;
                    }

                    if (Event.current.keyCode == KeyCode.D || Event.current.keyCode == KeyCode.RightArrow)
                    {
                        Event.current.Use();
                        _maximizedOffset -= new Vector2(30 * _maximizedZoom, 0f);
                        repaintRequestCounter = 5;
                    }

                    if (Event.current.keyCode == KeyCode.W || Event.current.keyCode == KeyCode.UpArrow)
                    {
                        Event.current.Use();
                        _maximizedOffset += new Vector2(0f, 30 * _maximizedZoom);
                        repaintRequestCounter = 5;
                    }

                    if (Event.current.keyCode == KeyCode.S || Event.current.keyCode == KeyCode.DownArrow)
                    {
                        Event.current.Use();
                        _maximizedOffset += new Vector2(0f, -30 * _maximizedZoom);
                        repaintRequestCounter = 5;
                    }
                }
            }
            else
            {
                _maximizedOffset = Vector2.zero;
            }

            #endregion


            lRect = GetDisplayRectRatio(lRect, editorRect);
            lRect.size *= _maximizedZoom;
            lRect.position += _maximizedOffset;

            if (GenTechnique == EMeshGenerator.Loft)
            {
                if ( _maximizedCurve == _loft_depth) lRect.width *= s._loftDepthCurveWidener;
                DrawLoftDepthRect(lRect);
            }

            SetDisplayRect(lRect);
            GUI.Box(lRect, GUIContent.none, FGUI_Resources.BGInBoxStyleH);
            //DrawHeaderLabelOnRect(r, title);
            //DrawAxisInRect(r, axis2, axis1);

            DrawCurves(lRect, _maximizedCurve, false, false);
            UpdateCurveInputEvents(lRect, editorRect, _maximizedCurve);
            DrawCurveOptionsButton(lRect, _maximizedCurve);

            // Curve point handlers must be called in a row because of WindowsBegin()
            BeginWindows();
            _latestEditorDisplayRect = _editorDisplayRect1;
            bool changed = DisplaySplineHandler(lRect, _maximizedCurve, false);
            EndWindows();

            if (changed)
            {
                shapeChanged = true;
                repaintRequest = true;
            }

            Rect rMenu = editorRect;
            rMenu.position = new Vector2(editorRect.width - 240, 0);
            rMenu.position += new Vector2(-10, 90);
            rMenu.width = 240;
            DrawCurvePointMenu(rMenu);
        }


        private void PrepareCurveDraw()
        {
            if (s == null) return;

            if (_editorPoints == null) _editorPoints = new List<CurvePoint>();
            if (_editorPoints2 == null) _editorPoints2 = new List<CurvePoint>();

            s.PrepareCurves();
        }

        void MeshAutoUpdateChanges()
        {
            GenerateTileMeshSetupMesh();
            repaintRequest = true;
            shapeEndChanging = false;
            SceneView.RepaintAll();
        }

        Rect _latestEditorDisplayRect
        {
            set
            {
                CurvePoint._latestEditorDisplayRect = value;
            }

            get
            {
                return CurvePoint._latestEditorDisplayRect;
            }
        }

        List<CurvePoint> GetOwnerCurve(CurvePoint point)
        {
            if (GenTechnique == EMeshGenerator.Loft)
            {
                if (_loft_depth.Contains(point)) return _loft_depth;
                if (_loft_distribute.Contains(point)) return _loft_distribute;
                if (_loft_height.Contains(point)) return _loft_height;
            }
            else if (GenTechnique == EMeshGenerator.Lathe)
            {
                if (s._lathe_points.Contains(point)) return s._lathe_points;
            }
            else if (GenTechnique == EMeshGenerator.Extrude)
            {
                if (s._extrude_curve.Contains(point)) return s._extrude_curve;
            }
            else if (GenTechnique == EMeshGenerator.Sweep)
            {
                if (s._sweep_path.Contains(point)) return s._sweep_path;
                if (s._sweep_radius.Contains(point)) return s._sweep_radius;
                if (s._sweep_shape.Contains(point)) return s._sweep_shape;
            }

            return null;
        }

        void DrawCurvePointMenu(Rect r)
        {

            if (selectedPoint != null)
            {

                if (switchSelPointFlag != selectedPoint)
                {
                    if (selectedPoint != null) editingCurve = GetOwnerCurve(selectedPoint);
                    switchSelPointFlag = selectedPoint;
                }

                Rect menuR = r;
                float tgtHeight = 120f;


                if (selectedPoint.Mode == CurvePoint.EPointMode.Manual) tgtHeight = 164;
                else if (selectedPoint.Mode == CurvePoint.EPointMode.Auto) tgtHeight = 144;


                int extraDrawMode = 0;

                if (GenTechnique == EMeshGenerator.Loft)
                {
                    if (editingCurve == _loft_height)
                    {
                        extraDrawMode = 1;
                        tgtHeight += 22;
                    }
                }
                else if (GenTechnique == EMeshGenerator.Sweep)
                {
                    if (editingCurve == s._sweep_path)
                    {
                        extraDrawMode = 2;
                        tgtHeight += 22;
                    }
                }


                menuR.height = Mathf.Min(tgtHeight, r.height);
                if (selectedPointListOwner == 1) menuR.height += 20;

                GUI.color = new Color(0.3f, 0.3f, 0.425f, 1f);
                GUI.Box(menuR, GUIContent.none, FGUI_Resources.HeaderBoxStyleH);
                GUI.color = Color.white;

                GUILayout.BeginArea(menuR, FGUI_Resources.BGInBoxBlankStyle);




                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(22);
                GUILayout.Label("Selected [" + selectedPoint.index + "]", FGUI_Resources.HeaderStyle);
                if (GUILayout.Button(FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(20))) { _editorEventRemoveSelected = true; }
                EditorGUILayout.EndHorizontal();

                Vector2 pos = selectedPoint.rect.position;
                EditorGUI.BeginChangeCheck();

                EditorGUIUtility.wideMode = false;

                Vector2 lpos = selectedPoint.localPos;
                lpos = EditorGUILayout.Vector2Field("Position:", lpos);
                if (selectedPoint.localPos != lpos)
                {
                    selectedPoint.localPos = lpos;
                }

                //pos = EditorGUILayout.Vector2Field("Position:", pos);
                //if (selectedPoint.rect.position != pos)
                //{
                //    Rect preDisp = _latestEditorDisplayRect;
                //    _latestEditorDisplayRect = _latestEditorDisplayRectSelected;
                //    selectedPoint.rPos = pos;
                //    _latestEditorDisplayRect = preDisp;
                //}

                EditorGUIUtility.wideMode = true;


                GUILayout.Space(2);

                var preMode = selectedPoint.Mode;
                selectedPoint.Mode = (CurvePoint.EPointMode)EditorGUILayout.EnumPopup("Curve Mode:", selectedPoint.Mode);

                if (selectedPoints.Count > 0)
                    if (selectedPoint.Mode != preMode)
                        for (int a = 0; a < selectedPoints.Count; a++)
                            selectedPoints[a].Mode = selectedPoint.Mode;

                if (selectedPointListOwner == 1)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUIUtility.labelWidth = 98;

                    Color prePCol = selectedPoint.VertexColor;
                    float preFallf = selectedPoint.VertexColorFalloff;

                    selectedPoint.VertexColor = EditorGUILayout.ColorField("Vertex Color:", selectedPoint.VertexColor);
                    GUILayout.Space(16);

                    Rect paletteRect = GUILayoutUtility.GetLastRect();
                    paletteRect.size = new Vector2(6, 8);
                    paletteRect.position += new Vector2(3, 3);
                    Color preC = GUI.color; float a = selectedPoint.VertexColor.a;
                    GUI.color = Color.red; if (GUI.Button(paletteRect, GUIContent.none, FGUI_Resources.HeaderBoxStyleH)) { selectedPoint.VertexColor = Color.red; selectedPoint.VertexColor.a = a; }
                    paletteRect.position += new Vector2(0, 9); GUI.color = Color.green; if (GUI.Button(paletteRect, GUIContent.none, FGUI_Resources.HeaderBoxStyleH)) { selectedPoint.VertexColor = Color.green; selectedPoint.VertexColor.a = a; }
                    paletteRect.position += new Vector2(8, -9); GUI.color = Color.blue; if (GUI.Button(paletteRect, GUIContent.none, FGUI_Resources.HeaderBoxStyleH)) { selectedPoint.VertexColor = Color.blue; selectedPoint.VertexColor.a = a; }
                    paletteRect.position += new Vector2(0, 9); GUI.color = Color.white; if (GUI.Button(paletteRect, GUIContent.none, FGUI_Resources.HeaderBoxStyleH)) { selectedPoint.VertexColor = Color.white; selectedPoint.VertexColor.a = a; }
                    GUI.color = preC;

                    EditorGUIUtility.labelWidth = 4;
                    selectedPoint.VertexColorFalloff = EditorGUILayout.FloatField(new GUIContent(" ", "Vertex Color Falloff"), selectedPoint.VertexColorFalloff, GUILayout.Width(38));
                    EditorGUIUtility.labelWidth = 0;

                    if (selectedPoint.VertexColor != prePCol)
                    {
                        if (selectedPoints.Count > 0)
                            for (int p = 0; p < selectedPoints.Count; p++)
                                selectedPoints[p].VertexColor = selectedPoint.VertexColor;
                    }

                    if (selectedPoint.VertexColorFalloff != preFallf)
                    {
                        if (selectedPoints.Count > 0)
                            for (int p = 0; p < selectedPoints.Count; p++)
                                selectedPoints[p].VertexColorFalloff = selectedPoint.VertexColorFalloff;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (selectedPoint.Mode == CurvePoint.EPointMode.Manual)
                {
                    EditorGUIUtility.labelWidth = 64;
                    GUILayout.Space(4);

                    if (selectedPoint.next != null) selectedPoint.localInTan = EditorGUILayout.Vector2Field("In Tan:", selectedPoint.localInTan);

                    if (selectedPoint.pre != null) selectedPoint.pre.localNextTan = EditorGUILayout.Vector2Field("Back Tan:", selectedPoint.pre.localNextTan);
                    else selectedPoint.localNextTan = EditorGUILayout.Vector2Field("Next Tan:", selectedPoint.localNextTan);

                    EditorGUIUtility.labelWidth = 0;
                }
                else if (selectedPoint.Mode == CurvePoint.EPointMode.Auto)
                {
                    float preAutoF = selectedPoint.AutoFactor;
                    selectedPoint.AutoFactor = EditorGUILayout.FloatField("Auto Curve Factor:", selectedPoint.AutoFactor);

                    if (selectedPoints.Count > 0)
                        if (selectedPoint.AutoFactor != preAutoF)
                            for (int a = 0; a < selectedPoints.Count; a++)
                                selectedPoints[a].AutoFactor = selectedPoint.AutoFactor;
                }

                GUILayout.Space(4);



                EditorGUILayout.BeginHorizontal();
                bool overr = selectedPoint.overrideNormal != Vector2.zero;
                EditorGUIUtility.labelWidth = 100;
                overr = EditorGUILayout.Toggle("Override Normal:", overr, GUILayout.Width(126));
                EditorGUIUtility.labelWidth = 0;
                EditorGUIUtility.fieldWidth = 24;

                if (overr)
                {
                    if (selectedPoint.overrideNormal == Vector2.zero) selectedPoint.overrideNormal = new Vector2(0.5f, 0.5f);
                    selectedPoint.overrideNormal = EditorGUILayout.Vector2Field(GUIContent.none, selectedPoint.overrideNormal, GUILayout.Width(100));
                }

                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.fieldWidth = 0;

                if (EditorGUI.EndChangeCheck())
                {
                    if (overr == false) selectedPoint.overrideNormal = Vector2.zero;
                    shapeChanged = true;
                    repaintRequest = true;
                }

                if (_editorPoints.Contains(selectedPoint))
                {

                }

                //selectedPoint.AlwaysInclude = EditorGUILayout.Toggle(new GUIContent("Always Include:", "If this curve point should be always included as vertex in the generated mesh"), selectedPoint.AlwaysInclude);
                //EditorGUILayout.Vector2Field("", selectedPoint.localPos);
                //EditorGUIUtility.labelWidth = 40;
                //EditorGUILayout.Vector2Field("lti", selectedPoint.localInTan);
                //EditorGUILayout.Vector2Field("ltn", selectedPoint.localNextTan);
                //EditorGUIUtility.labelWidth = 0;


                #region Extra curve point settings mode specific

                if (extraDrawMode == 1)
                {
                    bool pre = selectedPoint._Loft_Height_ShiftWhole;
                    selectedPoint._Loft_Height_ShiftWhole = EditorGUILayout.Toggle("Shift Down Height:", selectedPoint._Loft_Height_ShiftWhole, GUILayout.Width(100));
                    if (pre != selectedPoint._Loft_Height_ShiftWhole) shapeChanged = true;
                }
                else if (extraDrawMode == 2)
                {
                    EditorGUIUtility.labelWidth = 120;
                    float pre = selectedPoint._extra_z;
                    selectedPoint._extra_z = EditorGUILayout.FloatField("Path Z position:", selectedPoint._extra_z, GUILayout.Width(200));
                    if (pre != selectedPoint._extra_z) shapeChanged = true;
                    EditorGUIUtility.labelWidth = 0;
                }

                #endregion




                GUILayout.EndArea();
            }

        }

        static CurvePoint lastestDraggedNode = null;
        static Vector2 preNodeDragPos = Vector2.zero;

        /// <summary> Returns true if handles positions changed </summary>
        bool DisplaySplineHandler(Rect r, List<CurvePoint> list, bool? secondary)
        {
            if (secondary != true) _editorPoints = list;
            else _editorPoints2 = list; // secondary true

            bool changed = false;
            Color preC = GUI.color;

            if (secondary == null) BeginWindows();

            for (int i = 0; i < list.Count; i++)
            {
                CurvePoint p = list[i];
                CurvePoint pre = null;
                CurvePoint next = null;

                if (i - 1 >= 0) pre = list[i - 1];
                if (i + 1 < list.Count) next = list[i + 1];

                p.Update(i, pre, next);

                if (lastestDraggedNode != null) if (p == lastestDraggedNode) preNodeDragPos = lastestDraggedNode.localPos;

                Vector2 initRectPos = p.DisplayPos; // To change check

                GUI.color = p.VertexColor * (selectedPoint == p ? 1.1f : 0.8f);

                if (secondary != true)
                {
                    /* null or false */
                    p.rect = GUI.Window(i, p.rect, DrawCurveHandle, string.Empty, FGUI_Resources.HeaderBoxStyleH);
                }
                else // only true
                {
                    p.rect = GUI.Window(i + (_editorPoints.Count), p.rect, DrawCurveHandle2, string.Empty, FGUI_Resources.HeaderBoxStyleH);
                }

                GUI.color = preC;

                float s = p.rect.width / 2f;

                if (_latestEditorDisplayRect.width > 10 && _latestEditorDisplayRect.height > 10)
                {
                    if (p.DisplayPos != initRectPos)
                    {
                        p.SyncLocalPos(p.rect.position);
                        p.wasDrag = false;

                        Vector2 sPos = p.DisplayPos;
                        if (gridSnapping > 0f)
                        {
                            sPos = new Vector2(FlattenVal(sPos.x, gridSnapping), FlattenVal(sPos.y, gridSnapping));
                            if (sPos != p.DisplayPos) p.rPos = sPos;
                        }

                        float sx = s - r.x;
                        float sy = s - r.y;

                        if (p.DisplayPos.x < -sx) p.rPos = new Vector2(-sx, p.DisplayPos.y);
                        if (p.DisplayPos.x > r.width - sx) p.rPos = new Vector2(r.width - sx, p.DisplayPos.y);
                        if (p.DisplayPos.y < -sy) p.rPos = new Vector2(p.DisplayPos.x, -sy);
                        if (p.DisplayPos.y > r.height - sy) p.rPos = new Vector2(p.DisplayPos.x, r.height - sy);

                        changed = true;
                    }
                }

            }

            if (secondary == null) EndWindows();


            if (!(lastestDraggedNode is null)) if (list == dragSelectionOn) TranslateSelectedNodes();

            return changed;
        }


        void UpdateCurveInputEvents(Rect tgtRect, Rect displayArea, List<CurvePoint> list)
        {

            #region Selection Box

            if (_maximizedCurve == null)

                if (isSelectingMultiple)
                    if (dragSelectionOn == list)
                        if (Event.current != null)
                        {
                            Vector2 mousePos = Event.current.mousePosition;
                            selectionBox = new Rect();
                            Vector2 toMouse = mousePos - dragSelectingMultipleStart;

                            if (toMouse.magnitude > 20)
                            {
                                if (Event.current.type == EventType.Repaint)
                                {
                                    selectedPoints.Clear();

                                    if (toMouse.x > 0)
                                        selectionBox.position = new Vector2(dragSelectingMultipleStart.x, selectionBox.y);
                                    else
                                        selectionBox.position = new Vector2(mousePos.x, selectionBox.y);

                                    if (toMouse.y > 0)
                                        selectionBox.position = new Vector2(selectionBox.x, dragSelectingMultipleStart.y);
                                    else
                                        selectionBox.position = new Vector2(selectionBox.x, mousePos.y);

                                    selectionBox.size = new Vector2(Mathf.Abs(toMouse.x), Mathf.Abs(toMouse.y));

                                    GUI.Box(selectionBox, GUIContent.none, FGUI_Resources.HeaderBoxStyle);

                                    for (int p = 0; p < list.Count; p++)
                                    {
                                        var point = list[p];
                                        Rect trRect = list[p].rect; //_latestEditorDisplayRect;

                                        if (selectionBox.Overlaps(trRect))
                                        {
                                            if (!selectedPoints.Contains(point)) selectedPoints.Add(point);
                                        }
                                    }
                                }
                            }
                        }

            #endregion


            Event e = Event.current;

            if (e != null) if (tgtRect.Contains(e.mousePosition))
                {

                    if (e.type == EventType.MouseDown)
                    {

                        if (e.button == 0 || e.button == 1)
                        {
                            if (EditorApplication.timeSinceStartup - _doubleClickTimeMark < 0.225 || e.button == 1)
                            {
                                if (enteredPoint == null)
                                {
                                    AddNewPointInCanvasPos(displayArea, list, e);
                                    e.Use();
                                }
                            }

                            _doubleClickTimeMark = EditorApplication.timeSinceStartup;
                        }
                    }
                    else if (e.type == EventType.MouseUp)
                    {
                        if (enteredPoint == null)
                            if (dragSelectionOn == list)
                                if (wasSelectingMultiple < 0)
                                    if (!isSelectingMultiple && selectedPoints.Count > 0)
                                    {
                                        selectedPoints.Clear();
                                        Event.current.Use();
                                    }
                    }

                    //UnityEngine.Debug.Log("opa " + selectedPoint == null + " ? " + e.keyCode);
                    if (selectedPoint != null)
                        if (e.type == EventType.KeyDown)
                        {
                            if (e.keyCode == KeyCode.Delete)
                            {

                                if (dragSelectionOn == list)
                                {
                                    for (int i = 0; i < selectedPoints.Count; i++)
                                        if (list.Contains(selectedPoints[i])) list.Remove(selectedPoints[i]);

                                    selectedPoints.Clear();
                                }

                                if (list.Contains(selectedPoint)) list.Remove(selectedPoint);
                                selectedPoint = null;
                                selectedPointListOwner = -1;

                                e.Use();
                            }
                        }

                    if (enteredPoint == null)
                        if (_dragInfo == null)
                        {
                            if (selectedPoint == null || selectedPoint.wasDrag == false)
                            {
                                //if (displayArea.Contains(Event.current.mousePosition + new Vector2(displayArea.x, 0)))
                                if (_input_global_wasDrag <= 1)
                                {
                                    if (e.type == EventType.MouseDrag)
                                    {
                                        DragSelecting(list);
                                    }
                                    else if (e.type == EventType.MouseUp)
                                    {
                                        EndDragSelecting();
                                    }
                                }
                            }
                        }

                }

            #region Selecting events

            if (Event.current != null)
            {

                if (isSelectingMultiple)
                {
                    if (e.type == EventType.MouseDrag)
                    {
                        if (_maximizedCurve == null) // This was preventing movement of points in maximized curve mode
                            Event.current.Use();
                    }
                    else
                    {
                        if (e.type == EventType.MouseUp)
                        {
                            EndDragSelecting();
                            Event.current.Use();
                        }
                    }
                }
            }

            for (int i = 0; i < selectedPoints.Count; i++)
            {
                GUI.Box(selectedPoints[i].rect, GUIContent.none, FGUI_Resources.HeaderBoxStyleH);
            }

            #endregion

            if (_editorEventRemoveSelected)
                if (list.Contains(selectedPoint))
                {
                    list.Remove(selectedPoint);
                    selectedPoint = null;
                    selectedPointListOwner = -1;
                    _editorEventRemoveSelected = false;
                }

            wasSelectingMultiple -= 1;
        }



        private void TranslateSelectedNodes()
        {

            if (lastestDraggedNode != null)
                if (preNodeDragPos != Vector2.zero)
                {
                    if (selectedPoints.Contains(lastestDraggedNode))
                    {
                        Vector2 offset = lastestDraggedNode.localPos - preNodeDragPos;

                        for (int i = 0; i < selectedPoints.Count; i++)
                        {
                            if (selectedPoints[i] == lastestDraggedNode) continue;
                            selectedPoints[i].localPos += offset;
                        }
                    }
                }
        }



        bool _editorEventRemoveSelected = false;

        void DrawSubdivsPreviewLines(List<MeshShapePoint> previewShape, float width, float height, bool mirror, bool loop = false)
        {
            Color preC = GUI.color;
            Color preH = Handles.color;

            for (int i = 0; i < previewShape.Count; i++)
            {
                var sp = previewShape[i];
                Vector3 pos = new Vector3();
                pos.x = sp.p.x * width;
                pos.y = sp.p.y * height;

                if (i < previewShape.Count - 1)
                {
                    if (EditorGUIUtility.isProSkin == false)
                        Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.85f);

                    var sp2 = previewShape[i + 1];
                    Vector3 pos2 = new Vector3();
                    pos2.x = sp2.p.x * width;
                    pos2.y = sp2.p.y * height;

                    Handles.DrawAAPolyLine(4, 2, pos, pos2);

                    if (mirror)
                    {
                        Vector3 mirror1 = sp.p;
                        mirror1.x = -mirror1.x + 2f;
                        mirror1.x *= width; mirror1.y *= height;
                        Vector3 mirror2 = sp2.p;
                        mirror2.x = -mirror2.x + 2f;
                        mirror2.x *= width; mirror2.y *= height;

                        Handles.DrawAAPolyLine(3, 2, mirror1, mirror2);
                    }
                }
                else
                {
                    if (loop)
                        if (previewShape.Count > 2)
                        {
                            Vector3 mirror1 = previewShape[0].p;
                            mirror1.x *= width; mirror1.y *= height;
                            Vector3 mirror2 = previewShape[previewShape.Count - 1].p;
                            mirror2.x *= width; mirror2.y *= height;
                            Handles.DrawAAPolyLine(3, 2, mirror1, mirror2);
                        }
                }

                if (_display_normals)
                {
                    Handles.color = new Color(0.3f, 0.3f, 1f, 0.75f);
                    Handles.DrawAAPolyLine(pos, pos + (Vector3)sp.normal * (width * 0.15f));
                }

                Vector2 size = new Vector2(6, 6);
                Rect pPoint = new Rect((Vector2)pos - size / 2f, size);

                GUI.color = sp.c * 0.85f;
                GUI.DrawTexture(pPoint, FGUI_Resources.Tex_UpFold);
                GUI.color = preC;

                Handles.color = preH;
            }


        }

        private void CheckGeneratorUpdate()
        {
            if (shapeChanged)
            {
                shapeChanged = false;

                s.QuickUpdate();

                if (autoRefresh)
                    if (shapeEndChanging)
                    {
                        shapeEndChanging = false;
                        MeshAutoUpdateChanges();
                    }

                if (autoRefresh) if (autoRefreshFull) shapeEndChanging = true;
            }

            if (autoRefresh)
                if (shapeEndChanging)
                {
                    shapeEndChanging = false;
                    MeshAutoUpdateChanges();
                }
        }

    }
}
#endif